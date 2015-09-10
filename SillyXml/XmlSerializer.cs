using System;
using System.Collections;
using System.Linq;
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
                    var element = ToXml(value);
                    el.Add(element.HasElements
                        ? new XElement(property.Name, element.Elements())
                        : new XElement(property.Name, element.Value));
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
