using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chat.Messages
{
    public class MessageBase
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public string Message { get; set; }
        public string Date { get; set; }

        internal AppDb Db { get; set; }

        public MessageBase()
        {
        }

        public MessageBase(string userId, string text)
        {
            this.UserId = userId;
            this.Message = text;
        }

        internal MessageBase(AppDb db)
        {
            Db = db;
        }

        public async Task InsertAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `legacychat` (`UserId`, `Message`) VALUES (@userId, @text);";
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        /*
        public async Task UpdateAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `legacychat` SET `UserId` = @UserId, `Message` = @Message WHERE `id` = @id;";
            BindParams(cmd);
            BindId(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeleteAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM `legacychat` WHERE `id` = @id;";
            BindId(cmd);
            await cmd.ExecuteNonQueryAsync();
        }

        private void BindId(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.Int32,
                Value = id,
            });
        }
        */

        private void BindParams(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@text",
                DbType = DbType.String,
                Value = Message,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@userId",
                DbType = DbType.String,
                Value = UserId,
            });
        }
    }
}
