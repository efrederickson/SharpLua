using System;
using System.Collections.Generic;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.Core;
using ICSharpCode.NRefactory;
using ICSharpCode.NRefactory.Visitors;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Editor;
using ICSharpCode.SharpDevelop.Editor.CodeCompletion;
using System.Windows.Forms;
using SharpLua;
using System.Linq;
using System.Collections;

namespace SharpLuaAddIn
{
    public class CodeCompletion : DefaultCodeCompletionBinding
    {
        bool reset = true;

        public CodeCompletion()
        {
            AddDefaultIntellisense();
        }

        void AddDefaultIntellisense()
        {
            if (reset)
            {
                l.items = new List<ICompletionItem>
                {
                    new CompletionItem("__gc"),
                    new CompletionItem("__tostring"),
                    new CompletionItem("__add"),
                    new CompletionItem("__sub"),
                    new CompletionItem("__mul"),
                    new CompletionItem("__div"),
                    new CompletionItem("__mod"),
                    new CompletionItem("__pow"),
                    new CompletionItem("__unm"),
                    new CompletionItem("__rshift"),
                    new CompletionItem("__lshift"),
                    new CompletionItem("__bitand"),
                    new CompletionItem("__bitor"),
                    new CompletionItem("__len"),
                    new CompletionItem("__eq"),
                    new CompletionItem("__lt"),
                    new CompletionItem("__le"),
                    new CompletionItem("__call"),
                    new CompletionItem("__concat"),
                    new CompletionItem("__type"),
                    new CompletionItem("__index"),
                    new CompletionItem("__newindex"),
                    
                    // Used a script to generate this. Just run that script and 
                    // then copy/paste the output into here, replacing the current stuff.

new CompletionItem("string"), 
new CompletionItem("split"), 
new CompletionItem("match"), 
new CompletionItem("gmatch"), 
new CompletionItem("upper"), 
new CompletionItem("gsub"), 
new CompletionItem("format"), 
new CompletionItem("lower"), 
new CompletionItem("sub"), 
new CompletionItem("gfind"), 
new CompletionItem("find"), 
new CompletionItem("char"), 
new CompletionItem("dump"), 
new CompletionItem("undump"), 
new CompletionItem("reverse"), 
new CompletionItem("byte"), 
new CompletionItem("strmatch"), 
new CompletionItem("len"), 
new CompletionItem("rep"), 

new CompletionItem("xpcall"), 
new CompletionItem("package"), 
new CompletionItem("preload"), 
new CompletionItem("loadlib"), 
new CompletionItem("loaded"), 
new CompletionItem("loaders"), 
new CompletionItem("cpath"), 
new CompletionItem("config"), 
new CompletionItem("path"), 
new CompletionItem("seeall"), 

new CompletionItem("tostring"), 
new CompletionItem("print"), 
new CompletionItem("os"), 
new CompletionItem("exit"), 
new CompletionItem("setlocale"), 
new CompletionItem("date"), 
new CompletionItem("getenv"), 
new CompletionItem("difftime"), 
new CompletionItem("remove"), 
new CompletionItem("time"), 
new CompletionItem("clock"), 
new CompletionItem("tmpname"), 
new CompletionItem("rename"), 
new CompletionItem("execute"), 

new CompletionItem("unpack"), 
new CompletionItem("scanned"), 

new CompletionItem("set_global_mt"), 
new CompletionItem("require"), 
new CompletionItem("getfenv"), 
new CompletionItem("DEBUG"), 
new CompletionItem("setmetatable"), 
new CompletionItem("next"), 
new CompletionItem("luanet"), 
new CompletionItem("assert"), 
new CompletionItem("clr"), 
new CompletionItem("call"), 
new CompletionItem("create"), 
new CompletionItem("ns"), 
new CompletionItem("usingns"), 
new CompletionItem("getns"), 
new CompletionItem("load"), 
new CompletionItem("import"), 

new CompletionItem("tonumber"), 
new CompletionItem("newproxy"), 
new CompletionItem("io"), 
new CompletionItem("lines"), 
new CompletionItem("write"), 
new CompletionItem("close"), 
new CompletionItem("flush"), 
new CompletionItem("open"), 
new CompletionItem("output"), 
new CompletionItem("type"), 
new CompletionItem("read"), 
new CompletionItem("stderr"), 
new CompletionItem("stdin"), 
new CompletionItem("input"), 
new CompletionItem("stdout"), 
new CompletionItem("popen"), 
new CompletionItem("tmpfile"), 

new CompletionItem("rawequal"), 
new CompletionItem("Lua"), 
new CompletionItem("CLR"), 
new CompletionItem("Clr"), 
new CompletionItem("Parser"), 

new CompletionItem("collectgarbage"), 
new CompletionItem("getmetatable"), 
new CompletionItem("set"), 
new CompletionItem("sleep"), 
new CompletionItem("rawtype"), 
new CompletionItem("System"), 

new CompletionItem("_WORKDIR"), 
new CompletionItem("rawset"), 
new CompletionItem("bit32"), 
new CompletionItem("band"), 
new CompletionItem("extract"), 
new CompletionItem("bor"), 
new CompletionItem("bnot"), 
new CompletionItem("arshift"), 
new CompletionItem("rshift"), 
new CompletionItem("rrotate"), 
new CompletionItem("replace"), 
new CompletionItem("lshift"), 
new CompletionItem("lrotate"), 
new CompletionItem("btest"), 
new CompletionItem("bxor"), 

new CompletionItem("bit"), 

new CompletionItem("debug"), 
new CompletionItem("getupvalue"), 
new CompletionItem("sethook"), 
new CompletionItem("gethook"), 
new CompletionItem("setlocal"), 
new CompletionItem("traceback"), 
new CompletionItem("setfenv"), 
new CompletionItem("getinfo"), 
new CompletionItem("setupvalue"), 
new CompletionItem("getlocal"), 
new CompletionItem("getregistry"), 

new CompletionItem("module"), 
new CompletionItem("table"), 
new CompletionItem("getn"), 
new CompletionItem("cat"), 
new CompletionItem("icat"), 
new CompletionItem("isub"), 
new CompletionItem("transpose"), 
new CompletionItem("iall"), 
new CompletionItem("izip"), 
new CompletionItem("maxn"), 
new CompletionItem("concat"), 
new CompletionItem("irev"), 
new CompletionItem("shallow_copy"), 
new CompletionItem("iforeach"), 
new CompletionItem("tolookup"), 
new CompletionItem("foreachi"), 
new CompletionItem("foreach"), 
new CompletionItem("sort"), 
new CompletionItem("ifold"), 
new CompletionItem("invert"), 
new CompletionItem("ifilter"), 
new CompletionItem("iany"), 
new CompletionItem("pack"), 
new CompletionItem("imap"), 
new CompletionItem("deep_copy"), 
new CompletionItem("range"), 
new CompletionItem("override"), 
new CompletionItem("setn"), 
new CompletionItem("removeitem"), 
new CompletionItem("insert"), 
new CompletionItem("iflatten"), 

new CompletionItem("math"), 
new CompletionItem("sinh"), 
new CompletionItem("log"), 
new CompletionItem("max"), 
new CompletionItem("acos"), 
new CompletionItem("huge"), 
new CompletionItem("ldexp"), 
new CompletionItem("pi"), 
new CompletionItem("cos"), 
new CompletionItem("tanh"), 
new CompletionItem("pow"), 
new CompletionItem("deg"), 
new CompletionItem("tan"), 
new CompletionItem("cosh"), 
new CompletionItem("round"), 
new CompletionItem("random"), 
new CompletionItem("randomseed"), 
new CompletionItem("frexp"), 
new CompletionItem("ceil"), 
new CompletionItem("floor"), 
new CompletionItem("rad"), 
new CompletionItem("abs"), 
new CompletionItem("sqrt"), 
new CompletionItem("modf"), 
new CompletionItem("asin"), 
new CompletionItem("min"), 
new CompletionItem("mod"), 
new CompletionItem("fmod"), 
new CompletionItem("log10"), 
new CompletionItem("atan2"), 
new CompletionItem("exp"), 
new CompletionItem("sin"), 
new CompletionItem("atan"), 

new CompletionItem("coroutine"), 
new CompletionItem("resume"), 
new CompletionItem("yield"), 
new CompletionItem("status"), 
new CompletionItem("wrap"), 
new CompletionItem("running"), 

new CompletionItem("pcall"), 
new CompletionItem("SharpLua"), 

new CompletionItem("pairs"), 
new CompletionItem("ipairs"), 
new CompletionItem("_G"), 
new CompletionItem("select"), 
new CompletionItem("gcinfo"), 
new CompletionItem("rawget"), 
new CompletionItem("loadstring"), 
new CompletionItem("_VERSION"), 
new CompletionItem("wait"), 
new CompletionItem("dofile"), 
new CompletionItem("error"), 
new CompletionItem("loadfile"), 

                };
                reset = false;
            }
        }

