using System;
using System.Collections.Generic;
using System.Text;

using SharpLua.LuaTypes;

namespace SharpLua.AST
{
    [Serializable()]
    public partial class FunctionBody
    {
        public LuaValue Evaluate(LuaTable enviroment)
        {
            return new LuaFunction(
                new LuaFunc(delegate(LuaValue[] args)
                {
                    var table = new LuaTable(enviroment);

                    List<string> names = this.ParamList.NameList;

                    if (names.Count > 0)
                    {
                        int argCount = Math.Min(names.Count, args.Length);

                        for (int i = 0; i < argCount; i++)
                        {
                            table.SetNameValue(names[i], args[i]);
                        }

                        if (this.ParamList.HasVarArg)
                        {
                            if (argCount < args.Length)
                            {
                                LuaValue[] remainedArgs = new LuaValue[args.Length - argCount];
                                for (int i = 0; i < remainedArgs.Length; i++)
                                {
                                    remainedArgs[i] = args[argCount + i];
                                }
                                table.SetNameValue("...", new LuaMultiValue(remainedArgs));
                                table.SetNameValue("arg", new LuaMultiValue(remainedArgs));
                            }
                        }
                    }
                    else if (this.ParamList.IsVarArg != null)
                    {
                        table.SetNameValue("...", new LuaMultiValue(args));
                    }

                    this.Chunk.Enviroment = table;

                    return this.Chunk.Execute();
                })
            );
        }
    }
}
