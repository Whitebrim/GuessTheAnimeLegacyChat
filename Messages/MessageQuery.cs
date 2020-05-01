using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chat
{
    public class MessageQuery
    {
        public AppDb Db { get; }

        public MessageQuery(AppDb db)
        {
            Db = db;
        }

        public async Task<Message> FindOneAsync(int id)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM `legacychat` WHERE `id` = @id";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@id",
                DbType = DbType.Int32,
                Value = id,
            });
            var result = await ReadAllAsync(await cmd.ExecuteReaderAsync());
            return result.Count > 0 ? result[0] : null;
        }

        public async Task<List<Message>> LatestPostsAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM `legacychat` ORDER BY `id` DESC LIMIT 20;";
            return await ReadAllAsync(await cmd.ExecuteReaderAsync());
        }

        public async Task DeleteAllAsync()
        {
            using var txn = await Db.Connection.BeginTransactionAsync();
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"DELETE FROM `legacychat`";
            await cmd.ExecuteNonQueryAsync();
            await txn.CommitAsync();
        }

        private async Task<List<Message>> ReadAllAsync(DbDataReader reader)
        {
            var posts = new List<Message>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new Message(Db)
                    {
                        id = reader.GetInt32(0),
                        nickname = reader.GetString(1),
                        messageBody = reader.GetString(2),
                        messageDate = reader.GetDateTime(3),
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}