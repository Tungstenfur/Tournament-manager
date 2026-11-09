using System;
using System.Collections.Generic;
using System.Text;
using MySqlConnector;
using Dapper;
using System.Data;
using System.Diagnostics;

namespace TournamentManager.Database
{
    internal class Database : DatabaseData
    {
        
        public static bool CreateDatabase()
        {
            const string query = @" CREATE TABLE IF NOT EXISTS Users ( 
                Id INT NOT NULL AUTO_INCREMENT PRIMARY KEY,
                Username VARCHAR(255) NOT NULL UNIQUE,
                Password VARCHAR(255) NOT NULL,
                Salt VARCHAR(255) NOT NULL,
                Role VARCHAR(31) NOT NULL );

               CREATE TABLE IF NOT EXISTS players (
                id INT PRIMARY KEY AUTO_INCREMENT,
                name VARCHAR(255) NOT NULL,
                lastName VARCHAR(255) NOT NULL,
                birthDate DATE NOT NULL,
                matches INT NOT NULL DEFAULT 0,
                wins INT NOT NULL DEFAULT 0,
                losses INT NOT NULL DEFAULT 0,
                points INT NOT NULL DEFAULT 0
            );

                CREATE TABLE IF NOT EXISTS tournaments (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    name VARCHAR(255) NOT NULL UNIQUE,
                    date DATE NOT NULL,
                    players INT NOT NULL DEFAULT 0,
                    winner INT NULL,
                    CONSTRAINT fk_tournament_winner
                        FOREIGN KEY (winner)
                        REFERENCES players(id)
                        ON DELETE RESTRICT
                );

