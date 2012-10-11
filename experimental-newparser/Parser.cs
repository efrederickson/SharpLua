using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using experimental_newparser.Ast.Expression;
using experimental_newparser.Ast.Statement;

namespace experimental_newparser
{
    public class Parser
    {
        TokenReader tok;

        public Parser(TokenReader tr)
        {
            tok = tr;
        }

        void error(string msg)
        {
            throw new LuaSourceException(tok.Peek().Line, tok.Peek().Column, msg);
        }

        FunctionStatement ParseFunctionArgsAndBody()
        {
            FunctionStatement func = new FunctionStatement();

            if (tok.ConsumeSymbol('(') == false)
                error("'(' expected");

            // arg list
            List<Variable> arglist = new List<Variable>();
            bool isVarArg = false;
            while (tok.ConsumeSymbol(')') == false)
            {
                if (tok.Is(TokenType.Ident))
                {
                    Variable arg = new Variable();
                    arg.Name = tok.Get().Data;
                    func.AddLocal(arg);
                    arglist.Add(arg);
                    if (!tok.ConsumeSymbol(','))
                        if (tok.ConsumeSymbol(')'))
                            break;
                        else
                            error("')' expected");
                }
                else if (tok.ConsumeSymbol("..."))
                {
                    isVarArg = true;
                    if (!tok.ConsumeSymbol(')'))
                        error("'...' must be the last argument of a function");
                    break;
                }
                else
                    error("Argument name or '...' expected");
            }

            // body
            Chunk body = ParseStatementList(func);
            // end
            if (!tok.ConsumeKeyword("end"))
                error("'end' expected after function body");

            //nodeFunc.AstType = AstType.Function;
            func.Arguments = arglist;
            func.Body = body;
            func.VarArg = isVarArg;

            return func;
        }


        Expression ParsePrimaryExpr(Chunk c)
        {
            if (tok.ConsumeSymbol('('))
            {
                Expression ex = ParseExpr(c);
                if (!tok.ConsumeSymbol(')'))
                    error("')' expected");

                // save the information about parenthesized expressions somewhere
                ex.ParenCount = ex.ParenCount + 1;
                return ex;
            }
            else if (tok.Is(TokenType.Ident))
            {
                Token id = tok.Get();
                VariableExpression v = new VariableExpression();
                Variable var = c.GetLocal(id.Data);
                if (var == null)
                    v.IsGlobal = true;
                else
                    v.Var = var;
                //nodePrimExp.AstType = AstType.VarExpr;
                v.Name = id.Data;

                return v;
            }
            else
                error("primary expression expected");
        }


        Expression ParseSuffixedExpr(Chunk scope, bool onlyDotColon)
        {
            // base primary expression
            Expression prim = ParsePrimaryExpr(scope);

            while (true)
            {
                if (tok.IsSymbol('.') || tok.IsSymbol(':'))
                {
                    string symb = tok.Get().Data; // '.' or ':'
                    if (!tok.Is(TokenType.Ident))
                        error("<Ident> expected");

                    Token id = tok.Get();
                    MemberExpr m = new MemberExpr();
                    m.Base = prim;
                    m.Indexer = symb;
                    m.Ident = id;

                    prim = m;
                }
                else if (!onlyDotColon && tok.ConsumeSymbol('['))
                {
                    Expression ex = ParseExpr(scope);

                    if (!tok.ConsumeSymbol(']'))
                        error("']' expected");

                    IndexExpr i = new IndexExpr();
                    i.Base = prim;
                    i.Index = ex;

                    prim = i;
                }
                else if (!onlyDotColon && tok.ConsumeSymbol('('))
                {
                    List<Expression> args = new List<Expression>();
                    while (!tok.ConsumeSymbol(')'))
                    {
                        Expression ex = ParseExpr(scope);

                        args.Add(ex);
                        if (!tok.ConsumeSymbol(','))
                            if (tok.ConsumeSymbol(')'))
                                break;
                            else
                                error("')' expected");
                    }
                    CallExpr c = new CallExpr();
                    c.Base = prim;
                    c.Arguments = args;

                    prim = c;
                }
                else if (!onlyDotColon &&
                        (tok.Is(TokenType.SingleQuoteString) ||
                        tok.Is(TokenType.DoubleQuoteString) ||
                        tok.Is(TokenType.LongString)))
                {
                    //string call

                    StringCallExpr e = new StringCallExpr();
                    e.Base = prim;
                    e.Arguments = new List<Expression> { new StringExpr(tok.Get().Data) };

                    prim = e;
                }
                else if (!onlyDotColon && tok.IsSymbol('{'))
                {
                    // table call
                    Expression ex = ParseExpr(scope);

                    TableCallExpr t = new TableCallExpr();
                    t.Base = prim;
                    t.Arguments = new List<Expression> { ex };

                    prim = t;
                }
                else
                    break;
            }
            return prim;
        }