        static string textFrom(ITextEditor e, int s, int l)
        {
            try
            {
                if (e.Document.TextLength > Math.Abs(s))
                    return e.Document.Text.Substring(e.Caret.Offset - s, l);
                else
                    return "";
            }
            catch { }
            return "";
        }

        public override CodeCompletionKeyPressResult HandleKeyPress(ITextEditor editor, char ch)
        {
            if (reset) // don't call the function unless necessary. It keeps the if check also though.
                AddDefaultIntellisense();
            updateDocument(editor.Document.Text);

            bool isOk = string.IsNullOrWhiteSpace(textFrom(editor, 1, 1));
            if (isOk || (textFrom(editor, 1, 1).Length > 0 && char.IsLetter(textFrom(editor, 1, 1)[0]) == false && char.IsNumber(textFrom(editor, 1, 1)[0]) == false))
            {
                // syntax based upon key pressed. It will build up though...

                switch (ch)
                {
                    case 'f':
                        l.Items.Add(new CompletionItem("function"));
                        l.Items.Add(new CompletionItem("false"));
                        l.Items.Add(new CompletionItem("for"));
                        l.Items.Add(new CompletionItem("function"));
                        break;
                    case 'a':
                        l.Items.Add(new CompletionItem("and"));
                        break;
                    case 'b':
                        l.Items.Add(new CompletionItem("break"));
                        break;
                    case 'e':
                        l.Items.Add(new CompletionItem("else"));
                        l.Items.Add(new CompletionItem("elseif"));
                        l.Items.Add(new CompletionItem("end"));
                        break;
                    case 'd':
                        l.Items.Add(new CompletionItem("do"));
                        break;
                    case 'i':
                        l.Items.Add(new CompletionItem("if"));
                        l.Items.Add(new CompletionItem("in"));
                        break;
                    case 'l':
                        l.Items.Add(new CompletionItem("local"));
                        break;
                    case 'n':
                        l.Items.Add(new CompletionItem("nil"));
                        l.Items.Add(new CompletionItem("not"));
                        break;
                    case 'o':
                        l.Items.Add(new CompletionItem("or"));
                        break;
                    case 'r':
                        l.Items.Add(new CompletionItem("repeat"));
                        l.Items.Add(new CompletionItem("return"));
                        break;
                    case 't':
                        l.Items.Add(new CompletionItem("then"));
                        l.Items.Add(new CompletionItem("true"));
                        break;
                    case 'u':
                        l.Items.Add(new CompletionItem("until"));
                        l.Items.Add(new CompletionItem("using"));
                        break;
                    case 'w':
                        l.Items.Add(new CompletionItem("while"));
                        break;
                    default:
                        break;
                }
            }

            if (ch != '\r' &&
                ch != '\n' &&
                ch != ' ' &&
                ch != ')' &&
                ch != ']' &&
                ch != '}'
                )
            {
                if (l.items.Count > 0)
                    showCompletionWindow(editor);
            }

            return base.HandleKeyPress(editor, ch);
        }

