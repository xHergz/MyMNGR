using NUnit.Framework;

using MyMNGR.Data.MySQL;
using System;

namespace MyMNGER_Tests
{
    public class Column_Tests
    {
        private const string COLUMN_NAME = "Column_Name";

        private const int COLUMN_SIZE = 100;

        private static string PRIMARY_KEY_COLUMN = $"{COLUMN_NAME} INT NOT NULL AUTO_INCREMENT";

        private static string VARIABLE_STRING_COLUMN = $"{COLUMN_NAME} VARCHAR({COLUMN_SIZE})";

        [Test]
        public void Parse_PrimaryKeyColumn()
        {
            Column column = Column.Parse(PRIMARY_KEY_COLUMN);
            Assert.AreEqual(COLUMN_NAME, column.Name);
            Assert.AreEqual(DataTypes.INT, column.Type.Type);
            Assert.IsNull(column.Type.Size);
            Assert.IsFalse(column.Nullable);
            Assert.IsTrue(column.AutoIncrement);
        }

        [Test]
        public void Parse_VariableSizeColumn()
        {
            Column column = Column.Parse(VARIABLE_STRING_COLUMN);
            Assert.AreEqual(COLUMN_NAME, column.Name);
            Assert.AreEqual(DataTypes.VARCHAR, column.Type.Type);
            Assert.AreEqual(COLUMN_SIZE, column.Type.Size);
            Assert.IsTrue(column.Nullable);
            Assert.IsFalse(column.AutoIncrement);
        }

        [Test]
        public void Parse_NotNullableVariableSizeColumn()
        {
            Column column = Column.Parse($"{VARIABLE_STRING_COLUMN} NOT NULL");
            Assert.AreEqual(COLUMN_NAME, column.Name);
            Assert.AreEqual(DataTypes.VARCHAR, column.Type.Type);
            Assert.AreEqual(COLUMN_SIZE, column.Type.Size);
            Assert.IsFalse(column.Nullable);
            Assert.IsFalse(column.AutoIncrement);
        }

        [Test]
        public void Parse_IncompleteColumn()
        {
            Assert.Throws<FormatException>(() => Column.Parse(COLUMN_NAME));
        }
    }
}
