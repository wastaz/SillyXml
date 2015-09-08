using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NUnit.Framework;

namespace SillyXml.Tests
{

    

    [TestFixture]
    public class XmlSerializerTests
    {
        public class SimpleClass
        {
            public int Foo { get; } = 42;
            public string Bar { get; } = "Banana";
        }

        [Test]
        public void Serialize_Simple_Class()
        {
            var str = XmlSerializer.Serialize(new SimpleClass());
            Assert.IsNotNull(str);
        }

    }
}
