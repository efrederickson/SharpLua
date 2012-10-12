using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using experimental_newparser.Ast.Expression;
using experimental_newparser.Ast.Statement;
using experimental_newparser.Ast;

namespace experimental_newparser.Visitors
{
    public class Beautifier
    {
        StringBuilder sb = new StringBuilder();
        public string EOL = "\r\n";
        public string Tab = "    ";

        int indent = 0;
        void nlindent()
        {
            sb.Append(EOL);
            indent++;
            writeIndent();
        }

        void nldedent()
        {
            sb.Append(EOL);
            indent--;
            writeIndent();
        }

        void nl()
        {
            sb.Append(EOL);
            writeIndent();
        }

        void writeIndent()
        {
            for (int i = 0; i < indent; i++)
                sb.Append(Tab);
        }

        void write(string s)
        {
            sb.Append(s);
        }

        string Expr(Expression e)
        {
            if (e is AnonymousFunctionExpr) // function() ... end
                ;
            else if (e is BinOpExpr)
            {
                string left = Expr((e as BinOpExpr).Lhs);
                string op = (e as BinOpExpr).Op;
                string right = Expr((e as BinOpExpr).Rhs);
                return left + " " + op + " " + right;
            }
            else if (e is BoolExpr)
            {
                bool val = (e as BoolExpr).Value;
                return val ? "true" : "false";
            }
            else if (e is CallExpr)
                ;
            else if (e is StringCallExpr)
                ;
            else if (e is TableCallExpr)
                ;
            else if (e is IndexExpr)
                ;
            else if (e is InlineFunctionExpression) // |<args>| -> <exprs>
                ;
            else if (e is TableConstructorKeyExpr)
                ;
            else if (e is MemberExpr)
                ;
            else if (e is NilExpr)
                return "nil";
            else if (e is NumberExpr)
                return (e as NumberExpr).Value;
            else if (e is StringExpr)
            {
                string delim = (e as StringExpr).Delimiter;
                if (delim == "[")
                    delim = "[["; // ugh. gotta fix this
                string s = (e as StringExpr).Value;
                return delim + s + (delim == "[[" ? "]]" : delim);
            }
            else if (e is TableConstructorStringKeyExpr)
            {
                TableConstructorStringKeyExpr tcske = e as TableConstructorStringKeyExpr;
                return tcske.Key + " = " + Expr(tcske.Value);
            }
            else if (e is TableConstructorExpr)
                ;
            else if (e is UnOpExpr)
            {
                string op = (e as UnOpExpr).Op;
                string s = op;
                if (s.Length != 1)
                    s += " ";
                return s + Expr((e as UnOpExpr).Rhs);
            }
            else if (e is TableConstructorValueExpr)
                return Expr((e as TableConstructorValueExpr).Value);
            else if (e is VarargExpr)
                return "...";
            else if (e is VariableExpression)
                return (e as VariableExpression).Name;

            throw new NotImplementedException(e.GetType().Name + " is not implemented");
        }
    }
}
