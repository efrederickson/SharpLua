/*
 * User: elijah
 * Date: 3/26/2012
 * Time: 4:26 PM
 */
using System;
using System.Collections.Generic;

namespace SharpLua.Parser
{
    /// <summary>
    /// Just a parser exception
    /// </summary>
    public class ParserException : Exception
    {
        public List<Tuple<int, string>> Errors;
        
        public ParserException(List<Tuple<int, string>> errors, string msg)
            : base(msg)
        {
            this.Errors = errors;
        }
        
        public int FirstErrorIndex
        {
            get
            {
                return Errors[0].Item1;
            }
        }
    }
}
