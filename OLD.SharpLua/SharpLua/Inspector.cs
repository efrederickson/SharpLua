/*
 * Created by SharpDevelop.
 * User: elijah
 * Date: 12/25/2011
 * Time: 2:43 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Reflection;
using System.Text;

namespace SharpLua
{
    /// <summary>
    /// Provides a tool inspecting objects arbitrary objects as part of the
    /// debugging process
    /// </summary>
    public class Inspector
    {
        /// <summary>
        /// Reflects over all the fields of an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Fields(Object o)
        {
            StringBuilder stringBuilder = new StringBuilder();
            FieldInfo[] fieldInfos = o.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                stringBuilder.AppendFormat("\tfield {0} = {1};\r\n", fieldInfo.ToString(),fieldInfo.GetValue(o) );
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reflects over all the methods of an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Methods(Object o)
        {
            StringBuilder stringBuilder = new StringBuilder();

            MethodInfo[] methodInfos = o.GetType().GetMethods();

            foreach (MethodInfo methodInfo in methodInfos)
            {
                stringBuilder.Append ("\t");
                if (methodInfo.IsPublic) stringBuilder.Append ("public ");
                if (methodInfo.IsPrivate) stringBuilder.Append ("private ");
                if (methodInfo.IsAbstract) stringBuilder.Append ("abstract ");
                if (methodInfo.IsStatic) stringBuilder.Append ("static ");
                stringBuilder.AppendFormat("{0}\r\n", methodInfo.ToString());
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reflects over all the events for an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Events(Object o)
        {
            StringBuilder stringBuilder = new StringBuilder();

            EventInfo[] eventInfos = o.GetType().GetEvents();

            foreach (EventInfo eventInfo in eventInfos)
            {
                stringBuilder.Append ("\tevent ");
                if (eventInfo.IsMulticast) stringBuilder.Append ("multicast ");
                stringBuilder.AppendFormat("{0}\r\n", eventInfo.ToString());
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reflects over all the properties for an object
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Properties(Object o)
        {
            StringBuilder stringBuilder = new StringBuilder();

            PropertyInfo[] propertyInfos = o.GetType().GetProperties();

            foreach (PropertyInfo propertyInfo in propertyInfos)
            {
                stringBuilder.AppendFormat("\tproperty {0}\r\n", propertyInfo.ToString());
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Reflects over a type object
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string TypeInfo(Type type)
        {
            StringBuilder stringBuilder = new StringBuilder();

            
            if (type.IsPublic) stringBuilder.Append("public ");
            if (type.IsAbstract) stringBuilder.Append("abstract ");
            if (type.IsByRef) stringBuilder.Append("byref ");
            if (type.IsSealed) stringBuilder.Append("sealed ");
            if (type.IsImport) stringBuilder.Append("import ");
            if (type.IsClass) stringBuilder.Append("class ");
            if (type.IsArray) stringBuilder.Append("array ");
            if (type.IsCOMObject) stringBuilder.Append("comobject ");
            if (type.IsEnum) stringBuilder.Append("enum ");
            if (type.IsInterface) stringBuilder.Append("interface ");
            if (type.IsPointer) stringBuilder.Append("pointer ");
            stringBuilder.AppendFormat("{0} {{\r\n", type.Name);

            return stringBuilder.ToString();
        }


        /// <summary>
        /// Inspects an object by reflecting over its values and meta data
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string Inspect(Object o)
        {
            if (o == null) return "null";

            StringBuilder stringBuilder = new StringBuilder();

            stringBuilder.Append(string.Format("// Value={0}\r\n",o));
            stringBuilder.Append(TypeInfo(o.GetType()));
            stringBuilder.Append(Fields(o));
            stringBuilder.Append(Properties(o));
            stringBuilder.Append(Methods(o));
            stringBuilder.Append(Events(o));
            stringBuilder.Append("}\r\n");

            return stringBuilder.ToString();
        }
    }
}
