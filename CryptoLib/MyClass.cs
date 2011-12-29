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
                IExtendFramework.Encryption.AESProvider aes = new IExtendFramework.Encryption.AESProvider(key, iv);
                return new LuaString(aes.Encrypt(_in));
            }
            else if (encType == "ascii")
            {
                ASCIIProvider asc = new ASCIIProvider();
                // encrypt with first byte of key
                return new LuaString(asc.Encrypt(_in, int.Parse(key[0].ToString())));
            }
            if (encType == "des")
            {
                IExtendFramework.Encryption.DESProvider des = new IExtendFramework.Encryption.DESProvider(key, iv);
                return new LuaString(des.Encrypt(_in));
            }
            if (encType == "rc2")
            {
                IExtendFramework.Encryption.RC2Provider rc2 = new IExtendFramework.Encryption.RC2Provider(key, iv);
                return new LuaString(rc2.Encrypt(_in));
            }
            if (encType == "rijndael")
            {
                IExtendFramework.Encryption.RijndaelProvider rijndael = new IExtendFramework.Encryption.RijndaelProvider(key, iv);
                return new LuaString(rijndael.Encrypt(_in));
            }
            if (encType == "rsa")
            {
                IExtendFramework.Encryption.RSAProvider RSA = new IExtendFramework.Encryption.RSAProvider();
                return new LuaString(RSA.Encrypt(_in));
            }
            if (encType == "tripledes")
            {
                IExtendFramework.Encryption.TripleDESProvider tdes = new IExtendFramework.Encryption.TripleDESProvider(key, iv);
                return new LuaString(tdes.Encrypt(_in));
            }
            if (encType == "xor")
            {
                IExtendFramework.Encryption.XorProvider xor = new IExtendFramework.Encryption.XorProvider();
                return new LuaString(xor.Encrypt(_in, int.Parse(key[0].ToString())));
            }
            throw new Exception("Unsuported encryption '" + encType + "'!");
        }
        
        public static LuaValue Decrypt(LuaValue[] args)
        {
            string decType = (args[0] as LuaString).Text.ToLower();
            string _in = args[1].ToString();
            if (decType == "aes")
            {
                AESProvider aes = new AESProvider(key, iv);
                return new LuaString(aes.Decrypt(_in));
            }
            else if (decType == "ascii")
            {
                ASCIIProvider asc = new ASCIIProvider();
                // Decrypt with first byte of key
                return new LuaString(asc.Decrypt(_in, int.Parse(key[0].ToString())));
            }
            if (decType == "des")
            {
                DESProvider des = new DESProvider(key, iv);
                return new LuaString(des.Decrypt(_in));
            }
            if (decType == "rc2")
            {
                RC2Provider rc2 = new RC2Provider(key, iv);
                return new LuaString(rc2.Decrypt(_in));
            }
            if (decType == "rijndael")
            {
                RijndaelProvider rijndael = new RijndaelProvider(key, iv);
                return new LuaString(rijndael.Decrypt(_in));
            }
            if (decType == "rsa")
            {
                RSAProvider RSA = new RSAProvider();
                return new LuaString(RSA.Decrypt(_in));
            }
            if (decType == "tripledes")
            {
                TripleDESProvider tdes = new TripleDESProvider(key, iv);
                return new LuaString(tdes.Decrypt(_in));
            }
            if (decType == "xor")
            {
                XorProvider xor = new XorProvider();
                return new LuaString(xor.Decrypt(_in, int.Parse(key[0].ToString())));
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