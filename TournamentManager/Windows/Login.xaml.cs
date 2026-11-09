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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Linq;
using TournamentManager.Enums;

namespace TournamentManager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        Permission? _userPermission;
        public Permission? UserPermission { get { return _userPermission; }private set { _userPermission = value; } }
        public Login()
        {
            InitializeComponent();
        }

        private void loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(passwordBox.Password))
            {
                MessageBox.Show("Hasło nie może być puste!", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            UserPermission = Database.DatabaseUsers.Login(usernameBox.Text, passwordBox.Password);
            if(_userPermission is null)
            {
                MessageBox.Show("Niepoprawne dane logowania!", "Błąd logowania", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
