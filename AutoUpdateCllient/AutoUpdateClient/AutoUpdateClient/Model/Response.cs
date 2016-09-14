using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Model
{
    public class ResponseModel
    {
        public bool Success { get; set; }

        public string Msg { get; set; }

        public NewestVersionModel Data { get; set; }
    }

    public class NewestVersionModel
    {
        public string FilePath { get; set; }

        public string Version { get; set; }
    }
}
