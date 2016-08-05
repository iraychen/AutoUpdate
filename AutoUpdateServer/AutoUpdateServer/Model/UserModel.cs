using AutoUpdateServer.Common;
using AutoUpdateServer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class UserModel
    {
        [PrimaryKey]
        public string Name { get; set; }

        //[AutoIncrement]
        //public int ID { get; set; }

        [NotNull]
        public string PassWord { get; set; }

        [NotNull]
        public Permission Permission { get; set; }

        [NotNull]
        public DateTime LastLoginTime { get; set; }
    }
}