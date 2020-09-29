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

        private Profile _profile;

        private Target _currentTarget;

        private string CurrentAlias { get { return _currentTarget == Target.Production ? _profile.ProdAlias : _profile.DevAlias; } }

        public MySqlManager(Profile profile, Target initialTarget)
        {
            _profile = profile;
            _currentTarget = initialTarget;
        }

        public bool DeployDatabase()
        {
            // Find all files in directory

            // Create database
            CreateDatabase(_profile.DatabaseName);

            // Call each file in order: Tables, Views, Functions, Stored Procs, Data

            return true;
        }

        public void SwitchTarget(Target newTarget)
        {
            _currentTarget = newTarget;
        }

        private void CreateDatabase(string databaseName)
        {
            RunCommand($"CREATE DATABASE IF NOT EXISTS {databaseName};");
        }

        private void RunCommand(string command)
        {
            RunProcess(MYSQL, $"--login-path=${CurrentAlias} -e \"{command}\"");
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
            process.StartInfo.FileName = fileName;
            process.StartInfo.Arguments = arguments;
            process.Start();
            process.WaitForExit();
            return new ProcessResult()
            {
                ExitCode = process.ExitCode,
                Output = process.StandardOutput.ReadToEnd()
            };
        }
    }
}
