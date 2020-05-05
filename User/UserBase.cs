using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Chat.User
{
    public class UserBase
    {
        public int id { get; set; }
        public string UserId { get; set; }
        public string UserIdChanger { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastMessageCreated { get; set; }
        public string LastMessageText { get; set; }

        internal AppDb Db { get; set; }

        public UserBase()
        {
        }

        internal UserBase(AppDb db)
        {
            Db = db;
        }
    }
}
