using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class UpdatePackageInfo
    {
        public UpdatePackageInfo()
        {
            Introduction = new List<string>();
        }
        public string Version { get; set; }

        public List<string> Introduction { get; set; }

        public string Path { get; set; }

        public long Size { get; set; }

        public string UpLoadTime { get; set; }

        public string NewestVersion{ get; set; }
    }
}