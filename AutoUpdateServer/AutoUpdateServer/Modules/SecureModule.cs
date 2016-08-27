using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.ViewModel;
using Nancy;
using Nancy.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.Modules
{
    public class SecureModule : NancyModule
    {
        private static List<UserModel> users;
        private List<UserModel> Users
        {
            get
            {
                return users ?? UserManageViewModel.GetData();
            }
            set
            {
                users = value;
            }
        }

        private static List<HospitalModel> hospitals;
        private List<HospitalModel> Hospitals
        {
            get
            {
                return hospitals ?? HospitalManageViewModel.GetData();
            }
            set
            {
                hospitals = value;
            }
        }
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
                return View["UserManage", this.Users];
            };
            Post["QueryUser"] = p =>
            {
                string name = Request.Form["name"];
                this.Users = UserManageViewModel.GetData(name);
                return View["UserManage", this.Users];
            };
            Post["checkUserName/{Name}"] = p =>
            {
                var reaponseModel = new ResponseModel();
                string name = p.Name;
                if (this.Users?.FirstOrDefault(t => t.Name == name) == null)
                {
                    reaponseModel.Success = true;
                }
                else
                {
                    reaponseModel.Msg = "用户名已存在";
                }
                return Response.AsJson(reaponseModel);
            };
            Get["UserAdd"] = _ =>
            {
                return View["UserAdd"];
            };
            Post["UserAdd/{Name}/{PassWord}/{Status}"] = p =>
            {
                var responseModel = new ResponseModel();
                if (UserManageViewModel.Insert(p.Name, p.PassWord, p.Status))
                {
                    this.Users = UserManageViewModel.GetData();
                    responseModel.Success = true;
                }
                return Response.AsJson<ResponseModel>(responseModel);
            };
            Get["UserEdit/{Name}"] = p =>
            {
                var model = this.Users?.FirstOrDefault(t => t.Name == p.Name);
                if (model != null)
                {
                    return View["UserEdit", model];
                }
                return View["UserManage", this.Users];
            };
            Post["UserEdit"] = _ =>
            {
                UserManageViewModel.Update(Request.Form);
                this.Users = UserManageViewModel.GetData();
                return View["UserManage", this.Users];
            };
            Post["UserDelete/{Name}"] = p =>
            {
                var reponseMode = new ResponseModel();
                if (!string.IsNullOrEmpty(p.Name))
                {
                    var model = this.Users?.FirstOrDefault(t => t.Name == p.Name);
                    if (model != null)
                    {
                        if (UserManageViewModel.Delete(model))
                        {
                            this.Users = UserManageViewModel.GetData();
                            reponseMode.Success = true;
                        }
                    }
                }
                return Response.AsJson(reponseMode);
            };
            #endregion

            #region HospitalManagerRemote
            Get["HospitalManage"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                return View["HospitalManage", this.Hospitals];
            };
            Post["QueryHospital"] = p =>
            {
                string name = Request.Form["name"];
                this.Hospitals = HospitalManageViewModel.GetData(name);
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission;
                return View["HospitalManage", this.Hospitals];
            };
            Post["checkHospitalID/{HospitalID}"] = p =>
            {
                var reaponseModel = new ResponseModel();
                int HospitalID = p.HospitalID;
                if (this.Hospitals?.FirstOrDefault(t => t.ID == HospitalID) == null)
                {
                    reaponseModel.Success = true;
                }
                else
                {
                    reaponseModel.Msg = "已存在医院编号";
                }
                return Response.AsJson(reaponseModel);
            };
            Get["HospitalAdd"] = _ =>
            {
                return View["HospitalAdd"];
            };
            Post["HospitalAdd/{HospitalID}/{Name}"] = p =>
            {
                var responseModel = new ResponseModel();
                if (HospitalManageViewModel.Insert(p.HospitalID, p.Name))
                {
                    this.Hospitals = HospitalManageViewModel.GetData();
                    responseModel.Success = true;
                }
                return Response.AsJson<ResponseModel>(responseModel);
            };
            Get["HospitalEdit/{HospitalID}"] = p =>
            {
                var model = this.Hospitals?.FirstOrDefault(t => t.ID == p.HospitalID);
                if (model != null)
                {
                    return View["HospitalEdit", model];
                }
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission; ;
                return View["HospitalManage", this.Hospitals];
            };
            Post["HospitalEdit/"] = _ =>
            {
                HospitalManageViewModel.Update(Request.Form);
                this.Hospitals = HospitalManageViewModel.GetData();
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission; ;
                return View["HospitalManage", this.Hospitals];
            };
            Post["HospitalDelete/{HospitalID}"] = p =>
            {
                var reponseMode = new ResponseModel();
                var model = this.Hospitals?.FirstOrDefault(t => t.ID == p.HospitalID);
                if (model != null)
                {
                    if (HospitalManageViewModel.Delete(model))
                    {
                        this.Hospitals = HospitalManageViewModel.GetData();
                        reponseMode.Success = true;
                    }
                }
                return Response.AsJson(reponseMode);
            };

            Get["VersionManage/{HospitalID}"] = p =>
            {
                var models = VersionManageViewModel.GetModels(p.HospitalID);
                Session["versionModels"] = models;
                return View["VersionManage", models];
            };
            Get["VersionEdit/{VersionID}"] = p =>
            {
                var models = (List<VersionModel>)Session["versionModels"];
                var model = models == null ? null : models.FirstOrDefault(s => s.ID == p.VersionID);
                if (model != null)
                {
                    return View["VersionEdit", model];
                }
                return View["HospitalManage", this.Hospitals];
            };
            Post["VersionEdits/"] = _ =>
            {
                VersionManageViewModel.Update(Request.Form);
                this.Hospitals = HospitalManageViewModel.GetData();
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission; ;
                return View["HospitalManage", this.Hospitals];
            };
            Post["VersionDelete/{VersionID}"] = p =>
            {
                return null;
            };


            #endregion

            #region UpLoadRemote
            Get["HospitalFileUpLoad"] = _ =>
            {
                ViewBag["permission"] = ((UserIdentity)this.Context.CurrentUser).Permission; ;
                return View["HospitalFileUpLoad"];
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
                        string user = Request.Session["userName"].ToString();
                        model = HospitalFileUpLoadViewModel.instance.BatchFile(Request.Files, user);
                    }
                }
                return Response.AsJson<ResponseModel>(model);
            };
            #endregion

            #region Other
            Get["api/down/{name}"] = _ =>
            {
                string fileName = _.name;
                var relatePath = @"Content\uploads\" + fileName;
                return Response.AsFile(relatePath);
            };

            //Get["/show"] = _ =>
            //{
            //    var folder = new DirectoryInfo(uploadRootDirectory);
            //    IList<string> files = new List<string>();
            //    foreach (var file in folder.GetFiles())
            //    {
            //        files.Add(file.Name);
            //    }
            //    return View["Show", files];
            //};
            #endregion
        }
    }
}