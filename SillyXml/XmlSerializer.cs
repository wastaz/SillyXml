using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace SillyXml
{
    internal class SerializerOptions
    {
        public string DataType { get; set; }
    }

    public class XmlSerializer
    {
        public static string Serialize(object obj)
        {
            var root = ToXml(obj);
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            return doc.Declaration + Environment.NewLine + root;
        }

        public static T Deserialize<T>(string xml)
        {
            return FromXml<T>(xml);
        }

        private static T FromXml<T>(string xml)
        {
            var type = typeof(T);
            return (T) FromXml(type, xml);
        }

        private static object FromXml(Type type, string xml)
        {
            var xmlDoc = XDocument.Parse(xml);
            if (xmlDoc.Root == null || xmlDoc.Root.Name.LocalName != NameForType(type))
            {
                throw new ArgumentException("Xml cannot be parsed into an object of the type " + type.Name, nameof(xml));
            }

            if (xmlDoc.Root.HasElements)
            {
                var names = new HashSet<string>(xmlDoc.Root.Elements().Select(e => e.Name.LocalName.ToLowerInvariant()));

                int maxScore = -1;
                ConstructorInfo constructor = null;
                foreach (var current in type.GetTypeInfo().DeclaredConstructors)
                {
                    int score = current.GetParameters().Select(p => p.Name).Count(name => names.Contains(name));
                    if (score > maxScore && score == current.GetParameters().Length)
                    {
                        constructor = current;
                        maxScore = score;
                    }
                }
                if (constructor != null)
                {
                    var elements = xmlDoc.Root.Elements().ToDictionary(e => e.Name.LocalName.ToLowerInvariant());
                    var parameters = constructor.GetParameters().Select(pi => ToType(pi.ParameterType, elements[pi.Name.ToLowerInvariant()])).ToArray();
                    var obj = constructor.Invoke(parameters);
                    return obj;
                }
            }
            throw new NotImplementedException();
        }

        private static object ToType(Type t, XElement node)
        {
            if (t == typeof(string))
            {
                return node.Value;
            }
            if (t == typeof(int))
            {
                return Convert.ToInt32(node.Value);
            }

            node.Name = NameForType(t);
            return FromXml(t, node.ToString());
        }

        private static XElement ToXml(object obj, SerializerOptions options = null)
        {
            var type = obj.GetType();
            var typeInfo = type.GetTypeInfo();
            var el = new XElement(NameForType(type));

            if(typeInfo.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                el.Value = Convert.ToString(obj, CultureInfo.InvariantCulture);
            }
            else if (typeInfo.IsEnum)
            {
                el.Value = Enum.GetName(type, obj);
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
