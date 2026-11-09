using System.Text.Json;

namespace TournamentManager.Database;

internal class DatabaseDump
{
    public static string DumpPlayers()
    {
        var players = DatabasePlayer.GetPlayers();
        return JsonSerializer.Serialize(players);
    }

    public static string DumpTournaments()
    {
        var tournaments = DatabaseTournament.GetTournaments();
        return JsonSerializer.Serialize(tournaments);
    }
    public static string DumpMatches()
    {
        var matches = DatabaseMatch.GetMatches();
        return JsonSerializer.Serialize(matches);
    }

    public static string DumpUsers()
    {
        var user = DatabaseUsers.GetUsers();
        return JsonSerializer.Serialize(user);
    }
}