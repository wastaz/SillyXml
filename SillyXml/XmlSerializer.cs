using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace SillyXml
{
    internal class SerializerOptions
    {
        public string DataType { get; set; }
    }

    // Support XmlIgnore

    public class XmlSerializer
    {
        public static string Serialize(object obj)
        {
            var root = ToXml(obj);
            return root.ToString();
        }

        private static XElement ToXml(object obj, SerializerOptions options = null)
        {
            var type = obj.GetType();
            var typeInfo = type.GetTypeInfo();
            var el = new XElement(NameForType(type));

            if(type.GetTypeInfo().IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                el.Value = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            else if (type == typeof(DateTime))
            {
                var format = DateFormatForOptions(options);
                el.Value = ((DateTime) obj).ToString(format);
            }
            else if (typeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
            {
                foreach (var val in (IEnumerable) obj)
                {
                    el.Add(ToXml(val));
                }
            }
            else
            { 
                foreach(var property in type.GetRuntimeProperties())
                {
                    SerializerOptions opt = null;
                    var ignoreAttr = property.GetCustomAttribute<XmlIgnoreAttribute>();
                    if (ignoreAttr != null)
                    {
                        continue;
                    }

                    var attr = property.GetCustomAttribute<XmlElementAttribute>();
                    if (attr != null)
                    {
                        opt = new SerializerOptions { DataType = attr.DataType, };
                    }

                    var value = property.GetMethod.Invoke(obj, new object[0]);
                    var resultElement = new XElement(property.Name);
                    if (value != null)
                    {
                        var element = ToXml(value, opt);
                        if (element.HasElements)
                        {
                            resultElement.Add(element.Elements());
                        }
                        else
                        {
                            resultElement.Value = element.Value;
                        }
                    }
                    el.Add(resultElement);
                }
            }
            return el;
        }


        private static string NameForType(Type t)
        {
            var typeInfo = t.GetTypeInfo();
            var name = t.Name;
            if (typeInfo.IsGenericType)
            {
                var idx = name.IndexOf('`');
                if (idx != -1)
                {
                    name = name.Remove(idx);
                }
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
