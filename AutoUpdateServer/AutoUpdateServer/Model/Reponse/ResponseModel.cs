using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Reponse.Model
{
    public class ResponseModel
    {
        public bool Success { get; set; }
    
        public string Msg { get; set; }
    }

    public class RequestNewestPackageUrlResponseModel : ResponseModel
    {
        public NewestVersionModel Data { get; set; } = new NewestVersionModel();
    }
}