        Expression ParseSimpleExpr(Chunk scope)
        {
            if (tok.Is(TokenType.Number))
                return new NumberExpr { Value = tok.Get().Data };
            else if (tok.Is(TokenType.DoubleQuoteString) || tok.Is(TokenType.SingleQuoteString) || tok.Is(TokenType.LongString))
                return new StringExpr { Value = tok.Get().Data };
            else if (tok.ConsumeKeyword("nil"))
                return new NilExpr();
            else if (tok.IsKeyword("false") || tok.IsKeyword("true"))
                return new BoolExpr { Value = tok.Get().Data == "true" };
            else if (tok.ConsumeSymbol("..."))
                return new VarargExpr();
            else if (tok.ConsumeSymbol('{'))
            {
                TableConstructorExpr v = new TableConstructorExpr();
                while (true)
                {
                    if (tok.IsSymbol('['))
                    {
                        // key
                        tok.Get(); // eat '['
                        Expression key = ParseExpr(scope);

                        if (!tok.ConsumeSymbol(']'))
                            error("']' expected");

                        if (!tok.ConsumeSymbol('='))
                            error("'=' Expected");

                        Expression value = ParseExpr(scope);

                        v.EntryList.Add(new KeyExpr
                        {
                            Key = key,
                            Value = value,
                        });
                    }
                    else if (tok.Is(TokenType.Ident))
                    {
                        // value or key
                        Token lookahead = tok.Peek(1);
                        if (lookahead.Type == TokenType.Symbol && lookahead.Data == "=")
                        {
                            // we are a key
                            Token key = tok.Get();
                            if (!tok.ConsumeSymbol('='))
                                error("'=' Expected");

                            Expression value = ParseExpr(scope);

                            v.EntryList.Add(new StringKeyExpr
                            {
                                Key = key.Data,
                                Value = value,
                            });
                        }
                        else
                        {
                            // we are a value
                            Expression val = ParseExpr(scope);

                            v.EntryList.Add(new ValueExpr
                            {
                                Value = val
                            });

                        }
                    }
                    else if (tok.ConsumeSymbol('}'))
                        break;
                    else
                    {
                        //value
                        Expression value = ParseExpr(scope);
                        v.EntryList.Add(new ValueExpr
                        {
                            Value = value
                        });
                    }

                    if (tok.ConsumeSymbol(';') || tok.ConsumeSymbol(','))
                        ; // all is good
                    else if (tok.ConsumeSymbol('}'))
                        break;
                    else
                        error("'}' or table entry Expected");
                }
                return v;
            }
            else if (tok.ConsumeKeyword("function"))
            {
                FunctionStmt func = ParseFunctionArgsAndBody(scope);
                func.IsLocal = true;
                func.Name = "<anonymous>";
                return func;
            }
            else if (tok.ConsumeSymbol('|'))
            {
                // inline function... |<arg list> -> <expr>, <expr>
                InlineFunctionStatement func = new InlineFunctionStatement();

                // arg list
                List<Variable> arglist = new List<Variable>();
                bool isVarArg = false;
                while (tok.ConsumeSymbol('|') == false)
                {
                    if (tok.Is(TokenType.Ident))
                    {
                        Variable arg = new Variable();
                        arg.Name = tok.Get().Data;
                        func.AddLocal(arg);
                        arglist.Add(arg);
                        if (!tok.ConsumeSymbol(','))
                            if (tok.ConsumeSymbol(')'))
                                break;
                            else
                                error("'|' expected");
                    }
                    else if (tok.ConsumeSymbol("..."))
                    {
                        isVarArg = true;
                        if (!tok.ConsumeSymbol(')'))
                            error("'...' must be the last argument of a function");
                        break;
                    }
                    else
                        error("Argument name or '...' expected");
                }
                if (!tok.ConsumeSymbol("->"))
                    error("'->' expected");
                // body
                List<Expression> body = ParseExpr(func);
                while (tok.ConsumeSymbol(','))
                    body.Add(ParseExpr(func));
                // end
                if (!tok.ConsumeKeyword("end"))
                    error("'end' expected after function body");

                //nodeFunc.AstType = AstType.Function;
                func.Arguments = arglist;
                func.Expressions = body;
                func.VarArg = isVarArg;

                return func;
            }
            else
                return ParseSuffixedExpr(scope);
        }


