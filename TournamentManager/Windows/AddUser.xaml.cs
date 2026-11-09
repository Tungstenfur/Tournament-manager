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
    /// Logika interakcji dla klasy AddUser.xaml
    /// </summary>
    public partial class AddUser : Window
    {
        public IEnumerable<string> Roles { get; }

        public AddUser(Permission executorRole)
        {
            Roles = Enum.GetValues<Permission>()
                .Where(role => User.CanEditUsers(executorRole, role))
                .Select(role => role.ToString())
                .ToArray();
            InitializeComponent();
        }
        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(usernameTextBox.Text) || string.IsNullOrWhiteSpace(passwordTextBox.Password) || roleComboBox.SelectedItem == null)
            {
                MessageBox.Show("Wszystkie pola muszą być wypełnione.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string username = usernameTextBox.Text;
            string password = passwordTextBox.Password;
            string role = roleComboBox.SelectedItem.ToString()!;
            bool success = Database.DatabaseUsers.CreateUser(username, password, role);
            if (success)
            {
                MessageBox.Show("Użytkownik został dodany pomyślnie.", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Nie udało się dodać użytkownika. Sprawdź, czy nazwa użytkownika jest unikalna.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
