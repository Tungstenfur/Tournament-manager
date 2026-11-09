using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using TournamentManager.Enums;

namespace TournamentManager
{
    public class Player : INotifyPropertyChanged, IEquatable<Player>
    {
        [Browsable(false)]
        public uint Id { get; set; }
        [Browsable(false)]
        public string DisplayName => $"{Name} {LastName}".Trim();
        [DisplayName("Imię")]
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
        [DisplayName("Nazwisko")]
        public string LastName
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastName)));
            }
        } = string.Empty;
        [DisplayName("Data urodzenia")]
        public DateOnly BirthDate
        {
            get;
            set
            {
                if (field == value) return;
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BirthDate)));
            }
        }
        [DisplayName("Liczba meczów")]
        public int Matches { get; }
        [DisplayName("Liczba wygranych")]
        public int Wins { get;  }
        [DisplayName("Liczba przegranych")]
        public int Losses { get; }
        [DisplayName("Punkty")]
        public int Points { get; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public Player(uint id, string name, string lastName, DateOnly birthDate, int matches, int wins, int losses, int points)
        {
            this.Id = id;
            this.Name = name;
            this.LastName = lastName;
            this.BirthDate = birthDate;
            this.Matches = matches;
            this.Wins = wins;
            this.Losses = losses;
            this.Points = points;
        }

        public Player()
        {
            
        }

        public bool Equals(Player? other)
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
            return obj is Player other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id == 0 ? base.GetHashCode() : HashCode.Combine(typeof(Player), Id);
        }
    }
}
