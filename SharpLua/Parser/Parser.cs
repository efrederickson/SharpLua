using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.Parser
{
    public partial class Parser
    {
        public Chunk ParseChunk(ParserInput<char> input, out bool success)
        {
            this.SetInput(input);
            Chunk chunk = ParseChunk(out success);
            if (this.Position < input.Length)
            {
                success = false;
                Error("Failed to parse remained input.");
            }
            return chunk;
        }

        private Chunk ParseChunk(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Chunk");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Chunk;
            }

            int errorCount = Errors.Count;
            Chunk chunk = new Chunk();
            int start_position = position;

            ParseSpOpt(out success);

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    Statement statement = ParseStatement(out success);
                    if (success) { chunk.Statements.Add(statement); }
                    else { break; }

                    while (true)
                    {
                        int seq_start_position2 = position;
                        MatchTerminal(';', out success);
                        if (!success) { break; }

                        ParseSpOpt(out success);
                        break;
                    }
                    success = true;
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(chunk, success, position);
            return chunk;
        }

        private Statement ParseStatement(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Statement");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Statement;
            }

            int errorCount = Errors.Count;
            Statement statement = null;

            statement = ParseAssignment(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseFunction(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseLocalVar(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseLocalFunc(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseReturnStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseBreakStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseDoStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseIfStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseForStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseForInStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseWhileStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseRepeatStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            statement = ParseExprStmt(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(statement, success, position);return statement; }

            return statement;
        }

        private Assignment ParseAssignment(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Assignment");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Assignment;
            }

            int errorCount = Errors.Count;
            Assignment assignment = new Assignment();
            int start_position = position;

            assignment.VarList = ParseVarList(out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(assignment, success, position);return assignment;
            }

            ParseSpOpt(out success);

            MatchTerminal('=', out success);
            if (!success)
            {
                Error("Failed to parse '=' of Assignment.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(assignment, success, position);return assignment;
            }

            ParseSpOpt(out success);

            assignment.ExprList = ParseExprList(out success);
            if (!success)
            {
                Error("Failed to parse ExprList of Assignment.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(assignment, success, position);
            return assignment;
        }

        private Function ParseFunction(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Function");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Function;
            }

            int errorCount = Errors.Count;
            Function function = new Function();
            int start_position = position;

            MatchTerminalString("function", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(function, success, position);return function;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of Function.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(function, success, position);return function;
            }

            function.Name = ParseFunctionName(out success);
            if (!success)
            {
                Error("Failed to parse Name of Function.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(function, success, position);return function;
            }

            ParseSpOpt(out success);

            function.Body = ParseFunctionBody(out success);
            if (!success)
            {
                Error("Failed to parse Body of Function.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(function, success, position);
            return function;
        }

        private LocalVar ParseLocalVar(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "LocalVar");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as LocalVar;
            }

            int errorCount = Errors.Count;
            LocalVar localVar = new LocalVar();
            int start_position = position;

            MatchTerminalString("local", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localVar, success, position);return localVar;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of LocalVar.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localVar, success, position);return localVar;
            }

            localVar.NameList = ParseNameList(out success);
            if (!success)
            {
                Error("Failed to parse NameList of LocalVar.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localVar, success, position);return localVar;
            }

            ParseSpOpt(out success);

            while (true)
            {
                int seq_start_position1 = position;
                MatchTerminal('=', out success);
                if (!success) { break; }

                ParseSpOpt(out success);

                localVar.ExprList = ParseExprList(out success);
                if (!success)
                {
                    Error("Failed to parse ExprList of LocalVar.");
                    position = seq_start_position1;
                }
                break;
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(localVar, success, position);
            return localVar;
        }

        private LocalFunc ParseLocalFunc(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "LocalFunc");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as LocalFunc;
            }

            int errorCount = Errors.Count;
            LocalFunc localFunc = new LocalFunc();
            int start_position = position;

            MatchTerminalString("local", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);return localFunc;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of LocalFunc.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);return localFunc;
            }

            MatchTerminalString("function", out success);
            if (!success)
            {
                Error("Failed to parse 'function' of LocalFunc.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);return localFunc;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of LocalFunc.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);return localFunc;
            }

            localFunc.Name = ParseName(out success);
            if (!success)
            {
                Error("Failed to parse Name of LocalFunc.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);return localFunc;
            }

            ParseSpOpt(out success);

            localFunc.Body = ParseFunctionBody(out success);
            if (!success)
            {
                Error("Failed to parse Body of LocalFunc.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(localFunc, success, position);
            return localFunc;
        }

        private ExprStmt ParseExprStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ExprStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ExprStmt;
            }

            int errorCount = Errors.Count;
            ExprStmt exprStmt = new ExprStmt();

            exprStmt.Expr = ParseExpr(out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Expr of ExprStmt."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(exprStmt, success, position);
            return exprStmt;
        }

        private ReturnStmt ParseReturnStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ReturnStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ReturnStmt;
            }

            int errorCount = Errors.Count;
            ReturnStmt returnStmt = new ReturnStmt();
            int start_position = position;

            MatchTerminalString("return", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(returnStmt, success, position);return returnStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ReturnStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(returnStmt, success, position);return returnStmt;
            }

            returnStmt.ExprList = ParseExprList(out success);
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(returnStmt, success, position);
            return returnStmt;
        }

        private BreakStmt ParseBreakStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "BreakStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as BreakStmt;
            }

            int errorCount = Errors.Count;
            BreakStmt breakStmt = new BreakStmt();
            int start_position = position;

            MatchTerminalString("break", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(breakStmt, success, position);return breakStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of BreakStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(breakStmt, success, position);
            return breakStmt;
        }

        private DoStmt ParseDoStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "DoStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as DoStmt;
            }

            int errorCount = Errors.Count;
            DoStmt doStmt = new DoStmt();
            int start_position = position;

            MatchTerminalString("do", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(doStmt, success, position);return doStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of DoStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(doStmt, success, position);return doStmt;
            }

            doStmt.Body = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Body of DoStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(doStmt, success, position);return doStmt;
            }

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of DoStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(doStmt, success, position);return doStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of DoStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(doStmt, success, position);
            return doStmt;
        }

        private IfStmt ParseIfStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "IfStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as IfStmt;
            }

            int errorCount = Errors.Count;
            IfStmt ifStmt = new IfStmt();
            int start_position = position;

            MatchTerminalString("if", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            ifStmt.Condition = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Condition of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            MatchTerminalString("then", out success);
            if (!success)
            {
                Error("Failed to parse 'then' of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            ifStmt.ThenBlock = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse ThenBlock of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            while (true)
            {
                ElseifBlock elseifBlock = ParseElseifBlock(out success);
                if (success) { ifStmt.ElseifBlocks.Add(elseifBlock); }
                else { break; }
            }
            success = true;

            while (true)
            {
                int seq_start_position1 = position;
                MatchTerminalString("else", out success);
                if (!success) { break; }

                ParseSpReq(out success);
                if (!success)
                {
                    Error("Failed to parse SpReq of IfStmt.");
                    position = seq_start_position1;
                    break;
                }

                ifStmt.ElseBlock = ParseChunk(out success);
                if (!success)
                {
                    Error("Failed to parse ElseBlock of IfStmt.");
                    position = seq_start_position1;
                }
                break;
            }
            success = true;

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of IfStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);return ifStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of IfStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(ifStmt, success, position);
            return ifStmt;
        }

        private ElseifBlock ParseElseifBlock(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ElseifBlock");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ElseifBlock;
            }

            int errorCount = Errors.Count;
            ElseifBlock elseifBlock = new ElseifBlock();
            int start_position = position;

            MatchTerminalString("elseif", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);return elseifBlock;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ElseifBlock.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);return elseifBlock;
            }

            elseifBlock.Condition = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Condition of ElseifBlock.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);return elseifBlock;
            }

            MatchTerminalString("then", out success);
            if (!success)
            {
                Error("Failed to parse 'then' of ElseifBlock.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);return elseifBlock;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ElseifBlock.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);return elseifBlock;
            }

            elseifBlock.ThenBlock = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse ThenBlock of ElseifBlock.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(elseifBlock, success, position);
            return elseifBlock;
        }

        private ForStmt ParseForStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ForStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ForStmt;
            }

            int errorCount = Errors.Count;
            ForStmt forStmt = new ForStmt();
            int start_position = position;

            MatchTerminalString("for", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            forStmt.VarName = ParseName(out success);
            if (!success)
            {
                Error("Failed to parse VarName of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpOpt(out success);

            MatchTerminal('=', out success);
            if (!success)
            {
                Error("Failed to parse '=' of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpOpt(out success);

            forStmt.Start = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Start of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            MatchTerminal(',', out success);
            if (!success)
            {
                Error("Failed to parse ',' of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpOpt(out success);

            forStmt.End = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse End of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            while (true)
            {
                int seq_start_position1 = position;
                MatchTerminal(',', out success);
                if (!success) { break; }

                ParseSpOpt(out success);

                forStmt.Step = ParseExpr(out success);
                if (!success)
                {
                    Error("Failed to parse Step of ForStmt.");
                    position = seq_start_position1;
                }
                break;
            }
            success = true;

            MatchTerminalString("do", out success);
            if (!success)
            {
                Error("Failed to parse 'do' of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            forStmt.Body = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Body of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of ForStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);return forStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(forStmt, success, position);
            return forStmt;
        }

        private ForInStmt ParseForInStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ForInStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ForInStmt;
            }

            int errorCount = Errors.Count;
            ForInStmt forInStmt = new ForInStmt();
            int start_position = position;

            MatchTerminalString("for", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            forInStmt.NameList = ParseNameList(out success);
            if (!success)
            {
                Error("Failed to parse NameList of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            MatchTerminalString("in", out success);
            if (!success)
            {
                Error("Failed to parse 'in' of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            forInStmt.ExprList = ParseExprList(out success);
            if (!success)
            {
                Error("Failed to parse ExprList of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            MatchTerminalString("do", out success);
            if (!success)
            {
                Error("Failed to parse 'do' of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            forInStmt.Body = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Body of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of ForInStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);return forInStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of ForInStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(forInStmt, success, position);
            return forInStmt;
        }

        private WhileStmt ParseWhileStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "WhileStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as WhileStmt;
            }

            int errorCount = Errors.Count;
            WhileStmt whileStmt = new WhileStmt();
            int start_position = position;

            MatchTerminalString("while", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            whileStmt.Condition = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Condition of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            MatchTerminalString("do", out success);
            if (!success)
            {
                Error("Failed to parse 'do' of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            whileStmt.Body = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Body of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of WhileStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);return whileStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of WhileStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(whileStmt, success, position);
            return whileStmt;
        }

        private RepeatStmt ParseRepeatStmt(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "RepeatStmt");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as RepeatStmt;
            }

            int errorCount = Errors.Count;
            RepeatStmt repeatStmt = new RepeatStmt();
            int start_position = position;

            MatchTerminalString("repeat", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);return repeatStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of RepeatStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);return repeatStmt;
            }

            repeatStmt.Body = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Body of RepeatStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);return repeatStmt;
            }

            MatchTerminalString("until", out success);
            if (!success)
            {
                Error("Failed to parse 'until' of RepeatStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);return repeatStmt;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of RepeatStmt.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);return repeatStmt;
            }

            repeatStmt.Condition = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Condition of RepeatStmt.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(repeatStmt, success, position);
            return repeatStmt;
        }

        private List<Var> ParseVarList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "VarList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<Var>;
            }

            int errorCount = Errors.Count;
            List<Var> list_Var = new List<Var>();
            int start_position = position;

            Var var = ParseVar(out success);
            if (success) { list_Var.Add(var); }
            else
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_Var, success, position);return list_Var;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseSpOpt(out success);

                    MatchTerminal(',', out success);
                    if (!success)
                    {
                        Error("Failed to parse ',' of VarList.");
                        position = seq_start_position1;
                        break;
                    }

                    ParseSpOpt(out success);

                    var = ParseVar(out success);
                    if (success) { list_Var.Add(var); }
                    else
                    {
                        Error("Failed to parse Var of VarList.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(list_Var, success, position);
            return list_Var;
        }

        private List<Expr> ParseExprList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ExprList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<Expr>;
            }

            int errorCount = Errors.Count;
            List<Expr> list_Expr = new List<Expr>();
            int start_position = position;

            Expr expr = ParseExpr(out success);
            if (success) { list_Expr.Add(expr); }
            else
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_Expr, success, position);return list_Expr;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseSpOpt(out success);

                    MatchTerminal(',', out success);
                    if (!success)
                    {
                        Error("Failed to parse ',' of ExprList.");
                        position = seq_start_position1;
                        break;
                    }

                    ParseSpOpt(out success);

                    expr = ParseExpr(out success);
                    if (success) { list_Expr.Add(expr); }
                    else
                    {
                        Error("Failed to parse Expr of ExprList.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(list_Expr, success, position);
            return list_Expr;
        }

        private Expr ParseExpr(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Expr");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Expr;
            }

            int errorCount = Errors.Count;
            Expr expr = null;

            expr = ParseOperatorExpr(out success);
            if (success) { return expr.Simplify(); }

            expr = ParseTerm(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(expr, success, position);return expr; }

            return expr;
        }

        private Term ParseTerm(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Term");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Term;
            }

            int errorCount = Errors.Count;
            Term term = null;

            term = ParseNilLiteral(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseBoolLiteral(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseNumberLiteral(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseStringLiteral(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseFunctionValue(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseTableConstructor(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParseVariableArg(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            term = ParsePrimaryExpr(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(term, success, position);return term; }

            return term;
        }

        private NilLiteral ParseNilLiteral(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "NilLiteral");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as NilLiteral;
            }

            int errorCount = Errors.Count;
            NilLiteral nilLiteral = new NilLiteral();

            MatchTerminalString("nil", out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse 'nil' of NilLiteral."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(nilLiteral, success, position);
            return nilLiteral;
        }

        private BoolLiteral ParseBoolLiteral(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "BoolLiteral");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as BoolLiteral;
            }

            int errorCount = Errors.Count;
            ErrorStatck.Push(errorCount); errorCount = Errors.Count;
            BoolLiteral boolLiteral = new BoolLiteral();

            while (true)
            {
                boolLiteral.Text = MatchTerminalString("true", out success);
                if (success) { ClearError(errorCount); break; }

                boolLiteral.Text = MatchTerminalString("false", out success);
                if (success) { ClearError(errorCount); break; }

                break;
            }
            errorCount = ErrorStatck.Pop();
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Text of BoolLiteral."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(boolLiteral, success, position);
            return boolLiteral;
        }

        private NumberLiteral ParseNumberLiteral(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "NumberLiteral");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as NumberLiteral;
            }

            int errorCount = Errors.Count;
            NumberLiteral numberLiteral = new NumberLiteral();

            numberLiteral.HexicalText = ParseHexicalNumber(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(numberLiteral, success, position);return numberLiteral; }

            numberLiteral.Text = ParseFoatNumber(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(numberLiteral, success, position);return numberLiteral; }

            return numberLiteral;
        }

        private StringLiteral ParseStringLiteral(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "StringLiteral");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as StringLiteral;
            }

            int errorCount = Errors.Count;
            StringLiteral stringLiteral = new StringLiteral();

            while (true)
            {
                int seq_start_position1 = position;
                MatchTerminal('"', out success);
                if (!success) { break; }

                stringLiteral.Text = ParseDoubleQuotedText(out success);

                MatchTerminal('"', out success);
                if (!success)
                {
                    Error("Failed to parse '\\\"' of StringLiteral.");
                    position = seq_start_position1;
                }
                break;
            }
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(stringLiteral, success, position);return stringLiteral; }

            while (true)
            {
                int seq_start_position2 = position;
                MatchTerminal('\'', out success);
                if (!success) { break; }

                stringLiteral.Text = ParseSingleQuotedText(out success);

                MatchTerminal('\'', out success);
                if (!success)
                {
                    Error("Failed to parse ''' of StringLiteral.");
                    position = seq_start_position2;
                }
                break;
            }
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(stringLiteral, success, position);return stringLiteral; }

            stringLiteral.Text = ParseLongString(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(stringLiteral, success, position);return stringLiteral; }

            return stringLiteral;
        }

        private VariableArg ParseVariableArg(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "VariableArg");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as VariableArg;
            }

            int errorCount = Errors.Count;
            VariableArg variableArg = new VariableArg();

            variableArg.Name = MatchTerminalString("...", out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Name of VariableArg."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(variableArg, success, position);
            return variableArg;
        }

        private FunctionValue ParseFunctionValue(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FunctionValue");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as FunctionValue;
            }

            int errorCount = Errors.Count;
            FunctionValue functionValue = new FunctionValue();
            int start_position = position;

            MatchTerminalString("function", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionValue, success, position);return functionValue;
            }

            ParseSpOpt(out success);

            functionValue.Body = ParseFunctionBody(out success);
            if (!success)
            {
                Error("Failed to parse Body of FunctionValue.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(functionValue, success, position);
            return functionValue;
        }

        private FunctionBody ParseFunctionBody(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FunctionBody");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as FunctionBody;
            }

            int errorCount = Errors.Count;
            FunctionBody functionBody = new FunctionBody();
            int start_position = position;

            MatchTerminal('(', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionBody, success, position);return functionBody;
            }

            ParseSpOpt(out success);

            while (true)
            {
                int seq_start_position1 = position;
                functionBody.ParamList = ParseParamList(out success);
                if (!success) { break; }

                ParseSpOpt(out success);
                break;
            }
            success = true;

            MatchTerminal(')', out success);
            if (!success)
            {
                Error("Failed to parse ')' of FunctionBody.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionBody, success, position);return functionBody;
            }

            functionBody.Chunk = ParseChunk(out success);
            if (!success)
            {
                Error("Failed to parse Chunk of FunctionBody.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionBody, success, position);return functionBody;
            }

            MatchTerminalString("end", out success);
            if (!success)
            {
                Error("Failed to parse 'end' of FunctionBody.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionBody, success, position);return functionBody;
            }

            ParseSpReq(out success);
            if (!success)
            {
                Error("Failed to parse SpReq of FunctionBody.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(functionBody, success, position);
            return functionBody;
        }

        private Access ParseAccess(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Access");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Access;
            }

            int errorCount = Errors.Count;
            Access access = null;

            access = ParseNameAccess(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(access, success, position);return access; }

            access = ParseKeyAccess(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(access, success, position);return access; }

            access = ParseMethodCall(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(access, success, position);return access; }

            access = ParseFunctionCall(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(access, success, position);return access; }

            return access;
        }

        private BaseExpr ParseBaseExpr(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "BaseExpr");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as BaseExpr;
            }

            int errorCount = Errors.Count;
            BaseExpr baseExpr = null;

            baseExpr = ParseGroupExpr(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(baseExpr, success, position);return baseExpr; }

            baseExpr = ParseVarName(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(baseExpr, success, position);return baseExpr; }

            return baseExpr;
        }

        private KeyAccess ParseKeyAccess(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "KeyAccess");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as KeyAccess;
            }

            int errorCount = Errors.Count;
            KeyAccess keyAccess = new KeyAccess();
            int start_position = position;

            MatchTerminal('[', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyAccess, success, position);return keyAccess;
            }

            ParseSpOpt(out success);

            keyAccess.Key = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Key of KeyAccess.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyAccess, success, position);return keyAccess;
            }

            MatchTerminal(']', out success);
            if (!success)
            {
                Error("Failed to parse ']' of KeyAccess.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(keyAccess, success, position);
            return keyAccess;
        }

        private NameAccess ParseNameAccess(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "NameAccess");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as NameAccess;
            }

            int errorCount = Errors.Count;
            NameAccess nameAccess = new NameAccess();
            int start_position = position;

            MatchTerminal('.', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(nameAccess, success, position);return nameAccess;
            }

            ParseSpOpt(out success);

            nameAccess.Name = ParseName(out success);
            if (!success)
            {
                Error("Failed to parse Name of NameAccess.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(nameAccess, success, position);
            return nameAccess;
        }

        private MethodCall ParseMethodCall(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "MethodCall");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as MethodCall;
            }

            int errorCount = Errors.Count;
            MethodCall methodCall = new MethodCall();
            int start_position = position;

            MatchTerminal(':', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(methodCall, success, position);return methodCall;
            }

            ParseSpOpt(out success);

            methodCall.Method = ParseName(out success);
            if (!success)
            {
                Error("Failed to parse Method of MethodCall.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(methodCall, success, position);return methodCall;
            }

            ParseSpOpt(out success);

            methodCall.Args = ParseArgs(out success);
            if (!success)
            {
                Error("Failed to parse Args of MethodCall.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(methodCall, success, position);
            return methodCall;
        }

        private FunctionCall ParseFunctionCall(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FunctionCall");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as FunctionCall;
            }

            int errorCount = Errors.Count;
            FunctionCall functionCall = new FunctionCall();

            functionCall.Args = ParseArgs(out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Args of FunctionCall."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(functionCall, success, position);
            return functionCall;
        }

        private Var ParseVar(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Var");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Var;
            }

            int errorCount = Errors.Count;
            Var var = new Var();
            int start_position = position;

            var.Base = ParseBaseExpr(out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(var, success, position);return var;
            }

            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    while (true)
                    {
                        int seq_start_position1 = position;
                        ParseSpOpt(out success);

                        NameAccess nameAccess = ParseNameAccess(out success);
                        if (success) { var.Accesses.Add(nameAccess); }
                        else
                        {
                            Error("Failed to parse NameAccess of Var.");
                            position = seq_start_position1;
                        }
                        break;
                    }
                    if (success) { ClearError(errorCount); break; }

                    while (true)
                    {
                        int seq_start_position2 = position;
                        ParseSpOpt(out success);

                        KeyAccess keyAccess = ParseKeyAccess(out success);
                        if (success) { var.Accesses.Add(keyAccess); }
                        else
                        {
                            Error("Failed to parse KeyAccess of Var.");
                            position = seq_start_position2;
                        }
                        break;
                    }
                    if (success) { ClearError(errorCount); break; }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(var, success, position);
            return var;
        }

        private PrimaryExpr ParsePrimaryExpr(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "PrimaryExpr");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as PrimaryExpr;
            }

            int errorCount = Errors.Count;
            PrimaryExpr primaryExpr = new PrimaryExpr();
            int start_position = position;

            primaryExpr.Base = ParseBaseExpr(out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(primaryExpr, success, position);return primaryExpr;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseSpOpt(out success);

                    Access access = ParseAccess(out success);
                    if (success) { primaryExpr.Accesses.Add(access); }
                    else
                    {
                        Error("Failed to parse Access of PrimaryExpr.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(primaryExpr, success, position);
            return primaryExpr;
        }

        private VarName ParseVarName(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "VarName");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as VarName;
            }

            int errorCount = Errors.Count;
            VarName varName = new VarName();

            varName.Name = ParseName(out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Name of VarName."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(varName, success, position);
            return varName;
        }

        private FunctionName ParseFunctionName(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FunctionName");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as FunctionName;
            }

            int errorCount = Errors.Count;
            FunctionName functionName = new FunctionName();
            int start_position = position;

            functionName.FullName = ParseFullName(out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(functionName, success, position);return functionName;
            }

            while (true)
            {
                int seq_start_position1 = position;
                ParseSpOpt(out success);

                MatchTerminal(':', out success);
                if (!success)
                {
                    Error("Failed to parse ':' of FunctionName.");
                    position = seq_start_position1;
                    break;
                }

                ParseSpOpt(out success);

                functionName.MethodName = ParseName(out success);
                if (!success)
                {
                    Error("Failed to parse MethodName of FunctionName.");
                    position = seq_start_position1;
                }
                break;
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(functionName, success, position);
            return functionName;
        }

        private GroupExpr ParseGroupExpr(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "GroupExpr");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as GroupExpr;
            }

            int errorCount = Errors.Count;
            GroupExpr groupExpr = new GroupExpr();
            int start_position = position;

            MatchTerminal('(', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(groupExpr, success, position);return groupExpr;
            }

            ParseSpOpt(out success);

            groupExpr.Expr = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Expr of GroupExpr.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(groupExpr, success, position);return groupExpr;
            }

            MatchTerminal(')', out success);
            if (!success)
            {
                Error("Failed to parse ')' of GroupExpr.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(groupExpr, success, position);
            return groupExpr;
        }

        private TableConstructor ParseTableConstructor(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "TableConstructor");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as TableConstructor;
            }

            int errorCount = Errors.Count;
            TableConstructor tableConstructor = new TableConstructor();
            int start_position = position;

            MatchTerminal('{', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(tableConstructor, success, position);return tableConstructor;
            }

            ParseSpOpt(out success);

            tableConstructor.FieldList = ParseFieldList(out success);
            success = true;

            MatchTerminal('}', out success);
            if (!success)
            {
                Error("Failed to parse '}' of TableConstructor.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(tableConstructor, success, position);
            return tableConstructor;
        }

        private List<Field> ParseFieldList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FieldList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<Field>;
            }

            int errorCount = Errors.Count;
            List<Field> list_Field = new List<Field>();
            int start_position = position;

            Field field = ParseField(out success);
            if (success) { list_Field.Add(field); }
            else
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_Field, success, position);return list_Field;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseFieldSep(out success);
                    if (!success) { break; }

                    ParseSpOpt(out success);

                    field = ParseField(out success);
                    if (success) { list_Field.Add(field); }
                    else
                    {
                        Error("Failed to parse Field of FieldList.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            while (true)
            {
                int seq_start_position2 = position;
                ParseFieldSep(out success);
                if (!success) { break; }

                ParseSpOpt(out success);
                break;
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(list_Field, success, position);
            return list_Field;
        }

        private Field ParseField(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Field");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Field;
            }

            int errorCount = Errors.Count;
            Field field = null;

            field = ParseKeyValue(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(field, success, position);return field; }

            field = ParseNameValue(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(field, success, position);return field; }

            field = ParseItemValue(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(field, success, position);return field; }

            return field;
        }

        private KeyValue ParseKeyValue(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "KeyValue");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as KeyValue;
            }

            int errorCount = Errors.Count;
            KeyValue keyValue = new KeyValue();
            int start_position = position;

            MatchTerminal('[', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyValue, success, position);return keyValue;
            }

            ParseSpOpt(out success);

            keyValue.Key = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Key of KeyValue.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyValue, success, position);return keyValue;
            }

            MatchTerminal(']', out success);
            if (!success)
            {
                Error("Failed to parse ']' of KeyValue.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyValue, success, position);return keyValue;
            }

            ParseSpOpt(out success);

            MatchTerminal('=', out success);
            if (!success)
            {
                Error("Failed to parse '=' of KeyValue.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(keyValue, success, position);return keyValue;
            }

            ParseSpOpt(out success);

            keyValue.Value = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Value of KeyValue.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(keyValue, success, position);
            return keyValue;
        }

        private NameValue ParseNameValue(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "NameValue");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as NameValue;
            }

            int errorCount = Errors.Count;
            NameValue nameValue = new NameValue();
            int start_position = position;

            nameValue.Name = ParseName(out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(nameValue, success, position);return nameValue;
            }

            ParseSpOpt(out success);

            MatchTerminal('=', out success);
            if (!success)
            {
                Error("Failed to parse '=' of NameValue.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(nameValue, success, position);return nameValue;
            }

            ParseSpOpt(out success);

            nameValue.Value = ParseExpr(out success);
            if (!success)
            {
                Error("Failed to parse Value of NameValue.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(nameValue, success, position);
            return nameValue;
        }

        private ItemValue ParseItemValue(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ItemValue");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ItemValue;
            }

            int errorCount = Errors.Count;
            ItemValue itemValue = new ItemValue();

            itemValue.Value = ParseExpr(out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse Value of ItemValue."); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(itemValue, success, position);
            return itemValue;
        }

        private OperatorExpr ParseOperatorExpr(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "OperatorExpr");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as OperatorExpr;
            }

            int errorCount = Errors.Count;
            OperatorExpr operatorExpr = new OperatorExpr();
            int start_position = position;

            while (true)
            {
                int seq_start_position1 = position;
                string unaryOper = ParseUnaryOperator(out success);
                if (success) { operatorExpr.Add(unaryOper); }
                else { break; }

                ParseSpOpt(out success);
                break;
            }
            success = true;

            Term firstTerm = ParseTerm(out success);
            if (success) { operatorExpr.Add(firstTerm); }
            else
            {
                Error("Failed to parse firstTerm of OperatorExpr.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(operatorExpr, success, position);return operatorExpr;
            }

            ParseSpOpt(out success);

            while (true)
            {
                while (true)
                {
                    int seq_start_position2 = position;
                    string binaryOper = ParseBinaryOperator(out success);
                    if (success) { operatorExpr.Add(binaryOper); }
                    else { break; }

                    ParseSpOpt(out success);

                    Term nextTerm = ParseTerm(out success);
                    if (success) { operatorExpr.Add(nextTerm); }
                    else
                    {
                        Error("Failed to parse nextTerm of OperatorExpr.");
                        position = seq_start_position2;
                        break;
                    }

                    ParseSpOpt(out success);
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(operatorExpr, success, position);
            return operatorExpr;
        }

        private Args ParseArgs(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Args");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as Args;
            }

            int errorCount = Errors.Count;
            Args args = new Args();

            args.ArgList = ParseArgList(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(args, success, position);return args; }

            args.String = ParseStringLiteral(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(args, success, position);return args; }

            args.Table = ParseTableConstructor(out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(args, success, position);return args; }

            return args;
        }

        private List<Expr> ParseArgList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ArgList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<Expr>;
            }

            int errorCount = Errors.Count;
            List<Expr> list_Expr = new List<Expr>();
            int start_position = position;

            MatchTerminal('(', out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_Expr, success, position);return list_Expr;
            }

            ParseSpOpt(out success);

            while (true)
            {
                int seq_start_position1 = position;
                list_Expr = ParseExprList(out success);
                if (!success) { break; }

                ParseSpOpt(out success);
                break;
            }
            success = true;

            MatchTerminal(')', out success);
            if (!success)
            {
                Error("Failed to parse ')' of ArgList.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(list_Expr, success, position);
            return list_Expr;
        }

        private ParamList ParseParamList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "ParamList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as ParamList;
            }

            int errorCount = Errors.Count;
            ParamList paramList = new ParamList();

            while (true)
            {
                int seq_start_position1 = position;
                paramList.NameList = ParseNameList(out success);
                if (!success) { break; }

                while (true)
                {
                    int seq_start_position2 = position;
                    MatchTerminal(',', out success);
                    if (!success) { break; }

                    ParseSpOpt(out success);

                    MatchTerminalString("...", out success);
                    if (!success)
                    {
                        Error("Failed to parse '...' of ParamList.");
                        position = seq_start_position2;
                    }
                    break;
                }
                paramList.HasVarArg = success;
                success = true;
                break;
            }
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(paramList, success, position);return paramList; }

            paramList.IsVarArg = MatchTerminalString("...", out success);
            if (success) { ClearError(errorCount); ParsingResults[reskey] = new Tuple<object, bool, int>(paramList, success, position);return paramList; }

            return paramList;
        }

        private List<string> ParseFullName(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FullName");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<string>;
            }

            int errorCount = Errors.Count;
            List<string> list_string = new List<string>();
            int start_position = position;

            string str = ParseName(out success);
            if (success) { list_string.Add(str); }
            else
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_string, success, position);return list_string;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseSpOpt(out success);

                    MatchTerminal('.', out success);
                    if (!success)
                    {
                        Error("Failed to parse '.' of FullName.");
                        position = seq_start_position1;
                        break;
                    }

                    ParseSpOpt(out success);

                    str = ParseName(out success);
                    if (success) { list_string.Add(str); }
                    else
                    {
                        Error("Failed to parse Name of FullName.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(list_string, success, position);
            return list_string;
        }

        private List<string> ParseNameList(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "NameList");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as List<string>;
            }

            int errorCount = Errors.Count;
            List<string> list_string = new List<string>();
            int start_position = position;

            string str = ParseName(out success);
            if (success) { list_string.Add(str); }
            else
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(list_string, success, position);return list_string;
            }

            while (true)
            {
                while (true)
                {
                    int seq_start_position1 = position;
                    ParseSpOpt(out success);

                    MatchTerminal(',', out success);
                    if (!success)
                    {
                        Error("Failed to parse ',' of NameList.");
                        position = seq_start_position1;
                        break;
                    }

                    ParseSpOpt(out success);

                    str = ParseName(out success);
                    if (success) { list_string.Add(str); }
                    else
                    {
                        Error("Failed to parse Name of NameList.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(list_string, success, position);
            return list_string;
        }

        private string ParseName(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "Name");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();
            int start_position = position;

            int not_start_position1 = position;
            while (true)
            {
                ParseKeyword(out success);
                if (!success) { break; }

                ParseSpReq(out success);
                break;
            }
            position = not_start_position1;
            success = !success;
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            char ch = ParseLetter(out success);
            if (success) { text.Append(ch); }
            else
            {
                Error("Failed to parse Letter of Name.");
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    ch = ParseLetter(out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    ch = ParseDigit(out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private string ParseFoatNumber(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "FoatNumber");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();
            int start_position = position;

            int counter = 0;
            while (true)
            {
                char ch = ParseDigit(out success);
                if (success) { text.Append(ch); }
                else { break; }
                counter++;
            }
            if (counter > 0) { success = true; }
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            while (true)
            {
                int seq_start_position1 = position;
                char ch = MatchTerminal('.', out success);
                if (success) { text.Append(ch); }
                else { break; }

                counter = 0;
                while (true)
                {
                    ch = ParseDigit(out success);
                    if (success) { text.Append(ch); }
                    else { break; }
                    counter++;
                }
                if (counter > 0) { success = true; }
                if (!success)
                {
                    Error("Failed to parse (Digit)+ of FoatNumber.");
                    position = seq_start_position1;
                }
                break;
            }
            success = true;

            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                int seq_start_position2 = position;
                while (true)
                {
                    char ch = MatchTerminal('e', out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    ch = MatchTerminal('E', out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }

                counter = 0;
                while (true)
                {
                    char ch = ParseDigit(out success);
                    if (success) { text.Append(ch); }
                    else { break; }
                    counter++;
                }
                if (counter > 0) { success = true; }
                if (!success)
                {
                    Error("Failed to parse (Digit)+ of FoatNumber.");
                    position = seq_start_position2;
                }
                break;
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private string ParseHexicalNumber(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "HexicalNumber");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();
            int start_position = position;

            MatchTerminalString("0x", out success);
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            int counter = 0;
            while (true)
            {
                char ch = ParseHexDigit(out success);
                if (success) { text.Append(ch); }
                else { break; }
                counter++;
            }
            if (counter > 0) { success = true; }
            if (!success)
            {
                Error("Failed to parse (HexDigit)+ of HexicalNumber.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private string ParseSingleQuotedText(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "SingleQuotedText");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();

            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    char ch = MatchTerminalSet("'\\", true, out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    ch = ParseEscapeChar(out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
            }
            success = true;
            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private string ParseDoubleQuotedText(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "DoubleQuotedText");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();

            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    char ch = MatchTerminalSet("\"\\", true, out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    ch = ParseEscapeChar(out success);
                    if (success)
                    {
                        ClearError(errorCount);
                        text.Append(ch);
                        break;
                    }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
            }
            success = true;
            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private string ParseLongString(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "LongString");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();
            int start_position = position;

            List<char> opening = new List<char>();
            while (true)
            {
                int seq_start_position1 = position;
                char ch = MatchTerminal('[', out success);
                if (success) { opening.Add(ch); }
                else { break; }

                while (true)
                {
                    ch = MatchTerminal('=', out success);
                    if (success) { opening.Add(ch); }
                    else { break; }
                }
                success = true;

                ch = MatchTerminal('[', out success);
                if (success) { opening.Add(ch); }
                else
                {
                    Error("Failed to parse '[' of LongString.");
                    position = seq_start_position1;
                }
                break;
            }
            if (!success)
            {
                position = start_position;
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ParseEol(out success);
            string closing = new string(opening.ToArray()).Replace('[', ']');
            success = true;

            while (true)
            {
                MatchTerminalString(closing, out success);
                if (success) break;
                char ch = MatchTerminalSet("", true, out success);
                if (success) { text.Append(ch); }
                else { break; }
            }
            success = true;

            ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);
            return text.ToString();
        }

        private void ParseKeyword(out bool success)
        {
            int errorCount = Errors.Count;
            MatchTerminalString("and", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("break", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("do", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("elseif", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("else", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("end", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("false", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("for", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("function", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("if", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("in", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("local", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("nil", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("not", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("or", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("repeat", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("return", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("then", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("true", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("until", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminalString("while", out success);
            if (success) { ClearError(errorCount); return; }

        }

        private char ParseDigit(out bool success)
        {
            int errorCount = Errors.Count;
            char ch = MatchTerminalRange('0', '9', out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse '0'...'9' of Digit."); }
            
            return ch;
        }

        private char ParseHexDigit(out bool success)
        {
            int errorCount = Errors.Count;
            char ch = MatchTerminalSet("0123456789ABCDEFabcdef", false, out success);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse \"0123456789ABCDEFabcdef\" of HexDigit."); }
            
            return ch;
        }

        private char ParseLetter(out bool success)
        {
            int errorCount = Errors.Count;
            char ch = default(char);

            ch = MatchTerminalRange('A', 'Z', out success);
            if (success) { ClearError(errorCount); return ch; }

            ch = MatchTerminalRange('a', 'z', out success);
            if (success) { ClearError(errorCount); return ch; }

            ch = MatchTerminal('_', out success);
            if (success) { ClearError(errorCount); return ch; }

            return ch;
        }

        private string ParseUnaryOperator(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "UnaryOperator");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();

            char ch = MatchTerminal('#', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('-', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            string str = MatchTerminalString("not", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            return text.ToString();
        }

        private string ParseBinaryOperator(out bool success)
        {
            var reskey = new Tuple<int, string>(position, "BinaryOperator");
            if (ParsingResults.ContainsKey(reskey))
            {
                var parsingResult = ParsingResults[reskey];
                success = parsingResult.Item2;
                position = parsingResult.Item3;
                return parsingResult.Item1 as string;
            }

            int errorCount = Errors.Count;
            StringBuilder text = new StringBuilder();

            char ch = MatchTerminal('+', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('-', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('*', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('/', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('%', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('^', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            string str = MatchTerminalString("..", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString("==", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString("~=", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString("<=", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString(">=", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('<', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            ch = MatchTerminal('>', out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(ch);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString("and", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            str = MatchTerminalString("or", out success);
            if (success)
            {
                ClearError(errorCount);
                text.Append(str);
                ParsingResults[reskey] = new Tuple<object, bool, int>(text.ToString(), success, position);return text.ToString();
            }

            return text.ToString();
        }

        private void ParseFieldSep(out bool success)
        {
            int errorCount = Errors.Count;
            MatchTerminal(',', out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminal(';', out success);
            if (success) { ClearError(errorCount); return; }

        }

        private void ParseSpReq(out bool success)
        {
            int errorCount = Errors.Count;
            int counter = 0;
            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    MatchTerminalSet(" \t\r\n", false, out success);
                    if (success) { ClearError(errorCount); break; }

                    ParseComment(out success);
                    if (success) { ClearError(errorCount); break; }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
                counter++;
            }
            if (counter > 0) { success = true; }
            if (success) { ClearError(errorCount); return; }

            while (true)
            {
                int seq_start_position1 = position;
                ParseSpOpt(out success);

                ParseEof(out success);
                if (!success)
                {
                    Error("Failed to parse Eof of SpReq.");
                    position = seq_start_position1;
                }
                break;
            }
            if (success) { ClearError(errorCount); return; }

        }

        private void ParseSpOpt(out bool success)
        {
            int errorCount = Errors.Count;
            while (true)
            {
                ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                while (true)
                {
                    MatchTerminalSet(" \t\r\n", false, out success);
                    if (success) { ClearError(errorCount); break; }

                    ParseComment(out success);
                    if (success) { ClearError(errorCount); break; }

                    break;
                }
                errorCount = ErrorStatck.Pop();
                if (!success) { break; }
            }
            success = true;
        }

        private void ParseComment(out bool success)
        {
            int errorCount = Errors.Count;
            int start_position = position;

            MatchTerminalString("--", out success);
            if (!success)
            {
                position = start_position;
                return;
            }

            ErrorStatck.Push(errorCount); errorCount = Errors.Count;
            while (true)
            {
                ParseLongString(out success);
                if (success) { ClearError(errorCount); break; }

                while (true)
                {
                    int seq_start_position1 = position;
                    while (true)
                    {
                        MatchTerminalSet("\r\n", true, out success);
                        if (!success) { break; }
                    }
                    success = true;

                    ErrorStatck.Push(errorCount); errorCount = Errors.Count;
                    while (true)
                    {
                        ParseEol(out success);
                        if (success) { ClearError(errorCount); break; }

                        ParseEof(out success);
                        if (success) { ClearError(errorCount); break; }

                        break;
                    }
                    errorCount = ErrorStatck.Pop();
                    if (!success)
                    {
                        Error("Failed to parse (Eol / Eof) of Comment.");
                        position = seq_start_position1;
                    }
                    break;
                }
                if (success) { ClearError(errorCount); break; }

                break;
            }
            errorCount = ErrorStatck.Pop();
            if (!success)
            {
                Error("Failed to parse (LongString / (-\"\r\n\")* (Eol / Eof)) of Comment.");
                position = start_position;
            }

            if (success) { ClearError(errorCount); }
        }

        private void ParseEol(out bool success)
        {
            int errorCount = Errors.Count;
            MatchTerminalString("\r\n", out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminal('\n', out success);
            if (success) { ClearError(errorCount); return; }

            MatchTerminal('\r', out success);
            if (success) { ClearError(errorCount); return; }

        }

        private void ParseEof(out bool success)
        {
            int errorCount = Errors.Count;
            success = !Input.HasInput(position);
            if (success) { ClearError(errorCount); }
            else { Error("Failed to parse end of Eof."); }
        }

        private char ParseEscapeChar(out bool success)
        {
            int errorCount = Errors.Count;
            char ch = default(char);

            MatchTerminalString("\\\\", out success);
            if (success) { return '\\'; }

            MatchTerminalString("\\'", out success);
            if (success) { return '\''; }

            MatchTerminalString("\\\"", out success);
            if (success) { return '\"'; }

            MatchTerminalString("\\r", out success);
            if (success) { return '\r'; }

            MatchTerminalString("\\n", out success);
            if (success) { return '\n'; }

            MatchTerminalString("\\t", out success);
            if (success) { return '\t'; }

            MatchTerminalString("\\v", out success);
            if (success) { return '\v'; }

            MatchTerminalString("\\a", out success);
            if (success) { return '\a'; }

            MatchTerminalString("\\b", out success);
            if (success) { return '\b'; }

            MatchTerminalString("\\f", out success);
            if (success) { return '\f'; }

            MatchTerminalString("\\0", out success);
            if (success) { return '\0'; }

            return ch;
        }

    }
}
