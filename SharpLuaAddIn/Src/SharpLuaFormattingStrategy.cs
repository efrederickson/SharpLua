using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ICSharpCode.AvalonEdit.Indentation.CSharp;
using ICSharpCode.SharpDevelop;
using ICSharpCode.SharpDevelop.Dom;
using ICSharpCode.SharpDevelop.Editor;
using SharpLua;
using ICSharpCode.Core;

namespace SharpLuaAddIn
{
    /// <summary>
    /// This class handles the auto and smart indenting in the text buffer while
    /// you type.
    /// </summary>
    public class SharpLuaFormattingStrategy : DefaultFormattingStrategy
    {
        #region Smart Indentation
        public override void IndentLine(ITextEditor editor, IDocumentLine line)
        {
            IDocumentLine above = editor.Document.GetLine(line.LineNumber - 1);
            if (above != null)
            {
                string up = above.Text.Trim();
                if (up.StartsWith("--") == false)
                {
                    // above line is an indent statement
                    if (up.EndsWith("do") || up.EndsWith("then") || (up.StartsWith("function") && up.EndsWith(")")))
                    {
                        string indentation = DocumentUtilitites.GetWhitespaceAfter(editor.Document, above.Offset);
                        string newLine = line.Text.TrimStart();
                        newLine = indentation + editor.Options.IndentationString + newLine;
                        editor.Document.SmartReplaceLine(line, newLine);
                    }
                    else // above line is not an indent statement
                    {
                        string indentation = DocumentUtilitites.GetWhitespaceAfter(editor.Document, above.Offset);
                        string newLine = line.Text.TrimStart();
                        newLine = indentation + newLine;
                        editor.Document.SmartReplaceLine(line, newLine);
                    }
                }

                if (line.Text.StartsWith("end"))
                {
                    string indentation = DocumentUtilitites.GetWhitespaceAfter(editor.Document, above.Offset);
                    string newLine = line.Text.TrimStart();
                    string newIndent = "";

                    if (indentation.Length >= editor.Options.IndentationSize)
                        newIndent = indentation.Substring(0, indentation.Length - editor.Options.IndentationSize);

                    newLine = newIndent + newLine;
                    editor.Document.SmartReplaceLine(line, newLine);
                }
            }
            else
                base.IndentLine(editor, line);
        }

        public override void IndentLines(ITextEditor editor, int beginLine, int endLine)
        {
            //DocumentAccessor acc = new DocumentAccessor(editor.Document, beginLine, endLine);
            //CSharpIndentationStrategy indentStrategy = new CSharpIndentationStrategy();
            //indentStrategy.IndentationString = editor.Options.IndentationString;
            //indentStrategy.Indent(acc, true);
            try
            {
                Lexer l = new Lexer();
                Parser p = new Parser(l.Lex(editor.Document.Text));
                SharpLua.Ast.Chunk c = p.Parse();
                SharpLua.Visitors.Beautifier b = new SharpLua.Visitors.Beautifier();
                b.options.Tab = editor.Options.IndentationString;
                b.options.TabsToSpaces = editor.Options.ConvertTabsToSpaces;
                editor.Document.Text = b.Beautify(c);
            }
            catch (System.Exception ex)
            {
                LoggingService.Error("Error parsing document:", ex);
                // probably parse exception
            }
        }
        #endregion

        #region Private functions

