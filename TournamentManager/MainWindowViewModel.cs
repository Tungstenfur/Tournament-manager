using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TournamentManager.Enums;
using System.Collections.Specialized;
using System.IO;
using Microsoft.Win32;
using TournamentManager.Database;

namespace TournamentManager;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly Permission _permission;
    private string _currentView = "Tournaments";
    private Tournament? _selectedTournament;

    public ObservableCollection<Player> Players { get; } = new(Database.DatabasePlayer.GetPlayers());
    public ObservableCollection<Tournament> Tournaments { get; } = new(Database.DatabaseTournament.GetTournaments());
    public ObservableCollection<Match> Matches { get; } = new(Database.DatabaseMatch.GetMatches());
    public ObservableCollection<User> Users { get; } = new();

    public string CurrentView
    {
        get => _currentView;
        private set
        {
            if (_currentView == value) return;
            _currentView = value;
            OnPropertyChanged(nameof(CurrentView));
            OnPropertyChanged(nameof(IsPlayersVisible));
            OnPropertyChanged(nameof(IsTournamentsVisible));
            OnPropertyChanged(nameof(IsMatchesVisible));
            OnPropertyChanged(nameof(IsUsersVisible));
            OnPropertyChanged(nameof(PlayersVisibility));
            OnPropertyChanged(nameof(TournamentsVisibility));
            OnPropertyChanged(nameof(MatchesVisibility));
            OnPropertyChanged(nameof(UsersVisibility));
            OnPropertyChanged(nameof(AdministrationVisibility));
        }
    }

    public bool IsPlayersVisible => CurrentView == "Players";
    public bool IsTournamentsVisible => CurrentView == "Tournaments";
    public bool IsMatchesVisible => CurrentView == "Matches";
    public bool IsUsersVisible => CurrentView == "Users";
    public bool IsAdministrationVisible => CurrentView == "Administration";
    public Visibility PlayersVisibility => IsPlayersVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility TournamentsVisibility => IsTournamentsVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility MatchesVisibility => IsMatchesVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility UsersVisibility => IsUsersVisible ? Visibility.Visible : Visibility.Collapsed;
    public Visibility AdministrationVisibility => IsAdministrationVisible ? Visibility.Visible : Visibility.Collapsed;
    public bool CanViewUsers => User.CanViewUsers(_permission);
    public bool CanModifyMatches => User.CanModifyMatches(_permission);
    public bool CanAssignPlayers => User.CanAssignPlayers(_permission);
    public bool CanAddTournaments => User.CanAddTournaments(_permission);
    public bool CanEditTournaments => CanAddTournaments;
    public bool CanEditPlayers => _permission >= Permission.Employee;
    public bool CanEditMatchesGrid => CanModifyMatches;
    public bool IsAdmin => _permission >= Permission.Admin;
    public bool TournamentsReadOnly => !CanEditTournaments;
    public bool MatchesReadOnly => !CanEditMatchesGrid;

    public ICommand ShowPlayersCommand { get; }
    public ICommand ShowTournamentsCommand { get; }
    public ICommand ShowMatchesCommand { get; }
    public ICommand ShowUsersCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand ShowTournamentMatchesCommand { get; }
    public ICommand AssignTournamentPlayersCommand { get; }
    public ICommand GenerateRandomMatchesCommand { get; }
    public ICommand GenerateNextRoundCommand { get; }
    public ICommand AddUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand ShowAdministrationCommand { get; }
    public ICommand ExportPlayersCommand { get; }
    public ICommand ExportTournamentsCommand { get; }
    public ICommand ExportMatchesCommand { get; }
    public ICommand ExportUsersCommand { get; }

    public MainWindowViewModel(Permission permission)
    {
        _permission = permission;
        if (CanViewUsers)
        {
            foreach (var user in Database.DatabaseUsers.GetUsers())
                Users.Add(user);
        }
        foreach (var player in Players) player.PropertyChanged += PlayerChanged;
        foreach (var tournament in Tournaments) tournament.PropertyChanged += TournamentChanged;
        foreach (var match in Matches) match.PropertyChanged += MatchChanged;
        Players.CollectionChanged += PlayersChanged;
        Tournaments.CollectionChanged += TournamentsChanged;
        Matches.CollectionChanged += MatchesChanged;

        ShowPlayersCommand = new RelayCommand(() => CurrentView = "Players", () => CanEditPlayers);
        ShowTournamentsCommand = new RelayCommand(() => CurrentView = "Tournaments");
        ShowMatchesCommand = new RelayCommand(() => { _selectedTournament = null; CurrentView = "Matches"; });
        ShowUsersCommand = new RelayCommand(() => CurrentView = "Users", () => CanViewUsers);
        ShowAdministrationCommand = new RelayCommand(() => CurrentView = "Administration",()=>IsAdmin);
        ExportPlayersCommand = new RelayCommand(()=>SaveToFile(DatabaseDump.DumpPlayers()));
        LogoutCommand = new RelayCommand(Logout);
        ShowTournamentMatchesCommand = new RelayCommand<Tournament>(tournament =>
        {
            _selectedTournament = tournament;
            CurrentView = "Matches";
        });
        AssignTournamentPlayersCommand = new RelayCommand<Tournament>(AssignTournamentPlayers, _ => CanAssignPlayers);
        GenerateRandomMatchesCommand = new RelayCommand(GenerateRandomMatches, () => CanModifyMatches);
        GenerateNextRoundCommand = new RelayCommand(GenerateNextRound, () => CanModifyMatches);
        AddUserCommand = new RelayCommand(AddUser, () => CanViewUsers);
        EditUserCommand = new RelayCommand<User>(EditUser, user => user is not null && User.CanEditUsers(_permission, user.Role));
        DeleteUserCommand = new RelayCommand<User>(DeleteUser, _ => false);

        foreach (var match in Matches) NormalizeMatchReferences(match);
    }

    private void AssignTournamentPlayers(Tournament? tournament)
    {
        if (tournament is null) return;
        var window = new Windows.AssignTournamentPlayers(tournament, Players);
        if (Application.Current?.MainWindow is Window owner) window.Owner = owner;
        if (window.ShowDialog() == true)
            tournament.SetPlayersCount(Database.DatabaseTournamentPlayer.GetAssignedPlayersCount(tournament.Id));
    }

    private void AddUser()
    {
        var window = new Windows.AddUser(_permission);
        if (window.ShowDialog() == true) RefreshUsers();
    }

    private void EditUser(User? user)
    {
        if (user is null) return;
        if (!User.CanEditUsers(_permission, user.Role)) return;
        var window = new Windows.AlterUser(_permission, user.Role)
        {
            UserId = user.Id
        };
        window.UsernameTextBox.Text = user.Username;
        window.RoleComboBox.SelectedItem = user.Role.ToString();
        if (window.ShowDialog() == true) RefreshUsers();
    }

    private void DeleteUser(User? user)
    {
        if (user is null || MessageBox.Show($"Czy na pewno chcesz usunąć użytkownika {user.Username}?",
                "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        if (!User.CanEditUsers(_permission, user.Role)) return;
        Database.DatabaseUsers.DeleteUser(user.Id);
        RefreshUsers();
    }

    private void RefreshUsers()
    {
        Users.Clear();
        foreach (var user in Database.DatabaseUsers.GetUsers()) Users.Add(user);
    }

    private void Logout()
    {
        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        Application.Current.Shutdown();
    }

    private void GenerateRandomMatches()
    {
        if (_selectedTournament is null)
        {
            MessageBox.Show("Najpierw otwórz mecze wybranego turnieju.", "Brak turnieju", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var ids = Database.DatabaseTournamentPlayer.GetAssignedPlayerIds(_selectedTournament.Id).ToHashSet();
        var players = Players.Where(p => ids.Contains(p.Id)).GroupBy(p => p.Id).Select(g => g.First()).ToList();
        if (players.Count < 2)
        {
            MessageBox.Show("Do losowania potrzeba co najmniej 2 przypisanych graczy.", "Za mało graczy", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        var round = LatestRound(_selectedTournament.Id) + 1;
        if (Matches.Any(m => m.Tournament?.Id == _selectedTournament.Id && m.Round == round))
        {
            MessageBox.Show($"Runda {round} już istnieje dla tego turnieju.", "Nie można wygenerować rundy", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        for (var i = players.Count - 1; i > 0; i--)
        {
            var j = Random.Shared.Next(i + 1);
            (players[i], players[j]) = (players[j], players[i]);
        }
        for (var i = 0; i + 1 < players.Count; i += 2)
        {
            var match = new Match(0, round, _selectedTournament, players[i], players[i + 1], null);
            match.Id = Database.DatabaseMatch.InsertMatch(match);
            Matches.Add(match);
        }
    }

    private void GenerateNextRound()
    {
        if (_selectedTournament is null) return;
        var round = LatestRound(_selectedTournament.Id);
        var winners = Matches.Where(m => m.Tournament?.Id == _selectedTournament.Id && m.Round == round && m.Winner is not null)
            .Select(m => m.Winner!).ToList();
        if (winners.Count < 2 || winners.Count % 2 != 0)
        {
            MessageBox.Show("Liczba zwycięzców musi być dodatnia i parzysta.", "Nieprawidłowa liczba zwycięzców", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        var nextRound = round + 1;
        for (var i = 0; i < winners.Count; i += 2)
        {
            var match = new Match(0, nextRound, _selectedTournament, winners[i], winners[i + 1], null);
            match.Id = Database.DatabaseMatch.InsertMatch(match);
            Matches.Add(match);
        }
    }

    private int LatestRound(int tournamentId) => Matches.Where(m => m.Tournament?.Id == tournamentId).Select(m => m.Round).DefaultIfEmpty(0).Max();
    private void NormalizeMatchReferences(Match match)
    {
        match.Tournament = Tournaments.FirstOrDefault(t => t.Id == match.Tournament?.Id) ?? match.Tournament;
        match.Player1 = Players.FirstOrDefault(p => p.Id == match.Player1?.Id) ?? match.Player1;
        match.Player2 = Players.FirstOrDefault(p => p.Id == match.Player2?.Id) ?? match.Player2;
        match.Winner = Players.FirstOrDefault(p => p.Id == match.Winner?.Id) ?? match.Winner;
    }
    private void PlayerChanged(object? sender, PropertyChangedEventArgs e) { if (sender is Player p && p.Id != 0) Database.DatabasePlayer.UpdatePlayer(p); }
    private void TournamentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (CanEditTournaments && sender is Tournament t && t.Id != 0)
            Database.DatabaseTournament.UpdateTournament(t);
    }
    private void MatchChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (!CanModifyMatches) return;
        if (sender is not Match match) return;
        if (match.Player1?.Id > 0 && match.Player1.Id == match.Player2?.Id)
        {
            MessageBox.Show("Gracz nie może grać sam ze sobą.", "Nieprawidłowy mecz",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (match.Id == 0 && match.Tournament?.Id > 0 && match.Player1?.Id > 0 && match.Player2?.Id > 0)
        {
            match.Round = Math.Max(1, match.Round);
            match.Id = Database.DatabaseMatch.InsertMatch(match);
        }
        else if (match.Id != 0)
        {
            Database.DatabaseMatch.UpdateMatch(match);
        }
    }
    private void PlayersChanged(object? sender, NotifyCollectionChangedEventArgs e) { if (e.NewItems is not null) foreach (Player p in e.NewItems) { p.Id = Database.DatabasePlayer.InsertPlayer(p); p.PropertyChanged += PlayerChanged; } }
    private void TournamentsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null) return;
        if (!CanAddTournaments)
        {
            foreach (Tournament tournament in e.NewItems) Tournaments.Remove(tournament);
            return;
        }
        foreach (Tournament t in e.NewItems)
        {
            t.Id = Database.DatabaseTournament.InsertTournament(t);
            t.PropertyChanged += TournamentChanged;
        }
    }
    private void MatchesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is null) return;
        if (!CanModifyMatches)
        {
            foreach (Match match in e.NewItems) Matches.Remove(match);
            return;
        }
        foreach (Match m in e.NewItems) m.PropertyChanged += MatchChanged;
    }
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    public event PropertyChangedEventHandler? PropertyChanged;

    private void SaveToFile(string content)
    {
        SaveFileDialog saveFileDialog = new();
        saveFileDialog.Filter = "Pliki json (*.json)|*.json";
        if (saveFileDialog.ShowDialog()==true)
        {
            File.WriteAllText(saveFileDialog.FileName, content);
        }
    }
}

public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    private readonly Action _execute = execute;
    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter) => _execute();
    public event EventHandler? CanExecuteChanged;
}

public sealed class RelayCommand<T>(Action<T?> execute, Func<T?, bool>? canExecute = null) : ICommand
{
    public bool CanExecute(object? parameter)
    {
        if (parameter is not null and not T)
            return false;

        return canExecute?.Invoke(parameter is T value ? value : default) ?? true;
    }

    public void Execute(object? parameter)
    {
        if (parameter is null or T)
            execute(parameter is T value ? value : default);
    }

    public event EventHandler? CanExecuteChanged;
}
