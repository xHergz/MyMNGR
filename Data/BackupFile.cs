using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyMNGR.Data
{
    public class BackupFile
    {
        private enum VALID_EXTENSIONS {
            bak
        };

        private const string TIMESTAMP_FORMAT = "yyyy-MM-dd_HHmm";

        public string FullPath;

        public string DatabaseName;

        public DateTime Timestamp;

        public static BackupFile FromPath(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                throw fullPath == null 
                    ? new ArgumentNullException("Path cannot be null")
                    : new ArgumentException("Path cannot be empty/whitespace");
            }

            string[] pathPieces = fullPath.Split('\\');
            string fileName = pathPieces.LastOrDefault();
            Regex fileNamePattern = new Regex(@"(?<databaseName>[\w$]+)_(?<timestamp>[\d\-_]+).(?<fileExtension>\w+)", RegexOptions.None);
            Match match = fileNamePattern.Match(fileName);
            string databaseName = match.Groups["databaseName"].Value;
            string timestampString = match.Groups["timestamp"].Value;
            string fileExtension = match.Groups["fileExtension"].Value;

            if (!Enum.GetNames(typeof(VALID_EXTENSIONS)).Contains(fileExtension))
            {
                throw new ArgumentException($"Backup file has an invalid file extension (.{fileExtension})");
            }

            DateTime timestamp = DateTime.MinValue;
            try
            {
                timestamp = DateTime.ParseExact(timestampString, TIMESTAMP_FORMAT, CultureInfo.InvariantCulture);
            }
            catch(FormatException ex)
            {
                // Throw a more specific error message
                throw new FormatException($"Timestamp format did not match expected format: {TIMESTAMP_FORMAT}");
            }

            return new BackupFile()
            {
                FullPath = fullPath,
                DatabaseName = databaseName,
                Timestamp = timestamp
            };
        }
    }
}
