using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Utils
{
    public class FileManager
    {
        private const string SQL_EXTENSION = "*.sql";

        private const string TABLE_DIRECTORY = "Tables";

        private string _rootDirectory;

        public List<string> Tables { get; private set; }

        public FileManager()
        {
            _rootDirectory = string.Empty;
            Tables = new List<string>();
        }

        public void LoadFiles(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            Tables = GetSqlFiles(TABLE_DIRECTORY);
        }

        private List<string> GetSqlFiles(string folder)
        {
            return Directory.GetFiles($"{_rootDirectory}\\{folder}", SQL_EXTENSION, SearchOption.TopDirectoryOnly).ToList();
        }
    }
}
