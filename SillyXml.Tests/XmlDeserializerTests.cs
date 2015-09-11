using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace SillyXml.Tests
{
    public class SimpleDeserializationClass
    {
        public SimpleDeserializationClass(int foo, string bar)
        {
            Foo = foo;
            Bar = bar;
        }

        public int Foo { get; }
        public string Bar { get; }
    }

    public class SimpleNestedDeserializationClass
    {
        public SimpleNestedDeserializationClass(SimpleDeserializationClass contained)
        {
            Contained = contained;
        }

        public SimpleDeserializationClass Contained { get; }
    }

    [TestFixture]
    public class XmlDeserializerTests
    {
        private static string Declaration { get; } = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes"" ?>";

        [Test]
        public void Deserialize_Simple_Class()
        {
            var xml = Declaration + @"<SimpleDeserializationClass><Foo>42</Foo><Bar>Banana</Bar></SimpleDeserializationClass>";
            var actual = XmlSerializer.Deserialize<SimpleDeserializationClass>(xml);
            Assert.IsNotNull(actual);
            Assert.AreEqual(42, actual.Foo);
            Assert.AreEqual("Banana", actual.Bar);
        }

        [Test]
        public void Deserialize_Simple_Nested_Class()
        {
            var xml = Declaration + @"<SimpleNestedDeserializationClass><Contained><Foo>42</Foo><Bar>Banana</Bar></Contained></SimpleNestedDeserializationClass>";
            var actual = XmlSerializer.Deserialize<SimpleNestedDeserializationClass>(xml);
            Assert.IsNotNull(actual);
            Assert.IsNotNull(actual.Contained);
            Assert.AreEqual(42, actual.Contained.Foo);
            Assert.AreEqual("Banana", actual.Contained.Bar);
        }
    }
}
