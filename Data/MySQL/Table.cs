using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyMNGR.Data.MySQL
{
    public class Table
    {
        private const string COMMENT_INDICATOR = "--";

        public static Table Parse(string command)
        {
            Regex tablePattern = new Regex(@"CREATE TABLE\s(?<tableName>\w+)[\n\r\s]+\((?<tableContent>.*?)\);", RegexOptions.Singleline);
            Match match = tablePattern.Match(command);
            string tableName = match.Groups["tableName"].Value;
            string tableContents = match.Groups["tableContents"].Value;
            return new Table(tableName, new Dictionary<string, Column>(), new HashSet<string>(), new Dictionary<string, ForeignKey>());
        }

        public string Name { get; private set; }

        public Dictionary<string, Column> Columns { get; private set; }

        public HashSet<string> PrimaryKeys { get; private set; }

        public Dictionary<string, ForeignKey> ForeignKeys { get; private set; }

        public Table(string name, Dictionary<string, Column> columns, HashSet<string> primaryKeys, Dictionary<string, ForeignKey> foreignKeys)
        {
            Name = name;
            Columns = columns;
            PrimaryKeys = primaryKeys;
            ForeignKeys = foreignKeys;
        }

        public IEnumerable<string> GetDependencies()
        {
            if (!ForeignKeys.Any())
            {
                return Enumerable.Empty<string>();
            }

            return ForeignKeys.Values.Select(fKey => fKey.ForeignTable).Distinct();
        }
    }
}
