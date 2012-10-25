/*
 * User: elijah
 * Date: 10/24/2012
 * Time: 2:18 PM
 * Copyright 2012 LoDC
 */
using System;
using ICSharpCode.SharpDevelop.Editor.CodeCompletion;

namespace SharpLuaAddIn
{
    using System;
    using System.Collections.Generic;
    
    public class CompletionList : ICompletionItemList
    {
        public List<ICompletionItem> items = new List<ICompletionItem>();
        public List<ICompletionItem> Items
        {
            get
            {
                return this.items;
            }
            set
            {
                items = value;
            }
        }
        public virtual bool ContainsAllAvailableItems
        {
            get
            {
                return true;
            }
        }
        public int PreselectionLength
        {
            get;
            set;
        }
        public ICompletionItem SuggestedItem
        {
            get;
            set;
        }
        IEnumerable<ICompletionItem> ICompletionItemList.Items
        {
            get
            {
                return this.items;
            }
        }
        public bool InsertSpace
        {
            get;
            set;
        }
        public void SortItems()
        {
            this.items.Sort(delegate(ICompletionItem a, ICompletionItem b)
                            {
                                int num = string.Compare(a.Text, b.Text, StringComparison.CurrentCultureIgnoreCase);
                                int result;
                                if (num != 0)
                                {
                                    result = num;
                                }
                                else
                                {
                                    result = string.Compare(a.Text, b.Text, StringComparison.CurrentCulture);
                                }
                                return result;
                            });
        }
        public virtual CompletionItemListKeyResult ProcessInput(char key)
        {
            CompletionItemListKeyResult result;
            if (key == ' ' && this.InsertSpace)
            {
                this.InsertSpace = false;
                result = CompletionItemListKeyResult.BeforeStartKey;
            }
            else
            {
                if (char.IsLetterOrDigit(key) || key == '_')
                {
                    this.InsertSpace = false;
                    result = CompletionItemListKeyResult.NormalKey;
                }
                else
                {
                    result = CompletionItemListKeyResult.InsertionKey;
                }
            }
            return result;
        }
        public virtual void Complete(CompletionContext context, ICompletionItem item)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            checked
            {
                if (this.InsertSpace)
                {
                    this.InsertSpace = false;
                    context.Editor.Document.Insert(context.StartOffset, " ");
                    context.StartOffset++;
                    context.EndOffset++;
                }
                item.Complete(context);
            }
        }
    }
}
