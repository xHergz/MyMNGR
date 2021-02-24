using NUnit.Framework;

using MyMNGR.Data.MySQL;

namespace MyMNGER_Tests
{
    public class Table_Tests
    {
        private const string TABLE_NAME = "Table";

        private const string COLUMN_ONE = "Column_One";

        private const string COLUMN_TWO = "Column_Two";

        private const string COLUMN_THREE = "Column_Three";

        private const string PARENT_TABLE_NAME = "ParentTable";

        private const string PARENT_COLUMN_NAME = "Parent_Column";

        private static string FULL_TABLE =
            $@"CREATE TABLE {TABLE_NAME}
            (
                {COLUMN_ONE} INT NOT NULL AUTO_INCREMENT,
                {COLUMN_TWO} INT NOT NULL,
                {COLUMN_THREE} VARCHAR(100) NOT NULL,
                PRIMARY KEY ({COLUMN_ONE}),
                FOREIGN KEY ({COLUMN_TWO}) {PARENT_TABLE_NAME}({PARENT_COLUMN_NAME})
            );";

        [SetUp]
        protected void SetUp()
        {

        }

        [Test]
        public void Parse_FullTable()
        {
            Table table = Table.Parse(FULL_TABLE);
            Assert.AreEqual(TABLE_NAME, table.Name);
        }
    }
}
