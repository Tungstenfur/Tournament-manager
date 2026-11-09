using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace TournamentManager.Windows;

public partial class AssignTournamentPlayers : Window
{
    private readonly Tournament _tournament;
    private readonly ObservableCollection<PlayerAssignmentItem> _items;

    public AssignTournamentPlayers(Tournament tournament, IEnumerable<Player> allPlayers)
    {
        InitializeComponent();
        _tournament = tournament;

        var assignedIds = Database.DatabaseTournamentPlayer.GetAssignedPlayerIds(tournament.Id)
            .ToHashSet();

        _items = new ObservableCollection<PlayerAssignmentItem>(
            allPlayers
                .OrderBy(player => player.LastName)
                .ThenBy(player => player.Name)
                .Select(player => new PlayerAssignmentItem(player, assignedIds.Contains(player.Id))));

        PlayersAssignmentDataGrid.ItemsSource = _items;
        TournamentLabel.Text = $"Turniej: {_tournament.Name}";
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var selectedPlayerIds = _items
                .Where(item => item.IsAssigned)
                .Select(item => item.Player.Id)
                .ToArray();

            Database.DatabaseTournamentPlayer.ReplaceAssignments(_tournament.Id, selectedPlayerIds);
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Nie udało się zapisać przypisań graczy.\n{ex.Message}",
                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private sealed class PlayerAssignmentItem : INotifyPropertyChanged
    {
        public PlayerAssignmentItem(Player player, bool isAssigned)
        {
            Player = player;
            _isAssigned = isAssigned;
        }

        public Player Player { get; }

        public bool IsAssigned
        {
            get => _isAssigned;
            set
            {
                if (_isAssigned == value)
                {
                    return;
                }

                _isAssigned = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsAssigned)));
            }
        }

        private bool _isAssigned;
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}

