/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/23/2011
 * Time: 12:33 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using SharpLua.LuaTypes;

namespace SharpLua.Library
{
    /// <summary>
    /// lClass in C# with a new module name
    /// </summary>
    public class ClassLib
    {
        public ClassLib()
        {
        }
        
        public static void RegisterModule(LuaTable env)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            env.SetNameValue("class", module);
        }
        
        public static void RegisterFunctions(LuaTable mod)
        {
            
        }
        
        public static LuaValue FindMethod(LuaValue[] args)
        {
            //(method, ...)
            string method = (args[0] as LuaString).Text;
            
            foreach (LuaValue k in args)
            {
                LuaValue m = (k as LuaTable).GetValue(method);
                if (m != null && m.GetTypeCode() == "function")
                    return m;
            }
            return LuaNil.Nil;
        }

        public static LuaValue IsClass(LuaValue[] args)
        {
            LuaTable item = args[0] as LuaTable;
            //arg
            if (item == null)
            {
                return LuaBoolean.False;
            }
            else if (item.GetValue("__class") == LuaBoolean.True ||
                     item.GetValue("__static") != LuaNil.Nil ||
                     item.GetValue("__final") != LuaNil.Nil ||
                     item.GetValue("__super") != LuaNil.Nil ||
                     item.GetValue("__indexfunction") != LuaNil.Nil ||
                     item.GetValue("__newindexfunction") != LuaNil.Nil ||
                     item.GetValue("__callfunction") != LuaNil.Nil ||
                     item.GetValue("__subClasses") != LuaNil.Nil )
            {
                return LuaBoolean.True;
            }
            return LuaBoolean.False;
        }
        
        public static LuaValue IsObject(LuaValue[] args)
        {
            LuaTable item = args[0] as LuaTable;
            if (item.GetValue("GetClass") != null || item.GetValue("GetParentClasses") != null)
                return LuaBoolean.False; // its a class
            else
                return LuaBoolean.True;
        }
        
        public static LuaValue IsObjectOf(LuaValue[] args)
        {
            LuaTable obj = args[0] as LuaTable;
            LuaTable _class = args[1] as LuaTable;
            if (! (IsClass(new LuaValue[] {_class}) as LuaBoolean).BoolValue)
            {
                throw new Exception("item 'class' isn't a valid class!");
            }
            if (obj.GetValue("GetClass") == null)
            {
                throw new Exception("item 'obj' isn't a valid class!");
            }
            LuaTable c = (obj.GetValue("GetClass") as LuaFunction).Invoke(new LuaValue[] {}) as LuaTable;
            return LuaBoolean.From(c == _class);
        }
        
        public static LuaValue IsMemberOf(LuaValue[] args)
        {
            LuaTable _class = args[1] as LuaTable;
            LuaValue obj = args[0];
            if (! (IsClass(new LuaValue[] {_class}) as LuaBoolean).BoolValue)
                throw new Exception("item 'class' isn't a valid class!");
            
            return LuaBoolean.From(_class.GetValue(obj) != LuaNil.Nil);
        }
        
        public static LuaValue SetMembers(LuaValue[] args)
        {
            LuaTable _class = args[0] as LuaTable;
            if ((IsClass(new LuaValue[] {_class}) as LuaBoolean).BoolValue == false)
            {
                throw new Exception("item 'class' isn't a valid class!");
            }
            for (int i = 1; i < args.Length; i++)
                SharpLua.Library.TableLib.Copy(new LuaValue[] {_class, args[i]});
            return _class;
        }
        
        #region ITERATORS ** FIX **
        /*
        public static LuaValue IterateChildClasses(LuaValue[] args)
        {
            LuaTable _class = args[0] as LuaTable;
            if ( (IsClass(new LuaValue[] {_class}) as LuaBoolean).BoolValue = true)
                throw new Exception("item 'class' isn't a valid class!");
            int current = 0;
            int count = (_class.GetValue("__subClasses") as LuaTable).Length;
            return new LuaMultiValue(new LuaValue[] { new LuaFunction(SharpLua.Library.BaseLib.next), _class, LuaNil.Nil });
           
            return function()
                current = current + 1
                if current <= count then
                return _class.__subClasses[current]
        }
        
        
            // parent/super class iterat||
            // iterates over all the parent classes of 'class'
            // (classes which 'class' inherits from
            // e.g.
            // for obj in lclass:IterateParentClasses(classA) do
            //  print(tostring(obj))
            // end
            function lclass:IterateParentClasses(_class)
            if not lclass:IsClass(_class) then
            error("item 'class' isn't a valid class!", 2)
            end
            local current = 0
            local count = #_class.__super
            return function()
            current = current + 1
            if current <= count then
            return _class.__super[current]
            end
            end
            end
         */
        #endregion
        
        // creates and returns a final/not inheritable class
        public static LuaValue CreateFinalClass(LuaValue[] args)
        {
            LuaClass c = CreateClass(args);
            c.Final = true;
            return c;
        }

        // creates and returns a static class
        public static LuaValue CreateStaticClass(LuaValue[] args)
        {
            LuaClass c = CreateClass(args);
            c.Static = true;
            return c;
        }
    }
}
