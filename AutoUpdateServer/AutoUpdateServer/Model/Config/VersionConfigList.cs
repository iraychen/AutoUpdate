using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Model.Config
{
    public class VersionConfigList
    {
        public VersionConfigList()
        {
            VersionConfigs = new List<VersionConfig>();
        }

        public string NewestVersion { get; set; }
        public List<VersionConfig> VersionConfigs { get; set; }

    }
    public class VersionConfig
    {
        public VersionConfig()
        {
            DLLInfos = new List<DLLInfo>();
        }
        public string Version { get; set; }

        public string HospitalID { get; set; }

        public DateTime UpLoadTime { get; set; }

        public List<DLLInfo> DLLInfos { get; set; }
    }

    public class DLLInfo
    {
        public string Name { get; set; }

        public UpdateStatus UpdateStatus { get; set; }

        public long Size { get; set; }

    }

    public enum UpdateStatus
    {
        ADD,
        UPDATE,
        DELETE,
        KEEP
    }
}