        private void removeDuplicates(ref List<ICompletionItem> list)
        {
            // I had to use a custom duplicate removing algorithm.
            // Why? .Distinct() wasn't working right. This is.
            List<ICompletionItem> ret = new List<ICompletionItem>();
            Hashtable h = new Hashtable();
            foreach (ICompletionItem i in list)
            {
                if (h.ContainsKey(i.Text) == false)
                {
                    h.Add(i.Text, true);
                    ret.Add(i);
                }
            }
            list = ret;

            //list = list.Distinct().ToList();
            //list.Sort();

            if (false)
                foreach (ICompletionItem i in list)
                    LoggingService.Info(i.Text);
        }

        private void updateDocument(string s)
        {
            try
            {
                Lexer l = new Lexer();
                Parser p = new Parser(l.Lex(s));
                p.ThrowParsingErrors = false;
                SharpLua.Ast.Chunk c = p.Parse();
                this.l.items.AddRange(AstExtractor.ExtractSymbols(c));
            }
            catch (System.Exception ex)
            {
                LoggingService.Warn("Error parsing document");
            }
        }

        public override bool CtrlSpace(ITextEditor editor)
        {
            updateDocument(editor.Document.Text);
            showCompletionWindow(editor);
            return true;
            //return base.CtrlSpace(editor);
        }

        CompletionList l = new CompletionList();

        public override bool HandleKeyword(ITextEditor editor, string word)
        {
            if (word == "require")
            {
                reset = true;
                l.items.Clear();
                foreach (string s in Common.ListModules())
                    l.Items.Add(new LuaModuleCompletionData(s));
                showCompletionWindow(editor);
                return true;
            }
            else if (word == "load")
            {
                reset = true;
                l.items.Clear();
                string begin = "";
                if (editor.Document.TextLength >= 8)
                    begin = editor.Document.Text.Substring(editor.Caret.Offset - 8, 4);
                if (begin == "clr.")
                {
                    // .NET Assemblies
                    foreach (string s in Common.ListCLRAssemblies())
                        l.Items.Add(new ClrAssemblyCompletionData(s));
                    showCompletionWindow(editor);
                    return true;
                }
                return false;
            }
            return base.HandleKeyword(editor, word);
        }

        void showCompletionWindow(ITextEditor editor)
        {
            removeDuplicates(ref l.items);
            editor.ShowCompletionWindow(l);
        }

    }
}
