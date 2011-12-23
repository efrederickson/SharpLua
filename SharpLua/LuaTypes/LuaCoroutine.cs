/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/22/2011
 * Time: 12:12 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Threading;

namespace SharpLua.LuaTypes
{
    /// <summary>
    /// A coroutine
    /// </summary>
    public class LuaCoroutine : LuaValue
    {
        public static LuaCoroutine Running;
        
        string _status;
        Thread thread;
        LuaFunction func;
        
        public LuaCoroutine(LuaFunction f)
        {
            this.func = f;
            _status = "normal";
        }
        
        public override object Value {
            get {
                return thread;
            }
        }
        
        public override string GetTypeCode()
        {
            return "thread";
        }
        
        public bool Resume(LuaValue[] args)
        {
            if (thread == null)
            {
                thread = new Thread(new ThreadStart(delegate()
                                                    {
                                                        try {
                                                            _status = "running";
                                                            func.Invoke(args);
                                                            _status = "dead";
                                                        } catch (Exception) {
                                                            _status = "dead";
                                                        }
                                                    }));
                thread.SetApartmentState(ApartmentState.MTA);
                thread.Start();
            }
            else
                if (_status == "dead")
                    throw new Exception("Error: coroutine is dead, it cannot be resumed!");
            try {
                if (_status == "suspended")
                    thread.Resume();
                else
                    thread.Start();
                _status = "running";
            } catch (Exception ex) {
                _status = "dead";
                throw ex;
            }
            
            Running = this;
            return true;
        }
        
        public string Status
        {
            get
            {
                if (thread == null)
                    return "dead";
                return _status;
            }
        }
        
        public void Pause()
        {
            if (thread != null)
            {
                thread.Suspend();
                _status = "suspended";
            }
        }
    }
}
