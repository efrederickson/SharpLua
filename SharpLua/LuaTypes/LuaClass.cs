/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/23/2011
 * Time: 12:35 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using SharpLua.Library;

namespace SharpLua.LuaTypes
{
    /// <summary>
    /// A Lua class
    /// </summary>
    public class LuaClass : LuaValue
    {
        public bool Final = false;
        public bool Static = false;
        public string Name = "";
        public List<LuaClass> ParentClasses = new List<LuaClass>();
        public List<LuaClass> ChildClasses = new List<LuaClass>();
        public LuaFunction IndexFunction;
        public LuaFunction NewIndexFunction;
        public LuaFunction CallFunction;
        public LuaFunction ToStringFunction;
        public LuaTable Self;
        
        public LuaClass(string name, bool final, bool _static, List<LuaClass> parents, List<LuaClass> childs)
        {
            this.Name = name;
            this.Final = final;
            this.Static = _static;
            this.ParentClasses = parents;
            this.ChildClasses = childs;
            
            this.IndexFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                             {return null; }));
            this.CallFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                            {return null; }));
            this.NewIndexFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                                {return null; }));
            this.ToStringFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                                {return new LuaString("Class: " + GetHashCode()); }));
            
            this.Self = new LuaTable();
            Self.MetaTable.Register("__index",new LuaFunc(delegate(LuaValue[] args)
                                                          { //(table, key)
                                                              LuaTable table = args[0] as LuaTable;
                                                              LuaValue key = args[1];
                                                              IndexFunction.Invoke(new LuaValue[] {table, key}); // user defined __index function
                                                              return CallMethod.Invoke(new LuaValue[] {key});
                                                          }));
            
            Self.MetaTable.Register("__call", new LuaFunc(delegate(LuaValue[] args)
                                                          { //(func, ...)
                                                              LuaFunction func = args[0] as LuaFunction;
                                                              List<LuaValue> args2 = new List<LuaValue>();
                                                              foreach (LuaValue a in args)
                                                                  args2.Add(a);
                                                              args2.RemoveAt(0);
                                                              CallFunction.Invoke(new LuaValue[] {func, new LuaMultiValue(args2.ToArray())}); // user defined __call function
                                                              return func.Invoke(args2.ToArray()); // call function
                                                          }));
            
            Self.MetaTable.Register("__newindex",new LuaFunc(delegate(LuaValue[] args)
                                                             {
                                                                 LuaValue key = args[1];
                                                                 LuaValue value = args[2];
                                                                 //(table, key, value)
                                                                 NewIndexFunction.Invoke(new LuaValue[] {Self, key, value}); // user defined __newindex function
                                                                 // check for user defined "metamethods"
                                                                 string func = key.Value.ToString();
                                                                 if (func == "__index")
                                                                 {
                                                                     // assign this to the __indexfunction variable
                                                                     Self.RawSetValue("__indexfunction", value);
                                                                 }
                                                                 else if (func == "__newindex")
                                                                     Self.RawSetValue("__newindexfunction", value);
                                                                 else if (func == "__call")
                                                                     Self.RawSetValue("__callfunction", value);
                                                                 else if (func == "__tostring")
                                                                     Self.RawSetValue("__tostringfunction", value);
                                                                 else // its a normal key/value pair
                                                                     Self.SetKeyValue(key, value);
                                                                 return LuaNil.Nil;
                                                             }));
            Self.MetaTable.Register("__tostring", new LuaFunc(delegate(LuaValue[] args)
                                                              {
                                                                  return ToStringFunction.Invoke(new LuaValue[] {});
                                                              }));
        }
        
        public override object Value {
            get {
                return Self;
            }
        }
        
        public override string GetTypeCode()
        {
            return "class";
        }
        
        // returns a new instance of the class
        public LuaValue New(LuaValue[] args)
        {
            return ClassLib.CreateInstance(table1);
        }

        // copies table "m" into class
        public LuaValue Set(LuaValue[] args)
        {
            for (int i = 1; i < args.Length; i++)
                TableLib.Copy(new LuaValue[] {Self, args[i]});
            return Self;
        }

        // finds out whether "m" is a child of the class
        public LuaValue HasMember(LuaValue[] args)
        { //(m)
            return IsMemberOf(new LuaValue[] {args[0], table1});
        }

        // determines whether the class inherits from "_class"
        public LuaValue Inherits(LuaValue[] args)
        { //(_class)
            foreach (LuaClass c in ParentClasses)
            {
                if (Name == c.Name)
                    return LuaBoolean.True;
            }
            return LuaBoolean.False;
        }

        // determines whether the class is a parent of "_class"
        public LuaValue IsParentOf(LuaValue[] args)
        {
            LuaClass _class = args[0] as LuaClass;
            foreach (LuaClass c in ChildClasses)
                if c.Name == _class.Name)
                    return LuaBoolean.True;
            return LuaBoolean.False;
        }

        // returns the class
        //function table1:GetClass()
        //    return table1
        //end

        public LuaValue GetParentClasses()
        {
            return ParentClasses;
        }

        public LuaValue GetChildClasses()
        {
            return ChildClasses;
        }

        public LuaValue CallParentMethod(LuaValue[] args)
        {
            //method, ...
            string method = args[0].Value.ToString();
            // strip method name
            List<LuaValue> args2 = new List<LuaValue>();
            foreach (LuaValue a in args)
                args2.Add(a);
            args2.RemoveAt(0);
            
            LuaFunction func = InternalCallParentMethod(method) as LuaFunction;
            if (func == null)
                throw new Exception("Cannot find function '" + method + "'!");
            else
                return func.Invoke(args2.ToArray());
        }
        

        public LuaValue InternalCallParentMethod(string method)
        {
            LuaValue m = LuaNil.Nil;
            if (ParentClasses.Count > 0)
                m = ClassLib.FindMethod(method, ParentClasses);
            
            if (m == LuaNil.Nil)
                for (int i = 0;i < ParentClasses.Count; i++)
                    m = p[i].InternalCallParentMethod(method);
            return m;
        }

        // creates a subclass inheriting all args except arg[1] if its a table
        // e.g. x = c:CreateSubclass()
        public LuaValue CreateSubClass(LuaValue[] args)
        {
            args[args.Length + 1] = this;
            return ClassLib.CreateClass(args);
    }

        public LuaValue CallMethod(LuaValue[] args)
            {
            //(func, ...)
            // strip method name
            string func = args[0].Value.ToString();
            List<LuaValue> args2 = new List<LuaValue>();
            foreach (LuaValue a in args)
                args2.Add(a);
            args2.RemoveAt(0);
            LuaFunction f = ClassLib.FindMethod(new LuaValue[] {func, Self});
            if ((f == LuaNil.Nil) || (f == null))
                f = InternalCallParentMethod(func) as LuaFunction;
            // if its still LuaNil.Nil then throw an error
            if ((f == LuaNil.Nil) || (f == null))
                throw new Exception("Cannot find function '" + func + "'!");
            
            return f.Invoke(args2.ToArray());
        }
    }
}
