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

namespace TournamentManager.Windows
{
    /// <summary>
    /// Logika interakcji dla klasy RegisterFirstUser.xaml
    /// </summary>
    public partial class RegisterFirstUser : Window
    {
        public RegisterFirstUser()
        {
            InitializeComponent();
        }

        private void registerButton_Click(object sender, RoutedEventArgs e)
        {
            if(passwordBox.Password != repeatPasswordBox.Password)
            {
                MessageBox.Show("Hasła nie są takie same!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(usernameBox.Text.Length==0)
            {
                MessageBox.Show("Nazwa użytkownika nie może być pusta!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if(passwordBox.Password.Length < 8)
            {
                MessageBox.Show("Hasło musi mieć co najmniej 8 znaków!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Database.DatabaseUsers.CreateUser(usernameBox.Text, passwordBox.Password, "superadmin");
            DialogResult = true;
            Close();
        }
    }
}
