using AutoUpdateClient.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class UserConfigInstance
    {
        
        private UserConfigInstance()
        {
            UserConfig = FileUtil.XMLLoadData<UserConfig>(ConfigPath);
        }

        public static readonly UserConfigInstance instance = new UserConfigInstance();

        public UserConfig UserConfig { get; set; }

        public string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "UserConfig.xml");
    }
}
