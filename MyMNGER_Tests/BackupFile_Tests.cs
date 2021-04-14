using System;

using NUnit.Framework;

using MyMNGR.Data;

namespace MyMNGER_Tests
{
    class BackupFile_Tests
    {
        private const string DATABASE_NAME = "DatabaseName";

        private const string TIMESTAMP = "2021-03-27_0749";

        private const string EXTENSION = "sql";

        private static readonly string PATH_WITH_INVALID_EXTENSION = $"C:\\Files\\{DATABASE_NAME}_{TIMESTAMP}.shoe";

        private static readonly string PATH_WITH_INVALID_TIMESTAMP = $"C:\\Files\\{DATABASE_NAME}_03-04-21_715.{EXTENSION}";

        private static readonly string VALID_PATH = $"C:\\Files\\{DATABASE_NAME}_{TIMESTAMP}.{EXTENSION}";

        [TestCase(null, typeof(ArgumentNullException), TestName = "FromPath_Null")]
        [TestCase("", typeof(ArgumentException), TestName = "FromPath_EmptyString")]
        [TestCase("   ", typeof(ArgumentException), TestName = "FromPath_Whitespace")]
        public void FromPath_InvalidPath(string path, Type exceptionType)
        {
            TestDelegate testMethod = () => BackupFile.FromPath(path);
            typeof(Assert).GetMethod("Throws", new Type[] { typeof(TestDelegate) }).MakeGenericMethod(exceptionType).Invoke(null, new object[] { testMethod });
        }

        [Test]
        public void FromPath_InvalidFileExtension()
        {
            Assert.Throws<ArgumentException>(() => BackupFile.FromPath(PATH_WITH_INVALID_EXTENSION));
        }

        [Test]
        public void FromPath_InvalidTimestamp()
        {
            Assert.Throws<FormatException>(() => BackupFile.FromPath(PATH_WITH_INVALID_TIMESTAMP));
        }

        [Test]
        public void FromPath_Valid()
        {
            BackupFile file = BackupFile.FromPath(VALID_PATH);
            file.FullPath = VALID_PATH;
            Assert.AreEqual(DATABASE_NAME, file.DatabaseName);
            Assert.AreEqual(2021, file.Timestamp.Year);
            Assert.AreEqual(03, file.Timestamp.Month);
            Assert.AreEqual(27, file.Timestamp.Day);
            Assert.AreEqual(07, file.Timestamp.Hour);
            Assert.AreEqual(49, file.Timestamp.Minute);
        }
    }
}
