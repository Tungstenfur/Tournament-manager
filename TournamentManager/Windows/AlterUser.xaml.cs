using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TournamentManager.Enums;

namespace TournamentManager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy AlterUser.xaml
    /// </summary>
    public partial class AlterUser : Window
    {
        public IEnumerable<string> Roles { get; }
        public int UserId { get; set; }
        public AlterUser(Permission executorRole, Permission targetRole)
        {
            Roles = Enum.GetValues<Enums.Permission>()
                .Where(role => User.CanEditUsers(executorRole, role) || role == targetRole)
                .Select(role => role.ToString())
                .ToArray();
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Nazwa użytkownika nie może być pusta.");
                return;
            }
            if(ChangePasswordCheckBox.IsChecked == true && string.IsNullOrWhiteSpace(PasswordTextBox.Password))
            {
                MessageBox.Show("Hasło nie może być puste.");
                return;
            }
            if(RoleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Rola nie może być pusta.");
                return;
            }
            if (ChangePasswordCheckBox.IsChecked==true)
                Database.DatabaseUsers.UpdateUser(UserId, UsernameTextBox.Text, PasswordTextBox.Password, RoleComboBox.SelectedItem.ToString()!);
            else
                Database.DatabaseUsers.UpdateUserWithoutPassword(UserId, UsernameTextBox.Text, RoleComboBox.SelectedItem.ToString()!);
            DialogResult = true;
            Close();
        }
    }
}
