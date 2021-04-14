using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HergBot.SqlParser.Data.MySQL;

using MyMNGR.Data;

namespace MyMNGR.Utils
{
    public class MySqlManager
    {
        private enum ConfigEditorAction
        {
            Print,
            Set
        }

        private const string MYSQL = "mysql";

        private const string MYSQL_CONFIG_EDITOR = "mysql_config_editor";

        private const string MYSQL_DUMP = "mysqldump";

        private delegate bool DatabaseAction(string databaseName);

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
            return PerformDatabaseAction(DbName, false, Deploy);
        }

        public bool ForceDeployDatabase()
        {
            _consoleManager.LogMessage($"Force deploying {DbName}...");
            return PerformDatabaseAction(DbName, true, Deploy);
        }

        public bool DropDatabase()
        {
            return PerformDatabaseAction(DbName, true);
        }

        public bool BackupDatabase()
        {
            return PerformDatabaseAction(DbName, false, Backup);
        }

        public bool RestoreDatabase()
        {
            _consoleManager.LogMessage($"Restoring {DbName}...");
            return PerformDatabaseAction(DbName, false, RestoreDatabase);
        }

        public bool ForceRestoreDatabase()
        {

            _consoleManager.LogMessage($"Force restoring {DbName}...");
            return PerformDatabaseAction(DbName, true, RestoreDatabase);
        }

        public bool SetAlias(Alias alias)
        {
            ProcessResult result = RunConfigEditor(ConfigEditorAction.Set, alias.ToArgs());
            if (!result.Success)
            {
                _consoleManager.LogMessage($"Failed to set alias {alias.Name}");
                _consoleManager.LogMessage(result.Error);
                return false;
            }
            _consoleManager.LogMessage($"Successfully set alias {alias.Name}");
            return true;
        }

        private bool PerformDatabaseAction(string databaseName, bool force, DatabaseAction action = null)
        {
            // Check if the current alias exists
            if (!DoesAliasExist(CurrentAlias))
            {
                _consoleManager.LogMessage($"The current alias \"{CurrentAlias}\" does not exist.");
                return false;
            }

            bool dbExists = DoesDatabaseExist(databaseName);
            // Check if you need to drop the database from a force
            if (force && dbExists && !DropDatabase(databaseName))
            {
                return false;
            }

            return action != null ? action(databaseName) : true;
        }

        private bool CreateDatabase(string databaseName)
        {
            ProcessResult result = RunSqlCommand($"CREATE DATABASE IF NOT EXISTS {databaseName};");
            return result.Success;
        }

        private bool DropDatabase(string databaseName)
        {
            _consoleManager.LogMessage($"Dropping database {databaseName}...");
            ProcessResult  result = RunSqlCommand($"DROP DATABASE {databaseName};");

            if (!result.Success)
            {
                _consoleManager.LogMessage($"Failed to drop datbase {databaseName}");
                _consoleManager.LogMessage(result.Error);
                return false;
            }

            _consoleManager.LogMessage($"Database dropped.");
            return result.Success;
        }

