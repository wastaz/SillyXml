﻿using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace SillyXml
{
    internal class SerializerOptions
    {
        public string DataType { get; set; }
    }
    public interface IXmlWritable
    {
        XNode WriteXml();
    }

    public class XmlSerializer
    {
        public static string Serialize(object obj)
        {
            var root = ToXml(obj);
            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            return doc.Declaration + Environment.NewLine + root;
        }

        private static XNode ToXmlNode(object obj, SerializerOptions opt)
        {
            var writable = obj as IXmlWritable;
            if (writable != null) { return writable.WriteXml(); }
            return ToXml(obj, opt);
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
                        var element = ToXmlNode(value, opt);

                        var xelem = element as XElement;
                        if (xelem != null)
                        {
                            if (xelem.HasElements)
                            {
                                resultElement.Add(xelem.Elements());
                            }
                            else
                            {
                                resultElement.Value = xelem.Value;
                            }
                        }
                        else
                        {
                            resultElement.Add(element);
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
