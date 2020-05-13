using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chat.Messages
{
    public class MessageQuery
    {
        public AppDb Db { get; }

        public MessageQuery(AppDb db)
        {
            Db = db;
        }

        public async Task<MessageBase> FindOneAsync(int id)
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

        public async Task<List<MessageBase>> LatestPostsAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM( SELECT * FROM `legacychat` ORDER BY `id` DESC LIMIT 26) legacychat ORDER BY id ASC;";
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

        private async Task<List<MessageBase>> ReadAllAsync(DbDataReader reader)
        {
            var posts = new List<MessageBase>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new MessageBase(Db)
                    {
                        id = reader.GetInt32(0),
                        uID = reader.GetInt32(1),
                        UserId = reader.GetString(2),
                        Message = reader.GetString(3),
                        Date = reader.GetDateTime(4).ToString("dd.MM.yy HH:mm:ss"),
                    };
                    posts.Add(post);
                }
            }
            return posts;
        }
    }
}