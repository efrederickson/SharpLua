using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpLua.LuaTypes
{
    [Serializable()]
    public class LuaTable : LuaValue
    {
        private Dictionary<LuaValue, LuaValue> table = new Dictionary<LuaValue, LuaValue>();

        public LuaTable() { }

        public LuaTable(LuaTable parent)
        {
            this.MetaTable = new LuaTable();
            this.MetaTable.SetNameValue("__index", parent);
            this.MetaTable.SetNameValue("__newindex", parent);
        }

        public override object Value
        {
            get { return this; }
        }

        public override string GetTypeCode()
        {
            return "table";
        }

        public int Length
        {
            get
            {
                return this.Count;
            }
        }

        public int Count
        {
            get
            {
                if (this.table == null)
                {
                    return 0;
                }
                else
                {
                    return this.table.Count;
                }
            }
        }

        public override string ToString()
        {
            if (this.MetaTable != null)
            {
                LuaFunction function = this.MetaTable.GetValue("__tostring") as LuaFunction;
                if (function != null)
                {
                    return function.Invoke(new LuaValue[] { this }).ToString();
                }
            }

            return "Table " + this.GetHashCode();
        }
        
        public IEnumerable<LuaValue> Values
        {
            //get { return this.list; }
            get
            {
                foreach (LuaValue v in table.Values)
                    yield return v;
            }
        }
        
        public IEnumerable<LuaValue> Keys
        {
            get
            {
                if (this.Count > 0)
                {
                    foreach (LuaValue key in this.table.Keys)
                    {
                        yield return key;
                    }
                }
            }
        }

        public IEnumerable<KeyValuePair<LuaValue, LuaValue>> KeyValuePairs
        {
            get { return this.table; }
        }

        public bool ContainsKey(LuaValue key)
        {
            if(this.table != null){
                if(this.table.ContainsKey(key)){
                    return true;
                }
            }
            if (key as LuaNumber != null)
            {
                double val = (key as LuaNumber).Number;
                foreach (LuaValue key2 in table.Keys)
                    if (((key2 as LuaNumber != null) && ((key2 as LuaNumber).Number == val)))
                        return true;
            }

            return false;
        }

        public void AddValue(LuaValue value)
        {
            // adds 1, because Lua index starts at 1.
            // and 1 item == count = 1 == index = 1
            // BUT, GetValue removes 1 from index, so its back to original C# index
            // so: count = 34 == index == 34 (- 1) == table[34], C#: table[33]
            this.table.Add(new LuaNumber(table.Count + 1), value);
        }

        public void InsertValue(int index, LuaValue value)
        {
            if (index > 0 && index <= this.Length + 1)
            {
                //this.table.Insert(index - 1, value);
                AddValue(value);
            }
            else
            {
                throw new ArgumentOutOfRangeException("index");
            }
        }

        public bool Remove(LuaValue item)
        {
            return this.table.Remove(item);
        }

        public void RemoveAt(int index)
        {
            foreach (LuaValue val in table.Keys)
            {
                if ((val as LuaNumber) != null)
                {
                    int n = int.Parse(val.Value.ToString());
                    if (n == (index - 1))
                    {
                        table.Remove(val);
                        return;
                    }
                }
            }
            //this.table.RemoveAt(index - 1);
        }

        public void Sort()
        {/*
            this.table.Sort((a, b) =>
                           {
                               LuaNumber n = a as LuaNumber;
                               LuaNumber m = b as LuaNumber;
                               if (n != null && m != null)
                               {
                                   return n.Number.CompareTo(m.Number);
                               }

                               LuaString s = a as LuaString;
                               LuaString t = b as LuaString;
                               if (s != null && t != null)
                               {
                                   return s.Text.CompareTo(t.Text);
                               }

                               return 0;
                           });
             */
        }

        public void Sort(LuaFunction compare)
        {/*
            this.list.Sort((a, b) =>
                           {
                               LuaValue result = compare.Invoke(new LuaValue[] { a, b });
                               LuaBoolean boolValue = result as LuaBoolean;
                               if (boolValue != null && boolValue.BoolValue == true)
                               {
                                   return 1;
                               }
                               else
                               {
                                   return -1;
                               }
                           });*/
        }

        public LuaValue GetValue(int index)
        {
            if (index > 0 && index <= this.Length)
            {
                foreach (LuaValue v in table.Keys)
                {
                    if (((v as LuaNumber) != null) && (int.Parse((v as LuaNumber).Number.ToString()) == index))
                        return table[v];
                }
                //return this.table[index - 1];
            }

            return LuaNil.Nil;
        }

        public LuaValue GetValue(string name)
        {
            LuaValue key = this.GetKey(name);

            if (key == LuaNil.Nil)
            {
                if (this.MetaTable != null)
                {
                    return this.GetValueFromMetaTable(name);
                }

                return LuaNil.Nil;
            }
            else
            {
                return this.table[key];
            }
        }

        public LuaValue GetKey(string key)
        {
            if (this.table == null) return LuaNil.Nil;

            foreach (LuaValue value in this.table.Keys)
            {
                LuaString str = value as LuaString;

                if (str != null && string.Equals(str.Text, key, StringComparison.Ordinal))
                {
                    return value;
                }
            }

            return LuaNil.Nil;
        }

        public void SetNameValue(string name, LuaValue value)
        {
            if (value == LuaNil.Nil)
            {
                this.RemoveKey(name);
            }
            else
            {
                this.RawSetValue(name, value);
            }
        }

        public void RemoveKey(string name)
        {
            LuaValue key = this.GetKey(name);

            if (key != LuaNil.Nil)
            {
                this.table.Remove(key);
            }
        }

        public void SetKeyValue(LuaValue key, LuaValue value)
        {
            LuaNumber number = key as LuaNumber;

            if (number != null && number.Number == (int)number.Number)
            {
                int index = (int)number.Number;

                if (index == this.Length + 1)
                {
                    this.AddValue(value);
                    return;
                }

                if (index > 0 && index <= this.Length)
                {
                    foreach (LuaValue v in table.Keys)
                    {
                        if (((v as LuaNumber) != null) && (int.Parse((v as LuaNumber).Number.ToString()) == index))
                        {
                            table[v] = value;
                            return;
                        }
                    }
                    //this.list[index - 1] = value;
                    //return;
                }
            }

            if (value == LuaNil.Nil)
            {
                this.RemoveKey(key);
                return;
            }

            if (this.table == null)
            {
                this.table = new Dictionary<LuaValue, LuaValue>();
            }

            this.table[key] = value;
        }

        public void RemoveKey(LuaValue key)
        {
            if (key != LuaNil.Nil && this.table != null && this.table.ContainsKey(key))
            {
                this.table.Remove(key);
            }
        }

        public LuaValue GetValue(LuaValue key)
        {
            if (key == LuaNil.Nil)
            {
                return LuaNil.Nil;
            }
            else
            {
                LuaNumber number = key as LuaNumber;
                
                if (number != null && number.Number == (int)number.Number)
                {
                    int index = (int)number.Number;
                    //index--; //Convert to C# index
                    
                    if (index > 0 && index <= this.Length)
                    {
                        foreach (LuaValue v in table.Keys)
                        {
                            if (((v as LuaNumber) != null) && (int.Parse((v as LuaNumber).Number.ToString()) == index))
                                return table[v];
                        }
                        //return this.list[index - 1];
                    }
                }
                else if (this.table != null && this.table.ContainsKey(key))
                {
                    return this.table[key];
                }
                else if (this.MetaTable != null)
                {
                    // pass to meta table function
                    return this.GetValueFromMetaTable(key);
                }

                return LuaNil.Nil;
            }
        }

        public LuaValue GetValueFromMetaTable(string name)
        {
            LuaValue indexer = this.MetaTable.GetValue("__index");

            LuaTable table = indexer as LuaTable;

            if (table != null)
            {
                return table.GetValue(name);
            }

            LuaFunction function = indexer as LuaFunction;

            if (function != null)
            {
                return function.Function.Invoke(new LuaValue[] { new LuaString(name) });
            }

            return LuaNil.Nil;
        }

        public LuaValue GetValueFromMetaTable(LuaValue key)
        {
            LuaValue indexer = this.MetaTable.GetValue("__index");

            LuaTable table = indexer as LuaTable;

            if (table != null)
            {
                return table.GetValue(key);
            }

            LuaFunction function = indexer as LuaFunction;

            if (function != null)
            {
                return function.Function.Invoke(new LuaValue[] { key });
            }

            return LuaNil.Nil;
        }

        public LuaFunction Register(string name, LuaFunc function)
        {
            LuaFunction luaFunc = new LuaFunction(function);
            this.SetNameValue(name, luaFunc);
            return luaFunc;
        }

        public LuaValue RawGetValue(LuaValue key)
        {
            if (this.table != null && this.table.ContainsKey(key))
            {
                return this.table[key];
            }

            return LuaNil.Nil;
        }

        public void RawSetValue(string name, LuaValue value)
        {
            LuaValue key = this.GetKey(name);

            if (key == LuaNil.Nil)
            {
                key = new LuaString(name);
            }

            if (this.table == null)
            {
                this.table = new Dictionary<LuaValue, LuaValue>();
            }

            this.table[key] = value;
        }
        
        public string this[LuaValue key]
        {
            get
            {
                return table[key];
            }
            set
            {
                table[key] = value;
            }
        }
        
        public string this[int index]
        {
            get
            {
                return GetValue(index);
            }
            set
            {
                SetKeyValue(new LuaNumber(index) /* + 1*/, value);
            }
        }
    }
}
