using Dapper;
using MySqlConnector;

namespace TournamentManager.Database;

internal static class DatabaseTournamentPlayer
{
    public static IReadOnlyCollection<uint> GetAssignedPlayerIds(int tournamentId)
    {
        const string query = """
            SELECT player_id
            FROM tournament_players
            WHERE tournament_id = @TournamentId
            ORDER BY player_id
            """;

        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.Query<uint>(query, new { TournamentId = tournamentId }).ToArray();
    }

    public static void AssignPlayer(int tournamentId, uint playerId)
    {
        const string query = """
            INSERT IGNORE INTO tournament_players (tournament_id, player_id)
            VALUES (@TournamentId, @PlayerId)
            """;

        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Execute(query, new { TournamentId = tournamentId, PlayerId = playerId });
    }

    public static void ReplaceAssignments(int tournamentId, IEnumerable<uint> playerIds)
    {
        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();

        connection.Execute("DELETE FROM tournament_players WHERE tournament_id = @TournamentId",
            new { TournamentId = tournamentId }, transaction);

        const string insertQuery = """
            INSERT IGNORE INTO tournament_players (tournament_id, player_id)
            VALUES (@TournamentId, @PlayerId)
            """;

        foreach (var playerId in playerIds.Distinct())
        {
            connection.Execute(insertQuery, new { TournamentId = tournamentId, PlayerId = playerId }, transaction);
        }

        transaction.Commit();
    }

    public static int GetAssignedPlayersCount(int tournamentId)
    {
        const string query = """
            SELECT COUNT(*)
            FROM tournament_players
            WHERE tournament_id = @TournamentId
            """;

        using var connection = new MySqlConnection(DatabaseData.ConnectionString);
        return connection.ExecuteScalar<int>(query, new { TournamentId = tournamentId });
    }
}

