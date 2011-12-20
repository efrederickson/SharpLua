using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SharpLua.Library
{
    public static class OSLib
    {
        public static void RegisterModule(LuaTable enviroment)
        {
            LuaTable module = new LuaTable();
            RegisterFunctions(module);
            enviroment.SetNameValue("os", module);
        }

        public static void RegisterFunctions(LuaTable module)
        {
            module.Register("clock", clock);
            module.Register("date", date);
            module.Register("time", time);
            module.Register("execute", execute);
            module.Register("exit", exit);
            module.Register("getenv", getenv);
            module.Register("remove", remove);
            module.Register("rename", rename);
            module.Register("tmpname", tmpname);
        }

        public static LuaValue clock(LuaValue[] values)
        {
            int seconds = Environment.TickCount / 1000;
            return new LuaNumber (seconds );
        }

        public static LuaValue date(LuaValue[] values)
        {
            LuaString format = values[0] as LuaString;
            if (format != null)
            {
                if (format.Text == "*t")
                {
                    LuaTable table = new LuaTable();
                    DateTime now = DateTime.Now;
                    table.SetNameValue("year", new LuaNumber (now.Year));
                    table.SetNameValue("month", new LuaNumber (now.Month ));
                    table.SetNameValue("day", new LuaNumber (now.Day));
                    table.SetNameValue("hour", new LuaNumber (now.Hour));
                    table.SetNameValue("min", new LuaNumber (now.Minute));
                    table.SetNameValue("sec", new LuaNumber (now.Second));
                    table.SetNameValue("wday", new LuaNumber ((int)now.DayOfWeek));
                    table.SetNameValue("yday", new LuaNumber (now.DayOfYear));
                    table.SetNameValue("isdst", LuaBoolean.From(now.IsDaylightSavingTime()));
                }
                else
                {
                    return new LuaString(DateTime.Now.ToString(format.Text));
                }
            }

            return new LuaString(DateTime.Now.ToShortDateString());
        }

        public static LuaValue time(LuaValue[] values)
        {
            return new LuaNumber (new TimeSpan(DateTime.Now.Ticks).TotalSeconds);
        }

        public static LuaValue execute(LuaValue[] values)
        {
            if (values.Length > 0)
            {
                LuaString command = values[0] as LuaString;
                System.Diagnostics.Process.Start(command.Text);
            }
            return new LuaNumber (1);
        }

        public static LuaValue exit(LuaValue[] values)
        {
            System.Threading.Thread.CurrentThread.Abort();
            return new LuaNumber(0);
        }

        public static LuaValue getenv(LuaValue[] values)
        {
            LuaString name = values[0] as LuaString;
            string variable = Environment.GetEnvironmentVariable(name.Text);
            if (variable == null)
            {
                return LuaNil.Nil;
            }
            else
            {
                return new LuaString(variable);
            }
        }

        public static LuaValue remove(LuaValue[] values)
        {
            LuaString file = values[0] as LuaString;
            if (File.Exists(file.Text))
            {
                File.Delete(file.Text);
                return new LuaString("File is deleted.");
            }
            else if (Directory.Exists(file.Text))
            {
                if (Directory.GetFileSystemEntries(file.Text).Length == 0)
                {
                    Directory.Delete(file.Text);
                    return new LuaString("Directory is deleted.");
                }
                else
                {
                    return new LuaMultiValue(new LuaValue[] { LuaNil.Nil, new LuaString("Directory is not empty.") });
                }
            }
            else
            {
                return new LuaMultiValue(new LuaValue[] { LuaNil.Nil, new LuaString("File or directory does not exist.") });
            }
        }

        public static LuaValue rename(LuaValue[] values)
        {
            LuaString oldName = values[0] as LuaString;
            LuaString newName = values[1] as LuaString;

            if (File.Exists(oldName.Text))
            {
                File.Move(oldName.Text, newName.Text);
                return new LuaString("File is renamed.");
            }
            else if (Directory.Exists(oldName.Text))
            {
                Directory.Move(oldName.Text, newName.Text);
                return new LuaString("Directory is renamed.");
            }
            else
            {
                return new LuaMultiValue(new LuaValue[] { LuaNil.Nil, new LuaString("File or directory does not exist.") });
            }
        }

        public static LuaValue tmpname(LuaValue[] values)
        {
            return new LuaString(Path.GetTempFileName());
        }
    }
}
