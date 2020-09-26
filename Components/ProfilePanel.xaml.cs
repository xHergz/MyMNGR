using System;
using System.Windows;
using System.Windows.Controls;

using Microsoft.WindowsAPICodePack.Dialogs;

using MyMNGR.Data;

namespace MyMNGR.Components
{
    /// <summary>
    /// Interaction logic for ProfilePanel.xaml
    /// </summary>
    public partial class ProfilePanel : UserControl
    {
        public event EventHandler Cancel;

        public event EventHandler<Profile> Save;

        public ProfilePanel()
        {
            InitializeComponent();
        }

        public void FillInFields(Profile profile)
        {
            _profileNameTextBox.Text = profile.Name;
            _devAliasTextBox.Text = profile.DevAlias;
            _prodAliasTextBox.Text = profile.ProdAlias;
            _folderTextBox.Text = profile.Directory;
        }

        public void Reset()
        {
            _profileNameTextBox.Clear();
            _devAliasTextBox.Clear();
            _prodAliasTextBox.Clear();
            _folderTextBox.Clear();
        }

        private void BrowseFolders_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            CommonFileDialogResult result = dialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok)
            {
                _folderTextBox.Text = dialog.FileName;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Profile newProfile = new Profile()
            {
                Name = _profileNameTextBox.Text,
                DevAlias = _devAliasTextBox.Text,
                ProdAlias = _prodAliasTextBox.Text,
                Directory = _folderTextBox.Text,
            };
            Save?.Invoke(this, newProfile);
        }
    }
}
