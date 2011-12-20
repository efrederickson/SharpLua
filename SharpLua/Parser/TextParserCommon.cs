using System;
using System.Collections.Generic;
using System.Text;

namespace SharpLua
{
    public partial class Parser
    {
        int position;

        public int Position
        {
            get { return position; }
            set { position = value; }
        }

        ParserInput<char> Input;

        public List<Tuple<int, string>> Errors = new List<Tuple<int, string>>();
        private Stack<int> ErrorStatck = new Stack<int>();
    
        /// <summary>
        /// Memories parsing results, key is (PositionStart, Noterminal), value is (SyntacticElement, success, PostionAfter).
        /// </summary>
        private Dictionary<Tuple<int, string>, Tuple<object, bool, int>> ParsingResults = new Dictionary<Tuple<int, string>, Tuple<object, bool, int>>();
        
        public Parser() { }

        private void SetInput(ParserInput<char> input)
        {
            Input = input;
            position = 0;
            ParsingResults.Clear();
        }

        private bool TerminalMatch(char terminal)
        {
            if (Input.HasInput(position))
            {
                char symbol = Input.GetInputSymbol(position);
                return terminal == symbol;
            }
            return false;
        }

        private bool TerminalMatch(char terminal, int pos)
        {
            if (Input.HasInput(pos))
            {
                char symbol = Input.GetInputSymbol(pos);
                return terminal == symbol;
            }
            return false;
        }

        private char MatchTerminal(char terminal, out bool success)
        {
            success = false;
            if (Input.HasInput(position))
            {
                char symbol = Input.GetInputSymbol(position);
                if (terminal == symbol)
                {
                    position++;
                    success = true;
                }
                return symbol;
            }
            return default(char);
        }

        private char MatchTerminalRange(char start, char end, out bool success)
        {
            success = false;
            if (Input.HasInput(position))
            {
                char symbol = Input.GetInputSymbol(position);
                if (start <= symbol && symbol <= end)
                {
                    position++;
                    success = true;
                }
                return symbol;
            }
            return default(char);
        }

        private char MatchTerminalSet(string terminalSet, bool isComplement, out bool success)
        {
            success = false;
            if (Input.HasInput(position))
            {
                char symbol = Input.GetInputSymbol(position);
                bool match = isComplement ? terminalSet.IndexOf(symbol) == -1 : terminalSet.IndexOf(symbol) > -1;
                if (match)
                {
                    position++;
                    success = true;
                }
                return symbol;
            }
            return default(char);
        }

        private string MatchTerminalString(string terminalString, out bool success)
        {
            int currrent_position = position;
            foreach (char terminal in terminalString)
            {
                MatchTerminal(terminal, out success);
                if (!success)
                {
                    position = currrent_position;
                    return null;
                }
            }
            success = true;
            return terminalString;
        }

        private int Error(string message)
        {
            Errors.Add(new Tuple<int, string>(position, message));
            return Errors.Count;
        }

        private void ClearError(int count)
        {
            Errors.RemoveRange(count, Errors.Count - count);
        }

        public string GetErrorMessages()
        {
            StringBuilder text = new StringBuilder();
            foreach (Tuple<int, string> msg in Errors)
            {
                text.Append(Input.FormErrorMessage(msg.Item1, msg.Item2));
                text.AppendLine();
            }
            return text.ToString();
        }
    }
}
