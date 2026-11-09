using Dapper;
using MySqlConnector;

namespace TournamentManager.Database;

internal static class DatabaseTournament
{
    public static int InsertTournament(Tournament tournament)
    {
        const string query = """
            INSERT INTO tournaments (name, date, players, winner)
            VALUES (@Name, @Date, @Players, @WinnerId);
            SELECT LAST_INSERT_ID();
            """;
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.ExecuteScalar<int>(query, new
        {
            tournament.Name,
            tournament.Date,
            tournament.Players,
            WinnerId = tournament.Winner?.Id
        });
    }

    public static void UpdateTournament(Tournament tournament)
    {
        const string query = """
            UPDATE tournaments
            SET name = @Name, date = @Date, winner = @WinnerId
            WHERE id = @Id
            """;
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Execute(query, new
        {
            tournament.Id,
            tournament.Name,
            tournament.Date,
            WinnerId = tournament.Winner?.Id
        });
    }

    public static void DeleteTournament(int id)
    {
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Execute("DELETE FROM tournaments WHERE id = @id", new { id });
    }

    public static IEnumerable<Tournament> GetTournaments()
    {
        const string query = """
            SELECT
                t.id,
                t.name,
                t.date,
                t.players,
                w.id AS winner_id,
                w.name AS winner_name,
                w.lastName AS winner_lastName,
                w.birthDate AS winner_birthDate,
                w.matches AS winner_matches,
                w.wins AS winner_wins,
                w.losses AS winner_losses,
                w.points AS winner_points
            FROM tournaments t
            LEFT JOIN players w ON w.id = t.winner
            ORDER BY t.id
            """;

        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.Query<TournamentRow>(query).Select(row => new Tournament(
            row.Id,
            row.Name,
            row.Date,
            row.Players,
            row.WinnerId.HasValue
                ? new Player(row.WinnerId.Value, row.WinnerName!, row.WinnerLastName!,
                    row.WinnerBirthDate, row.WinnerMatches, row.WinnerWins,
                    row.WinnerLosses, row.WinnerPoints)
                : null));
    }

    private sealed class TournamentRow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateOnly Date { get; set; }
        public int Players { get; set; }
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
