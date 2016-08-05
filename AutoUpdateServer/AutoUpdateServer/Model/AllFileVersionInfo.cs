using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model
{
    public class AllFileVersionInfo
    {
        public AllFileVersionInfo()
        {
            this.UpdatePackageInfoList = new List<UpdatePackageInfo>();
        }

        public List<UpdatePackageInfo> UpdatePackageInfoList { get; set; }

        public string NewestVersion { get; set; }
    }
}