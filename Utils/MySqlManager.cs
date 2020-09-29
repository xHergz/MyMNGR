using MyMNGR.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Utils
{
    public class MySqlManager
    {
        private const string MYSQL = "mysql";

        private const string MYSQL_CONFIG_EDITOR = "mysql_config_editor";

        private ConsoleManager _consoleManager;

        private FileManager _fileManager;

        private SettingsManager _settingsManager;

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

        private bool CreateDatabase(string databaseName)
        {
            ProcessResult result = RunCommand($"CREATE DATABASE IF NOT EXISTS {databaseName};");
            return result.Success;
        }

        private void DropDatabase(string databaseName)
        {
            RunCommand($"DROP DATABASE {databaseName};");
        }

        private bool DoesDatabaseExist(string databaseName)
        {
            ProcessResult result = RunCommand($"SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}'");
            return result.Success && !result.IsEmpty;
        }

        private ProcessResult RunCommand(string command)
        {
            return RunProcess(MYSQL, $"--login-path={CurrentAlias} -e \"{command}\"");
        }

        private void RunFile(string filePath)
        {

        }

        private void RunConfigEditor(string action, string arguments)
        {

        }

        private ProcessResult RunProcess(string fileName, string arguments)
        {
            Process process = new Process();
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return new ProcessResult()
            {
                ExitCode = process.ExitCode,
                Output = output
            };
        }
    }
}
