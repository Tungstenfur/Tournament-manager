using System.ComponentModel;

namespace TournamentManager;

public class Tournament : INotifyPropertyChanged, IEquatable<Tournament>
{
    [Browsable(false)]
    public int Id { get; set; }
    [Browsable(false)]
    public string DisplayName => Name;
    [DisplayName("Nazwa")]
    public string Name
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
        }
    } = string.Empty;
    [DisplayName("Data")]
    public DateOnly Date
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Date)));
        }
    }
    [DisplayName("Liczba graczy")]
    [ReadOnly(true)]
    public int Players { get; private set; }
    [DisplayName("Zwycięzca")]
    public Player? Winner
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Winner)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    public Tournament(int id, string name, DateOnly date, int players, Player? winner)
    {
        Id = id;
        Name = name;
        Date = date;
        Players = players;
        Winner = winner;
    }
    public Tournament() { }

    public void SetPlayersCount(int players)
    {
        if (Players == players)
        {
            return;
        }

        Players = players;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
    }

    public bool Equals(Tournament? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is null)
        {
            return false;
        }
        return Id != 0 && other.Id != 0 && Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        return obj is Tournament other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Id == 0 ? base.GetHashCode() : HashCode.Combine(typeof(Tournament), Id);
    }
}