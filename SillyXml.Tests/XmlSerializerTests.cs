using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;
using NUnit.Framework;
using System.Xml.Schema;
using System.Xml;
using System.Xml.Linq;

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

    public class ClassWithStaticProperty
    {
        public static string Foo { get; } = "Baboon";
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

    public class ClassWithArrayEnumerable
    {
        public IEnumerable<int> Collection { get; } = new List<int> { 42, 15, 22 }.ToArray();
    }

    public class ClassWithNull
    {
        public object NullObject { get; } = null;
    }

    public class ClassWithNestedObjects
    {
        public SimpleClass Monkey { get; } = new SimpleClass();
        public ClassWithEnumerable Avocado { get; } = new ClassWithEnumerable();
    }
    
    public class ClassWithDateTimes
    {
        [XmlElement(DataType = "date")]
        public DateTime AsDate { get; } = DateTime.Parse("2014-02-01T22:15:00");

        [XmlElement(DataType = "time")]
        public DateTime AsTime { get; } = DateTime.Parse("2014-02-01T22:15:00");

        public DateTime AsDateAndTime { get; } = DateTime.Parse("2014-02-01T22:15:00");
    }

    public class ClassWithIgnoredProperties
    {
        [XmlIgnore]
        public string Foo { get; } = "Banana";

        public string Bar { get; } = "Baboon";
    }

    public enum MonkeyBreed
    {
        None = 0,
        Baboon,
        Gorilla,
        Chimpanzee
    }

    public class ClassWithEnum
    {
        public MonkeyBreed Breed { get; } = MonkeyBreed.Gorilla;
    }

    public class ClassContainingValueType
    {
        public ValueType Id { get; }
        public ClassContainingValueType(ValueType id)
        {
            Id = id;
        }
    }

    public struct ValueType : IEquatable<ValueType>, IXmlWritable
    {
        public int Value { get; }

        public ValueType(int value)
        {
            this.Value = value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public bool Equals(ValueType obj)
        {
            return obj.Value == Value;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj is ValueType)
            {
                return base.Equals((ValueType)obj);
            }
            return false;
        }

        public async Task WriteXml(XmlWriter writer)
        {
            await writer.WriteStringAsync(Value.ToString());
        }
    }


    [TestFixture]
    public class XmlSerializerTests
    {
        private static string Declaration { get; } = @"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes"" ?>";

        private void AreEqualXmlDisregardingWhitespace(string expected, string actual)
        {
            var normalizedExpected = Regex.Replace(Declaration + expected, @"\s", "");
            var normalizedActual = Regex.Replace(actual, @"\s", "");

            Assert.AreEqual(normalizedExpected, normalizedActual);
        }

        [Test]
        public async Task Serialize_Single_Value()
        {
            var str = await XmlSerializer.Serialize(42);
            AreEqualXmlDisregardingWhitespace(@"<Int32>42</Int32>", str);
        }

        [Test]
        public async Task Serialize_Single_String()
        {
            var str = await XmlSerializer.Serialize("Banana");
            AreEqualXmlDisregardingWhitespace(@"<String>Banana</String>", str);
        }

        [Test]
        public async Task Serialize_Decimal()
        {
            var str = await XmlSerializer.Serialize(3.14m);
            AreEqualXmlDisregardingWhitespace(@"<Decimal>3.14</Decimal>", str);
        }

        [Test]
        public async Task Serialize_Enum()
        {
            var str = await XmlSerializer.Serialize(new ClassWithEnum());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithEnum><Breed>Gorilla</Breed></ClassWithEnum>", str);
        }

        [Test]
        public async Task Serialize_Simple_Class()
        {
            var str = await XmlSerializer.Serialize(new SimpleClass());
            AreEqualXmlDisregardingWhitespace(@"<SimpleClass><Foo>42</Foo><Bar>Banana</Bar></SimpleClass>", str);
        }

        [Test]
        public async Task Serialize_Class_With_Enumerable()
        {
            var str = await XmlSerializer.Serialize(new ClassWithEnumerable());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithEnumerable><Collection><Int32>42</Int32><Int32>15</Int32><Int32>22</Int32></Collection></ClassWithEnumerable>", str);
        }

        [Test]
        public async Task Serialize_Class_With_Array_Enumerable()
        {
            var str = await XmlSerializer.Serialize(new ClassWithArrayEnumerable());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithArrayEnumerable><Collection><Int32>42</Int32><Int32>15</Int32><Int32>22</Int32></Collection></ClassWithArrayEnumerable>", str);
        }

        [Test]
        public async Task Seriazlize_Property_With_Null_Value()
        {
            var str = await XmlSerializer.Serialize(new ClassWithNull());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithNull><NullObject /></ClassWithNull>", str);
        }

        [Test]
        public async Task Serialize_DateTimes()
        {
            var str = await XmlSerializer.Serialize(new ClassWithDateTimes());
            AreEqualXmlDisregardingWhitespace(
                @"<ClassWithDateTimes>
                    <AsDate>2014-02-01</AsDate>
                    <AsTime>22:15:00</AsTime>
                    <AsDateAndTime>2014-02-01T22:15:00</AsDateAndTime>
                  </ClassWithDateTimes>", str);
        }

        [Test]
        public async Task Serialize_Skips_Ignored_Properties()
        {
            var str = await XmlSerializer.Serialize(new ClassWithIgnoredProperties());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithIgnoredProperties><Bar>Baboon</Bar></ClassWithIgnoredProperties>", str);
        }


        [Test]
        public async Task Serialize_Class_With_Nested_Objects()
        {
            var str = await XmlSerializer.Serialize(new ClassWithNestedObjects());
            AreEqualXmlDisregardingWhitespace(
                @"<ClassWithNestedObjects>
                    <Monkey><Foo>42</Foo><Bar>Banana</Bar></Monkey>
                    <Avocado><Collection><Int32>42</Int32><Int32>15</Int32><Int32>22</Int32></Collection></Avocado>
                  </ClassWithNestedObjects>", str);
        }

        [Test]
        public async Task Serialize_Generic_Class()
        {
            var str = await XmlSerializer.Serialize(new GenericClass<SimpleClass>(new SimpleClass()));
            AreEqualXmlDisregardingWhitespace(@"<GenericClassOfSimpleClass><Contained><Foo>42</Foo><Bar>Banana</Bar></Contained></GenericClassOfSimpleClass>", str);
        }

        [Test]
        public async Task  Serialize_Anonymous_Class()
        {
            var str = await XmlSerializer.Serialize(new { Foo = 42, Bar = "Banana" });
            AreEqualXmlDisregardingWhitespace(@"<AnonymousTypeOfInt32AndString><Foo>42</Foo><Bar>Banana</Bar></AnonymousTypeOfInt32AndString>", str);
        }

        [Test]
        public async Task Serialize_Ignores_Static_Properties_Of_The_Class()
        {
            var str = await XmlSerializer.Serialize(new ClassWithStaticProperty());
            AreEqualXmlDisregardingWhitespace(@"<ClassWithStaticProperty />", str);
        }

        [Test]
        public async Task Serialize_value_type()
        {
            var str = await XmlSerializer.Serialize(new ClassContainingValueType( new ValueType(12) ));
            AreEqualXmlDisregardingWhitespace(@"<ClassContainingValueType><Id>12</Id></ClassContainingValueType>", str);
        }
    }
}
