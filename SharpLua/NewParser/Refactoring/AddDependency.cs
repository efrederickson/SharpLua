using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpLua.Ast;
using SharpLua.Ast.Expression;
using SharpLua.Ast.Statement;

namespace SharpLua
{
    public partial class Refactoring
    {
        /// <summary>
        /// Adds a dependency using the "require" function (for loading Lua modules)
        /// </summary>
        /// <param name="c"></param>
        /// <param name="name">The name of the module to reference</param>
        /// <param name="varName">Optional. The variable name to assign to the module</param>
        public static void AddModuleDependency(Chunk c, string name, string varName = "")
        {
            if (varName == "")
            {
                // FunctionCall
                StringCallExpr call = new StringCallExpr();

                Variable require = c.Scope.GetVariable("require");
                VariableExpression v = new VariableExpression();
                if (require == null)
                {
                    require = c.Scope.CreateGlobal("require");
                    require.IsGlobal = true;
                }
                v.Var = require;
                call.Base = v;

                call.Arguments.Add(new StringExpr(name) { StringType = TokenType.DoubleQuoteString });

                c.Body.Insert(0, new CallStatement() { Expression = call, Scope = c.Scope });
            }
            else
            {
                // Assignment with FunctionCall
                AssignmentStatement a = new AssignmentStatement();

                StringCallExpr call = new StringCallExpr();
                call.Scope = c.Scope;
                Variable require = c.Scope.GetVariable("require");
                VariableExpression v = new VariableExpression();
                if (require == null)
                {
                    require = c.Scope.CreateGlobal("require");
                    require.IsGlobal = true;
                }
                v.Var = require;
                call.Base = v;
                call.Arguments.Add(new StringExpr(name) { StringType = TokenType.DoubleQuoteString });
                a.IsLocal = true; // local import
                a.Rhs.Add(call);

                Variable var = c.Scope.GetVariable(varName);
                VariableExpression v2 = new VariableExpression();
                if (var == null)
                {
                    var = c.Scope.CreateLocal(varName);
                }
                v2.Var = var;
                a.Lhs.Add(v2);

                c.Body.Insert(0, a);
            }
        }

        // clr.load "<assembly>"

        // ? clr.usingns(<ns>)

        // local __ns = clr.getns "<type.ns>"
        // local <type> = __ns.<type.typename>

        /// <summary>
        /// Adds a dependency to a .NET/CLR library using clr.load
        /// </summary>
        /// <param name="c"></param>
        /// <param name="assembly"></param>
        public static void AddClrDependency(Chunk c, string assembly)
        {
            // FunctionCall
            StringCallExpr call = new StringCallExpr();

            Variable require = c.Scope.GetVariable("clr");
            VariableExpression v = new VariableExpression();
            if (require == null)
            {
                require = c.Scope.CreateGlobal("clr");
                require.IsGlobal = true;
            }
            v.Var = require;
            MemberExpr me = new MemberExpr();
            me.Base = v;
            me.Indexer = ".";
            me.Ident = "load";
            call.Base = me;

            call.Arguments.Add(new StringExpr(assembly) { StringType = TokenType.DoubleQuoteString });

            c.Body.Insert(0, new CallStatement() { Expression = call, Scope = c.Scope });
        }

        /// <summary>
        /// Adds a dependency to a .NET/CLR library using clr.load,
        /// then it sets type to the type in the assembly.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="assembly"></param>
        /// <param name="type"></param>
        public static void AddClrDependency(Chunk c, string assembly, string type)
        {
            AddClrDependency(c, assembly);

            // Assignment with FunctionCall
            AssignmentStatement a = new AssignmentStatement();

            CallExpr call = new CallExpr();
            call.Scope = c.Scope;
            Variable require = c.Scope.GetVariable("clr");
            VariableExpression v = new VariableExpression();
            if (require == null)
            {
                require = c.Scope.CreateGlobal("clr");
                require.IsGlobal = true;
            }

            string name = "", varName = "";

            if (type.Contains('.'))
            {
                name = type.Substring(0, type.LastIndexOf('.'));
                varName = type.Substring(type.LastIndexOf('.') + 1);
            }
            else
            {
                name = assembly;
                varName = type;
            }

            v.Var = require;
            MemberExpr me = new MemberExpr();
            me.Base = v;
            me.Indexer = ".";
            me.Ident = "getns";
            call.Base = me;
            call.Arguments.Add(new StringExpr(name) { StringType = TokenType.DoubleQuoteString });
            a.IsLocal = true; // local import
            MemberExpr me2 = new MemberExpr();
            me2.Base = call;
            me2.Indexer = ".";
            me2.Ident = varName;
            a.Rhs.Add(me2);

            Variable var = c.Scope.GetVariable(varName);
            VariableExpression v2 = new VariableExpression();
            if (var == null)
            {
                var = c.Scope.CreateLocal(varName);
            }
            v2.Var = var;
            a.Lhs.Add(v2);

            // Insert after the load
            c.Body.Insert(1, a);
        }
    }
}
