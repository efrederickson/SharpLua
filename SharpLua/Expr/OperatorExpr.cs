using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class OperatorExpr : Expr
    {
        public LinkedList<object> Terms = new LinkedList<object>();

        public void Add(string oper)
        {
            Terms.AddLast(oper);
        }

        public void Add(Term term)
        {
            Terms.AddLast(term);
        }

        public Term BuildExpressionTree()
        {
            var node = this.Terms.First;
            Term term = node.Value as Term;

            if (this.Terms.Count == 1)
            {
                return term;
            }
            else
            {
                if (term != null)
                {
                    return BuildExpressionTree(node.Value as Term, node.Next);
                }

                string oper = node.Value as string;

                if (oper != null)
                {
                    return BuildExpressionTree(null, node);
                }

                return null;
            }
        }

        // Operator-precedence parsing algorithm
        private static Term BuildExpressionTree(Term leftTerm, LinkedListNode<object> node)
        {
            string oper = node.Value as string;
            var rightNode = node.Next;
            Term rightTerm = rightNode.Value as Term;

            if (rightNode.Next == null) // last node
            {
                return new Operation(oper, leftTerm, rightTerm);
            }
            else
            {
                string nextOper = rightNode.Next.Value as string;

                if (OperTable.IsPrior(oper, nextOper))
                {
                    return BuildExpressionTree(new Operation(oper, leftTerm, rightTerm), rightNode.Next);
                }
                else
                {
                    return new Operation(oper, leftTerm, BuildExpressionTree(rightTerm, rightNode.Next));
                }
            }
        }

        public override LuaValue Evaluate(LuaTable enviroment)
        {
            Term term = this.BuildExpressionTree();
            return term.Evaluate(enviroment);
        }

        public override Term Simplify()
        {
            return this.BuildExpressionTree().Simplify();
        }
    }
}
