using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Data.MySQL
{
    public class Column
    {
        public string Name;

        public DataType Type;

        public int? Size = null;

        public bool Nullable = false;

        public bool AutoIncrement = false;
    }
}
