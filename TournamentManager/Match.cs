using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TournamentManager;

public class Match : INotifyPropertyChanged
{
    [Browsable(false)]
    public int Id { get; set; }
    [DisplayName("Runda")]
    public int Round
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Round)));
        }
    }
    [DisplayName("Turniej")]
    public Tournament? Tournament
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tournament)));
        }
    }
    [DisplayName("Gracz 1")]
    public Player? Player1
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Player1)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WinnerOptions)));
        }
    }
    [DisplayName("Gracz 2")]
    public Player? Player2
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Player2)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WinnerOptions)));
        }
    }
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

    public IEnumerable<Player> WinnerOptions
    {
        get
        {
            if (Player1 is null && Player2 is null)
            {
                return Array.Empty<Player>();
            }

            if (Player1 is null)
            {
                return Player2 is null ? Array.Empty<Player>() : new[] { Player2 };
            }

            if (Player2 is null)
            {
                return new[] { Player1 };
            }

            return new[] { Player1, Player2 };
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    public Match(int id, int round, Tournament tournament, Player player1, Player player2, Player? winner)
    {
        Id = id;
        Round = round;
        Tournament = tournament;
        Player1 = player1;
        Player2 = player2;
        Winner = winner;
    }

    public Match() { }
}