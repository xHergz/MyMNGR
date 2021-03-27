using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.WindowsAPICodePack.Dialogs;

namespace MyMNGR.Utils
{
    public class FileManager
    {
        private const string SQL_EXTENSION = "*.sql";

        private const string DATA_DIRECTORY = "Data";

        private const string FUNCTIONS_DIRECTORY = "Functions";

        private const string STORED_PROCEDURES_DIRECTORY = "StoredProcedures";

        private const string TABLES_DIRECTORY = "Tables";

        private const string VIEWS_DIRECTORY = "Views";

        private string _rootDirectory;

        public IEnumerable<string> Data { get; private set; }

        public IEnumerable<string> Functions { get; private set; }

        public IEnumerable<string> StoredProcedures { get; private set; }

        public IEnumerable<string> Tables { get; private set; }

        public IEnumerable<string> Views { get; private set; }

        public FileManager()
        {
            _rootDirectory = string.Empty;
            Tables = new List<string>();
        }

        public void LoadFiles(string rootDirectory)
        {
            _rootDirectory = rootDirectory;
            Data = GetSqlFiles(DATA_DIRECTORY);
            Functions = GetSqlFiles(FUNCTIONS_DIRECTORY);
            StoredProcedures = GetSqlFiles(STORED_PROCEDURES_DIRECTORY);
            Tables = GetSqlFiles(TABLES_DIRECTORY);
            Views = GetSqlFiles(VIEWS_DIRECTORY);
        }

        public string GetFile(string fullPath)
        {
            if (!File.Exists(fullPath))
            {
                return null;
            }
            return File.ReadAllText(fullPath);
        }

        public bool WriteFile(string contents, string fullPath)
        {
            try
            {
                File.WriteAllText(fullPath, contents);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public string SelectFile(string folder)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            CommonFileDialogResult result = dialog.ShowDialog();
            return result == CommonFileDialogResult.Ok ? dialog.FileName : string.Empty;
        }

        private IEnumerable<string> GetSqlFiles(string folder)
        {
            string fullDirectory = $"{_rootDirectory}\\{folder}";
            if (!Directory.Exists(fullDirectory))
            {
                return Enumerable.Empty<string>();
            }
            return Directory.GetFiles(fullDirectory, SQL_EXTENSION, SearchOption.TopDirectoryOnly);
        }
    }
}
