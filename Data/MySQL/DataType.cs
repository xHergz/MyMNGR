using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Data.MySQL
{
    public enum DataTypes
    {
        TINYINT,
        SMALLINT,
        MEDIUMINT,
        BIGINT,
        INT,
        INTEGER,
        DECIMAL,
        NUMERIC,
        FLOAT,
        DOUBLE,
        BIT,
        DATE,
        DATETIME,
        TIMESTAMP,
        TIME,
        YEAR,
        CHAR,
        VARCHAR,
        BINARY,
        VARBINARY,
        BLOB,
        TEXT,
        ENUM,
        SET,
    }

    public class DataType
    {
        public DataTypes Type { get; set; }

        public int? Size { get; set; }

        public static DataType Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw text == null
                    ? new ArgumentNullException("Data Type cannot be null")
                    : new ArgumentException("Data Type cannot be empty");
            }

            char[] brackets = { '(', ')' };
            string[] pieces = text.Split(brackets);
            int? size = null;
            bool validType = Enum.TryParse<DataTypes>(pieces[0], out DataTypes type);

            if (!validType)
            {
                throw new ArgumentException($"Unsupported type: {text}");
            }


            if (pieces.Length > 1 && int.TryParse(pieces[1], out int parsedSize))
            {
                size = parsedSize;
            }

            return new DataType()
            {
                Type = type,
                Size = size
            };
        }
    }
}
