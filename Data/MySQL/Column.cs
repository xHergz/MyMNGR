using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Data.MySQL
{
    public class Column
    {
        private const string AUTO_INCREMENT = "AUTO_INCREMENT";

        private const string NOT_NULL = "NOT NULL";

        public string Name;

        public DataType Type;

        public bool Nullable = false;

        public bool AutoIncrement = false;

        public static Column Parse(string command)
        {
            string[] pieces = command.Trim().Split(' ');
            if (pieces.Length < 2)
            {
                throw new FormatException("A column requires at least 2 space separated pieces (name and type)");
            }

            string columnName = pieces[0];
            DataType type = DataType.Parse(pieces[1]);

            return new Column()
            {
                Name = columnName,
                Type = type,
                Nullable = !command.Contains(NOT_NULL),
                AutoIncrement = command.Contains(AUTO_INCREMENT),
            };
        }
    }
}
