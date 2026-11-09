using Dapper;
using MySqlConnector;

namespace TournamentManager.Database;


internal class DatabasePlayer: DatabaseData
{
    public static Player[] GetPlayers()
    {
        using var connection = new MySqlConnection(ConnectionString);
        return connection.Query<Player>("SELECT * FROM Players").ToArray();
    }

    public static uint InsertPlayer(Player player)
    {
        const string query = """
            INSERT INTO players (name, lastName, birthDate, matches, wins, losses, points)
            VALUES (@name, @lastName, @birthDate, @matches, @wins, @losses, @points);
            SELECT LAST_INSERT_ID();
            """;

        using var connection = new MySqlConnection(ConnectionString);
        return connection.ExecuteScalar<uint>(query, player);
    }

    public static void DeletePlayer(uint id)
    {
        using var connection = new MySqlConnection(ConnectionString);
        connection.Execute("DELETE FROM players WHERE id = @id", new { id });
    }

    public static void UpdatePlayer(Player player)
    {
        const string query = """
            UPDATE players
            SET name = @name, lastName = @lastName, birthDate = @birthDate
            WHERE id = @id
            """;

        using var connection = new MySqlConnection(ConnectionString);
        connection.Execute(query, player);
    }
}