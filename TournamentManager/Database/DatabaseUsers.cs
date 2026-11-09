using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;
using Dapper;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using TournamentManager.Enums;

namespace TournamentManager.Database
{
    internal class DatabaseUsers : DatabaseData
    {
        private static string GetPasswordHash(string password, byte[] salt)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 65536,
                Iterations = 4
            };
            var hash= Convert.ToBase64String(argon2.GetBytes(32));
            return hash;
        }
        private static byte[] GenerateSalt()
        {
            var salt = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(salt);
            return salt;
        }
        public static bool CreateUser(string username, string password, string role)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            var checkUserQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
            var userCount = connection.ExecuteScalar<int>(checkUserQuery, new { Username = username });
            if (userCount > 0)
            {
                return false;
            }
            var salt = GenerateSalt();
            var passwordHash = GetPasswordHash(password, salt);
            var saltString = Convert.ToBase64String(salt);
            var insertUserQuery = "INSERT INTO Users (Username, Password, Salt, Role) VALUES (@Username, @Password, @Salt, @Role)";
            connection.Execute(insertUserQuery, new { Username = username, Password = passwordHash, Salt = saltString, Role = role });
            return true;
        }
        public static Permission? Login(string username, string password)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            var getUserQuery = "SELECT Password, Salt, Role FROM Users WHERE Username = @Username";
            var user = connection.QuerySingleOrDefault(getUserQuery, new { Username = username });
            if (user == null)
            {
                return null;
            }
            var storedPasswordHash = (string)user.Password;
            var storedSalt = Convert.FromBase64String((string)user.Salt);
            var role = (string)user.Role;
            var passwordHash = GetPasswordHash(password, storedSalt);
            if (passwordHash == storedPasswordHash)
            {
                return Enum.TryParse<Permission>(role,true, out var permission) ? permission : throw new InvalidOperationException("Nieprawidłowa rola użytkownika.");
            }
            else
            {
                return null;
            }
        }
        public static User[] GetUsers()
        {
            using var connection = new MySqlConnection(ConnectionString);
            var getUsersQuery = "SELECT Id, Username, Password, Salt, Role FROM Users";
            var users = connection.Query<User>(getUsersQuery).ToArray();
            return users;
        }
        public static void DeleteUser(int userId)
        {
            using var connection = new MySqlConnection(ConnectionString);
            var deleteUserQuery = "DELETE FROM Users WHERE Id = @Id";
            connection.Execute(deleteUserQuery, new { Id = userId });
        }
        public static void UpdateUser(int userId, string username, string password, string role)
        {
            using var connection = new MySqlConnection(ConnectionString);
            var salt = GenerateSalt();
            var passwordHash = GetPasswordHash(password, salt);
            var saltString = Convert.ToBase64String(salt);
            var updateUserQuery = "UPDATE Users SET Username = @Username, Password = @Password, Salt = @Salt, Role = @Role WHERE Id = @Id";
            connection.Execute(updateUserQuery, new { Id = userId, Username = username, Password = passwordHash, Salt = saltString, Role = role });
        }
        public static void UpdateUserWithoutPassword(int userId, string username, string role)
        {
            using var connection = new MySqlConnection(ConnectionString);
            var updateUserQuery = "UPDATE Users SET Username = @Username, Role = @Role WHERE Id = @Id";
            connection.Execute(updateUserQuery, new { Id = userId, Username = username, Role = role });
        }
    }
}
