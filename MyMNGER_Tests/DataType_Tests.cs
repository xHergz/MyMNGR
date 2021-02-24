using NUnit.Framework;

using MyMNGR.Data.MySQL;
using System;

namespace MyMNGER_Tests
{
    public class DataType_Tests
    {
        private const string REGULAR_TYPE = "INT";

        private const string SIZE_TYPE = "VARCHAR(100)";

        [Test]
        public void Parse_RegularType()
        {
            DataType type = DataType.Parse(REGULAR_TYPE);
            Assert.AreEqual(DataTypes.INT, type.Type);
            Assert.IsNull(type.Size);
        }

        [Test]
        public void Parse_SizeType()
        {
            DataType type = DataType.Parse(SIZE_TYPE);
            Assert.AreEqual(DataTypes.VARCHAR, type.Type);
            Assert.AreEqual(100, type.Size);
        }

        [TestCase("unknown", typeof(ArgumentException), TestName = "Parse_UnknownType")]
        [TestCase("", typeof(ArgumentException), TestName = "Parse_EmptyString")]
        [TestCase(null, typeof(ArgumentNullException), TestName = "Parse_Null")]
        public void Parse_InvalidInput(string value, Type exceptionType)
        {
            TestDelegate testMethod = () => DataType.Parse(value);
            typeof(Assert).GetMethod("Throws", new Type[] { typeof(TestDelegate) }).MakeGenericMethod(exceptionType).Invoke(null, new object[] { testMethod });
        }
    }
}