        bool IsInsideStringOrComment(ITextEditor textArea, IDocumentLine curLine, int cursorOffset)
        {
            // scan cur line if it is inside a string or single line comment (--)
            bool insideString = false;
            char stringstart = ' ';
            //bool verbatim = false; // true if the current string is verbatim (@-string)
            char c = ' ';
            char lastchar;

            for (int i = curLine.Offset; i < cursorOffset; ++i)
            {
                lastchar = c;
                c = textArea.Document.GetCharAt(i);
                if (insideString)
                {
                    if (c == stringstart)
                    {
                        //if (verbatim && i + 1 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '"')
                        //{
                        //    ++i; // skip escaped character
                        //}
                        //else
                        //{
                        insideString = false;
                        //}
                    }
                    //else if (c == '\\' && !verbatim)
                    //{
                    //    ++i; // skip escaped character
                    //}
                }
                else if (c == '/' && i + 1 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '/')
                {
                    return true;
                }
                else if (c == '"' || c == '\'')
                {
                    stringstart = c;
                    insideString = true;
                    //verbatim = (c == '"') && (lastchar == '@');
                }
            }

            return insideString;
        }

        bool IsInsideDocumentationComment(ITextEditor textArea, IDocumentLine curLine, int cursorOffset)
        {
            for (int i = curLine.Offset; i < cursorOffset; ++i)
            {
                char ch = textArea.Document.GetCharAt(i);
                if (ch == '"')
                {
                    // parsing strings correctly is too complicated (see above),
                    // but I don't now any case where a doc comment is after a string...
                    return false;
                }
                if (ch == '/' && i + 2 < cursorOffset && textArea.Document.GetCharAt(i + 1) == '/' && textArea.Document.GetCharAt(i + 2) == '/')
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        #region FormatLine

        public override void FormatLine(ITextEditor textArea, char ch) // used for comment tag formater/inserter
        {
            using (textArea.Document.OpenUndoGroup())
            {
                FormatLineInternal(textArea, textArea.Caret.Line, textArea.Caret.Offset, ch);
            }
        }

        void FormatLineInternal(ITextEditor textArea, int lineNr, int cursorOffset, char ch)
        {
            IDocumentLine curLine = textArea.Document.GetLine(lineNr);
            IDocumentLine lineAbove = lineNr > 1 ? textArea.Document.GetLine(lineNr - 1) : null;
            string terminator = DocumentUtilitites.GetLineTerminator(textArea.Document, lineNr);

            string curLineText;
            //// local string for curLine segment
            if (ch == '-')
            {
                curLineText = curLine.Text;
                string lineAboveText = lineAbove == null ? "" : lineAbove.Text;
                if (curLineText != null && curLineText.EndsWith("---") && (lineAboveText == null || !lineAboveText.Trim().StartsWith("---")))
                {
                    string indentation = DocumentUtilitites.GetWhitespaceAfter(textArea.Document, curLine.Offset);
                    StringBuilder sb = new StringBuilder();
                    sb.Append(" <summary>");
                    sb.Append(terminator);
                    sb.Append(indentation);
                    sb.Append("--- ");
                    sb.Append(terminator);
                    sb.Append(indentation);
                    sb.Append("--- </summary>");

                    //sb.Append(terminator);
                    //sb.Append(indentation);
                    //sb.Append("--- <returns></returns>");
                    textArea.Document.Insert(cursorOffset, sb.ToString());

                    textArea.Caret.Offset = cursorOffset + indentation.Length + "--- ".Length + " <summary>".Length + terminator.Length;
                }
                else if (curLineText != null && curLineText.Trim() == "-" && (lineAboveText != null && lineAboveText.Trim().StartsWith("---")))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append("-- ");

                    //sb.Append(terminator);
                    //sb.Append(indentation);
                    //sb.Append("--- <returns></returns>");
                    textArea.Document.Insert(cursorOffset, sb.ToString());

                    textArea.Caret.Offset = cursorOffset + "-- ".Length;
                }
                return;
            }

            if (ch != '\n' && ch != '>')
            {
                if (IsInsideStringOrComment(textArea, curLine, cursorOffset))
                {
                    return;
                }
            }

            if (IsInsideStringOrComment(textArea, curLine, cursorOffset) == false
                && textArea.Caret.Offset == curLine.EndOffset // end of line, not editing something inside the line
                && (curLine.Text.TrimEnd().EndsWith("then")
                || curLine.Text.TrimEnd().EndsWith("do")))
            {
                string indentation = DocumentUtilitites.GetWhitespaceAfter(textArea.Document, curLine.Offset);
                StringBuilder sb = new StringBuilder();
                sb.Append(terminator);
                sb.Append(indentation + textArea.Options.IndentationString);
                sb.Append(terminator);
                sb.Append(indentation);
                sb.Append("end");
                textArea.Document.Insert(cursorOffset, sb.ToString());
                textArea.Caret.Offset =
                    cursorOffset
                    + terminator.Length // end of line
                    + indentation.Length // indentation
                    + textArea.Options.IndentationString.Length
                    ;
            }

            if (IsInsideStringOrComment(textArea, curLine, cursorOffset) == false
                && textArea.Caret.Offset == curLine.EndOffset // end of line, not editing something inside the line
                && curLine.Text.TrimStart().StartsWith("repeat"))
            {
                string indentation = DocumentUtilitites.GetWhitespaceAfter(textArea.Document, curLine.Offset);
                StringBuilder sb = new StringBuilder();
                sb.Append(terminator);
                sb.Append(indentation + textArea.Options.IndentationString);
                sb.Append(terminator);
                sb.Append(indentation);
                sb.Append("until ");
                textArea.Document.Insert(cursorOffset, sb.ToString());

                textArea.Caret.Offset =
                    cursorOffset
                    + indentation.Length
                    + terminator.Length // line 1
                    + indentation.Length
                    + textArea.Options.IndentationString.Length
                    + terminator.Length // line 2
                    + "until ".Length
                    ;

            }

            switch (ch)
            {
                case '>':
                    if (IsInsideDocumentationComment(textArea, curLine, cursorOffset))
                    {
                        curLineText = curLine.Text;
                        int column = cursorOffset - curLine.Offset;
                        int index = Math.Min(column - 1, curLineText.Length - 1);

                        while (index >= 0 && curLineText[index] != '<')
                        {
                            --index;
                            if (curLineText[index] == '/')
                                return; // the tag was an end tag or already
                        }

                        if (index > 0)
                        {
                            StringBuilder commentBuilder = new StringBuilder("");
                            for (int i = index; i < curLineText.Length && i < column && !Char.IsWhiteSpace(curLineText[i]); ++i)
                            {
                                commentBuilder.Append(curLineText[i]);
                            }
                            string tag = commentBuilder.ToString().Trim();
                            if (!tag.EndsWith(">"))
                            {
                                tag += ">";
                            }
                            if (!tag.StartsWith("/"))
                            {
                                textArea.Document.Insert(cursorOffset, "</" + tag.Substring(1), AnchorMovementType.BeforeInsertion);
                            }
                        }
                    }
                    break;
                case ')':
                    if (curLine.Text.TrimStart().StartsWith("function"))
                    {
                        string indentation = DocumentUtilitites.GetWhitespaceAfter(textArea.Document, curLine.Offset);
                        StringBuilder sb = new StringBuilder();
                        sb.Append(terminator);
                        sb.Append(indentation + textArea.Options.IndentationString);
                        sb.Append(terminator);
                        sb.Append(indentation);
                        sb.Append("end");
                        textArea.Document.Insert(cursorOffset, sb.ToString());
                        textArea.Caret.Offset =
                            cursorOffset
                            + terminator.Length // end of line
                            + indentation.Length // indentation
                            + textArea.Options.IndentationString.Length
                            ;
                    }
                    else
                        IndentLine(textArea, curLine);
                    break;
                case ':':
                case ']':
                case '}':
                case '{':
                    IndentLine(textArea, curLine);
                    break;
                case '\n':
                    string lineAboveText = lineAbove == null ? "" : lineAbove.Text;
                    //// curLine might have some text which should be added to indentation
                    curLineText = curLine.Text;


                    ISyntaxHighlighter highlighter = textArea.GetService(typeof(ISyntaxHighlighter)) as ISyntaxHighlighter;
                    bool isInMultilineComment = false;
                    bool isInMultilineString = false;
                    if (highlighter != null && lineAbove != null)
                    {
                        var spanStack = highlighter.GetSpanColorNamesFromLineStart(lineNr);
                        isInMultilineComment = spanStack.Contains(SyntaxHighligherKnownSpanNames.Comment);
                        isInMultilineString = spanStack.Contains(SyntaxHighligherKnownSpanNames.String);
                    }
                    bool isInNormalCode = !(isInMultilineComment || isInMultilineString);

                    if (lineAbove != null && isInMultilineComment)
                    {
                        string lineAboveTextTrimmed = lineAboveText.TrimStart();
                        if (lineAboveTextTrimmed.StartsWith("--[[", StringComparison.Ordinal))
                        {
                            textArea.Document.Insert(cursorOffset, " - ");
                            return;
                        }

                        if (lineAboveTextTrimmed.StartsWith("-", StringComparison.Ordinal))
                        {
                            textArea.Document.Insert(cursorOffset, "- ");
                            return;
                        }
                    }

                    if (lineAbove != null && isInNormalCode)
                    {
                        IDocumentLine nextLine = lineNr + 1 <= textArea.Document.TotalNumberOfLines ? textArea.Document.GetLine(lineNr + 1) : null;
                        string nextLineText = (nextLine != null) ? nextLine.Text : "";

                        int indexAbove = lineAboveText.IndexOf("---");
                        int indexNext = nextLineText.IndexOf("---");
                        if (indexAbove > 0 && (indexNext != -1 || indexAbove + 4 < lineAbove.Length))
                        {
                            textArea.Document.Insert(cursorOffset, "--- ");
                            return;
                        }

                        if (IsInNonVerbatimString(lineAboveText, curLineText))
                        {
                            textArea.Document.Insert(cursorOffset, "\"");
                            textArea.Document.Insert(lineAbove.Offset + lineAbove.Length,
                                                     "\" +");
                        }
                    }
                    return;
            }

        }

        /// <summary>
        /// Checks if the cursor is inside a non-verbatim string.
        /// This method is used to check if a line break was inserted in a string.
        /// The text editor has already broken the line for us, so we just need to check
        /// the two lines.
        /// </summary>
        /// <param name="start">The part before the line break</param>
        /// <param name="end">The part after the line break</param>
        /// <returns>
        /// True, when the line break was inside a non-verbatim-string, so when
        /// start does not contain a comment, but a non-even number of ", and
        /// end contains a non-even number of " before the first comment.
        /// </returns>
        bool IsInNonVerbatimString(string start, string end)
        {
            bool inString = false;
            for (int i = 0; i < start.Length; ++i)
            {
                char c = start[i];
                if (c == '"')
                {
                    inString = !inString;
                }
                if (!inString && i > 0
                    && start[i - 1] == '-'
                    && c == '-')
                    return false;
                if (inString && start[i] == '\\')
                    ++i;
            }
            if (!inString)
                return false;
            // we are possibly in a string, or a multiline string has just ended here
            // check if the closing double quote is in end
            for (int i = 0; i < end.Length; ++i)
            {
                char c = end[i];
                if (c == '"')
                {
                    if (!inString && i > 0 && end[i - 1] == '@')
                        break; // no string line break for verbatim strings
                    inString = !inString;
                }
                if (!inString && i > 0 && end[i - 1] == '/' && (c == '/' || c == '*'))
                    break;
                if (inString && end[i] == '\\')
                    ++i;
            }
            // return true if the string was closed properly
            return !inString;
        }
        #endregion

        public override void SurroundSelectionWithComment(ITextEditor editor)
        {
            if (editor.SelectedText.IndexOf("\n") != -1)
            {
                // TODO: multi line comment
                SurroundSelectionWithSingleLineComment(editor, "--");
            }
            else
                SurroundSelectionWithSingleLineComment(editor, "--");
        }
    }
}
