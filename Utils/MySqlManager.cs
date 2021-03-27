using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HergBot.SqlParser.Data.MySQL;

using MyMNGR.Data;

namespace MyMNGR.Utils
{
    public class MySqlManager
    {
        private const string MYSQL = "mysql";

        private const string MYSQL_CONFIG_EDITOR = "mysql_config_editor";

        private const string MYSQL_DUMP = "mysqldump";

        private ConsoleManager _consoleManager;

        private FileManager _fileManager;

        private SettingsManager _settingsManager;

        private Dictionary<string, Table> _tables;

        private string CurrentAlias
        {
            get
            {
                return _settingsManager.CurrentTarget == Target.Production
                    ? _settingsManager.CurrentProfile.ProdAlias
                    : _settingsManager.CurrentProfile.DevAlias;
            }
        }

        private string DbName { get { return _settingsManager.CurrentProfile.DatabaseName; } }

        public MySqlManager(ConsoleManager console, FileManager files, SettingsManager settings)
        {
            _consoleManager = console;
            _fileManager = files;
            _settingsManager = settings;

            _tables = new Dictionary<string, Table>();

            LoadSchema();
        }

        public void LoadSchema()
        {
            foreach(string tableFile in _fileManager.Tables)
            {
                string tableContents = _fileManager.GetFile(tableFile);
                if (string.IsNullOrWhiteSpace(tableContents))
                {
                    _consoleManager.LogMessage($"{tableFile} is empty.");
                }
                try
                {
                    _tables.Add(tableFile, Table.Parse(tableContents));
                }
                catch(Exception e)
                {
                    _consoleManager.LogMessage($"Failed to parse {tableFile}.");
                    _consoleManager.LogMessage(e.Message);
                }
            }
        }

        public bool DeployDatabase()
        {
            _consoleManager.LogMessage($"Deploying {DbName}...");

            if (DoesDatabaseExist(DbName))
            {
                _consoleManager.LogMessage("Database already exists.");
                return false;
            }

            // Create database
            if (!CreateDatabase(DbName))
            {
                _consoleManager.LogMessage("Failed to create the database.");
                return false;
            }

            // Call each file in order: Tables, Views, Functions, Stored Procs, Data
            IEnumerable<string> orderedFiles = new List<string>()
                .Concat(GetOrderedTables())
                .Concat(_fileManager.Views)
                .Concat(_fileManager.Functions)
                .Concat(_fileManager.StoredProcedures)
                .Concat(_fileManager.Data);

            foreach(string filePath in orderedFiles)
            {
                ProcessResult result = RunFile(DbName, filePath);
                if (!result.Success)
                {
                    _consoleManager.LogMessage($"Failed to run {filePath}.");
                    _consoleManager.LogMessage(result.Error);
                    return false;
                }
            }

            _consoleManager.LogMessage("Deploy succeeded.");
            return true;
        }

        public bool ForceDeployDatabase()
        {
            _consoleManager.LogMessage($"Force deploying {DbName}...");
            if (DoesDatabaseExist(DbName))
            {
                _consoleManager.LogMessage($"Database already exists, dropping {DbName}...");
                DropDatabase(DbName);
            }

            return DeployDatabase();
        }

        public void DropDatabase()
        {
            _consoleManager.LogMessage($"Dropping {DbName}...");
            DropDatabase(DbName);
            _consoleManager.LogMessage($"Database dropped.");
        }

        public void BackupDatabase()
        {
            _consoleManager.LogMessage($"Backing up {DbName}...");
            ProcessResult result = RunBackupCommand(DbName);
            if (!result.Success)
            {
                _consoleManager.LogMessage($"Failed to back up {DbName}.");
                _consoleManager.LogMessage(result.Error);
                return;
            }
            if (!_fileManager.WriteFile(result.Output, CreateBackupPath(DbName)))
            {
                _consoleManager.LogMessage($"Failed to write back up file for {DbName}.");
                return;
            }
            _consoleManager.LogMessage($"Backup succeeded.");
        }

        public void RestoreDatabase()
        {
            //_fileManager.
        }

        private bool CreateDatabase(string databaseName)
        {
            ProcessResult result = RunSystemCommand($"CREATE DATABASE IF NOT EXISTS {databaseName};");
            return result.Success;
        }

        private void DropDatabase(string databaseName)
        {
            RunSystemCommand($"DROP DATABASE {databaseName};");
        }

        private bool DoesDatabaseExist(string databaseName)
        {
            ProcessResult result = RunSystemCommand($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'");
            return result.Success && !result.IsOutputEmpty;
        }

        private ProcessResult RunSystemCommand(string command)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} -e \"{command}\"");
        }

        private ProcessResult RunCommand(string databaseName, string command)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} {databaseName} -e \"{command}\"");
        }

        private ProcessResult RunFile(string databaseName, string filePath)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} {databaseName} -e \"source {filePath}\"");
        }

        private ProcessResult RunBackupCommand(string databaseName)
        {
            string args = $"--login-path={CurrentAlias} --protocol=tcp --port=3306 --default-character-set=utf8 --routines --skip-extended-insert"
                + $" --databases {databaseName}";
            return RunProcess(MYSQL_DUMP, args);
        }

        private string CreateBackupPath(string databaseName)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
            return $"{_settingsManager.BackupFolder}\\{databaseName}_{timestamp}.sql";
        }

        private void RunConfigEditor(string action, string arguments)
        {

        }

        private ProcessResult RunProcess(string fileName, string arguments)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            //process.StartInfo.Verb = "runas";
            //process.StartInfo.WorkingDirectory = "C:\\Users\\Justin\\Documents\\MyMNGR\\";
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult()
            {
                Error = error,
                ExitCode = process.ExitCode,
                Output = output
            };
        }

        private IEnumerable<string> GetOrderedTables()
        {
            // Table name/full path pairs. Start off with tables that have no dependencies
            Dictionary<string, string> tableOrder = _tables
                .Where(table => !table.Value.GetDependencies().Any())
                .ToDictionary(table => table.Value.Name, table => table.Key);

            // All tables with any dependencies are left
            Dictionary<string, Table> remainingTables = _tables
                .Where(x => x.Value.GetDependencies().Any())
                .ToDictionary(table => table.Key, table => table.Value);

            while (remainingTables.Any())
            {
                if (tableOrder.Count > _tables.Count)
                {
                    _consoleManager.LogMessage("Error ordering tables: There are more tables in the ordered list than there are tables");
                    break;
                }

                // Tables where all dependencies are already in the table order list (will already exist)
                List<string> eligibleTables = remainingTables
                    .Where(table => table.Value.GetDependencies().All(dependencyTableName => tableOrder.ContainsKey(dependencyTableName)))
                    .Select(table => table.Key)
                    .ToList();

                // It should be impossible in a proper set of table files to be in a position where there are tables left but none are eligible
                if (!eligibleTables.Any())
                {
                    _consoleManager.LogMessage("Error ordering tables: There is a table with dependencies that can't be resolved.");
                    break;
                }

                foreach(string eligibleTable in eligibleTables)
                {
                    tableOrder.Add(remainingTables[eligibleTable].Name, eligibleTable);
                   remainingTables.Remove(eligibleTable);
                }
            }

            return tableOrder.Values;
        }
    }
}
