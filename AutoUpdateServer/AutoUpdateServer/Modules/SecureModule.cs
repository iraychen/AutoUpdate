using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.ViewModel;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Modules
{
    public class SecureModule : NancyModule
    {
        public SecureModule()
        {
            this.RequiresAuthentication();

            #region IndexRemote
            Get["index"] = _ =>
            {
                ViewBag["userName"] = this.Context.CurrentUser.UserName;
                return View["index"];
            };
            #endregion

            #region UserManageRemote
            Get["UserManage"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                return View["UserManage", UserManageViewModel.GetData()];
            };
            Post["QueryUser"] = p =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                string name = Request.Form["name"];
                var users = UserManageViewModel.GetData(name);
                return View["UserManage", users];
            };
            Post["checkUserName/{Name}"] = p =>
            {
                return (UserManageViewModel.GetData()?.FirstOrDefault(t => t.Name == p.Name) != null);
            };
            Get["UserAdd"] = _ =>
            {
                return View["UserAdd"];
            };
            Post["UserAdd/{Name}/{PassWord}/{Status}"] = p =>
            {
                return UserManageViewModel.Insert(p.Name, p.PassWord, p.Status);
            };
            Get["UserEdit/{Name}"] = p =>
            {
                var users = UserManageViewModel.GetData();
                var model = users?.FirstOrDefault(t => t.Name == p.Name);
                return (model != null) ? View["UserEdit", model] : View["UserManage", users];
            };
            Post["UserEdit"] = _ =>
            {
                UserManageViewModel.Update(Request.Form);
                return Response.AsRedirect("UserManage");
            };
            Post["UserDelete/{Name}"] = p =>
            {
                var model = UserManageViewModel.GetData()?.FirstOrDefault(t => t.Name == p.Name);
                return (model != null) && (UserManageViewModel.Delete(model));
            };
            #endregion

            #region HospitalManagerRemote
            Get["HospitalManage"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                return View["HospitalManage", HospitalManageViewModel.GetData()];
            };
            Post["QueryHospital"] = p =>
            {
                string name = Request.Form["name"];
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                return View["HospitalManage", HospitalManageViewModel.GetData(name)];
            };
            Post["checkHospitalID/{HospitalID}"] = p =>
            {
                var hospitals = HospitalManageViewModel.GetData();
                return (hospitals?.FirstOrDefault(t => t.ID == p.HospitalID) != null);
            };
            Get["HospitalAdd"] = _ =>
            {
                return View["HospitalAdd"];
            };
            Post["HospitalAdd/{HospitalID}/{Name}"] = p =>
            {
                return HospitalManageViewModel.Insert(p.HospitalID, p.Name);
            };
            Get["HospitalEdit/{HospitalID}"] = p =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                var model = HospitalManageViewModel.GetData()?.FirstOrDefault(t => t.ID == p.HospitalID);
                if (model != null)
                {
                    return View["HospitalEdit", model];
                }
                return Response.AsRedirect("HospitalManage");
            };
            Post["HospitalEdit/"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                HospitalManageViewModel.Update(Request.Form);
                return Response.AsRedirect("HospitalManage");
            };
            Post["HospitalDelete/{HospitalID}"] = p =>
            {
                var model = HospitalManageViewModel.GetData()?.FirstOrDefault(t => t.ID == p.HospitalID);
                return (model != null) ? HospitalManageViewModel.Delete(model) : false;
            };
            #endregion

            #region VersionManageRemote
            Get["VersionManage/{HospitalID}"] = p =>
            {
                var versionModels = VersionManageViewModel.GetModels(p.HospitalID);
                var hospitalModel = HospitalManageViewModel.GetData()?.FirstOrDefault(s => s.ID == p.HospitalID);
                Session["versionModels"] = versionModels;
                ViewBag["NewestVersion"] = hospitalModel.NewestVersion;
                return View["VersionManage", versionModels];
            };
            Get["VersionEdit/{VersionID}"] = p =>
            {
                var model = VersionManageViewModel.GetModel(p.VersionID);
                return (model != null) ? View["VersionEdit", model] : View["VersionManage/" + model.HospitalID];
            };
            Post["VersionEdit"] = _ =>
            {
                VersionManageViewModel.Update(Request.Form);
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                string url = string.Format("VersionManage/{0}", Request.Form["HospitalID"]);
                return Response.AsRedirect(url);
            };
            Post["VersionDelete/{VersionID}/{HospitalID}"] = p =>
            {
                var errorMsg = VersionManageViewModel.Delete(p.VersionID, p.HospitalID);
                return (!string.IsNullOrEmpty(errorMsg)) ? errorMsg : null;
            };

            Post["VersionSet/{VersionNumber}/{HospitalID}"] = p =>
            {
                return VersionManageViewModel.Set(p.VersionNumber, p.HospitalID);
            };
            #endregion

            #region UpLoadRemote
            Get["HospitalFileUpLoad"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission; ;
                return View["HospitalFileUpLoad"];
            };

            Post["api/HospitalFileUpLoadBaseModelFile"] = p =>
            {
                var model = new ResponseModel();
                if (HospitalFileUpLoadViewModel.instance.IsRuning)
                {
                    model.Success = false;
                    model.Msg = "服务端正在文件处理，请稍后再上传";
                }
                else
                {
                    lock (HospitalFileUpLoadViewModel.instance)
                    {
                        model = HospitalFileUpLoadViewModel.instance.BatchBaseModelFile(Request.Files, this.Context.CurrentUser.UserName);
                    }
                }
                return Response.AsJson<ResponseModel>(model);
            };

            Post["api/HospitalFileUpLoad"] = p =>
            {
                var model = new ResponseModel();
                if (HospitalFileUpLoadViewModel.instance.IsRuning)
                {
                    model.Success = false;
                    model.Msg = "服务端正在文件处理，请稍后再上传";
                }
                else
                {
                    lock (HospitalFileUpLoadViewModel.instance)
                    {
                        model = HospitalFileUpLoadViewModel.instance.BatchFile(Request.Files, this.Context.CurrentUser.UserName);
                    }
                }
                return Response.AsJson<ResponseModel>(model);
            };
            #endregion
        }
    }
}