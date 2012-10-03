using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class TableConstructor : Term
    {
        public override LuaValue Evaluate(LuaTable enviroment)
        {
            LuaTable table = new LuaTable();

            foreach (Field field in this.FieldList)
            {
                NameValue nameValue = field as NameValue;
                if (nameValue != null)
                {
                    table.SetNameValue(nameValue.Name, nameValue.Value.Evaluate(enviroment));
                    continue;
                }

                KeyValue keyValue = field as KeyValue;
                if (keyValue != null)
                {
                    table.SetKeyValue(
                        keyValue.Key.Evaluate(enviroment),
                        keyValue.Value.Evaluate(enviroment));
                    continue;
                }

                ItemValue itemValue = field as ItemValue;
                if (itemValue != null)
                {
                    table.AddValue(itemValue.Value.Evaluate(enviroment));
                    continue;
                }
            }

            return table;
        }
    }
}
