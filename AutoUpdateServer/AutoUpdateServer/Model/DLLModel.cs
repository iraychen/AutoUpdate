using AutoUpdateServer.Common;
using AutoUpdateServer.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class DLLModel
    {
        [PrimaryKey,AutoIncrement]
        public int ID { get; set; }

        [NotNull]
        public string VersionID { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public UpdateStatus UpdateStatus { get; set; }

        [NotNull]
        public long Size { get; set; }
    }
}