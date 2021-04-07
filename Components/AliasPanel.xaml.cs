using System;
using System.Windows;
using System.Windows.Controls;

using MyMNGR.Data;

namespace MyMNGR.Components
{
    /// <summary>
    /// Interaction logic for AliasPanel.xaml
    /// </summary>
    public partial class AliasPanel : UserControl
    {
        public event EventHandler Cancel;

        public event EventHandler<Alias> Save;

        public AliasPanel()
        {
            InitializeComponent();
            Width = 500;
            Height = 250;
        }

        public void FillInFields(Alias alias)
        {
            _aliasNameTextBox.Text = alias.Name;
            _hostTextBox.Text = alias.Host;
            _usernameTextBox.Text = alias.Username;
            _usePasswordCheckbox.IsChecked = alias.UsePassword;
        }

        public void Reset()
        {
            _aliasNameTextBox.Clear();
            _hostTextBox.Clear();
            _usernameTextBox.Clear();
            _usePasswordCheckbox.IsChecked = false;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Cancel?.Invoke(this, EventArgs.Empty);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Alias newAlias = new Alias()
            {
                Name = _aliasNameTextBox.Text,
                Host = _hostTextBox.Text,
                Username = _usernameTextBox.Text,
                UsePassword = _usePasswordCheckbox.IsChecked.HasValue ? _usePasswordCheckbox.IsChecked.Value : false,
            };
            Save?.Invoke(this, newAlias);
        }
    }
}
