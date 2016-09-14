using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Common
{
    public class ConstFile
    {
        public const string CLIENTFILESDIRETORYNAME = "ClientFiles";

        public const string CONTENTFILEDIRECTORY = "Content";

        public const string BASEVERSION = "2.0.0";

        public static string BaseDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,CLIENTFILESDIRETORYNAME);

        public static string WorkPath = Path.Combine(BaseDirectory, "Work");

        public static string WareHousePath = Path.Combine(BaseDirectory, "WareHouse");

        public static string PackagesPath = Path.Combine(BaseDirectory, "Packages");

        public static string TempPath = Path.Combine(BaseDirectory, "Temp");

        public static string BaseFilePath = Path.Combine(BaseDirectory, "BaseModel");
    }
}