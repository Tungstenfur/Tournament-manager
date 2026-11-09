using System;
using TournamentManager.Enums;

namespace TournamentManager
{
    public class User
    {
        public int Id { get; private set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public Permission Role { get; set; }
        public User(int id, string username, string passwordHash, string salt, Permission role)
        {
            Id = id;
            Username = username;
            PasswordHash = passwordHash;
            Salt = salt;
            Role = role;
        }
        public User(int id, string username,string password,string salt,string role)
        {
            Id = id;
            Username = username;
            PasswordHash = password;
            Salt = salt;
            Role = Enum.Parse<Permission>(role, true);
        }
        public static bool CanEditUsers(Permission executorRole, Permission targetRole)
        {
            return executorRole switch
            {
                Permission.Employee => targetRole == Permission.Guest,
                Permission.Admin => targetRole is Permission.Guest or Permission.Employee,
                Permission.Superadmin => true,
                _ => false
            };
        }

        public static bool CanViewUsers(Permission role) => role >= Permission.Employee;
        public static bool CanDeleteUsers(Permission executorRole, Permission targetRole) =>
            executorRole == Permission.Superadmin ||
            (executorRole == Permission.Admin && targetRole is Permission.Guest or Permission.Employee);
        public static bool CanModifyMatches(Permission role) => role >= Permission.Employee;
        public static bool CanAssignPlayers(Permission role) => role >= Permission.Employee;
        public static bool CanAddTournaments(Permission role) => role >= Permission.Admin;
    }
}
