using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class ConstFile
    {
        public const string COMPAREINFOCONFIGFILENAME = "CompareInfoConfig{0}.xml";

        public const string COMPAREINFOCONFIGDIRETORYNAME = "CompareInfoConfigs";

        public const string CLIENTFILESDIRETORYNAME = "ClientFiles";

        public const string VERSIONCONFIGDIRETORYNAME = "VersionConfigsList";

        public const string VERSIONCONFIGFILENAME = "VersionConfigsList{0}.xml";

        public const string BASEVERSION = "2.0.0";
    }
}