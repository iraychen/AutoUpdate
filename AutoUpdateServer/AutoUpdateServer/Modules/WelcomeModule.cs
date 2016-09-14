using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.ViewModel;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AutoUpdateServer.Moudles
{
    public class WelcomeModule : NancyModule
    {
        public WelcomeModule()
        {

            #region LoginRemote
            Get["/"] = _ =>
            {
                return View["Login"];
            };
            Get["Login"] = _ =>
            {
                //生成CSRF token.
                this.CreateNewCsrfToken();
                return View["Login"];
            };
            Post["Login"] = p =>
            {
                //CSRF token 检验
                this.ValidateCsrfToken();
                var loginModel = this.Bind<LoginModel>();
                var model = LoginViewModel.Verify(UserManageViewModel.GetData(), loginModel.username, loginModel.password);
                if (model != null)
                {
                    Guid guid = Guid.NewGuid();
                    Context.Request.Session[guid.ToString()] = model;
                    return this.LoginAndRedirect(guid, fallbackRedirectUrl: "/index");
                }
                return View["Login"];
            };
            Get["LoginOut"] = _ =>
            {
                Session.DeleteAll();
                return this.LogoutAndRedirect("~/");
            };
            #endregion

            #region ClientUpdate

            Get["api/DownLoadNewest/{HopitalID}/{VersionID}"] = p =>
            {
                DownLoadNewestResponseModel res =ClientUpdateViewModel.DownLoadNewest(p.HopitalID, p.VersionID);
                return Response.AsJson<DownLoadNewestResponseModel>(res);
            };

            Get["api/DownLoadNewestCompleted/{Packagename}"] = p =>
            {
                ResponseModel res = ClientUpdateViewModel.DeletePackage(p.Packagename);
                return Response.AsJson<ResponseModel>(res);
            };


            #endregion

            #region TEST
            Get["/down/{name}"] = _ =>
            {
                string fileName = _.name;
                return Response.AsFile(@"Content\uploads\123.txt");
            };

            Get["/show"] = _ =>
            {
                var uploadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "uploads");
                var folder = new DirectoryInfo(uploadDirectory);
                IList<string> files = new List<string>();
                foreach (var file in folder.GetFiles())
                {
                    files.Add(file.Name);
                }
                return View["Show", files];
            };
            #endregion
        }

        public class LoginModel
        {
            public string username { get; set; }

            public string password { get; set; }
        }
    }
}