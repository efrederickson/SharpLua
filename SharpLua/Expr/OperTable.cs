using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua.AST
{
    [Serializable()]
    public enum Associativity
    {
        NonAssociative,
        LeftAssociative,
        RightAssociative
    }

    [Serializable()]
    public class OperTable
    {
        static Dictionary<string, int> precedence = new Dictionary<string, int>();
        static Associativity[] associativity;

        static OperTable()
        {
            List<string[]> operators = new List<string[]>();
            operators.Add(new string[] { "or" });
            operators.Add(new string[] { "and" });
            operators.Add(new string[] { "==", "~=" });
            operators.Add(new string[] { ">", ">=", "<", "<=" });
            operators.Add(new string[] { ".." });
            operators.Add(new string[] { "+", "-" });
            operators.Add(new string[] { "*", "/", "%" });
            operators.Add(new string[] { "#", "not" });
            operators.Add(new string[] { "^" });

            for (int index = 0; index < operators.Count; index++)
            {
                foreach (string oper in operators[index])
                {
                    precedence.Add(oper, index);
                }
            }

            associativity = new Associativity[operators.Count];
            associativity[0] = Associativity.LeftAssociative;
            associativity[1] = Associativity.LeftAssociative;
            associativity[2] = Associativity.NonAssociative;
            associativity[3] = Associativity.LeftAssociative;
            associativity[4] = Associativity.LeftAssociative;
            associativity[5] = Associativity.LeftAssociative;
            associativity[6] = Associativity.LeftAssociative;
            associativity[7] = Associativity.NonAssociative;
            associativity[8] = Associativity.RightAssociative;
        }

        /// <summary>
        /// Whether the input text is an operator or not
        /// </summary>
        /// <param name="oper"></param>
        /// <returns></returns>
        public static bool Contains(string oper)
        {
            return precedence.ContainsKey(oper);
        }

        /// <summary>
        /// whether operLeft has higher precedence than operRight
        /// </summary>
        /// <param name="operLeft"></param>
        /// <param name="operRight"></param>
        /// <returns></returns>
        public static bool IsPrior(string operLeft, string operRight)
        {
            if (operLeft == null) return false;
            if (operRight == null) return true;

            int priLeft = precedence[operLeft];
            int priRight = precedence[operRight];
            if (priLeft > priRight)
            {
                return true;
            }
            else if (priLeft < priRight)
            {
                return false;
            }
            else
            {
                switch (associativity[priLeft])
                {
                    case Associativity.LeftAssociative:
                        return true;
                    case Associativity.RightAssociative:
                        return false;
                    default:
                        return true;
                }
            }
        }
    }
}
