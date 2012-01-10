/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/28/2011
 * Time: 3:45 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using IExtendFramework.Encryption;
using SharpLua.LuaTypes;

namespace CryptoLib
{
    /// <summary>
    /// ILuaLibrary class implementer
    /// </summary>
    public class CryptoLib : SharpLua.Library.ILuaLibrary
    {
        public string ModuleName {
            get {
                return "crypto";
            }
        }
        public static byte[] key = SampleObjects.CreateDESKey();
        public static byte[] iv = SampleObjects.CreateDESIV();
        
        public void RegisterModule(SharpLua.LuaTypes.LuaTable environment)
        {
            LuaTable mod = new LuaTable();
            RegisterFunctions(mod);
            
            LuaTable mt = new LuaTable();
            mt.Register("__newindex", (LuaValue[] args) =>
                        {
                            string key = args[1].ToString();
                            if (key.ToLower() == "key")
                            {
                                CryptoLib.key = IExtendFramework.Encryption.SampleObjects.CreateRijndaelKeyWithSHA512(args[2].ToString());
                                Console.WriteLine("Encryption Key is now " + ByteToString(CryptoLib.key));
                            }
                            else if (key.ToLower() == "iv")
                            {
                                iv = IExtendFramework.Encryption.SampleObjects.CreateRijndaelIVWithSHA512(args[2].ToString());
                                Console.WriteLine("Encryption IV is now " + ByteToString(CryptoLib.iv));
                            }
                            
                            return LuaNil.Nil;
                        });
            mod.MetaTable = mt;
            environment.SetNameValue(ModuleName, mod);
        }
        
        public void RegisterFunctions(LuaTable mod)
        {
            mod.Register("encrypt", Encrypt);
            mod.Register("decrypt", Decrypt);
        }
        
        public static LuaValue Encrypt(LuaValue[] args)
        {
            string encType = (args[0] as LuaString).Text.ToLower();
            string _in = args[1].ToString();
            if (encType == "aes")
            {
                return new LuaString(AESProvider.Encrypt(_in));
            }
            else if (encType == "ascii")
            {
                // encrypt with first byte of key
                return new LuaString(ASCIIProvider.Encrypt(_in, int.Parse(key[0].ToString())));
            }
            if (encType == "des")
            {
                return new LuaString(DESProvider.Encrypt(_in));
            }
            if (encType == "rc2")
            {
                return new LuaString(RC2Provider.Encrypt(_in));
            }
            if (encType == "rijndael")
            {
                return new LuaString(RijndaelProvider.Encrypt(_in));
            }
            if (encType == "rsa")
            {
                return new LuaString(RSAProvider.Encrypt(_in));
            }
            if (encType == "tripledes")
            {
                return new LuaString(TripleDESProvider.Encrypt(_in));
            }
            if (encType == "xor")
            {
                return new LuaString(XorProvider.Encrypt(_in, int.Parse(key[0].ToString())));
            }
            throw new Exception("Unsuported encryption '" + encType + "'!");
        }
        
        public static LuaValue Decrypt(LuaValue[] args)
        {
            string decType = (args[0] as LuaString).Text.ToLower();
            string _in = args[1].ToString();
            if (decType == "aes")
            {
                return new LuaString(AESProvider.Decrypt(_in));
            }
            else if (decType == "ascii")
            {
                // Decrypt with first byte of key
                return new LuaString(ASCIIProvider.Decrypt(_in, int.Parse(key[0].ToString())));
            }
            if (decType == "des")
            {
                return new LuaString(DESProvider.Decrypt(_in));
            }
            if (decType == "rc2")
            {
                return new LuaString(RC2Provider.Decrypt(_in));
            }
            if (decType == "rijndael")
            {
                return new LuaString(RijndaelProvider.Decrypt(_in));
            }
            if (decType == "rsa")
            {
                return new LuaString(RSAProvider.Decrypt(_in));
            }
            if (decType == "tripledes")
            {
                return new LuaString(TripleDESProvider.Decrypt(_in));
            }
            if (decType == "xor")
            {
                return new LuaString(XorProvider.Decrypt(_in, int.Parse(key[0].ToString())));
            }
            throw new Exception("Unsuported Decryption '" + decType + "'!");
        }
        
        public static string ByteToString(byte[] i)
        {
            string o = "{ ";
            foreach (byte b in i)
                o += b.ToString() + ", ";
            o = o.Substring(0, o.LastIndexOf(","));
            o += " }";
            return o;
        }
    }
}