using AutoUpdateServer.Common;
using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class LoginViewModel
    {
        public static UserModel Verify(List<UserModel> models,string name,string passWord)
        {
            var model = models.FirstOrDefault(t => t.Name == name);
            if (model != null)
            {
                var md5PassWord = MD5Helper.MD5Encode(passWord);
                if (md5PassWord != model.PassWord)
                {
                    model = null;
                }
            }
            return model;
        }
    }
}

