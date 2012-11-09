/*
 * User: elijah
 * Date: 10/24/2012
 * Time: 3:14 PM
 * Copyright 2012 LoDC
 */
using System;
using ICSharpCode.SharpDevelop;

namespace SharpLuaAddIn
{
    /// <summary>
    /// Description of LuaLanguageBinding.
    /// </summary>
    public class SharpLuaLanguageBinding : DefaultLanguageBinding, ILanguageBinding
    {
        public SharpLuaLanguageBinding()
        {
        }

        public override ICSharpCode.SharpDevelop.Editor.IFormattingStrategy FormattingStrategy
        {
            get
            {
                return new SharpLuaFormattingStrategy();
            }
        }

        public override ICSharpCode.SharpDevelop.Editor.IBracketSearcher BracketSearcher
        {
            get
            {
                return new BracketSearcher();
                //return base.BracketSearcher;
            }
        }

        public override ICSharpCode.SharpDevelop.Dom.LanguageProperties Properties
        {
            get
            {
                return base.Properties;
            }
        }

    }
}
