using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;

namespace SillyXml
{
    public class XmlSerializer
    {
        public static string Serialize(object obj)
        {
            var root = ToXml(obj);
            return root.ToString();
        }

        private static XElement ToXml(object obj)
        {
            var type = obj.GetType();
            var typeInfo = type.GetTypeInfo();
            var el = new XElement(NameForType(type));

            if(type.GetTypeInfo().IsPrimitive || type == typeof(string))
            {
                el.Value = obj.ToString();
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
                    var value = property.GetMethod.Invoke(obj, new object[0]);
                    var valueType = value.GetType();
                    var valueTypeInfo = valueType.GetTypeInfo();
                    if (value is string || valueTypeInfo.IsPrimitive)
                    {
                        el.Add(new XElement(property.Name, value));
                    }
                    else if (valueTypeInfo.ImplementedInterfaces.Contains(typeof(IEnumerable)))
                    {
                        var collectionElement = new XElement(property.Name);
                        foreach(var val in (IEnumerable)value)
                        {
                            collectionElement.Add(ToXml(val));
                        }
                        el.Add(collectionElement);
                    }
                    else
                    {
                        var xml = ToXml(value);
                        el.Add(new XElement(property.Name, xml.Elements()));
                    }
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
    }
}
