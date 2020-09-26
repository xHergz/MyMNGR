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
        private ConsoleManager _consoleManager;

        private SettingsManager _settingsManager;

        private ProfilePanel _profilePanel;

        private UserControl _visiblePanel;

        public MainWindow()
        {
            InitializeComponent();
            _consoleManager = new ConsoleManager(_consoleWindow);
            _settingsManager = new SettingsManager();
            _profilePanel = new ProfilePanel();
            _profilePanel.Cancel += ProfilePanel_Cancel;
            _profilePanel.Save += ProfilePanel_Save;
            _profilePanel.Width = 350;
            _profilePanel.Height = 250;
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
    }
}
