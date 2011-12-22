/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/21/2011
 * Time: 11:02 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SharpLua;
using SharpLua.LuaTypes;

namespace lClass
{
    /// <summary>
    /// Lua class library
    /// </summary>
    public class lClass : SharpLua.Library.ILuaLibrary
    {
        /*
         *************** SEE LCLASS DOCUMENTATION FOR USAGE AND MORE INFO
         *** https://github.com/mlnlover11/LuaLibs/tree/master/lClass/ **
         *************** THIS USES THE MODULE CLASS INSTEAD OF LCLASS ***
        */
        
        public void RegisterModule(LuaTable environment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            environment.SetNameValue("class", module);
        }
        
        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("FindMethod", FindMethod);
            module.Register("IsClass", IsClass);
            module.Register("IsObject", IsObject);
            module.Register("IsObjectOf", IsObjectOf);
            module.Register("IsMemberOf", IsMemberOf);
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
            // f|| obj in lclass:IterateParentClasses(classA) do
            //  print(tostring(obj))
            // end
            function lclass:IterateParentClasses(_class)
            if not lclass:IsClass(_class) then
            err||("item 'class' isn't a valid class!", 2)
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

        private static int _nameCount = 0;
/*
        public static LuaValue CreateClass(LuaValue[] args)
        {
            LuaTable table1 = new LuaTable();
            if ((args[0] as LuaValue).GetTypeCode() == "table" && ((IsClass(new LuaValue[] {arg[0]}) as LuaBoolean).BoolValue == false))
                table1 = args[0] as LuaTable;
            
            LuaTable supers = new LuaTable();
            if (args && args.Length > 0)
            {
                foreach (LuaTable p in args)
                {
                    if ((p.GetValue("__final") as LuaBoolean).BoolValue == true)
                        throw new Exception("Cannot inherit from a final class");
                    else
                        supers.AddValue(p);
                    (p.GetValue("__subClasses") as LuaTable).AddValue(table1);
                }
            }
            table1.SetNameValue("__super", supers);
            table1.SetNameValue("__subClasses", new LuaTable());
            table1.SetNameValue("__indexfunction", new LuaFunction(new LuaFunc(delegate(LuaValue[] args) { return LuaNil.Nil; })));
            table1.SetNameValue("__callfunction", new LuaFunction(new LuaFunc(delegate(LuaValue[] args) { return LuaNil.Nil; })));
            table1.SetNameValue("__newindexfunction", new LuaFunction(new LuaFunc(delegate(LuaValue[] args) { return LuaNil.Nil; })));
            table1.SetNameValue("__tostringfunction", new LuaFunc(delegate(LuaValue[] args) { return new LuaString("lclass: " + table1.GetValue("__name").Value); }));
            table1.__index = table1;
            table1.MetaTable.SetNameValue("__index",new LuaFunc(delegate(LuaValue[] args)
                                                                { //(table, key)
                                                                    LuaTable table = args[0] as LuaTable;
                                                                    LuaValue key = args[1];
                                                                    (table1.GetValue("__indexfunction") as LuaFunction).Invoke(new LuaValue[] {table, key}); // user defined __index function
                                                                    return (table1.GetValue("CallMethod") as LuaFunction).Invoke(new LuaValue[] {key});
                                                                }));

            table1.MetaTable.SetNameValue("__call", new LuaFunc(delegate(LuaValue[] args)
                                                                { //(func, ...)
                                                                    LuaFunction func = args[0] as LuaFunction;
                                                                    List<LuaValue> args2 = new List<LuaValue>();
                                                                    foreach (LuaValue a in args)
                                                                        args2.Add(a);
                                                                    args2.RemoveAt(0);
                                                                    (table1.GetValue("__callfunction") as LuaFunction).Invoke(func, new LuaMultiValue(args2.ToArray()); // user defined __call function
                                                                                                                              func.Invoke(args2.ToArray());; // call function
                                                                    }));

            table1.MetaTable.SetNameValue("__newindex",new LuaFunc(delegate(LuaValue[] args)
                                                                   {
                                                                       LuaTable table = args[0] as LuaTable;
                                                                       LuaValue key = args[1];
                                                                       LuaValue value = args[2];
                                                                       //(table, key, value)
                                                                       (table.GetValue("__newindexfunction") as LuaFunction).Invoke(new LuaValue[] {table, key, value}); // user defined __newindex function
                                                                       // check for user defined "metamethods"
                                                                       string func = key.Value.ToString();
                                                                       if (func == "__index")
                                                                       {
                                                                           // assign this to the __indexfunction variable
                                                                           table.RawSetValue("__indexfunction", value);
                                                                       }
                                                                       else if (func == "__newindex")
                                                                           table.RawSetValue("__newindexfunction", value);
                                                                       else if (func == "__call")
                                                                           table.RawSetValue("__callfunction", value);
                                                                       else if (func == "__tostring")
                                                                           table.RawSetValue("__tostringfunction", value);
                                                                       else
                                                                       {
                                                                           LuaTable h;
                                                                           if (table.GetTypeCode() == "table")
                                                                           {
                                                                               LuaValue v = table.RawGetValue(key);
                                                                               if (v != LuaNil.Nil)
                                                                               {
                                                                                   table.RawSetValue(key, value);
                                                                                   return LuaNil.Nil;
                                                                               }
                                                                               h = table.MetaTable.GetValue("__newindex");
                                                                               if (h == LuaNil.Nil)
                                                                               {
                                                                                   table.RawSetValue(key, value);
                                                                                   return;
                                                                               }
                                                                           }
                                                                           else
                                                                               h = table.MetaTable.GetValue("__newindex");
                                                                           if (h == LuaNil.Nil)
                                                                               throw new Exception("cannot get __newindex field!");
                                                                           if (h.GetTypeCode() == "function")
                                                                               table.RawSetValue(key, value); // was h(table, key, value)
                                                                           else
                                                                               h[key] = value;           // repeat operation on it
                                                                       }
                                                                   }));
            table1.MetaTable.Register("__tostring", new LuaFunc(delegate(LuaValue[] args)
                                                                {
                                                                    return (table1.GetValue("__tostringfunction") as LuaFunction).Invoke(args);
                                                                }));

            table1.MetaTable.Register("__gc", new LuaFunc(delegate(LuaValue[] args)
                                                          {
                                                              if (table1.RawGetValue("Destructor") != LuaNil.Nil)
                                                              {
                                                                  (table1.GetValue("Destructor") as LuaFunction).Invoke(new LuaValue[] {table1});
                                                              }
                                                          }));
            table1.SetNameValue("__static",false);
            table1.SetNameValue("__final", false);
            table1.SetNameValue("__class",true);
            table1.SetNameValue("__name", "CLASS_" + _nameCount);
            _nameCount = _nameCount + 1;

            // returns a new instance of the class
            table1.Register("new", new LuaFunc(delegate(LuaValue[] args)
                                               {
                                                   return CreateInstance(table1);
                                               }));

            // copies table "m" into class
            table1.Register("Set", new LuaFunc(delegate(LuaValue[] args)
                                               {
                                                   return SetMembers(new LuaValue[] {table1, args[0]});
                                               }));

            // finds out whether "m" is a child of the class
            table.Register("HasMember", new LuaFunc(delegate(LuaValue[] args)
                                                    { //(m)
                                                        return IsMemberOf(new LuaValue[] {args[0], table1});
                                                    }));

            // determines whether the class inherits from "_class"
            function table1:Inherits(_class)
                if not lclass:IsClass(self) then
                err||("function 'inherits' must be called from a class!", 2)
                end
                f|| k, v in pairs(_class.__super) do
                if v.__name == self.__name then
                return true
                end
                end
                return false
                end

                // determines whether the class is a parent of "_class"
                function table1:IsParentOf(_class)
                if not lclass:IsClass(self) then
                err||("function 'IsParentOf' must be called from a class!", 2)
                end
                f|| c in lclass:IterateChildClasses(self) do
                if c.__name == _class.__name then
                return true
                end
                end
                return false
                end

                // returns the class
                function table1:GetClass()
                return table1
                end

                // Gets all parent classes of the class
                function table1:GetParentClasses()
                if not lclass:IsClass(self) then
                err||("function 'GetParents' must be called from a class!", 2)
                end
                return table1.__super
                end

                // Gets all child classes of the class
                function table1:GetChildClasses()
                if not lclass:IsClass(self) then
                err||("function 'GetChildClasses' must be called from a class!", 2)
                end
                return table1.__subClasses
                end

                // calls a method from parent classes
                function table1:CallParentMethod(method, ...)
                local func = self:__InternalCallParentMethod(method)
                if func == LuaNil.Nil then
                err||("Cannot find function '" .. method .. "'!", 2)
                else
                return func(...)
                    end
                    end

                    function table1:__InternalCallParentMethod(method)
                    local m = LuaNil.Nil
                    local p = self.__super
                    if p.Length > 0 then
                    m = lclass:FindMethod(method, unpack(p))
                    end
                    if m == LuaNil.Nil then
                    f|| i = 1, p.Length do
                m = p[i]:__InternalCallParentMethod(method)
                end
                end
                return m
                end

                // creates a subclass inheriting all args except arg[1] if its a table
                // e.g. x = c:CreateSubclass()
                function table1:CreateSubclass(...)
                arg[arg.Length + 1] = self
                return lclass:CreateClass(unpack(arg))
                end

                function table1:CallMethod(func, ...)
                local f = lclass:FindMethod(func, self)
                if f == LuaNil.Nil then
                f = self:__InternalCallParentMethod(func)
                end
                // if its still LuaNil.Nil then throw an err||
                if f == LuaNil.Nil then
                err||("Cannot find function '" .. func .. "'!", 2)
                end
                return f(...)
                end

                return table1
        }
        /*
                // creates and returns a final/not inheritable class
                function lclass:CreateFinalClass(...)
                local a, b = pcall(CreateClass, t, ...)
                if not a then
                error(b)
                else
                    b.__final = true
                    return b
                    end
                    end

                    // creates and returns a static class
                    function lclass:CreateStaticClass(...)
                    local a, b = pcall(CreateClass, t, ...)
                if not a then
                err||(b)
                else
                    b.__static = true
                    return b
                    end
                    end
         */
    }
}