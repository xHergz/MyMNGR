using Microsoft.Win32;
using MyMNGR.Components;
using MyMNGR.Data;
using MyMNGR.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyMNGR
{
    /// <summary>
    /// Interaction logic for MyMNGR.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly SolidColorBrush DEV_COLOUR = new SolidColorBrush(Colors.Lime);

        private static readonly SolidColorBrush PROD_COLOUR = new SolidColorBrush(Colors.Red);

        private const string DEV_LABEL = "DEV";

        private const string PROD_LABEL = "PROD";

        private ConsoleManager _consoleManager;

        private FileManager _fileManager;

        private MySqlManager _mySqlManager;

        private SettingsManager _settingsManager;

        private ProfilePanel _profilePanel;

        private UserControl _visiblePanel;

        public bool ProfileLoaded { get { return _settingsManager.CurrentProfile != null; } }

        public bool ForceActionsEnabled { get { return _settingsManager.CurrentTarget == Target.Development; } }

        public MainWindow()
        {
            InitializeComponent();
            _consoleManager = new ConsoleManager(_consoleWindow);
            _fileManager = new FileManager();
            _settingsManager = new SettingsManager();
            _profilePanel = new ProfilePanel();
            _profilePanel.Cancel += ProfilePanel_Cancel;
            _profilePanel.Save += ProfilePanel_Save;

            UpdateButtons();
            UpdateTargetLabel();
        }

        private void CloseVisiblePanel()
        {
            if (_visiblePanel != null)
            {
                _visiblePanel.Visibility = Visibility.Hidden;
            }
            _mainContent.Children.Clear();
        }

        private void SwitchVisiblePanel(UserControl newPanel)
        {
            _visiblePanel = newPanel;
            _mainContent.Children.Add(_profilePanel);
        }

        private void UpdateButtons()
        {
            _deployButton.IsEnabled = ProfileLoaded;
            _forceDeployButton.IsEnabled = ProfileLoaded && ForceActionsEnabled;
            _dropButton.IsEnabled = ProfileLoaded;
            _backupButton.IsEnabled = ProfileLoaded;
            _restoreButton.IsEnabled = ProfileLoaded;
            _forceRestoreButton.IsEnabled = ProfileLoaded && ForceActionsEnabled;
        }

        private void UpdateTargetLabel()
        {
            switch(_settingsManager.CurrentTarget)
            {
                case Target.Production:
                    _targetLabel.Content = PROD_LABEL;
                    _targetLabel.Background = PROD_COLOUR;
                    break;
                case Target.Development:
                default:
                    _targetLabel.Content = DEV_LABEL;
                    _targetLabel.Background = DEV_COLOUR;
                    break;
            }
        }

        private void ProductionConfirmation(string description, Action actionFunction)
        {
            if (_settingsManager.CurrentTarget != Target.Development)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"You are about to {description} the {_settingsManager.CurrentTarget.ToString("G")} database. Please confirm this action.",
                    "MyMNGR - Confirmation",
                    MessageBoxButton.OKCancel,
                    MessageBoxImage.Warning,
                    MessageBoxResult.Cancel
                );
                if (result != MessageBoxResult.OK)
                {
                    return;
                }
            }
            actionFunction();
        }

        private void FileNew_Click(object sender, RoutedEventArgs e)
        {
            _profilePanel.Reset();
            SwitchVisiblePanel(_profilePanel);
        }

        private void FileLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = _settingsManager.ProfileFolder;
            openFileDialog.Filter = "JSON files (*.json)|*.json";
            if (openFileDialog.ShowDialog() == true)
            {
                if (_settingsManager.LoadProfile(openFileDialog.FileName))
                {
                    _consoleManager.LogMessage($"Successfully loaded profile from {openFileDialog.FileName}");
                    _fileManager.LoadFiles(_settingsManager.CurrentProfile.Directory);
                    _mySqlManager = new MySqlManager(_consoleManager, _fileManager, _settingsManager);
                    UpdateButtons();
                    UpdateTargetLabel();
                }
                else
                {
                    _consoleManager.LogMessage($"Failed to load profile from {openFileDialog.FileName}");
                }
            }
        }

        private void ProfilePanel_Cancel(object sender, EventArgs e)
        {
            CloseVisiblePanel();
        }

        private void ProfilePanel_Save(object sender, Profile newProfile)
        {
            
            if (_settingsManager.SaveProfile(newProfile))
            {
                _consoleManager.LogMessage($"Successfully saved a new profile for '{newProfile.Name}'");
            }
            else
            {
                _consoleManager.LogMessage($"Failed to save a profile for '{newProfile.Name}'");
            }
            CloseVisiblePanel();
        }

        private void DeployButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("deploy", () => _mySqlManager.DeployDatabase());
        }

        private void ForceDeplyButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("force deploy", () => _mySqlManager.ForceDeployDatabase());
        }

        private void DropButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("drop", () => _mySqlManager.DropDatabase());
        }

        private void BackupButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("backup", () => _mySqlManager.BackupDatabase());
        }

        private void RestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("restore", () => _mySqlManager.RestoreDatabase());
        }

        private void ForceRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            ProductionConfirmation("force restore", () => _mySqlManager.ForceRestoreDatabase());
        }

        private void SwitchTargetButton_Click(object sender, RoutedEventArgs e)
        {
            _settingsManager.SwitchTargets();
            UpdateTargetLabel();
            UpdateButtons();
        }
    }
}
