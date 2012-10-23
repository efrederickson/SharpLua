using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;
using SharpLua.Ast;

namespace SharpLua
{
    public class Parser
    {
        public List<LuaSourceException> Errors = new List<LuaSourceException>();
        public bool ThrowParsingErrors = true;

        TokenReader reader;

        public Parser(TokenReader tr)
        {
            reader = tr;
        }

        void error(string msg)
        {
            LuaSourceException ex = new LuaSourceException(reader.Peek().Line, reader.Peek().Column, msg + ", got '" + reader.Peek().Data + "'");
            Errors.Add(ex);
            if (ThrowParsingErrors)
                throw ex;
        }

        AnonymousFunctionExpr ParseExprFunctionArgsAndBody(Scope scope)
        {
            AnonymousFunctionExpr func = new AnonymousFunctionExpr();
            func.Scope = new Scope(scope);

            if (reader.ConsumeSymbol('(') == false)
                error("'(' expected");

            // arg list
            List<Variable> arglist = new List<Variable>();
            bool isVarArg = false;
            while (reader.ConsumeSymbol(')') == false)
            {
                if (reader.Is(TokenType.Ident))
                {
                    Variable arg = new Variable();
                    arg.Name = reader.Get().Data;
                    func.Scope.AddLocal(arg);
                    arglist.Add(arg);
                    if (!reader.ConsumeSymbol(','))
                        if (reader.ConsumeSymbol(')'))
                            break;
                        else
                            error("')' expected");
                }
                else if (reader.ConsumeSymbol("..."))
                {
                    isVarArg = true;
                    if (!reader.ConsumeSymbol(')'))
                        error("'...' must be the last argument of a function");
                    break;
                }
                else
                    error("Argument name or '...' expected");
            }

            // body
            List<Statement> body = ParseStatementList(func.Scope);
            // end
            if (!reader.ConsumeKeyword("end"))
                error("'end' expected after function body");

            //nodeFunc.AstType = AstType.Function;
            func.Arguments = arglist;
            func.Body = body;
            func.IsVararg = isVarArg;

            return func;
        }

        FunctionStatement ParseFunctionArgsAndBody(Scope scope)
        {
            FunctionStatement func = new FunctionStatement(scope);

            if (reader.ConsumeSymbol('(') == false)
                error("'(' expected");

            // arg list
            List<Variable> arglist = new List<Variable>();
            bool isVarArg = false;
            while (reader.ConsumeSymbol(')') == false)
            {
                if (reader.Is(TokenType.Ident))
                {
                    Variable arg = new Variable();
                    arg.Name = reader.Get().Data;
                    func.Scope.AddLocal(arg);
                    arglist.Add(arg);
                    if (!reader.ConsumeSymbol(','))
                        if (reader.ConsumeSymbol(')'))
                            break;
                        else
                            error("')' expected");
                }
                else if (reader.ConsumeSymbol("..."))
                {
                    isVarArg = true;
                    if (!reader.ConsumeSymbol(')'))
                        error("'...' must be the last argument of a function");
                    break;
                }
                else
                    error("Argument name or '...' expected");
            }

            // body
            List<Statement> body = ParseStatementList(func.Scope);
            // end
            if (!reader.ConsumeKeyword("end"))
                error("'end' expected after function body");

            //nodeFunc.AstType = AstType.Function;
            func.Arguments = arglist;
            func.Body = body;
            func.IsVararg = isVarArg;

            return func;
        }

        Expression ParsePrimaryExpr(Scope c)
        {
            //Console.WriteLine(tok.Peek().Type + " " + tok.Peek().Data);
            if (reader.ConsumeSymbol('('))
            {
                Expression ex = ParseExpr(c);
                if (!reader.ConsumeSymbol(')'))
                    error("')' expected");

                // save the information about parenthesized expressions somewhere
                ex.ParenCount = ex.ParenCount + 1;
                return ex;
            }
            else if (reader.Is(TokenType.Ident))
            {
                Token id = reader.Get();
                VariableExpression v = new VariableExpression();
                Variable var = c.GetLocal(id.Data);
                if (var == null)
                {
                    var = c.GetGlobal(id.Data);
                    if (var == null)
                    {
                        v.Var = c.CreateGlobal(id.Data);
                    }
                    else
                    {
                        v.Var = var;
                        v.Var.References++;
                    }
                }
                else
                {
                    v.Var = var;
                    v.Var.References++;
                }
                return v;
            }
            else
                error("primary expression expected");

            return null; // satisfy the C# compiler, but this will never happen
        }

