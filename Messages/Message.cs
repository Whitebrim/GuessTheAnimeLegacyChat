using System;
using System.Data;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Chat
{
    public class Message
    {
        public int id { get; set; }
        public string nickname { get; set; }
        public string messageBody { get; set; }
        public DateTime messageDate { get; set; }

        internal AppDb Db { get; set; }

        public Message()
        {
        }

        internal Message(AppDb db)
        {
            Db = db;
        }

        public async Task InsertAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `legacychat` (`id`, `nickname`, `messageBody`, `messageDate`) VALUES (NULL, @nickname, @messageBody, NULL);";
            BindParams(cmd);
            await cmd.ExecuteNonQueryAsync();
            //Id = (int)cmd.LastInsertedId;
        }

        public async Task UpdateAsync()
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `legacychat` SET `nickname` = @nickname, `messageBody` = @messageBody WHERE `id` = @id;";
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

        private void BindTime(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@messageDate",
                DbType = DbType.DateTime,
                Value = messageDate,
            });
        }

        private void BindParams(MySqlCommand cmd)
        {
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@messageBody",
                DbType = DbType.String,
                Value = messageBody,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@nickname",
                DbType = DbType.String,
                Value = nickname,
            });
        }
    }
}
