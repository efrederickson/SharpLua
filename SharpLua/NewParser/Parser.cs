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

        TokenReader tok;

        public Parser(TokenReader tr)
        {
            tok = tr;
        }

        void error(string msg)
        {
            LuaSourceException ex = new LuaSourceException(tok.Peek().Line, tok.Peek().Column, msg + ", got '" + tok.Peek().Data + "'");
            Errors.Add(ex);
            if (ThrowParsingErrors)
                throw ex;
        }

        AnonymousFunctionExpr ParseExprFunctionArgsAndBody(Scope scope)
        {
            AnonymousFunctionExpr func = new AnonymousFunctionExpr();
            func.Scope = new Scope(scope);

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
                    func.Scope.AddLocal(arg);
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
            List<Statement> body = ParseStatementList(func.Scope);
            // end
            if (!tok.ConsumeKeyword("end"))
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
                    func.Scope.AddLocal(arg);
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
            List<Statement> body = ParseStatementList(func.Scope);
            // end
            if (!tok.ConsumeKeyword("end"))
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
                {
                    v.IsGlobal = true;
                    v.Var = new Variable { Name = id.Data };
                }
                else
                    v.Var = var;

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
                if (tok.IsSymbol('.') || tok.IsSymbol(':'))
                {
                    string symb = tok.Get().Data; // '.' or ':'
                    // TODO: should we allow keywords?
                    if (!tok.Is(TokenType.Ident))
                        error("<Ident> expected");

                    Token id = tok.Get();
                    MemberExpr m = new MemberExpr();
                    m.Base = prim;
                    m.Indexer = symb;
                    m.Ident = id.Data;

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
                    e.Arguments = new List<Expression> { new StringExpr(tok.Peek().Data) { StringType = tok.Peek().Type } };
                    tok.Get();
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

        Expression ParseSimpleExpr(Scope scope)
        {
            if (tok.Is(TokenType.Number))
                return new NumberExpr { Value = tok.Get().Data };
            else if (tok.Is(TokenType.DoubleQuoteString) || tok.Is(TokenType.SingleQuoteString) || tok.Is(TokenType.LongString))
            {
                StringExpr s = new StringExpr
                {
                    Value = tok.Peek().Data,
                    StringType = tok.Peek().Type
                };
                tok.Get();
                return s;
            }
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

                        v.EntryList.Add(new TableConstructorKeyExpr
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
                    else if (tok.ConsumeSymbol('}'))
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

                    if (tok.ConsumeSymbol(';') || tok.ConsumeSymbol(','))
                    {
                        // I could have used just an empty ';' here, 
                        // but that leaves a warning, which clutters up the output
                        // other than that, all is good
                    }
                    else if (tok.ConsumeSymbol('}'))
                        break;
                    else
                        error("'}' or table entry Expected");
                }
                return v;
            }
            else if (tok.ConsumeKeyword("function"))
            {
                AnonymousFunctionExpr func = ParseExprFunctionArgsAndBody(scope);
                //func.IsLocal = true;
                return func;
            }
            else if (tok.ConsumeSymbol('|'))
            {
                // inline function... |<arg list>| -> <expr>, <expr>
                InlineFunctionExpression func = new InlineFunctionExpression();
                func.Scope = new Scope(scope);
                // arg list
                List<Variable> arglist = new List<Variable>();
                bool isVarArg = false;
                while (tok.ConsumeSymbol('|') == false)
                {
                    if (tok.Is(TokenType.Ident))
                    {
                        Variable arg = new Variable();
                        arg.Name = tok.Get().Data;
                        func.Scope.AddLocal(arg);
                        arglist.Add(arg);
                        if (!tok.ConsumeSymbol(','))
                            if (tok.ConsumeSymbol('|'))
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
                List<Expression> body = new List<Expression> { ParseExpr(func.Scope) };
                while (tok.ConsumeSymbol(','))
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
            if (isUnOp(tok.Peek().Data) &&
                (tok.Peek().Type == TokenType.Symbol || tok.Peek().Type == TokenType.Keyword))
            {
                string op = tok.Get().Data;
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

        Expression ParseExpr(Scope scope)
        {
            return ParseSubExpr(scope, 0);
        }

        Statement ParseStatement(Scope scope)
        {
            int startP = tok.p;
            Statement stat = null;
            // print(tok.Peek().Print())
            if (tok.ConsumeKeyword("if"))
            {
                //setup
                IfStmt _if = new IfStmt();

                //clauses
                do
                {
                    int sP = tok.p;
                    Expression nodeCond = ParseExpr(scope);

                    if (!tok.ConsumeKeyword("then"))
                        error("'then' expected");

                    List<Statement> nodeBody = ParseStatementList(scope);

                    List<Token> range = new List<Token>();
                    range.Add(tok.tokens[sP - 1]);
                    range.AddRange(tok.Range(sP, tok.p));

                    _if.Clauses.Add(new ElseIfStmt(scope)
                    {
                        Condition = nodeCond,
                        Body = nodeBody,
                        ScannedTokens = range
                    });
                }
                while (tok.ConsumeKeyword("elseif"));

                // else clause
                if (tok.ConsumeKeyword("else"))
                {
                    int sP = tok.p;
                    List<Statement> nodeBody = ParseStatementList(scope);
                    List<Token> range = new List<Token>();
                    range.Add(tok.tokens[sP - 1]);
                    range.AddRange(tok.Range(sP, tok.p));

                    _if.Clauses.Add(new ElseStmt(scope)
                    {
                        Body = nodeBody,
                        ScannedTokens = range
                    });
                }

                // end
                if (!tok.ConsumeKeyword("end"))
                    error("'end' expected");

                stat = _if;
            }
            else if (tok.ConsumeKeyword("while"))
            {
                WhileStatement w = new WhileStatement(scope);

                // condition
                Expression nodeCond = ParseExpr(scope);

                // do
                if (!tok.ConsumeKeyword("do"))
                    error("'do' expected");

                // body
                List<Statement> body = ParseStatementList(scope);

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
                List<Statement> b = ParseStatementList(scope);

                if (!tok.ConsumeKeyword("end"))
                    error("'end' expected");

                stat = new DoStatement(scope) { Body = b };
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
                    NumericForStatement forL = new NumericForStatement(scope);
                    Variable forVar = new Variable() { Name = baseVarName.Data };
                    forL.Scope.AddLocal(forVar);

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


                    List<Statement> body = ParseStatementList(forL.Scope);

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
                    GenericForStatement forL = new GenericForStatement(scope);

                    List<Variable> varList = new List<Variable> { forL.Scope.CreateLocal(baseVarName.Data) };
                    while (tok.ConsumeSymbol(','))
                    {
                        if (!tok.Is(TokenType.Ident))
                            error("for variable expected");

                        varList.Add(forL.Scope.CreateLocal(tok.Get().Data));
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

                    List<Statement> body = ParseStatementList(forL.Scope);

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
                List<Statement> body = ParseStatementList(scope);

                if (!tok.ConsumeKeyword("until"))
                    error("'until' expected");

                Expression cond = ParseExpr(scope);

                RepeatStatement r = new RepeatStatement(scope);
                r.Condition = cond;
                r.Body = body;
                stat = r;
            }
            else if (tok.ConsumeKeyword("function"))
            {
                if (!tok.Is(TokenType.Ident))
                    error("function name expected");

                Expression name = ParseSuffixedExpr(scope, true);
                // true => only dots and colons

                FunctionStatement func = ParseFunctionArgsAndBody(scope);

                func.IsLocal = false;
                func.Name = name;
                stat = func;
            }
            else if (tok.ConsumeKeyword("local"))
            {
                if (tok.Is(TokenType.Ident))
                {
                    List<string> varList = new List<string> { tok.Get().Data };
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
                else if (tok.ConsumeKeyword("function"))
                {
                    if (!tok.Is(TokenType.Ident))
                        error("Function name expected");
                    string name = tok.Get().Data;
                    Variable localVar = scope.CreateLocal(name);

                    FunctionStatement func = ParseFunctionArgsAndBody(scope);

                    func.Name = new VariableExpression { Var = localVar };
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
                // using <a, b = 1, x()> do <statements> end
                UsingStatement us = new UsingStatement(scope);
                us.Scope = new Scope(scope);

                List<Expression> lhs = new List<Expression> { ParseExpr(us.Scope) };
                while (tok.ConsumeSymbol(','))
                {
                    lhs.Add(ParseSuffixedExpr(us.Scope));
                }

                // equals
                if (!tok.ConsumeSymbol('='))
                    error("'=' expected");

                //rhs
                List<Expression> rhs = new List<Expression>();
                rhs.Add(ParseExpr(us.Scope));
                while (tok.ConsumeSymbol(','))
                {
                    rhs.Add(ParseExpr(scope));
                }

                AssignmentStatement a = new AssignmentStatement();
                a.Lhs = lhs;
                a.Rhs = rhs;

                if (!tok.ConsumeKeyword("do"))
                    error("'do' expected");

                List<Statement> block = ParseStatementList(us.Scope);

                if (!tok.ConsumeKeyword("end"))
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
                if (tok.IsSymbol(',') || tok.IsSymbol('='))
                {
                    // check that it was not parenthesized, making it not an lvalue
                    if (suffixed.ParenCount > 0)
                        error("Can not assign to parenthesized expression, it is not an lvalue");

                    // more processing needed
                    List<Expression> lhs = new List<Expression> { suffixed };
                    while (tok.ConsumeSymbol(','))
                    {
                        lhs.Add(ParseSuffixedExpr(scope));
                    }

                    // equals
                    if (!tok.ConsumeSymbol('='))
                        error("'=' expected");

                    //rhs
                    List<Expression> rhs = new List<Expression>();
                    rhs.Add(ParseExpr(scope));
                    while (tok.ConsumeSymbol(','))
                    {
                        rhs.Add(ParseExpr(scope));
                    }

                    AssignmentStatement a = new AssignmentStatement();
                    a.Lhs = lhs;
                    a.Rhs = rhs;
                    stat = a;
                }
                else if (isAugmentedAssignment(tok.Peek()))
                {
                    AugmentedAssignmentStatement aas = new AugmentedAssignmentStatement();
                    Expression left = suffixed;
                    Expression right = null;
                    string augmentedOp = tok.Get().Data;
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

            stat.ScannedTokens = tok.Range(startP, tok.p);
            if (tok.Peek().Data == ";" && tok.Peek().Type == TokenType.Symbol)
            {
                stat.HasSemicolon = true;
                stat.SemicolonToken = tok.Get();
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

            while (!isClosing(tok.Peek().Data) && !tok.IsEof())
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
            return new Chunk { Body = ParseStatementList(s), Scope = s };
        }
    }
}
