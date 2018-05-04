using System;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using MUD_Server.Game.Entities.Players;

namespace MUD_Server.Essentials.Storing
{
    public class UserSQL : SQL_DB
    {
        private static (byte[], byte[]) GetHash(string password, string salt = null)
        {
            byte[] saltArr = null;

            if (string.IsNullOrEmpty(salt))
            {
                saltArr = new byte[new Random().Next(6, 10)];
                new RNGCryptoServiceProvider().GetNonZeroBytes(saltArr);
            }
            else saltArr = Convert.FromBase64String(salt);

            byte[] password_UTF8 = Encoding.UTF8.GetBytes(password);
            byte[] saltedPassword = new byte[saltArr.Length + password_UTF8.Length];

            for (int i = 0; i < saltArr.Length; i++) saltedPassword[i] = saltArr[i];
            for (int i = 0; i < password_UTF8.Length; i++) saltedPassword[saltArr.Length + i] = password_UTF8[i];

            return (new SHA256Managed().ComputeHash(saltedPassword), saltArr);
        }

        public UserSQL(string path) : base(path)
        {
        }

        public async Task CreateUser(string email, string username, string password)
        {
            var hash = GetHash(password);

            int lastID = -1;
            var maxIDReader = await new SQLiteCommand("SELECT * FROM users ORDER BY ID DESC LIMIT 1", db_Connection).ExecuteReaderAsync();

            if(maxIDReader.Read()) lastID = (int)maxIDReader["ID"];

            var command = new SQLiteCommand(
                $"INSERT INTO users (ID, Username, PasswordHash, PasswordSalt, Email) " +
                $"VALUES ('{lastID + 1}', '{username}', @hash, '{Convert.ToBase64String(hash.Item2)}', '{email}')", db_Connection);

            command.Parameters.Add("@hash", System.Data.DbType.Binary, 32).Value = hash.Item1;
            await command.ExecuteNonQueryAsync();
        }

        public async Task<bool> UserExistsWithEmail(string email) => (await new SQLiteCommand($"SELECT * FROM users WHERE Email = '{email}'", db_Connection).ExecuteReaderAsync()).Read();
        public async Task<bool> UserExistsWithUsername(string username) => (await new SQLiteCommand($"SELECT * FROM users WHERE Username = '{username}'", db_Connection).ExecuteReaderAsync()).Read();

        public async Task<SocketUser> GetUser(SocketUser user, string username, string password)
        {
            var reader = await new SQLiteCommand($"SELECT * FROM users WHERE Username = '{username}'", db_Connection).ExecuteReaderAsync();

            if(reader.Read())
            {
                int ID = (int)reader["ID"];
                string salt = reader["PasswordSalt"].ToString();
                string email = reader["Email"].ToString();
                byte[] hash = (byte[])reader["PasswordHash"];

                byte[] inputHash = GetHash(password, salt).Item1;

                if(hash.SequenceEqual(inputHash))
                {
                    user.Email = email;
                    user.Username = username;

                    return user;
                }
            }

            return null;
        }

        public SocketCharacter GetCharacter(SocketUser user)
        {
            return null;
        }
    }
}
