﻿/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/26/2011
 * Time: 12:19 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using SharpLua.AST;

namespace SharpLua
{
    /// <summary>
    /// Description of Serializer.
    /// </summary>
    public class Serializer
    {
        public static void Serialize(object obj, string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();
        }
        
        public static Chunk Deserialize(string filename)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(filename, FileMode.Open);
            object o = formatter.Deserialize(stream);
            stream.Close();
            return (Chunk) o;
        }
    }
}
