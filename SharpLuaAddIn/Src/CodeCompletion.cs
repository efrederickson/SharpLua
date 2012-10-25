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
                    new CompletionItem("string"),
                    new CompletionItem("table"),
                    new CompletionItem("io"),
                    new CompletionItem("os"),
                    new CompletionItem("bit"),
                    new CompletionItem("bit32"),
                    new CompletionItem("coroutine"),
                    new CompletionItem("math"),
                    new CompletionItem("debug"),
                    new CompletionItem("package"),

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

                    new CompletionItem("_G"),
                    new CompletionItem("_VERSION"),

                    new CompletionItem("ipairs"),
                    new CompletionItem("pairs"),
                    new CompletionItem("assert"),
                    new CompletionItem("collectgarbage"),
                    new CompletionItem("dofile"),
                    new CompletionItem("error"),
                    new CompletionItem("gcinfo"),
                    new CompletionItem("getfenv"),
                    new CompletionItem("setfenv"),
                    new CompletionItem("getmetatable"),
                    new CompletionItem("loadfile"),
                    new CompletionItem("load"),
                    new CompletionItem("loadstring"),
                    new CompletionItem("next"),
                    new CompletionItem("pcall"),
                    new CompletionItem("xpcall"),
                    new CompletionItem("print"),
                    new CompletionItem("rawequal"),
                    new CompletionItem("rawget"),
                    new CompletionItem("rawset"),
                    new CompletionItem("select"),
                    new CompletionItem("setmetatable"),
                    new CompletionItem("tonumber"),
                    new CompletionItem("tostring"),
                    new CompletionItem("type"),
                    new CompletionItem("unpack"),
                    new CompletionItem("wait"),
                    new CompletionItem("sleep"),

                    new CompletionItem("arshift"),
                    new CompletionItem("band"),
                    new CompletionItem("bnot"),
                    new CompletionItem("bor"),
                    new CompletionItem("bxor"),
                    new CompletionItem("btest"),
                    new CompletionItem("extract"),
                    new CompletionItem("lrotate"),
                    new CompletionItem("lshift"),
                    new CompletionItem("replace"),
                    new CompletionItem("rrotate"),
                    new CompletionItem("rshift"),

                    new CompletionItem("getfenv"),
                    new CompletionItem("gethook"),
                    new CompletionItem("getinfo"),
                    new CompletionItem("getlocal"),
                    new CompletionItem("getregistry"),
                    new CompletionItem("getmetatable"),
                    new CompletionItem("getupvalue"),
                    new CompletionItem("setfenv"),
                    new CompletionItem("sethook"),
                    new CompletionItem("setlocal"),
                    new CompletionItem("setmetatable"),
                    new CompletionItem("setupvalue"),
                    new CompletionItem("traceback"),

                    new CompletionItem("close"),
                    new CompletionItem("flush"),
                    new CompletionItem("write"),
                    new CompletionItem("close"),
                    new CompletionItem("input"),
                    new CompletionItem("lines"),
                    new CompletionItem("open"),
                    new CompletionItem("output"),
                    new CompletionItem("popen"),
                    new CompletionItem("tmpfile"),
                    new CompletionItem("setvbuf"),

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
                    new CompletionItem("sinh"),
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
                    new CompletionItem("round"),

                    new CompletionItem("loadlib"),
                    new CompletionItem("seeall"),
                    new CompletionItem("module"),
                    new CompletionItem("require"),

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

new CompletionItem("sub"),
new CompletionItem("upper"),
new CompletionItem("len"),
new CompletionItem("gfind"),
new CompletionItem("rep"),
new CompletionItem("find"),
new CompletionItem("match"),
new CompletionItem("char"),
new CompletionItem("dump"),
new CompletionItem("gmatch"),
new CompletionItem("reverse"),
new CompletionItem("byte"),
new CompletionItem("format"),
new CompletionItem("gsub"),
new CompletionItem("lower"),

new CompletionItem("setn"),
new CompletionItem("insert"),
new CompletionItem("getn"),
new CompletionItem("foreachi"),
new CompletionItem("maxn"),
new CompletionItem("foreach"),
new CompletionItem("concat"),
new CompletionItem("sort"),
new CompletionItem("remove"),

new CompletionItem("index"),
new CompletionItem("get_constructor_bysig"),
new CompletionItem("ctype"),
new CompletionItem("load_assembly"),
new CompletionItem("each"),
new CompletionItem("import_type"),
new CompletionItem("get_object_member"),
new CompletionItem("get_method_bysig"),
new CompletionItem("make_array"),
new CompletionItem("make_object"),
new CompletionItem("free_object"),
new CompletionItem("namespace"),
new CompletionItem("enum"),
new CompletionItem("getmetatable"),
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
            AddDefaultIntellisense();
            updateDocument(editor.Document.Text);

            bool isOk = string.IsNullOrWhiteSpace(textFrom(editor, 1, 1));
            if (isOk || (textFrom(editor, 1, 1).Length > 0 && char.IsLetter(textFrom(editor, 1, 1)[0]) == false && char.IsNumber(textFrom(editor, 1, 1)[0]) == false))
            {
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
                ch != ' ')
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
                this.l.items.AddRange(AstExtractor.Disect(c));
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
