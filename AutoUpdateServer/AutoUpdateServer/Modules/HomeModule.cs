using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using AutoUpdateServer.ViewModel;
using Nancy;
using System.Collections.Generic;
using System.Linq;

namespace AutoUpdateServer.Moudles
{
    public class HomeModule : NancyModule
    {
        private static List<UserModel> users;
        public List<UserModel> Users
        {
            get
            {
                return users ?? UserManageViewModel.GetData();
            }
            set
            {
                users = value;
                ViewBag["Users"] = value;
            }
        }
        public HomeModule()
        {
            #region LoginRemote
            Get["Login"] = _ =>
            {
                return View["Login"];
            };
            Post["Login/{userName}/{passWord}"] = p =>
            {
                string userName = p.userName;
                string passWord = p.passWord;
                var reponseMode = new ResponseModel();
                Permission permission;
                reponseMode.Success = LoginViewModel.Verify(this.Users, userName, passWord, out permission);
                if (reponseMode.Success)
                {
                    Session["userName"] = userName;
                    Session["permission"] = permission;
                }
                return Response.AsJson(reponseMode);
            };
            Get["LoginOut"] = _ =>
            {
                Request.Session["userName"] = null;
                return View["Login"];
            };
            #endregion

            #region IndexRemote
            Get["Index"] = _ =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                ViewBag["userName"] = Request.Session["userName"];
                return View["Index"];
            };
            #endregion

            #region UserManageRemote
            Get["UserManage"] = _ =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                this.Users = UserManageViewModel.GetData();
                ViewBag["permission"] = Session["permission"];
                return View["UserManage"];
            };
            Post["QueryUser"] = p =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                string name =Request.Form["name"];
                this.Users = UserManageViewModel.GetData(name);
                ViewBag["permission"] = Session["permission"];
                return View["UserManage"];
            };
            Post["checkName/{Name}"] = p =>
            {
                var reaponseModel = new ResponseModel();
                string name = p.Name;
                var users = UserManageViewModel.GetData();
                if (users == null || users.FirstOrDefault(t => t.Name == name) == null)
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                return View["UserAdd"];
            };
            Post["UserAdd/{Name}/{PassWord}/{Status}"] = p =>
            {
                var responseModel = new ResponseModel();
                var bo = UserManageViewModel.Insert(p.Name,p.PassWord,p.Status);
                if (bo)
                {
                    this.Users = UserManageViewModel.GetData();
                    responseModel.Success = true;
                }
                return Response.AsJson<ResponseModel>(responseModel);
            };
            Get["UserEdit/{Name}"] = p =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                if (!string.IsNullOrEmpty(p.Name))
                {
                    var model = this.Users.FirstOrDefault(t => t.Name == p.Name);
                    if (model != null)
                    {
                        return View["UserEdit", model];
                    }
                }
                ViewBag["Users"] = this.Users;
                return View["UserManage"];
            };
            Post["UserEdit"] = _ =>
            {
                UserManageViewModel.Update(Request.Form);
                this.Users = UserManageViewModel.GetData();
                ViewBag["permission"] = Session["permission"];
                return View["UserManage"];
            };
            Post["UserDelete/{Name}"] = p =>
            {
                var reponseMode = new ResponseModel();
                if (!string.IsNullOrEmpty(p.Name))
                {
                    var model = this.Users.FirstOrDefault(t => t.Name == p.Name);
                    if (model != null)
                    {
                        var bo = UserManageViewModel.Delete(model);
                        if (bo)
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
            Get["HospitalManange"] = _ =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
               
                return View["HospitalManange"];
            };
            Get["HospitalFileUpLoad"] = _ =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                ViewBag["permission"] = Session["permission"];
                return View["HospitalFileUpLoad"];
            };
            Post["api/HospitalFileUpLoad"] = _ =>
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
                        model = HospitalFileUpLoadViewModel.instance.BatchFile(Request.Files);
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