        Expression ParseSuffixedExpr(Scope scope, bool onlyDotColon = false)
        {
            // base primary expression
            Expression prim = ParsePrimaryExpr(scope);

            while (true)
            {
                if (reader.IsSymbol('.') || reader.IsSymbol(':'))
                {
                    string symb = reader.Get().Data; // '.' or ':'
                    // TODO: should we allow keywords?
                    if (!reader.Is(TokenType.Ident))
                        error("<Ident> expected");

                    Token id = reader.Get();
                    MemberExpr m = new MemberExpr();
                    m.Base = prim;
                    m.Indexer = symb;
                    m.Ident = id.Data;

                    prim = m;
                }
                else if (!onlyDotColon && reader.ConsumeSymbol('['))
                {
                    Expression ex = ParseExpr(scope);

                    if (!reader.ConsumeSymbol(']'))
                        error("']' expected");

                    IndexExpr i = new IndexExpr();
                    i.Base = prim;
                    i.Index = ex;

                    prim = i;
                }
                else if (!onlyDotColon && reader.ConsumeSymbol('('))
                {
                    List<Expression> args = new List<Expression>();
                    while (!reader.ConsumeSymbol(')'))
                    {
                        Expression ex = ParseExpr(scope);

                        args.Add(ex);
                        if (!reader.ConsumeSymbol(','))
                            if (reader.ConsumeSymbol(')'))
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
                        (reader.Is(TokenType.SingleQuoteString) ||
                        reader.Is(TokenType.DoubleQuoteString) ||
                        reader.Is(TokenType.LongString)))
                {
                    //string call

                    StringCallExpr e = new StringCallExpr();
                    e.Base = prim;
                    e.Arguments = new List<Expression> { new StringExpr(reader.Peek().Data) { StringType = reader.Peek().Type } };
                    reader.Get();
                    prim = e;
                }
                else if (!onlyDotColon && reader.IsSymbol('{'))
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

        Expression ParseSimpleExpr(Scope scope)
        {
            if (reader.Is(TokenType.Number))
                return new NumberExpr { Value = reader.Get().Data };
            else if (reader.Is(TokenType.DoubleQuoteString) || reader.Is(TokenType.SingleQuoteString) || reader.Is(TokenType.LongString))
            {
                StringExpr s = new StringExpr
                {
                    Value = reader.Peek().Data,
                    StringType = reader.Peek().Type
                };
                reader.Get();
                return s;
            }
            else if (reader.ConsumeKeyword("nil"))
                return new NilExpr();
            else if (reader.IsKeyword("false") || reader.IsKeyword("true"))
                return new BoolExpr { Value = reader.Get().Data == "true" };
            else if (reader.ConsumeSymbol("..."))
                return new VarargExpr();
            else if (reader.ConsumeSymbol('{'))
            {
                TableConstructorExpr v = new TableConstructorExpr();
                while (true)
                {
                    if (reader.IsSymbol('['))
                    {
                        // key
                        reader.Get(); // eat '['
                        Expression key = ParseExpr(scope);

                        if (!reader.ConsumeSymbol(']'))
                            error("']' expected");

                        if (!reader.ConsumeSymbol('='))
                            error("'=' Expected");

                        Expression value = ParseExpr(scope);

                        v.EntryList.Add(new TableConstructorKeyExpr
                        {
                            Key = key,
                            Value = value,
                        });
                    }
                    else if (reader.Is(TokenType.Ident))
                    {
                        // value or key
                        Token lookahead = reader.Peek(1);
                        if (lookahead.Type == TokenType.Symbol && lookahead.Data == "=")
                        {
                            // we are a key
                            Token key = reader.Get();
                            if (!reader.ConsumeSymbol('='))
                                error("'=' Expected");

                            Expression value = ParseExpr(scope);

                            v.EntryList.Add(new TableConstructorStringKeyExpr
                            {
                                Key = key.Data,
                                Value = value,
                            });
                        }
                        else
                        {
                            // we are a value
                            Expression val = ParseExpr(scope);

                            v.EntryList.Add(new TableConstructorValueExpr
                            {
                                Value = val
                            });

                        }
                    }
                    else if (reader.ConsumeSymbol('}'))
                        break;
                    else
                    {
                        //value
                        Expression value = ParseExpr(scope);
                        v.EntryList.Add(new TableConstructorValueExpr
                        {
                            Value = value
                        });
                    }

                    if (reader.ConsumeSymbol(';') || reader.ConsumeSymbol(','))
                    {
                        // I could have used just an empty ';' here, 
                        // but that leaves a warning, which clutters up the output
                        // other than that, all is good
                    }
                    else if (reader.ConsumeSymbol('}'))
                        break;
                    else
                        error("'}' or table entry Expected");
                }
                return v;
            }
            else if (reader.ConsumeKeyword("function"))
            {
                AnonymousFunctionExpr func = ParseExprFunctionArgsAndBody(scope);
                //func.IsLocal = true;
                return func;
            }
            else if (reader.ConsumeSymbol('|'))
            {
                // inline function... |<arg list>| -> <expr>, <expr>
                InlineFunctionExpression func = new InlineFunctionExpression();
                func.Scope = new Scope(scope);
                // arg list
                List<Variable> arglist = new List<Variable>();
                bool isVarArg = false;
                while (reader.ConsumeSymbol('|') == false)
                {
                    if (reader.Is(TokenType.Ident))
                    {
                        Variable arg = new Variable();
                        arg.Name = reader.Get().Data;
                        func.Scope.AddLocal(arg);
                        arglist.Add(arg);
                        if (!reader.ConsumeSymbol(','))
                            if (reader.ConsumeSymbol('|'))
                                break;
                            else
                                error("'|' expected");
                    }
                    else if (reader.ConsumeSymbol("..."))
                    {
                        isVarArg = true;
                        if (!reader.ConsumeSymbol('|'))
                            error("'...' must be the last argument of a function");
                        break;
                    }
                    else
                        error("Argument name or '...' expected");
                }
                if (!reader.ConsumeSymbol("->"))
                    error("'->' expected");
                // body
                List<Expression> body = new List<Expression> { ParseExpr(func.Scope) };
                while (reader.ConsumeSymbol(','))
                    body.Add(ParseExpr(func.Scope));
                // end

                //nodeFunc.AstType = AstType.Function;
                func.Arguments = arglist;
                func.Expressions = body;
                func.IsVararg = isVarArg;

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

        Expression ParseSubExpr(Scope scope, int level)
        {
            // base item, possibly with unop prefix
            Expression exp = null;
            if (isUnOp(reader.Peek().Data) &&
                (reader.Peek().Type == TokenType.Symbol || reader.Peek().Type == TokenType.Keyword))
            {
                string op = reader.Get().Data;
                exp = ParseSubExpr(scope, unopprio);
                exp = new UnOpExpr { Rhs = exp, Op = op };
            }
            else
                exp = ParseSimpleExpr(scope);

            if (exp is InlineFunctionExpression)
                return exp; // inline functions cannot have any extra parts

            // next items in chain
            while (true)
            {
                priority_ prio = getpriority(reader.Peek().Data);
                if (prio != null && prio.l > level)
                {
                    string op = reader.Get().Data;
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

        Expression ParseExpr(Scope scope)
        {
            return ParseSubExpr(scope, 0);
        }

        Statement ParseStatement(Scope scope)
        {
            int startP = reader.p;
            Statement stat = null;
            // print(tok.Peek().Print())
            if (reader.ConsumeKeyword("if"))
            {
                //setup
                IfStmt _if = new IfStmt();

                //clauses
                do
                {
                    int sP = reader.p;
                    Expression nodeCond = ParseExpr(scope);

                    if (!reader.ConsumeKeyword("then"))
                        error("'then' expected");

                    List<Statement> nodeBody = ParseStatementList(scope);

                    List<Token> range = new List<Token>();
                    range.Add(reader.tokens[sP - 1]);
                    range.AddRange(reader.Range(sP, reader.p));

                    _if.Clauses.Add(new ElseIfStmt(scope)
                    {
                        Condition = nodeCond,
                        Body = nodeBody,
                        ScannedTokens = range
                    });
                }
                while (reader.ConsumeKeyword("elseif"));

                // else clause
                if (reader.ConsumeKeyword("else"))
                {
                    int sP = reader.p;
                    List<Statement> nodeBody = ParseStatementList(scope);
                    List<Token> range = new List<Token>();
                    range.Add(reader.tokens[sP - 1]);
                    range.AddRange(reader.Range(sP, reader.p));

                    _if.Clauses.Add(new ElseStmt(scope)
                    {
                        Body = nodeBody,
                        ScannedTokens = range
                    });
                }

                // end
                if (!reader.ConsumeKeyword("end"))
                    error("'end' expected");

                stat = _if;
            }
            else if (reader.ConsumeKeyword("while"))
            {
                WhileStatement w = new WhileStatement(scope);

                // condition
                Expression nodeCond = ParseExpr(scope);

                // do
                if (!reader.ConsumeKeyword("do"))
                    error("'do' expected");

                // body
                List<Statement> body = ParseStatementList(scope);

                //end
                if (!reader.ConsumeKeyword("end"))
                    error("'end' expected");


                // return
                w.Condition = nodeCond;
                w.Body = body;
                stat = w;
            }
            else if (reader.ConsumeKeyword("do"))
            {
                // do block
                List<Statement> b = ParseStatementList(scope);

                if (!reader.ConsumeKeyword("end"))
                    error("'end' expected");

                stat = new DoStatement(scope) { Body = b };
            }
            else if (reader.ConsumeKeyword("for"))
            {
                //for block
                if (!reader.Is(TokenType.Ident))
                    error("<ident> expected");

                Token baseVarName = reader.Get();
                if (reader.ConsumeSymbol('='))
                {
                    //numeric for
                    NumericForStatement forL = new NumericForStatement(scope);
                    Variable forVar = new Variable() { Name = baseVarName.Data };
                    forL.Scope.AddLocal(forVar);

                    Expression startEx = ParseExpr(scope);

                    if (!reader.ConsumeSymbol(','))
                        error("',' expected");

                    Expression endEx = ParseExpr(scope);

                    Expression stepEx = null;
                    if (reader.ConsumeSymbol(','))
                    {
                        stepEx = ParseExpr(scope);
                    }
                    if (!reader.ConsumeKeyword("do"))
                        error("'do' expected");


                    List<Statement> body = ParseStatementList(forL.Scope);

                    if (!reader.ConsumeKeyword("end"))
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
                    GenericForStatement forL = new GenericForStatement(scope);

                    List<Variable> varList = new List<Variable> { forL.Scope.CreateLocal(baseVarName.Data) };
                    while (reader.ConsumeSymbol(','))
                    {
                        if (!reader.Is(TokenType.Ident))
                            error("for variable expected");

                        varList.Add(forL.Scope.CreateLocal(reader.Get().Data));
                    }
                    if (!reader.ConsumeKeyword("in"))
                        error("'in' expected");

                    List<Expression> generators = new List<Expression>();
                    Expression first = ParseExpr(scope);

                    generators.Add(first);
                    while (reader.ConsumeSymbol(','))
                    {
                        Expression gen = ParseExpr(scope);
                        generators.Add(gen);
                    }
                    if (!reader.ConsumeKeyword("do"))
                        error("'do' expected");

                    List<Statement> body = ParseStatementList(forL.Scope);

                    if (!reader.ConsumeKeyword("end"))
                        error("'end' expected");

                    forL.VariableList = varList;
                    forL.Generators = generators;
                    forL.Body = body;
                    stat = forL;
                }
            }
            else if (reader.ConsumeKeyword("repeat"))
            {
                List<Statement> body = ParseStatementList(scope);

                if (!reader.ConsumeKeyword("until"))
                    error("'until' expected");

                Expression cond = ParseExpr(scope);

                RepeatStatement r = new RepeatStatement(scope);
                r.Condition = cond;
                r.Body = body;
                stat = r;
            }
            else if (reader.ConsumeKeyword("function"))
            {
                if (!reader.Is(TokenType.Ident))
                    error("function name expected");

                Expression name = ParseSuffixedExpr(scope, true);
                // true => only dots and colons

                FunctionStatement func = ParseFunctionArgsAndBody(scope);

                func.IsLocal = false;
                func.Name = name;
                stat = func;
            }
            else if (reader.ConsumeKeyword("local"))
            {
                if (reader.Is(TokenType.Ident))
                {
                    List<string> varList = new List<string> { reader.Get().Data };
                    while (reader.ConsumeSymbol(','))
                    {
                        if (!reader.Is(TokenType.Ident))
                            error("local variable name expected");
                        varList.Add(reader.Get().Data);
                    }

                    List<Expression> initList = new List<Expression>();
                    if (reader.ConsumeSymbol('='))
                    {
                        do
                        {
                            Expression ex = ParseExpr(scope);
                            initList.Add(ex);
                        } while (reader.ConsumeSymbol(','));
                    }

                    //now patch var list
                    //we can't do this before getting the init list, because the init list does not
                    //have the locals themselves in scope.
                    List<Expression> newVarList = new List<Expression>();
                    for (int i = 0; i < varList.Count; i++)
                    {
                        Variable x = scope.CreateLocal(varList[i]);
                        newVarList.Add(new VariableExpression { Var = x });
                    }

                    AssignmentStatement l = new AssignmentStatement();
                    l.Lhs = newVarList;
                    l.Rhs = initList;
                    l.IsLocal = true;
                    stat = l;
                }
                else if (reader.ConsumeKeyword("function"))
                {
                    if (!reader.Is(TokenType.Ident))
                        error("Function name expected");
                    string name = reader.Get().Data;
                    Variable localVar = scope.CreateLocal(name);

                    FunctionStatement func = ParseFunctionArgsAndBody(scope);

                    func.Name = new VariableExpression { Var = localVar };
                    func.IsLocal = true;
                    stat = func;
                }
                else
                    error("local variable or function definition expected");
            }
            else if (reader.ConsumeSymbol("::"))
            {
                if (!reader.Is(TokenType.Ident))
                    error("label name expected");

                string label = reader.Get().Data;
                if (!reader.ConsumeSymbol("::"))
                    error("'::' expected");

                LabelStatement l = new LabelStatement();
                l.Label = label;
                stat = l;
            }
            else if (reader.ConsumeKeyword("return"))
            {
                List<Expression> exprList = new List<Expression>();
                if (!reader.IsKeyword("end"))
                {
                    Expression firstEx = ParseExpr(scope);
                    exprList.Add(firstEx);
                    while (reader.ConsumeSymbol(','))
                    {
                        Expression ex = ParseExpr(scope);
                        exprList.Add(ex);
                    }
                }
                ReturnStatement r = new ReturnStatement();
                r.Arguments = exprList;
                stat = r;
            }
            else if (reader.ConsumeKeyword("break"))
            {
                stat = new BreakStatement();
            }
            else if (reader.ConsumeKeyword("goto"))
            {
                if (!reader.Is(TokenType.Ident))
                    error("label expected");

                string label = reader.Get().Data;
                GotoStatement g = new GotoStatement();
                g.Label = label;
                stat = g;
            }
            else if (reader.ConsumeKeyword("using"))
            {
                // using <a, b = 1, x()> do <statements> end
                UsingStatement us = new UsingStatement(scope);
                us.Scope = new Scope(scope);

                List<Expression> lhs = new List<Expression> { ParseExpr(us.Scope) };
                while (reader.ConsumeSymbol(','))
                {
                    lhs.Add(ParseSuffixedExpr(us.Scope));
                }

                // equals
                if (!reader.ConsumeSymbol('='))
                    error("'=' expected");

                //rhs
                List<Expression> rhs = new List<Expression>();
                rhs.Add(ParseExpr(us.Scope));
                while (reader.ConsumeSymbol(','))
                {
                    rhs.Add(ParseExpr(scope));
                }

                AssignmentStatement a = new AssignmentStatement();
                a.Lhs = lhs;
                a.Rhs = rhs;

                if (!reader.ConsumeKeyword("do"))
                    error("'do' expected");

                List<Statement> block = ParseStatementList(us.Scope);

                if (!reader.ConsumeKeyword("end"))
                    error("'end' expected");

                us.Vars = a;
                us.Body = block;
                stat = us;
            }
            else
            {
                // statementParseExpr
                Expression suffixed = ParseSuffixedExpr(scope);
                // assignment or call?
                if (reader.IsSymbol(',') || reader.IsSymbol('='))
                {
                    // check that it was not parenthesized, making it not an lvalue
                    if (suffixed.ParenCount > 0)
                        error("Can not assign to parenthesized expression, it is not an lvalue");

                    // more processing needed
                    List<Expression> lhs = new List<Expression> { suffixed };
                    while (reader.ConsumeSymbol(','))
                    {
                        lhs.Add(ParseSuffixedExpr(scope));
                    }

                    // equals
                    if (!reader.ConsumeSymbol('='))
                        error("'=' expected");

                    //rhs
                    List<Expression> rhs = new List<Expression>();
                    rhs.Add(ParseExpr(scope));
                    while (reader.ConsumeSymbol(','))
                    {
                        rhs.Add(ParseExpr(scope));
                    }

                    AssignmentStatement a = new AssignmentStatement();
                    a.Lhs = lhs;
                    a.Rhs = rhs;
                    stat = a;
                }
                else if (isAugmentedAssignment(reader.Peek()))
                {
                    AugmentedAssignmentStatement aas = new AugmentedAssignmentStatement();
                    Expression left = suffixed;
                    Expression right = null;
                    string augmentedOp = reader.Get().Data;
                    right = ParseExpr(scope);
                    BinOpExpr nRight = new BinOpExpr();
                    nRight.Lhs = left;
                    nRight.Op = augmentedOp.Substring(0, augmentedOp.Length - 1); // strip the '='
                    nRight.Rhs = right;

                    aas.Lhs = new List<Expression> { left };
                    aas.Rhs = new List<Expression> { nRight };
                    stat = aas;
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

            stat.ScannedTokens = reader.Range(startP, reader.p);
            if (reader.Peek().Data == ";" && reader.Peek().Type == TokenType.Symbol)
            {
                stat.HasSemicolon = true;
                stat.SemicolonToken = reader.Get();
            }
            if (stat.Scope == null)
                stat.Scope = scope;
            return stat;
        }

        bool isAugmentedAssignment(Token token)
        {
            switch (token.Data)
            {
                case "..=":
                case ">>=":
                case "<<=":
                case "+=":
                case "-=":
                case "/=":
                case "*=":
                case "&=":
                case "|=":
                case "^=":
                case "%=":
                    return true;

                default:
                    return false;
            }
        }

        bool isClosing(string s)
        {
            foreach (string w in new string[] { "end", "else", "elseif", "until", "|" })
                if (w == s)
                    return true;
            return false;
        }


        List<Statement> ParseStatementList(Scope scope)
        {
            List<Statement> c = new List<Statement>();

            while (!isClosing(reader.Peek().Data) && !reader.IsEof())
            {
                Statement nodeStatement = ParseStatement(scope);
                //stats[#stats+1] = nodeStatement
                c.Add(nodeStatement);
            }
            return c;
        }


        public Chunk Parse()
        {
            Scope s = new Scope();
            return new Chunk
            {
                Body = ParseStatementList(s),
                Scope = s,
                ScannedTokens = reader.tokens
            };
        }
    }
}
