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
    [Serializable()]
    public class LuaClass : LuaValue
    {
        private static List<string> classNames = new List<string>();
        
        public bool Final = false;
        public bool Static = false;
        private string _name = "";
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = Rename(value);
            }
        }
        public List<LuaClass> ParentClasses = new List<LuaClass>();
        public List<LuaClass> ChildClasses = new List<LuaClass>();
        public LuaFunction IndexFunction;
        public LuaFunction NewIndexFunction;
        public LuaFunction CallFunction;
        public LuaFunction ToStringFunction;
        public LuaFunction Constructor;
        public LuaFunction Destructor;
        public LuaTable Self;
        
        public LuaClass(string name, bool final, bool _static)
        {
            this.Name = Rename(name);
            this.Final = final;
            this.Static = _static;
            
            this.IndexFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                             {return null; }));
            this.CallFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                            {return null; }));
            this.NewIndexFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                                {return null; }));
            this.ToStringFunction = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                                {return new LuaString("Lua Class: " + GetHashCode() + ", Name: " + Name); }));
            this.Constructor = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                           {
                                                               // do nothing
                                                               return LuaNil.Nil;
                                                           }));
            this.Destructor = new LuaFunction(new LuaFunc((LuaValue[] args) =>
                                                          {
                                                              // do nothing
                                                              return LuaNil.Nil;
                                                          }));
            this.Self = new LuaTable();
            Self.Register("new", New);
            Self.Register("Set", Set);
            Self.Register("HasMember", HasMember);
            Self.Register("Inherits", Inherits);
            Self.Register("IsParentOf", IsParentOf);
            Self.Register("GetParentClasses", GetParentClasses);
            Self.Register("GetChildClasses", GetChildClasses);
            Self.Register("CallParentMethod", CallParentMethod);
            Self.Register("CreateSubclass", CreateSubClass);
            Self.Register("CallMethod", CallMethod);
            Self.Register("GetTable", GetTable);
            GenerateMetaTable();
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
        
        public LuaValue New(LuaValue[] args)
        {
            if (Static)
                throw new Exception("Cannot create an instance of a static class!");
            LuaClass n = ClassLib.CreateInstance(new LuaValue[] {this }) as LuaClass;
            n.Constructor.Invoke(args);
            return n;
        }
        
        public LuaValue Set(LuaValue[] args)
        {
            for (int i = 1; i < args.Length; i++)
                TableLib.Copy(new LuaValue[] {Self, args[i]});
            return Self;
        }
        
        public LuaValue HasMember(LuaValue[] args)
        { //(m)
            return ClassLib.IsMemberOf(new LuaValue[] {args[0], this});
        }
        
        public LuaValue Inherits(LuaValue[] args)
        { //(_class)
            foreach (LuaClass c in this.ParentClasses)
            {
                if (Name == c.Name)
                    return LuaBoolean.True;
            }
            return LuaBoolean.False;
        }
        
        public LuaValue IsParentOf(LuaValue[] args)
        {
            LuaClass _class = args[0] as LuaClass;
            foreach (LuaClass c in ChildClasses)
                if (c.Name == _class.Name)
                    return LuaBoolean.True;
            return LuaBoolean.False;
        }
        
        public LuaValue GetTable(LuaValue[] args)
        {
            return Self;
        }
        
        public LuaValue GetParentClasses(LuaValue[] args)
        {
            LuaTable t = new LuaTable();
            foreach (LuaClass p in ParentClasses)
                t.AddValue(p);
            return t;
        }
        
        public LuaValue GetChildClasses(LuaValue[] args)
        {
            List<LuaClass> c = new List<LuaClass>();
            foreach (LuaClass p in ChildClasses)
                c.Add(p);
            return new LuaMultiValue(c.ToArray());
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
            {
                foreach (LuaClass c in ParentClasses)
                {
                    m = ClassLib.FindMethod(new LuaValue[] {new LuaString(method), c});
                    if (m != null)
                        break;
                }
            }
            if (m == LuaNil.Nil)
                for (int i = 0;i < ParentClasses.Count; i++)
                    m = ParentClasses[i].InternalCallParentMethod(method);
            return m;
        }

        // creates a subclass inheriting all args except arg[1] if its a table
        // e.g. x = c:CreateSubclass()
        public LuaValue CreateSubClass(LuaValue[] args)
        {
            List<LuaValue> args2 = new List<LuaValue>();
            foreach (LuaValue a in args)
            {
                args2.Add(a);
            }
            args2.Add(this);
            return ClassLib.CreateClass(args);
        }

        public LuaValue CallMethod(LuaValue[] args)
        {
            //(func, ...)
            // strip method name
            string func = args[0].Value.ToString();
            List<LuaValue> args2 = new List<LuaValue>();
            if ((args[1] as LuaUserdata) != null)
            {
                foreach (LuaValue a in ((args[1] as LuaUserdata).Value as LuaValue[]))
                    args2.Add(a);
            }
            else
            {
                foreach (LuaValue a in args)
                    args2.Add(a);
            }
            try
            {
            args2.RemoveAt(0);
            }
            catch
            {
                
            }
            LuaFunction f = ClassLib.FindMethod(new LuaValue[] {new LuaString(func), this}) as LuaFunction;
            if ((f == null))
                f = InternalCallParentMethod(func) as LuaFunction;
            // if its still LuaNil.Nil then throw an error
            if ((f == null))
                throw new Exception("Cannot find function '" + func + "'!");
            
            return f.Invoke(args2.ToArray());
        }
        
        public override string ToString()
        {
            return ToStringFunction.Invoke(new LuaValue[] {}).ToString();
        }
        
        public LuaValue GetObject(LuaValue key, LuaClass p)
        {
            if ((p.Self.RawGetValue(key) != null) && (p.Self.RawGetValue(key) != LuaNil.Nil))
                return p.Self.RawGetValue(key);
            else
            {
                foreach (LuaClass c in p.ParentClasses)
                    return GetObject(key, c);
            }
            return LuaNil.Nil;
        }
        
        public void GenerateMetaTable()
        {
            Self.MetaTable = new LuaTable();
            Self.MetaTable.Register("__index",new LuaFunc(delegate(LuaValue[] args)
                                                          { //(table, key)
                                                              LuaValue key = args[0];
                                                              IndexFunction.Invoke(new LuaValue[] {Self, key}); // user defined __index function
                                                              // attempt to get from parents also
                                                              return GetObject(key, this); //CallMethod(new LuaValue[] {key});
                                                          }));
            
            Self.MetaTable.Register("__call", new LuaFunc(delegate(LuaValue[] args)
                                                          { //(func, ...)
                                                              if (args.Length == 0)
                                                                  return LuaNil.Nil;
                                                              List<LuaValue> args2 = new List<LuaValue>();
                                                              foreach (LuaValue a in args)
                                                                  args2.Add(a);
                                                              args2.RemoveAt(0);
                                                              CallFunction.Invoke(new LuaValue[] {args[0], new LuaMultiValue(args2.ToArray())}); // user defined __call function
                                                              return CallMethod(new LuaValue[] {args[0], new LuaUserdata(args2.ToArray())}); // call function
                                                          }));
            
            Self.MetaTable.Register("__newindex",new LuaFunc(delegate(LuaValue[] args)
                                                             {
                                                                 LuaValue key = args[1];
                                                                 LuaValue value = args[2];
                                                                 //(table, key, value)
                                                                 NewIndexFunction.Invoke(new LuaValue[] {Self, key, value}); // user defined __newindex function
                                                                 // check for user defined "metamethods"
                                                                 string obj = key.Value.ToString();
                                                                 if (obj == "__index")
                                                                 {
                                                                     // assign this to the __indexfunction variable
                                                                     IndexFunction = value as LuaFunction;
                                                                 }
                                                                 else if (obj == "__newindex")
                                                                     NewIndexFunction = value as LuaFunction;
                                                                 else if (obj == "__call")
                                                                     CallFunction = value as LuaFunction;
                                                                 else if (obj == "__tostring")
                                                                     ToStringFunction = value as LuaFunction;
                                                                 else if (obj == "final")
                                                                     Final = true;
                                                                 else if (obj == "static")
                                                                     Static = true;
                                                                 else if (obj == "name")
                                                                     Name = Rename(value.Value.ToString());
                                                                 else if (obj == "constructor")
                                                                     Constructor = (LuaFunction) value;
                                                                 else if (obj == "destructor")
                                                                     Destructor = (LuaFunction) value;
                                                                 else // its a normal key/value pair
                                                                     Self.SetKeyValue(key, value);
                                                                 return LuaNil.Nil;
                                                             }));
            Self.MetaTable.Register("__tostring", new LuaFunc(delegate(LuaValue[] args)
                                                              {
                                                                  return ToStringFunction.Invoke(new LuaValue[] {});
                                                              }));
            this.MetaTable = Self.MetaTable;
        }
        
        string Rename(string val)
        {
            classNames.Remove(this.Name);
            if (classNames.Contains(val))
                throw new Exception("Class '" + val + "' is already defined!");
            return val;
        }

    }
}