                CREATE TABLE IF NOT EXISTS tournament_players (
                    tournament_id INT NOT NULL,
                    player_id INT NOT NULL,
                    PRIMARY KEY (tournament_id, player_id),
                    CONSTRAINT fk_tournament_players_tournament
                        FOREIGN KEY (tournament_id)
                        REFERENCES tournaments(id)
                        ON DELETE CASCADE,
                    CONSTRAINT fk_tournament_players_player
                        FOREIGN KEY (player_id)
                        REFERENCES players(id)
                        ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS matches (
                    id INT PRIMARY KEY AUTO_INCREMENT,
                    tournament INT NOT NULL,
                    player1 INT NOT NULL,
                    player2 INT NOT NULL,
                    winner INT NULL,

                    CONSTRAINT fk_match_tournament
                        FOREIGN KEY (tournament)
                        REFERENCES tournaments(id)
                        ON DELETE RESTRICT,

                    CONSTRAINT fk_match_player1
                        FOREIGN KEY (player1)
                        REFERENCES players(id)
                        ON DELETE RESTRICT,

                    CONSTRAINT fk_match_player2
                        FOREIGN KEY (player2)
                        REFERENCES players(id)
                        ON DELETE RESTRICT,

                    CONSTRAINT fk_match_winner
                        FOREIGN KEY (winner)
                        REFERENCES players(id)
                        ON DELETE RESTRICT
                                                   
                );

                INSERT IGNORE INTO tournament_players (tournament_id, player_id)
                SELECT tournament, player1 FROM matches
                UNION
                SELECT tournament, player2 FROM matches;

                DROP TRIGGER IF EXISTS update_tournament_players_after_match_insert;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_update;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_delete;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_insert
                AFTER INSERT ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = NEW.tournament_id
                    )
                    WHERE id = NEW.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_delete
                AFTER DELETE ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = OLD.tournament_id
                    )
                    WHERE id = OLD.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_player_stats_after_match_insert
                AFTER INSERT ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_update
                AFTER UPDATE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);

                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_delete
                AFTER DELETE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);
                END;

                DROP TRIGGER IF EXISTS update_player_points_before_insert;
                DROP TRIGGER IF EXISTS update_player_points_before_update;

                CREATE TRIGGER IF NOT EXISTS update_player_points_before_insert
                BEFORE INSERT ON players
                FOR EACH ROW
                BEGIN
                    SET NEW.points = NEW.wins - NEW.losses;
                END;

                CREATE TRIGGER IF NOT EXISTS update_player_points_before_update
                BEFORE UPDATE ON players
                FOR EACH ROW
                BEGIN
                    SET NEW.points = NEW.wins - NEW.losses;
                END;

                ALTER TABLE matches ADD COLUMN IF NOT EXISTS round INT NOT NULL DEFAULT 0;

                INSERT IGNORE INTO tournament_players (tournament_id, player_id)
                SELECT tournament, player1 FROM matches
                UNION
                SELECT tournament, player2 FROM matches;

                DROP TRIGGER IF EXISTS update_tournament_players_after_match_insert;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_update;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_delete;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_insert
                AFTER INSERT ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = NEW.tournament_id
                    )
                    WHERE id = NEW.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_delete
                AFTER DELETE ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = OLD.tournament_id
                    )
                    WHERE id = OLD.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_player_stats_after_match_insert
                AFTER INSERT ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_update
                AFTER UPDATE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);

                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_delete
                AFTER DELETE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);
                END;

                INSERT IGNORE INTO tournament_players (tournament_id, player_id)
                SELECT tournament, player1 FROM matches
                UNION
                SELECT tournament, player2 FROM matches;

                DROP TRIGGER IF EXISTS update_tournament_players_after_match_insert;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_update;
                DROP TRIGGER IF EXISTS update_tournament_players_after_match_delete;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_insert
                AFTER INSERT ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = NEW.tournament_id
                    )
                    WHERE id = NEW.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_tournament_players_after_assign_delete
                AFTER DELETE ON tournament_players
                FOR EACH ROW
                BEGIN
                    UPDATE tournaments
                    SET players = (
                        SELECT COUNT(*)
                        FROM tournament_players
                        WHERE tournament_id = OLD.tournament_id
                    )
                    WHERE id = OLD.tournament_id;
                END;

                CREATE TRIGGER IF NOT EXISTS update_player_stats_after_match_insert
                AFTER INSERT ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_update
                AFTER UPDATE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);

                    UPDATE players
                    SET matches = matches + 1,
                        wins = wins + IF(id = NEW.winner, 1, 0),
                        losses = losses + IF(NEW.winner IS NOT NULL AND id <> NEW.winner, 1, 0)
                    WHERE id IN (NEW.player1, NEW.player2);
                END;

                CREATE TRIGGER if not exists update_player_stats_after_match_delete
                AFTER DELETE ON matches
                FOR EACH ROW
                BEGIN
                    UPDATE players
                    SET matches = matches - 1,
                        wins = wins - IF(id = OLD.winner, 1, 0),
                        losses = losses - IF(OLD.winner IS NOT NULL AND id <> OLD.winner, 1, 0)
                    WHERE id IN (OLD.player1, OLD.player2);
                END;

                DROP TRIGGER IF EXISTS update_player_points_before_insert;
                DROP TRIGGER IF EXISTS update_player_points_before_update;

                CREATE TRIGGER IF NOT EXISTS update_player_points_before_insert
                BEFORE INSERT ON players
                FOR EACH ROW
                BEGIN
                    SET NEW.points = NEW.wins - NEW.losses;
                END;

                CREATE TRIGGER IF NOT EXISTS update_player_points_before_update
                BEFORE UPDATE ON players
                FOR EACH ROW
                BEGIN
                    SET NEW.points = NEW.wins - NEW.losses;
                END;

                ALTER TABLE matches ADD COLUMN IF NOT EXISTS round INT NOT NULL DEFAULT 0;
";

            const string checkUsersQuery = "SELECT COUNT(*) FROM Users WHERE ROLE='superadmin'";
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();
            try
            {
                connection.Execute(query, transaction: transaction);
                transaction.Commit();
            }
            catch(Exception e)
            {
                transaction.Rollback();
                Debug.WriteLine(e);
                throw;
            }

            var count = connection.QuerySingle<int>(checkUsersQuery);
            //Debug.WriteLine($"Superadmins: {count}");
            return count != 0;
        }
    }
}
