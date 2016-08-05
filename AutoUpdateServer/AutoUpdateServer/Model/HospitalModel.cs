using AutoUpdateServer.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class HospitalModel
    {
        [PrimaryKey]
        public int HosPitalID { get; set; }

        [NotNull]
        public string Name { get; set; }

        [NotNull]
        public string CurrentVersion { get; set; }
    }
}