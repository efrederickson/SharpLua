/*
 * User: elijah
 * Date: 10/24/2012
 * Time: 2:02 PM
 * Copyright 2012 LoDC
 */
using System;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.SharpDevelop.Editor.CodeCompletion;

namespace SharpLuaAddIn
{
    public class LuaModuleCompletionData : ICompletionItem
    {
        public LuaModuleCompletionData(string text)
        {
            Text = text;
            Description = "The '" + Text + "' lua module";
        }

        public string Text
        {
            get;
            set;
        }

        public object Content
        {
            get
            {
                return Text;
            }
        }

        public double Priority
        {
            get
            {
                return 0.0;
            }
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public string Description
        {
            get;
            set;
        }

        ICSharpCode.SharpDevelop.IImage ICompletionItem.Image
        {
            get
            {
                return new ICSharpCode.SharpDevelop.ResourceServiceImage("SharpLuaAddIn.Resources.Field.png");
                //return new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetDirectoryName(typeof(LuaModuleCompletionData).Assembly.Location) + "\\Resources\\Field.png"));
            }
        }

        public void Complete(CompletionContext context)
        {
            context.Editor.Document.Replace(context.StartOffset, context.Length, "\"" + Text + "\"");
        }
    }

    public class ClrAssemblyCompletionData : ICompletionItem
    {
        public ClrAssemblyCompletionData(string text)
        {
            Text = text;
            Description = "'" + Text + "' .NET Assembly";
        }

        public string Text
        {
            get;
            set;
        }

        public object Content
        {
            get
            {
                return Text;
            }
        }

        public double Priority
        {
            get
            {
                return 0.0;
            }
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public string Description
        {
            get;
            set;
        }

        public ICSharpCode.SharpDevelop.IImage Image
        {
            get
            {
                return new ICSharpCode.SharpDevelop.ResourceServiceImage("SharpLuaAddIn.Resources.Field.png");
                //return new System.Windows.Media.Imaging.BitmapImage(new Uri(System.IO.Path.GetDirectoryName(typeof(LuaModuleCompletionData).Assembly.Location) + "\\Resources\\Field.png"));
            }
        }

        public void Complete(CompletionContext context)
        {
            context.Editor.Document.Replace(context.StartOffset, context.Length, "\"" + Text + "\"");
        }
    }

    public class CompletionItem : ICompletionItem
    {
        public CompletionItem(string text)
        {
            Text = text;
            //Description = DocumentationManager.GetDocumentation(text);
            Image = null;
        }

        public string Text
        {
            get;
            set;
        }

        public object Content
        {
            get
            {
                return Text;
            }
        }

        public double Priority
        {
            get
            {
                return 0.0;
            }
        }

        public void Complete(ICSharpCode.AvalonEdit.Editing.TextArea textArea, ICSharpCode.AvalonEdit.Document.ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            textArea.Document.Replace(completionSegment, Text);
        }

        public string Description
        {
            get
            {
                return DocumentationManager.GetDocumentation(Text);
            }
        }

        public ICSharpCode.SharpDevelop.IImage Image
        {
            get;
            set;
        }

        public void Complete(CompletionContext context)
        {
            context.Editor.Document.Replace(context.StartOffset, context.Length, Text);
        }
    }
}
