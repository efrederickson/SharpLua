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

namespace SharpLuaAddIn
{
    public class CodeCompletion : DefaultCodeCompletionBinding
    {
        public CodeCompletion()
        {
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
                if (l.items.Count > 0)
                    showCompletionWindow(editor);
            }

            return base.HandleKeyPress(editor, ch);
        }

        private void removeDuplicates(ref List<ICompletionItem> list)
        {
            list = list.Distinct().ToList();

            List<ICompletionItem> ret = new List<ICompletionItem>();
            HashSet<ICompletionItem> passedValues = new HashSet<ICompletionItem>();

            //relatively simple dupe check alg used as example
            foreach (ICompletionItem item in list)
            {
                if (passedValues.Contains(item))
                    continue;
                else
                {
                    passedValues.Add(item);
                    ret.Add(item);
                }
            }
            list = ret;
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
            l.Items.Clear();
            return base.CtrlSpace(editor);
        }

        CompletionList l = new CompletionList();

        public override bool HandleKeyword(ITextEditor editor, string word)
        {
            l.Items.Clear();
            if (word == "require")
            {
                foreach (string s in Common.ListModules())
                    l.Items.Add(new LuaModuleCompletionData(s));
                showCompletionWindow(editor);
                return true;
            }
            else if (word == "load")
            {
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

            l.SuggestedItem = null;
            editor.ShowCompletionWindow(l);
        }

    }
}
