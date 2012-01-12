/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 1/5/2012
 * Time: 8:46 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using SharpLua.LuaTypes;

namespace SharpLua
{
    /// <summary>
    /// Converts the object to the Lua equivalent (Credit to Arjen Douma for the idea of having it static)
    /// </summary>
    public class ObjectToLua
    {
        
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
                LuaTable enviroment = LuaRuntime.GlobalEnvironment.GetValue("_G") as LuaTable;
                LuaString workDir = enviroment.GetValue("_WORKDIR") as LuaString;
                var image = System.Drawing.Image.FromFile(Path.Combine(workDir.Text, (string)value));
                propertyInfo.SetValue(obj, image, null);
            }
            else
            {
                propertyInfo.SetValue(obj, value, null);
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
        
        private static LuaValue GetMemberValue(object control, Type type, string member)
        {
            PropertyInfo propertyInfo = type.GetProperty(member);
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(control, null);
                return ToLuaValue(value);
            }
            FieldInfo fi = type.GetField(member);
            if (fi != null)
            {
                return ToLuaValue(fi.GetValue(control));
            }
            MethodInfo mi = type.GetMethod(member);
            if (mi != null)
            {
                return new LuaFunction((LuaValue[] args) =>
                                       {
                                           List<object> args2 = new List<object>();
                                           foreach (LuaValue v in args)
                                               args2.Add(v.Value);
                                           object result = mi.Invoke(control, args2.ToArray());
                                           return ToLuaValue(result);
                                       });
            }
            
            throw new Exception(string.Format("Cannot get {0} from {1}", member, control));
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

        public static LuaValue ToLuaValue(object value)
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

        public static LuaTable ToLuaTable(object o)
        {
            LuaTable ret = new LuaTable();
            
            System.Collections.IEnumerable ie = (o as System.Collections.IEnumerable);
            if (ie != null)
            {
                foreach (object obj in ie)
                {
                    ret.AddValue(ToLuaValue(obj));
                }
                return ret;
            }
            // check if Dictionary
            // check if <type>...
            // not an array
            ret.AddValue(ToLuaValue(o));
            return ret;
        }
    }
}
