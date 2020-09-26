using MyMNGR.Data;
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

        private string _backupFolder = string.Empty;

        private string _settingsFile = string.Empty;

        private Dictionary<string, Profile> _profiles;

        private Settings _settings;

        public string ProfileFolder = string.Empty;

        public SettingsManager()
        {
            _rootFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}\\MyMNGR";
            ProfileFolder = $"{_rootFolder}\\Profiles";
            _backupFolder = $"{_rootFolder}\\Backups";
            _settingsFile = $"{_rootFolder}\\{SETTINGS_FILE}";
            _profiles = new Dictionary<string, Profile>();

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
            }
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

        private void InitializeFolders()
        {
            Directory.CreateDirectory(_rootFolder);
            Directory.CreateDirectory(ProfileFolder);
            Directory.CreateDirectory(_backupFolder);
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
