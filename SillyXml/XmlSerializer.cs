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
            var el = new XElement(type.Name);

            if(type.GetTypeInfo().IsValueType)
            {
                el.Value = obj.ToString();
            }
            else
            { 
                foreach(var property in type.GetRuntimeProperties())
                {
                    var value = property.GetMethod.Invoke(obj, new object[0]);
                
                    if(value.GetType() != typeof(string) && value.GetType().GetTypeInfo().ImplementedInterfaces.Contains(typeof(IEnumerable)))
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
                        el.Add(new XElement(property.Name, value));
                    }
                }
            }
            return el;
        }

    }
}
