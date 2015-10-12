using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Xml;

namespace SillyXml
{
    internal class SerializerOptions
    {
        public string DataType { get; set; }
    }

    public interface IXmlWritable
    {
        Task WriteXml(XmlWriter writer);
    }

    public class XmlSerializer
    {
        public static async Task<string> Serialize(object obj)
        {
            using (var str = new MemoryStream()) { 
                await SerializeToStream(obj, str);

                str.Position = 0;
                using (var reader = new StreamReader(str))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }

        public static async Task SerializeToStream(object obj, Stream stream)
        {
            using (var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Async = true, OmitXmlDeclaration = false }))
            {
                await writer.WriteStartDocumentAsync(true);

                var type = obj.GetType();
                var nameForType = NameForType(type);
                await writer.WriteStartElementAsync(null, nameForType, null);
                await ToXml(obj, writer);
                await writer.WriteEndElementAsync();
            }
        }

        private static async Task ToXmlNode(object obj, XmlWriter writer, SerializerOptions opt)
        {
            var writable = obj as IXmlWritable;
            if (writable != null)
            {
                await writable.WriteXml(writer);
                return;
            }
            await ToXml(obj, writer, opt);
        }

        private static async Task ToXml(object obj, XmlWriter writer, SerializerOptions options = null)
        {
            var type = obj.GetType();
            var typeInfo = type.GetTypeInfo();

            if (typeInfo.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                await writer.WriteStringAsync(Convert.ToString(obj, CultureInfo.InvariantCulture));
            }
            else if (typeInfo.IsEnum)
            {
                await writer.WriteStringAsync(Enum.GetName(type, obj));
            }
            else if (type == typeof(DateTime))
            {
                var format = DateFormatForOptions(options);
                await writer.WriteStringAsync(((DateTime)obj).ToString(format));
            }
            else if (typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
            {
                foreach (var val in (IEnumerable) obj)
                {
                    await writer.WriteStartElementAsync(null, NameForType(val.GetType()), null);
                    await ToXml(val, writer);
                    await writer.WriteEndElementAsync();
                }
            }
            else
            { 
                foreach(var property in type.GetRuntimeProperties())
                {
                    SerializerOptions opt = null;
                    var ignoreAttr = property.GetCustomAttribute<XmlIgnoreAttribute>();
                    var isStatic = property.GetMethod.IsStatic;
                    if (ignoreAttr != null || isStatic)
                    {
                        continue;
                    }

                    var attr = property.GetCustomAttribute<XmlElementAttribute>();
                    if (attr != null)
                    {
                        opt = new SerializerOptions { DataType = attr.DataType, };
                    }

                    var value = property.GetMethod.Invoke(obj, new object[0]);
                    await writer.WriteStartElementAsync(null, property.Name, null);
                    if (value != null)
                    {
                        await ToXmlNode(value, writer, opt);
                    }
                    await writer.WriteEndElementAsync();
                }
            }

        }

        private static bool IsAnonymousType(Type t, TypeInfo ti)
        {
            // This is not a perfect way to detect anonymous classes since they are a compiler feature and not a CLR feature.
            // It is probably good enough though.
            // See also Jon Skeets exhaustive answer on anonymous classes: http://stackoverflow.com/a/315186/271746
            return
                t.Namespace == null && 
                ti.IsPublic == false &&
                t.IsNested == false &&
                ti.IsGenericType && 
                ti.IsSealed &&
                ti.GetCustomAttribute<CompilerGeneratedAttribute>() != null;
        }

        private static string NameForType(Type t)
        {
            var ti = t.GetTypeInfo();
            var name = t.Name;
            if (IsAnonymousType(t, ti))
            {
                name = "AnonymousTypeOf";
                name += string.Join("And", t.GenericTypeArguments.Select(NameForType).ToArray());
            }
            else if (ti.IsGenericType)
            {
                var idx = name.IndexOf('`');
                if (idx != -1)
                {
                    name = name.Remove(idx);
                }
                name += "Of";
                name += string.Join("And", t.GenericTypeArguments.Select(NameForType).ToArray());
            }else if (ti.IsArray)
            {
                name = "Array";
                name += "Of";
                name += string.Join("And", t.GenericTypeArguments.Select(NameForType).ToArray());
            }
            return name;
        }

        private static string DateFormatForOptions(SerializerOptions options)
        {
            if (options != null)
            {
                switch (options.DataType.ToLowerInvariant())
                {
                    case "date":
                        return "yyyy-MM-dd";
                    case "time":
                        return "HH:mm:ss";
                }
            }
            return "yyyy-MM-ddTHH:mm:ss";
        }
    }
}
