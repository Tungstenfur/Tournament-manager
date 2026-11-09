using Dapper;
using MySqlConnector;

namespace TournamentManager.Database;

internal static class DatabaseMatch
{
    public static int InsertMatch(Match match)
    {
        if (match.Tournament?.Id is not > 0
            || match.Player1?.Id is not > 0
            || match.Player2?.Id is not > 0)
        {
            throw new ArgumentException("A match requires a tournament and two players.", nameof(match));
        }

        var tournamentId = match.Tournament!.Id;
        var player1Id = match.Player1!.Id;
        var player2Id = match.Player2!.Id;
        if (player1Id == player2Id)
        {
            throw new ArgumentException("A player cannot play against themselves.", nameof(match));
        }
        const string query = """
            INSERT IGNORE INTO tournament_players (tournament_id, player_id)
            VALUES (@Tournament, @Player1), (@Tournament, @Player2);

            INSERT INTO matches (tournament, player1, player2, winner, round)
            VALUES (@Tournament, @Player1, @Player2, @Winner, @Round);
            SELECT LAST_INSERT_ID();
            """;
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.ExecuteScalar<int>(query, new
        {
            Tournament = tournamentId,
            Player1 = player1Id,
            Player2 = player2Id,
            Winner = match.Winner?.Id,
            match.Round
        });
    }

    public static void UpdateMatch(Match match)
    {
        if (match.Tournament?.Id is not > 0
            || match.Player1?.Id is not > 0
            || match.Player2?.Id is not > 0)
        {
            throw new ArgumentException("A match requires a tournament and two players.", nameof(match));
        }

        var tournamentId = match.Tournament!.Id;
        var player1Id = match.Player1!.Id;
        var player2Id = match.Player2!.Id;
        if (player1Id == player2Id)
        {
            throw new ArgumentException("A player cannot play against themselves.", nameof(match));
        }
        const string query = """
            INSERT IGNORE INTO tournament_players (tournament_id, player_id)
            VALUES (@Tournament, @Player1), (@Tournament, @Player2);

            UPDATE matches
            SET tournament = @Tournament, player1 = @Player1,
                player2 = @Player2, winner = @Winner, round = @Round
            WHERE id = @Id
            """;
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Execute(query, new
        {
            match.Id,
            Tournament = tournamentId,
            Player1 = player1Id,
            Player2 = player2Id,
            Winner = match.Winner?.Id,
            match.Round
        });
    }

    public static void DeleteMatch(int id)
    {
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Execute("DELETE FROM matches WHERE id = @id", new { id });
    }

    public static IEnumerable<Match> GetMatches()
    {
        const string query = """
            SELECT
                m.id,
                m.round,
                m.tournament AS TournamentId,
                t.name AS TournamentName,
                t.date AS TournamentDate,
                t.players AS TournamentPlayers,
                m.player1 AS Player1Id,
                p1.name AS Player1Name,
                p1.lastName AS Player1LastName,
                p1.birthDate AS Player1BirthDate,
                p1.matches AS Player1Matches,
                p1.wins AS Player1Wins,
                p1.losses AS Player1Losses,
                p1.points AS Player1Points,
                m.player2 AS Player2Id,
                p2.name AS Player2Name,
                p2.lastName AS Player2LastName,
                p2.birthDate AS Player2BirthDate,
                p2.matches AS Player2Matches,
                p2.wins AS Player2Wins,
                p2.losses AS Player2Losses,
                p2.points AS Player2Points,
                w.id AS WinnerId,
                w.name AS WinnerName,
                w.lastName AS WinnerLastName,
                w.birthDate AS WinnerBirthDate,
                w.matches AS WinnerMatches,
                w.wins AS WinnerWins,
                w.losses AS WinnerLosses,
                w.points AS WinnerPoints
            FROM matches m
            INNER JOIN tournaments t ON t.id = m.tournament
            INNER JOIN players p1 ON p1.id = m.player1
            INNER JOIN players p2 ON p2.id = m.player2
            LEFT JOIN players w ON w.id = m.winner
            ORDER BY m.round, m.id
            """;

        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.Query<MatchRow>(query).Select(row => new Match(
            row.Id,
            row.Round,
            new Tournament(row.TournamentId, row.TournamentName, row.TournamentDate,
                row.TournamentPlayers, null),
            CreatePlayer(row.Player1Id, row.Player1Name, row.Player1LastName,
                row.Player1BirthDate, row.Player1Matches, row.Player1Wins,
                row.Player1Losses, row.Player1Points),
            CreatePlayer(row.Player2Id, row.Player2Name, row.Player2LastName,
                row.Player2BirthDate, row.Player2Matches, row.Player2Wins,
                row.Player2Losses, row.Player2Points),
            row.WinnerId.HasValue
                ? CreatePlayer(row.WinnerId.Value, row.WinnerName!, row.WinnerLastName!,
                    row.WinnerBirthDate, row.WinnerMatches, row.WinnerWins,
                    row.WinnerLosses, row.WinnerPoints)
                : null));
    }

    private static Player CreatePlayer(uint id, string name, string lastName, DateOnly birthDate,
        int matches, int wins, int losses, int points) =>
        new(id, name, lastName, birthDate, matches, wins, losses, points);

    private sealed class MatchRow
    {
        public int Id { get; set; }
        public int Round { get; set; }
        public int TournamentId { get; set; }
        public string TournamentName { get; set; } = string.Empty;
        public DateOnly TournamentDate { get; set; }
        public int TournamentPlayers { get; set; }
        public uint Player1Id { get; set; }
        public string Player1Name { get; set; } = string.Empty;
        public string Player1LastName { get; set; } = string.Empty;
        public DateOnly Player1BirthDate { get; set; }
        public int Player1Matches { get; set; }
        public int Player1Wins { get; set; }
        public int Player1Losses { get; set; }
        public int Player1Points { get; set; }
        public uint Player2Id { get; set; }
        public string Player2Name { get; set; } = string.Empty;
        public string Player2LastName { get; set; } = string.Empty;
        public DateOnly Player2BirthDate { get; set; }
        public int Player2Matches { get; set; }
        public int Player2Wins { get; set; }
        public int Player2Losses { get; set; }
        public int Player2Points { get; set; }
        public uint? WinnerId { get; set; }
        public string? WinnerName { get; set; }
        public string? WinnerLastName { get; set; }
        public DateOnly WinnerBirthDate { get; set; }
        public int WinnerMatches { get; set; }
        public int WinnerWins { get; set; }
        public int WinnerLosses { get; set; }
        public int WinnerPoints { get; set; }
    }
}