        bool isUnOp(string o)
        {
            foreach (string s in new string[] { "-", "not", "#", "!" })
                if (s == o)
                    return true;
            return false;
        }

        int unopprio = 8;

        class priority_
        {
            public string op;
            public int l;
            public int r;

            public priority_(string op, int l, int r)
            {
                this.op = op;
                this.l = l;
                this.r = r;
            }
        }

        priority_[] priority = new priority_[] {
 		new priority_("+", 6,6),
 		new priority_("-", 6,6),
 		new priority_("%", 7,7),
 		new priority_("/", 7,7),
 		new priority_("*", 7,7),
 		new priority_("^", 10,9),
 		new priority_("..", 5,4),
 		new priority_("==", 3,3),
 		new priority_("<", 3,3),
 		new priority_("<=", 3,3),
 		new priority_("~=", 3,3),
 		new priority_(">", 3,3),
 		new priority_(">=", 3,3),
 		new priority_("and", 2,2),
 		new priority_("or", 1,1),
        new priority_(">>", 7, 7),
        new priority_("<<", 7, 7),
        new priority_("&", 7, 7),
        new priority_("|", 7, 7),
 	};

        priority_ getpriority(string d)
        {
            foreach (priority_ p in priority)
                if (p.op == d)
                    return p;
            return null;
        }

        Expression ParseSubExpr(Chunk scope, int level)
        {
            // base item, possibly with unop prefix
            Expression exp = null;
            if (isUnOp(tok.Peek().Data))
            {
                string op = tok.Get().Data;
                exp = ParseSubExpr(scope, unopprio);
                exp = new UnOpExpr { Rhs = exp, op = op };
            }
            else
                exp = ParseSimpleExpr(scope);

            if (exp is InlineFunctionStatement)
                return exp; // |arg| -> expr, expr    functions cannot have any extra parts

            // next items in chain
            while (true)
            {
                priority_ prio = getpriority(tok.Peek().Data);
                if (prio != null && prio.l > level)
                {
                    string op = tok.Get().Data;
                    Expression rhs = ParseSubExpr(scope, prio.r);

                    BinOpExpr binOpExpr = new BinOpExpr();
                    binOpExpr.Lhs = exp;
                    binOpExpr.Op = op;
                    binOpExpr.Rhs = rhs;
                    exp = binOpExpr;
                }
                else
                    break;
            }
            return exp;
        }

        Expression ParseExpr(Chunk scope)
        {
            return ParseSubExpr(scope, 0);
        }


