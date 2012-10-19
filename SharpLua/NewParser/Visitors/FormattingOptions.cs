/*
 * User: elijah
 * Date: 10/19/2012
 * Time: 9:27 AM
 * Copyright 2012 LoDC
 */
using System;

namespace SharpLua.Visitors
{
    /// <summary>
    /// Options for when coverting an Ast back to Code
    /// </summary>
    public class FormattingOptions
    {
        public FormattingOptions()
        {
            EOL = "\r\n";
            Tab = "    ";
            TabsToSpaces = false;
            ConvertNewLines = false;
        }
        
        /// <summary>
        /// The End-Of-Line character(s)
        /// </summary>
        public string EOL { get; set; }
        /// <summary>
        /// The Tab character(s). Four spaces by default
        /// </summary>
        public string Tab { get; set; }
        /// <summary>
        /// Whether to convert Tabs to spaces or not (ExactReconstructor)
        /// </summary>
        public bool TabsToSpaces { get; set; }
        /// <summary>
        /// Whether to convert new lines to the FormattingOptions.EOL (ExactReconstructor)
        /// </summary>
        public bool ConvertNewLines { get; set; }
        
    }
}
