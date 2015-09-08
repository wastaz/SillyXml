using System;
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
            var el = new XElement(type.Name);

            foreach(var property in type.GetRuntimeProperties())
            {
                var value = property.GetMethod.Invoke(obj, new object[0]);
                el.Add(new XElement(property.Name, value));
            }
            return el;
        }

    }
}