        private bool Deploy(string databaseName)
        {
            _consoleManager.LogMessage($"Deploying {DbName}...");

            if (DoesDatabaseExist(DbName))
            {
                _consoleManager.LogMessage("Database already exists.");
                return false;
            }

            // Create database
            if (!CreateDatabase(databaseName))
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

            foreach (string filePath in orderedFiles)
            {
                ProcessResult result = RunFile(filePath, databaseName);
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

        private bool Backup(string databaseName)
        {
            _consoleManager.LogMessage($"Backing up {DbName}...");
            ProcessResult result = RunBackupCommand(DbName);
            if (!result.Success)
            {
                _consoleManager.LogMessage($"Failed to back up {DbName}.");
                _consoleManager.LogMessage(result.Error);
                return false;
            }
            if (!_fileManager.WriteFile(result.Output, CreateBackupPath(DbName)))
            {
                _consoleManager.LogMessage($"Failed to write back up file for {DbName}.");
                return false;
            }
            _consoleManager.LogMessage($"Backup succeeded.");
            return true;
        }

        private BackupFile InitializeRestore(string databaseName)
        {
            try
            {
                string backupPath = _fileManager.SelectFile($"{BackupFolder(databaseName)}", "sql");
                if (string.IsNullOrWhiteSpace(backupPath))
                {
                    return null;
                }

                BackupFile backupFile = BackupFile.FromPath(backupPath);
                if (backupFile.DatabaseName != databaseName)
                {
                    MessageBoxResult result = MessageBox.Show(
                        "The backup file you selected is not for the current working database. Would you like to continue with the restore?",
                        "MyMNGR",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning,
                        MessageBoxResult.No
                    );
                    if (result == MessageBoxResult.No)
                    {
                        _consoleManager.LogMessage($"Restore canceled.");
                        return null;
                    }
                }
                return backupFile;
            }
            catch (Exception ex)
            {
                _consoleManager.LogMessage($"Failed to restore database.");
                _consoleManager.LogMessage(ex.Message);
                return null;
            }
        }

        private bool RestoreDatabase(string databaseName)
        {
            BackupFile backupFile = InitializeRestore(databaseName);
            if (backupFile == null)
            {
                return false;
            }

            if (DoesDatabaseExist(backupFile.DatabaseName))
            {
                _consoleManager.LogMessage($"Database already exists.");
                return false;
            }

            _consoleManager.LogMessage($"Restoring {backupFile.DatabaseName} from {backupFile.FullPath}");
            ProcessResult restoreResult = RunFile(backupFile.FullPath);
            if (!restoreResult.Success)
            {
                _consoleManager.LogMessage($"Failed to restore {backupFile.DatabaseName}.");
                _consoleManager.LogMessage(restoreResult.Error);
                return false;
            }

            _consoleManager.LogMessage("Restore succeeded.");
            return true;
        }

        private bool DoesDatabaseExist(string databaseName)
        {
            ProcessResult result = RunSqlCommand($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'");
            return result.Success && !result.IsOutputEmpty;
        }

        private bool DoesAliasExist(string alias)
        {
            ProcessResult result = RunConfigEditor(ConfigEditorAction.Print, $"--login-path={alias}");
            if (!result.Success)
            {
                _consoleManager.LogMessage("Failed to check alias");
                _consoleManager.LogMessage(result.Error);
                return false;
            }
            // The alias does not exist if there is no output
            return !string.IsNullOrWhiteSpace(result.Output);
        }

        private ProcessResult RunSqlCommand(string command)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} -e \"{command}\"");
        }

        private ProcessResult RunDatabaseCommand(string databaseName, string command)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} {databaseName} -e \"{command}\"");
        }

        private ProcessResult RunFile(string filePath, string databaseName = null)
        {
            return string.IsNullOrWhiteSpace(databaseName)
                ? RunProcess(MYSQL, $"--login-path={CurrentAlias} -e \"source {filePath}\"")
                : RunProcess(MYSQL, $"--login-path={CurrentAlias} {databaseName} -e \"source {filePath}\"");
        }

        private ProcessResult RunBackupCommand(string databaseName)
        {
            string args = $"--login-path={CurrentAlias} --protocol=tcp --port=3306 --default-character-set=utf8 --routines --skip-extended-insert"
                + $" --databases {databaseName}";
            return RunProcess(MYSQL_DUMP, args);
        }

        private ProcessResult RunConfigEditor(ConfigEditorAction action, string args)
        {
            string fullArguments = $"{action.ToString("G").ToLower()} {args}";
            return action == ConfigEditorAction.Set
                ? RunCommand(MYSQL_CONFIG_EDITOR, fullArguments)
                : RunProcess(MYSQL_CONFIG_EDITOR, fullArguments);
        }

        private string BackupFolder(string databaseName)
        {
            return $"{_settingsManager.BackupFolder}\\{databaseName}";
        }

        private string CreateBackupPath(string databaseName)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmm");
            return $"{BackupFolder(databaseName)}\\{databaseName}_{timestamp}.sql";
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

        private ProcessResult RunCommand(string fileName, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.WaitForExit();
            return new ProcessResult()
            {
                Error = string.Empty,
                ExitCode = process.ExitCode,
                Output = string.Empty
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
