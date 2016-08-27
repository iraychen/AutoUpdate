using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.ViewModel;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
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
                return View["Login"];
            };
            Post["Login"] = p =>
            {
                var loginModel = this.Bind<LoginModel>();
                var model = LoginViewModel.Verify(UserManageViewModel.GetData(), loginModel.username, loginModel.password);
                if (model != null)
                {
                    Guid guid = Guid.NewGuid();
                    Context.Request.Session[guid.ToString()] = model;
                    return this.LoginAndRedirect(guid, fallbackRedirectUrl: "/index");
                }
                return View["Login",ViewBag["ErrorTag"]="1"];
            };
            Get["LoginOut"] = _ =>
            {
                Session.DeleteAll();
                return this.LogoutAndRedirect("~/");
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