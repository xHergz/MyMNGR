﻿using MyMNGR.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMNGR.Utils
{
    public class SettingsManager
    {
        private const string SETTINGS_FILE = "settings.json";

        private string _rootFolder = string.Empty;

        private string _settingsFile = string.Empty;

        private Dictionary<string, Profile> _profiles;

        private Settings _settings;

        public string ProfileFolder = string.Empty;

        public string BackupFolder = string.Empty;

        public Profile CurrentProfile { get; private set; }

        public Target CurrentTarget { get; private set; }

        public SettingsManager()
        {
            _rootFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\MyMNGR";
            _settingsFile = $"{_rootFolder}\\{SETTINGS_FILE}";
            _profiles = new Dictionary<string, Profile>();

            ProfileFolder = $"{_rootFolder}\\Profiles";
            BackupFolder = $"{_rootFolder}\\Backups";
            CurrentProfile = null;
            CurrentTarget = Target.Development;

            InitializeFolders();
            LoadSettings();
        }

        public bool LoadProfile(string file)
        {
            if (!File.Exists(file))
            {
                return false;
            }
            using (StreamReader reader = new StreamReader(file))
            {
                string json = reader.ReadToEnd();
                Profile loaded = JsonConvert.DeserializeObject<Profile>(json);
                if (_profiles.ContainsKey(loaded.Name))
                {
                    _profiles.Remove(loaded.Name);
                }
                _profiles.Add(loaded.Name, loaded);
                CurrentProfile = loaded;
            }
            InitializeProfileFolders();
            return true;
        } 

        public bool SaveProfile(Profile profile)
        {
            string profilePath = $"{ProfileFolder}\\{profile.Name}.json";
            try
            {
                File.WriteAllText(profilePath, JsonConvert.SerializeObject(profile, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public void SwitchTargets()
        {
            switch(CurrentTarget)
            {
                case Target.Development:
                    CurrentTarget = Target.Production;
                    break;
                case Target.Production:
                default:
                    CurrentTarget = Target.Development;
                    break;
            }
        }

        private void InitializeFolders()
        {
            Directory.CreateDirectory(_rootFolder);
            Directory.CreateDirectory(ProfileFolder);
            Directory.CreateDirectory(BackupFolder);
        }

        private void InitializeProfileFolders()
        {
            Directory.CreateDirectory($"{BackupFolder}\\{CurrentProfile.DatabaseName}");
        }

        private void LoadSettings()
        {
            if (File.Exists(_settingsFile))
            {
                using (StreamReader reader = new StreamReader(_settingsFile))
                {
                    string json = reader.ReadToEnd();
                    _settings = JsonConvert.DeserializeObject<Settings>(json);
                }
            } else
            {
                _settings = new Settings()
                {
                    Test = "test"
                };
                SaveSettings();
            }
        }

        private bool SaveSettings()
        {
            try
            {
                File.WriteAllText(_settingsFile, JsonConvert.SerializeObject(_settings, Formatting.Indented));
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
