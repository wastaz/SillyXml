using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using NUnit.Framework;

namespace SillyXml.Tests
{
    
    public class GenericClass<T>
    {
        public GenericClass(T obj)
        {
            Contained = obj;
        }

        public T Contained { get; }
    }
    
    public class SimpleClass
    {
        public int Foo { get; } = 42;
        public string Bar { get; } = "Banana";
    }
    
    public class ClassWithEnumerable
    {
        public IEnumerable<int> Collection { get; } = new List<int> { 42, 15, 22 };
    }


    public class ClassWithNestedObjects
    {
        public SimpleClass Monkey { get; } = new SimpleClass();
        public ClassWithEnumerable Avocado { get; } = new ClassWithEnumerable();
    }

    [TestFixture]
    public class XmlSerializerTests
    {
        private void AreEqualDisregardingWhitespace(string expected, string actual)
        {
            var normalizedExpected = Regex.Replace(expected, @"\s", "");
            var normalizedActual = Regex.Replace(actual, @"\s", "");

            Assert.AreEqual(normalizedExpected, normalizedActual);
        }

        [Test]
        public void Serialize_Single_Value()
        {
            var str = XmlSerializer.Serialize(42);
            AreEqualDisregardingWhitespace(@"<Int32>42</Int32>", str);
        }

        [Test]
        public void Serialize_Single_String()
        {
            var str = XmlSerializer.Serialize("Banana");
            AreEqualDisregardingWhitespace(@"<String>Banana</String>", str);
        }

        [Test]
        public void Serialize_Simple_Class()
        {
            var str = XmlSerializer.Serialize(new SimpleClass());
            AreEqualDisregardingWhitespace(@"<SimpleClass><Foo>42</Foo><Bar>Banana</Bar></SimpleClass>", str);
        }

        [Test]
        public void Serialize_Class_With_Enumerable()
        {
            var str = XmlSerializer.Serialize(new ClassWithEnumerable());
            AreEqualDisregardingWhitespace(@"<ClassWithEnumerable><Collection><Int32>42</Int32><Int32>15</Int32><Int32>22</Int32></Collection></ClassWithEnumerable>", str);
        }

        [Test]
        public void Serialize_Class_With_Nested_Objects()
        {
            var str = XmlSerializer.Serialize(new ClassWithNestedObjects());
            AreEqualDisregardingWhitespace(
                @"<ClassWithNestedObjects>
                    <Monkey><Foo>42</Foo><Bar>Banana</Bar></Monkey>
                    <Avocado><Collection><Int32>42</Int32><Int32>15</Int32><Int32>22</Int32></Collection></Avocado>
                  </ClassWithNestedObjects>", str);
        }

        [Test]
        public void Serialize_Generic_Class()
        {
            var str = XmlSerializer.Serialize(new GenericClass<SimpleClass>(new SimpleClass()));
            AreEqualDisregardingWhitespace(@"<GenericClassOfSimpleClass><Contained><Foo>42</Foo><Bar>Banana</Bar></Contained></GenericClassOfSimpleClass>", str);
        }
    }
}
