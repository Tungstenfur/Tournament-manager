using System.Windows;
using TournamentManager.Enums;

namespace TournamentManager;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            if (!Database.Database.CreateDatabase())
            {
                var registerWindow = new Windows.RegisterFirstUser();
                if (registerWindow.ShowDialog() != true)
                {
                    MessageBox.Show(
                        "Nie można kontynuować bez utworzenia pierwszego użytkownika. Aplikacja zostanie zamknięta.",
                        "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    Application.Current.Shutdown();
                    return;
                }
            }
        }
        catch (MySqlConnector.MySqlException)
        {
            MessageBox.Show("Nie można połączyć się z bazą danych","Błąd!", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }
        var loginWindow = new Windows.Login();
        if (loginWindow.ShowDialog() != true)
        {
            MessageBox.Show(
                "Nie można kontynuować bez zalogowania się. Aplikacja zostanie zamknięta.",
                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
            return;
        }

        InitializeComponent();
        DataContext = new MainWindowViewModel(
            loginWindow.UserPermission ?? throw new InvalidOperationException());
    }
}
