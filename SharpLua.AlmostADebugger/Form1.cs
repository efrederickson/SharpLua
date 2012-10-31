using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using SharpLua.LASM;
using System.Threading;

namespace SharpLua.AlmostADebugger
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            //SharpLua.Lua.OnOpcodeRun += Lua_OnOpcodeRun;
            this.FormClosing += delegate { if (t != null)t.Abort(); };
        }

        void Lua_OnOpcodeRun(uint opcode)
        {
            // Jumping negatives? gotta fix that
            try
            {
                Invoke(new Action(delegate
                {
                    Lua.OpCode c = Lua.GET_OPCODE(opcode);
                    label1.Text = "Executing: " + ((Instruction.LuaOpcode)(int)c).ToString();
                    if (textBox3.Text == "")
                        textBox3.Text = ">";
                    else
                        textBox3.Text = "\r\n" + textBox3.Text;
                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        Thread t = null;

        private void button1_Click(object sender, EventArgs e)
        {
            if (t != null)
                if (t.ThreadState == System.Threading.ThreadState.Running)
                    return;
            try
            {
                //if (o.Length == 2)
                {
                    t = new Thread(new ThreadStart(delegate
                    {
                        Stopwatch sw = new Stopwatch();
                        sw.Start();
                        try
                        {
                            //string tmp = "return string.dump(loadstring([==============================[" + textBox1.Text + "]==============================]))";
                            string tmp = "return Load([==============================[" + textBox1.Text + "]==============================])";
                            sw.Stop();
                            object[] o = LuaRuntime.Run(tmp);
                            LuaFunction func = o[0] as LuaFunction;
                            LuaFile f = o[1] as LuaFile;
                            Invoke(new Action(delegate
                            {
                                textBox2.Text = "";
                                textBox2.Clear();
                                textBox3.Text = "";
                                dump(f.Main);
                            }));
                            SharpLua.Lua.OnOpcodeRun += Lua_OnOpcodeRun;
                            sw.Start();
                            //func.Call();

                            func.Call();
                            SharpLua.Lua.OnOpcodeRun -= Lua_OnOpcodeRun;
                            //func.Call();

                        }
                        catch (System.Exception ex)
                        {
                            Invoke(new Action(delegate { textBox2.Text = ex.ToString(); }));
                        }
                        finally
                        {
                            Invoke(new Action(delegate
                            {
                                sw.Stop();
                                label1.Text = "Ran in less than " + sw.ElapsedMilliseconds + " ms";
                            }));
                        }
                    }));
                    t.Start();
                }
            }
            catch (System.Exception ex)
            {
                textBox2.Text = ex.ToString();
            }
            finally
            {

            }
        }

        void dump(Chunk c)
        {
            //textBox2.Text += "; Chunk Name: " + c.Name + "\r\n";
            foreach (Instruction i in c.Instructions)
            {
                switch (i.OpcodeType)
                {
                    case OpcodeType.ABC:
                        textBox2.Text += i.OpcodeName;
                        textBox2.Text += " " + i.A;
                        textBox2.Text += " " + i.B;
                        textBox2.Text += " " + i.C;
                        break;
                    case OpcodeType.ABx:
                        textBox2.Text += i.OpcodeName;
                        textBox2.Text += " " + i.A;
                        textBox2.Text += " " + i.Bx;
                        break;
                    case OpcodeType.AsBx:
                        textBox2.Text += i.OpcodeName;
                        textBox2.Text += " " + i.A;
                        textBox2.Text += " " + i.sBx;
                        break;
                    default:
                        break;
                }
                textBox2.Text += "\r\n";
            }
            foreach (Chunk c2 in c.Protos)
            {
                //textBox2.Text += "\r\n";
                dump(c2);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (t != null)
                t.Abort();
        }
    }
}
