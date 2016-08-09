using AutoUpdateServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class VersionModel
    {

        [PrimaryKey]
        public string ID { get; set; }

        [NotNull]
        public int HospitalID { get; set; }

        [NotNull]
        public string Number { get; set; }

        [NotNull]
        public DateTime UpLoadTime { get; set; }

        [NotNull]
        public string Description { get; set; }
    }
}