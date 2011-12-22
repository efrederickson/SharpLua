using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    public static class WinFormLib
    {
        static LuaTable currentModule;

        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("WinForms", module);
            module.SetNameValue("_G", enviroment);
            currentModule = module;
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("Run", RunApp);
            module.Register("ShowMessage", ShowMessage);
            module.Register("MessageBox", ShowMessage);
            module.Register("msgbox", ShowMessage);
            LuaTable metaTable = new LuaTable();
            metaTable.Register("__index", GetControlCreator);
            module.MetaTable = metaTable;
        }

        public static LuaValue RunApp(LuaValue[] values)
        {
            LuaUserdata data = values[0] as LuaUserdata;
            Form form = (Form)data.Value;
            form.PerformLayout();
            Application.Run(form);
            return null;
        }

        public static LuaValue ShowMessage(LuaValue[] values)
        {
            LuaString messageString = values[0] as LuaString;
            LuaString captionString = values.Length > 1 ? values[1] as LuaString : null;
            if (captionString == null)
            {
                MessageBox.Show(messageString.Text);
            }
            else
            {
                MessageBox.Show(messageString.Text, captionString.Text);
            }
            return null;
        }

        public static LuaValue GetControlCreator(LuaValue[] values)
        {
            LuaString typeString = values[0] as LuaString;
            string typeName = "System.Windows.Forms." + typeString.Text;
            Type type = Assembly.GetAssembly(typeof(Application)).GetType(typeName);

            LuaFunction func = new LuaFunction((LuaValue[] args) =>
            {
                object control = Activator.CreateInstance(type);
                LuaTable table = args[0] as LuaTable;
                string name = null;

                if (table.Length > 0)
                {
                    AddChildControls(control, table);
                }

                if (table.Count > 0)
                {
                    foreach (var pair in table.KeyValuePairs)
                    {
                        string member = (pair.Key as LuaString).Text;

                        if (member == "Name")
                        {
                            name = (string)pair.Value.Value;
                            continue;
                        }

                        SetMemberValue(control, type, member, pair.Value.Value);
                    }
                }

                LuaUserdata data = new LuaUserdata(control);
                data.MetaTable = GetControlMetaTable();

                if (name != null)
                {
                    LuaTable enviroment = currentModule.GetValue("_G") as LuaTable;
                    enviroment.SetNameValue(name, data);
                }

                return data;
            }
            );

            currentModule.SetNameValue(typeString.Text, func);
            return func;
        }

        static LuaTable controlMetaTable;
        private static LuaTable GetControlMetaTable()
        {
            if (controlMetaTable == null)
            {
                controlMetaTable = new LuaTable();

                controlMetaTable.SetNameValue("__index", new LuaFunction((values) =>
                    {
                        LuaUserdata control = values[0] as LuaUserdata;
                        Type type = control.Value.GetType();

                        LuaString member = values[1] as LuaString;
                        if (member != null)
                        {
                            return GetMemberValue(control.Value, type, member.Text);
                        }

                        LuaNumber index = values[1] as LuaNumber;
                        if (index != null)
                        {
                            return GetIndexerValue(control.Value, type, index.Number);
                        }

                        return LuaNil.Nil;
                    }));

                controlMetaTable.SetNameValue("__newindex", new LuaFunction((values) =>
                    {
                        LuaUserdata control = values[0] as LuaUserdata;
                        LuaString member = values[1] as LuaString;
                        LuaValue value = values[2];

                        Type type = control.Value.GetType();
                        SetMemberValue(control.Value, type, member.Text, value.Value);
                        return null;
                    }));
            }

            return controlMetaTable;
        }

        private static LuaValue GetMemberValue(object control, Type type, string member)
        {
            PropertyInfo propertyInfo = type.GetProperty(member);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(control, null);
                return ToLuaValue(value);
            }
            else
            {
                return new LuaFunction((args) =>
                    {
                        MemberInfo[] members = type.GetMember(member);

                        if (members.Length == 0)
                        {
                            throw new InvalidOperationException(string.Format("{0} is not defined in {1}", member, type.FullName));
                        }

                        foreach (MemberInfo memberInfo in members)
                        {
                            MethodInfo methodInfo = memberInfo as MethodInfo;
                            if (methodInfo != null)
                            {
                                try
                                {
                                    object result = methodInfo.Invoke(control, args.Select(a => a.Value).ToArray());
                                    return ToLuaValue(result);
                                }
                                catch (TargetParameterCountException)
                                {
                                }
                                catch (ArgumentException)
                                {
                                }
                                catch (MethodAccessException)
                                {
                                }
                                catch (InvalidOperationException)
                                {
                                }
                            }
                        }
                        return LuaNil.Nil;
                    });

                throw new Exception(string.Format("Cannot get {0} from {1}", member, control));
            }
        }

        private static LuaValue GetIndexerValue(object control, Type type, double index)
        {
            MemberInfo[] members = type.GetMember("Item");

            if (members.Length == 0)
            {
                throw new InvalidOperationException(string.Format("Indexer is not defined in {0}", type.FullName));
            }

            foreach (MemberInfo memberInfo in members)
            {
                PropertyInfo propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    try
                    {
                        object result = propertyInfo.GetValue(control, new object[] { (int)index - 1 });
                        return ToLuaValue(result);
                    }
                    catch (TargetParameterCountException)
                    {
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (MethodAccessException)
                    {
                    }
                    catch (TargetInvocationException)
                    {
                    }
                }
            }

            return LuaNil.Nil;
        }

        private static LuaValue ToLuaValue(object value)
        {
            if (value is int || value is double)
            {
                return new LuaNumber(Convert.ToDouble(value));
            }
            else if (value is string)
            {
                return new LuaString((string)value);
            }
            else if (value is bool)
            {
                return LuaBoolean.From((bool)value);
            }
            else if (value == null)
            {
                return LuaNil.Nil;
            }
            else if (value.GetType().IsEnum)
            {
                return new LuaString(value.ToString());
            }
            else if (value.GetType().IsArray)
            {
                LuaTable table = new LuaTable();
                foreach (var item in (value as Array))
                {
                    table.AddValue(ToLuaValue(item));
                }
                return table;
            }
            else if (value is LuaValue)
            {
                return (LuaValue)value;
            }
            else
            {
                LuaUserdata data = new LuaUserdata(value);
                data.MetaTable = GetControlMetaTable();
                return data;
            }
        }

        private static void SetMemberValue(object control, Type type, string member, object value)
        {
            PropertyInfo propertyInfo = type.GetProperty(member);
            if (propertyInfo != null)
            {
                SetPropertyValue(control, value, propertyInfo);
            }
            else
            {
                EventInfo eventInfo = type.GetEvent(member);
                if (eventInfo != null)
                {
                    switch (eventInfo.EventHandlerType.FullName)
                    {
                        case "System.EventHandler":
                            eventInfo.AddEventHandler(control, new EventHandler((sender, e) =>
                            {
                                (value as LuaFunc).Invoke(new LuaValue[] { new LuaUserdata(sender), new LuaUserdata(e) });
                            }));
                            break;
                        case "System.Windows.Forms.TreeViewEventHandler":
                            eventInfo.AddEventHandler(control, new TreeViewEventHandler((sender, e) =>
                            {
                                (value as LuaFunc).Invoke(new LuaValue[] { new LuaUserdata(sender), new LuaUserdata(e) });
                            }));
                            break;
                        default:
                            throw new NotImplementedException(eventInfo.EventHandlerType.FullName + " type not implemented.");
                    }
                }
            }
        }

        private static void AddChildControls(object control, LuaTable table)
        {
            ToolStrip toolStrip = control as ToolStrip;
            if (toolStrip != null)
            {
                foreach (var item in table.ListValues)
                {
                    toolStrip.Items.Add((ToolStripItem)item.Value);
                }
                return;
            }

            SplitContainer splitContainer = control as SplitContainer;
            if (splitContainer != null)
            {
                splitContainer.Panel1.Controls.Add((Control)table.GetValue(1).Value);
                splitContainer.Panel2.Controls.Add((Control)table.GetValue(2).Value);
                return;
            }

            Control container = control as Control;
            foreach (var item in table.ListValues)
            {
                container.Controls.Add((Control)item.Value);
            }
        }

        private static void SetPropertyValue(object obj, object value, PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.FullName == "System.Int32")
            {
                propertyInfo.SetValue(obj, (int)(double)value, null);
            }
            else if (propertyInfo.PropertyType.IsEnum)
            {
                object enumValue = Enum.Parse(propertyInfo.PropertyType, (string)value);
                propertyInfo.SetValue(obj, enumValue, null);
            }
            else if (propertyInfo.PropertyType.FullName == "System.Drawing.Image")
            {
                LuaTable enviroment = currentModule.GetValue("_G") as LuaTable;
                LuaString workDir = enviroment.GetValue("_WORKDIR") as LuaString;
                var image = System.Drawing.Image.FromFile(Path.Combine(workDir.Text, (string)value));
                propertyInfo.SetValue(obj, image, null);
            }
            else
            {
                propertyInfo.SetValue(obj, value, null);
            }
        }
    }
}