        Statement ParseStatement(Chunk scope)
    {
 		Statement stat = null;
 		// print(tok.Peek().Print())
         if (tok.ConsumeKeyword("if"))
         {
 			//setup
 			IfStmt _if = new IfStmt();
 
 			//clauses
 			do
            {
 				Expr nodeCond = ParseExpr(scope);
 				
 				if (!tok.ConsumeKeyword("then"))
 					error("'then' expected");
 				
 				Chunk nodeBody = ParseStatementList(scope);
 				
                _if.Clauses.Add(new ElseIfStmt
                    { Condition = nodeCond, Body = nodeBody });
            }
 			while (tok.ConsumeKeyword("elseif"));
 
 			// else clause
 			if (tok.ConsumeKeyword("else"))
            {
 				Chunk nodeBody = ParseStatementList(scope);
 				
 				nodeIfStat.Clauses.Add(new ElseStmt{
 					Body = nodeBody
 				});
 			}
 
 			// end
 			if (!tok.ConsumeKeyword("end"))
 				error("'end' expected");
 
 			    stat = nodeIfStat;
        }
 		else if (tok.ConsumeKeyword("while"))
        {
 			//setup
 			WhileStatement w = new WhileStatement();
 
 			// condition
 			Expression nodeCond = ParseExpr(scope);
 			
 
 			// do
 			if (!tok.ConsumeKeyword("do"))
 				error("'do' expected");
 			
 
 			// body
 			Chunk body = ParseStatementList(scope);
 
 			//end
 			if (!tok.ConsumeKeyword("end"))
 				error("'end' expected");
 			
 
 			// return
 			w.Condition = nodeCond;
 			w.Body = body;
 			stat = w;
        }
 		else if (tok.ConsumeKeyword("do"))
         {
 			// do block
 			Chunk b = ParseStatementList(scope);
 			
 			if (!tok.ConsumeKeyword("end"))
 				error("'end' expected");
 			
 			stat = new DoStatement{Body= b};
        }
 		else if (tok.ConsumeKeyword("for"))
         {
 			//for block
 			if (!tok.Is(TokenType.Ident))
 				error("<ident> expected");
 			
 			Token baseVarName = tok.Get();
 			if (tok.ConsumeSymbol('='))
            {
 				//numeric for
 				NumericForStatement forL = new NumericForStatement();
                Variable forVar = new Variable() { Name = baseVarName.Data };
                forL.AddLocal(forVar);
 				
 				Expression startEx = ParseExpr(scope);
 				
 				if (!tok.ConsumeSymbol(','))
 					error("',' expected");
 				
 				Expression endEx = ParseExpr(scope);
 				
 				Expression stepEx = null;
 				if (tok.ConsumeSymbol(','))
                {
 					stepEx = ParseExpr(scope);
 				}
 				if (!tok.ConsumeKeyword("do"))
 					error("'do' expected");
 				
 				
 				Chunk body = ParseStatementList(forL);
 				
 				if (!tok.ConsumeKeyword("end"))
 					error("'end' expected");
 				
 				
 				forL.Variable = forVar;
 				forL.Start = startEx;
 				forL.End = endEx;
 				forL.Step = stepEx;
 				forL.Body = body;
 				stat = forL;
            }
 			else
            {
 				// generic for
 				GenericForStatement forL = new GenericForStatement();
 				
 				List<Variable> varList = new List<Variable>{forL.CreateLocal(baseVarName.Data)};
 				while (tok.ConsumeSymbol(','))
                {
 					if (!tok.Is(TokenType.Ident))
 						error("for variable expected");
 					
 					varList.Add(forL.CreateLocal(tok.Get().Data));
 				}
 				if (!tok.ConsumeKeyword("in"))
 					error("'in' expected");
 				
 				List<Expression> generators = new List<Expression>();
 				Expression first = ParseExpr(scope);
 				
 				generators.Add(first);
 				while (tok.ConsumeSymbol(','))
                {
 					Expression gen = ParseExpr(scope);
 					generators.Add(gen);
                }
 				if (!tok.ConsumeKeyword("do"))
 					error("'do' expected");
 				
 				Chunk body = ParseStatementList(forL);
 				
 				if (!tok.ConsumeKeyword("end"))
 					error("'end' expected");
 				
 				forL.VariableList = varList;
 				forL.Generators = generators;
 				forL.Body = body;
 				stat = forL;
            }
        }
 		else if (tok.ConsumeKeyword("repeat"))
        {
 			Chunk body = ParseStatementList(scope);
 			
 			if (!tok.ConsumeKeyword("until"))
 				error("'until' expected");
 			
 			Expression cond = ParseExpr(scope);
 			
            RepeatStatement r = new RepeatStatement();
 			r.Condition = cond;
 			r.Body = body;
 			stat = r;
        }
 		else if (tok.ConsumeKeyword("function"))
        {
 			if (!tok.Is(TokenType.Ident))
 				error("function name expected");
 			
 			Expression name = ParseSuffixedExpr(scope, true) // true => only dots and colons
 			
 			FunctionStatement func = ParseFunctionArgsAndBody(scope);
 			
 			func.IsLocal = false;
 			func.Name = name;
 			stat = func;
        }
 		else if (tok.ConsumeKeyword("local"))
         {
 			if (tok.Is(TokenType.Ident))
            {
                List<string> varList = new List<string>{tok:Get().Data};
 				while (tok.ConsumeSymbol(','))
                {
 					if (!tok.Is(TokenType.Ident))
 						error("local variable name expected");
 					varList.Add(tok.Get().Data);
 				}
 
 				List<Expression> initList = new List<Expression>();
 				if (tok.ConsumeSymbol('='))
                {
 					do
                    {
 						Expression ex = ParseExpr(scope);
 						initList.Add(ex);
                    } while (tok.ConsumeSymbol(','));
                }
 
 				//now patch var list
 				//we can't do this before getting the init list, because the init list does not
 				//have the locals themselves in scope.
                List<Variable> newVarList = new List<Variable>();
 				for (int i = 0; i < varList.Count; i++)
 					newVarList.Add(scope.CreateLocal(varList[i]));

                LocalVariableStatement l = new LocalVariableStatement();
 				f.LocalList = newVarList;
 				f.InitList = initList;
 				stat = nodeLocal;
            }
 			else if (tok.ConsumeKeyword("function"))
            {
 				if (!tok.Is(TokenType.Ident))
                    error("Function name expected");
 				string name = tok.Get().Data;
 				Variable localVar = scope.CreateLocal(name);
 				
 				FunctionStatement func = ParseFunctionArgsAndBody(scope);
 				
 				func.Name = localVar;
 				func.IsLocal = true;
 				stat = func;
            }
 			else
 				error("local variable or function definition expected");
        }
 		else if (tok.ConsumeSymbol("::"))
         {
 			if (!tok.Is(TokenType.Ident))
 				error("label name expected");
 			
 			string label = tok.Get().Data;
 			if (!tok.ConsumeSymbol("::"))
 				error("'::' expected");

 			LabelStatement l = new LabelStatement();
 			l.Label = label;
 			stat = l;
        }
 		else if (tok.ConsumeKeyword("return"))
        {
 			List<Expression> exprList = new List<Expression>();
 			if (!tok.IsKeyword("end"))
            {
 				Expression firstEx = ParseExpr(scope);
 		        exprList.Add(firstEx);
 				while (tok.ConsumeSymbol(','))
                {
 					Expression ex = ParseExpr(scope);
 					exprList.Add(ex);
                }
            }
            ReturnStatement r = new ReturnStatement();
 			r.Arguments = exprList;
 			stat = r;
        }
 		else if (tok.ConsumeKeyword("break"))
        {
 			stat = new BreakStatement();
        }
 		else if (tok.ConsumeKeyword("goto"))
        {
 			if (!tok.Is(TokenType.Ident))
 				error("label expected");
 			
 			string label = tok.Get().Data;
 			GotoStatement g = new GotoStatement();
 			g.Label = label;
 			stat = g;
        }
        else if (tok.ConsumeKeyword("using"))
        {
            // using <a = 1, b = x()> do <statements> end
            UsingStatement us = new UsingStatement();
            List<AssignmentStatement> exprList = new List<Assignment>();
            AssignmentStatement a = new AssignmentStatement();
            do
            {
                AssignmentStatement a = new AssignmentStatement();
                if (!tok.Is(TokenType.Ident))
                    error("<Ident> expected");
                a.Lhs = us.CreateLocal(tok.Get().Data);
                if (!tok.ConsumeSymbol("="))
                    error("'=' expected");
                a.Rhs = ParseExpr();
                exprList.Add(a);
            } while (tok.ConsumeSymbol(','));

            if (!tok.ConsumeKeyword("do"))
                error("'do' expected");

            Chunk block = ParseStatementList(us);

            if (!tok.ConsumeKeyword("end"))
                error("'end' expected");

            stat.Vars = exprList;
            stat.Block = block;
            stat = us;
        }
        else if (tok.ConsumeKeyword("match"))
        {
            //match <expr list> with  
            //    | <expr> -> <statement list> 
            //end

            // match <expr list> with
            MatchWithStatement m = new MatchWithStatement();
            do 
            {
                m.Exprs.Add(ParseExpr());
            } while (tok.ConsumeSymbol(','));


            // | <expr>, <expr> | <expr> ->
            //     <statement list>
            // | <expr> -> <statement list>

            do 
            {
                List<Expression> exprs = new List<Expression>();
                Chunk body = null;
                while (tok.ConsumeSymbol('|'))
                {
                    exprs.Add(ParseExpr());
                    while (tok.ConsumeSymbol(','))
                        exprs.Add(ParseExpr());
                }
                if (tok.ConsumeSymbol("->"))
                {
                    body = ParseStatementList(m);
                }
                else
                    error("'->' expected");
                m.Clauses.Add(new MatchClauseStatement() { exprs = exprs, Block = body });
            } while (!tok.ConsumeKeyword("end"));
            // end has been eaten
        }
 		else
        {
 			// statementParseExpr
 			Expression suffixed = ParseSuffixedExpr(scope);
 			
 			// assignment or call?
 			if (tok.IsSymbol(',') || tok.IsSymbol('='))
            {
 			    // check that it was not parenthesized, making it not an lvalue
 				if (suffixed.ParenCount > 0)
 					error("Can not assign to parenthesized expression, it is not an lvalue");
 
 				// more processing needed
 				List<Expression> lhs = new List<Expression>{ suffixed };
 				while (tok.ConsumeSymbol(','))
                {
 					lhs.Add(ParseSuffixedExpr(scope));
                }
 
 				// equals
 				if (!tok.ConsumeSymbol('='))
 					error("'=' expected");
 
 				//rhs
 				List<Expression> rhs = new List<Expression>();
 				rhs.Add(ParseExpr());
 				while (tok.ConsumeSymbol(','))
                {
 					rhs.Add(ParseExpr(scope));
                }
 
                AssignmentStatement a = new AssignmentStatement();
 				a.Lhs = lhs;
 				a.Rhs = rhs;
 				stat = a;
            }
 			else if (suffixed is CallExpr ||
 			       suffixed is TableCallExpr ||
 			       suffixed is StringCallExpr)
            {
 				//it's a call statement
                CallStatement c = new CallStatement();
 				c.Expression = suffixed;
 				stat = c;
            }
 			else
 				error("assignment statement expected");
        }
 
 		stat.HasSemicolon = tok.ConsumeSymbol(';');
 		return stat;
    }

        bool isClosing(string s)
        {
            foreach (string w in new string[] { "end", "else", "elseif", "until", "|" })
                if (w == s)
                    return true;
            return false;
        }


        Chunk ParseStatementList(Chunk scope)
        {
            Chunk c = new Chunk(scope);

            while (!isClosing(tok.Peek().Data) && !tok.IsEof())
            {
                Statement nodeStatement = ParseStatement(nodeStatlist.Scope);
                //stats[#stats+1] = nodeStatement
                c.Body.Add(nodeStatement);
            }
            return c;
        }


        public Chunk Parse()
        {
            Chunk c = new Chunk();
            return ParseStatementList(c);
        }
    }
}
