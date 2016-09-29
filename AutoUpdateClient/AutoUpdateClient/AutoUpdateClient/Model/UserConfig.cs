using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Model
{
    public class UserConfig
    {
        public string ServerUrl { get; set; }

        public string HospitalID { get; set; }

        public string Version { get; set; }

        public string CheckClientAliveTime { get; set; }

        public string CheckUpdateTime { get; set; }
    }
}
