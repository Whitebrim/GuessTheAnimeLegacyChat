﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Chat.User;
using MySql.Data.MySqlClient;

namespace Chat.Messages
{
    public class MessageBase
    {
        public int id { get; set; }
        public int uID { get; set; }
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
            Callback callback = await AnalyseUser(UserId, Message);
            if (callback.valid)
            {
                UserId = callback.userId;

                #region Moderation
                //if (Message.Length > 100) Message = Message.Substring(0, 97) + "...";
                Message = Message.Replace("[", "");
                Message = Message.Replace("]", "");
                Message = Regex.Replace(Message, @"<size=[^>]*>", "<size=100%>");
                Message = Regex.Replace(Message, "(<s(( |=)[^>]*)?>)?(<b(( |=)[^>]*)?>)?(<u(( |=)[^>]*)?>)?(<i(( |=)[^>]*)?>)?", String.Empty);
                #endregion

                using var cmd = Db.Connection.CreateCommand();
                cmd.CommandText = @"INSERT INTO `legacychat` (`UID`, `UserId`, `Message`) VALUES (@uid, @userId, @text);";
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
                cmd.Parameters.Add(new MySqlParameter
                {
                    ParameterName = "@uid",
                    DbType = DbType.String,
                    Value = callback.UID,
                });
                await cmd.ExecuteNonQueryAsync();
            }
            return;
        }

        private async Task<Callback> AnalyseUser(string userId, string message)
        {
            #region Moderation
            string untagged = Regex.Replace(userId, "<.*?>", String.Empty);
            #endregion
            
            var callback = new Callback()
            {
                valid = false
            };
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"SELECT * FROM `legacyuserlist` WHERE `UserId` = @userId";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@userId",
                DbType = DbType.String,
                Value = userId,
            });
            var result = await ReadUserAsync(await cmd.ExecuteReaderAsync());
            if (result != null)
            {
                if (result.LastMessageCreated.AddSeconds(2) <= DateTime.Now.AddHours(3))
                {
                    if (result.LastMessageText != ComputeSha256Hash(message) || result.LastMessageCreated.AddSeconds(20) <= DateTime.Now.AddHours(3))
                    {
                        callback.valid = true;
                        callback.userId = result.UserIdChanger;
                        callback.UID = result.id;
                        UserBase user = new UserBase()
                        {
                            UserId = userId,
                            LastMessageText = ComputeSha256Hash(message)
                        };
                        await UpdateUser(user);
                    }
                }
            }
            else
            {
                callback.valid = true;
                callback.userId = userId;
                UserBase newUser = new UserBase()
                {
                    UserId = userId,
                    UserIdChanger = userId,
                    LastMessageText = ComputeSha256Hash(message)
                };

                #region Moderation
                string managedNickname = userId;
                managedNickname = managedNickname.Replace("[", "");
                managedNickname = managedNickname.Replace("]", "");
                if (managedNickname.Length>21 && managedNickname.Substring(0,21) == "<sprite=\"16\" index=0>")
                    managedNickname = managedNickname.Substring(0, 21) + Regex.Replace(managedNickname.Substring(21), "<sprite=\"16\"[^>]*>", String.Empty);
                else
                    managedNickname = Regex.Replace(managedNickname, "<sprite=\"16\"[^>]*>", String.Empty);
                managedNickname = Regex.Replace(managedNickname, "(<s(( |=)[^>]*)?>)?(<b(( |=)[^>]*)?>)?(<u(( |=)[^>]*)?>)?(<i(( |=)[^>]*)?>)?", String.Empty);
                managedNickname = Regex.Replace(managedNickname, @"<size=[^>]*>", "<size=100%>");
                if (untagged.Length > 120)
                    managedNickname = "У меня слишком длинный ник";
                untagged = untagged.Replace("[", "");
                untagged = untagged.Replace("]", "");
                if (managedNickname != "<#d74a2a>Admin</color>")
                    if (Regex.IsMatch(untagged, @"[aа][dд][mм][iіїиn][nпhн]", RegexOptions.IgnoreCase))
                        managedNickname = "<#r11>С<#br2>А<#rt1>М<#shs>О<#bonk>З<#yyy16l>В<#oskk>А<#rtyo41>Н<#b11>Е<#he9>Ц";
                newUser.UserIdChanger = managedNickname;
                #endregion

                callback.userId = newUser.UserIdChanger;
                callback.UID = await AddNewUser(newUser);
            }
            return callback;
        }

        private async Task<int> AddNewUser(UserBase newUser)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"INSERT INTO `legacyuserlist` (`UserId`, `UserIdChanger`, `LastMessageText`) VALUES (@userId, @userIdChanger, @lastMessageText);";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@userId",
                DbType = DbType.String,
                Value = newUser.UserId,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@userIdChanger",
                DbType = DbType.String,
                Value = newUser.UserIdChanger,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@lastMessageText",
                DbType = DbType.String,
                Value = newUser.LastMessageText,
            });
            await cmd.ExecuteNonQueryAsync();
            return (int)cmd.LastInsertedId;
        }

        private async Task UpdateUser(UserBase user)
        {
            using var cmd = Db.Connection.CreateCommand();
            cmd.CommandText = @"UPDATE `legacyuserlist` SET `LastMessageText` = @lastMessageText, `LastMessageDate` = @lastMessageDate WHERE `UserId` = @userId;";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@userId",
                DbType = DbType.String,
                Value = user.UserId,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@lastMessageText",
                DbType = DbType.String,
                Value = user.LastMessageText,
            });
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@lastMessageDate",
                DbType = DbType.DateTime,
                Value = DateTime.Now.AddHours(3),
            });
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<UserBase> ReadUserAsync(DbDataReader reader)
        {
            var posts = new List<UserBase>();
            using (reader)
            {
                while (await reader.ReadAsync())
                {
                    var post = new UserBase(Db)
                    {
                        id = reader.GetInt32(0),
                        UserId = reader.GetString(1),
                        UserIdChanger = reader.GetString(2),
                        DateCreated = reader.GetDateTime(3),
                        LastMessageCreated = reader.GetDateTime(4),
                        LastMessageText = reader.GetString(5)
                    };
                    posts.Add(post);
                }
            }
            return posts.Count > 0 ? posts[0] : null;
        }

        private string ComputeSha256Hash(string rawData)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private class Callback{
            public bool valid = false;
            public string userId = null;
            public int UID;
        }
    }
}
