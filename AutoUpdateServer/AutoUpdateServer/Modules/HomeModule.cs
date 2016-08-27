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
        private  List<HospitalModel> Hospitals
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

        public HomeModule()
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
                ViewBag["permission"] = Session["permission"];
                return View["UserManage", this.Users];
            };
            Post["QueryUser"] = p =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                ViewBag["permission"] = Session["permission"];
                return View["HospitalManage", this.Hospitals];
            };
            Post["QueryHospital"] = p =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                string name = Request.Form["name"];
                this.Hospitals = HospitalManageViewModel.GetData(name);
                ViewBag["permission"] = Session["permission"];
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                var model = this.Hospitals?.FirstOrDefault(t => t.ID == p.HospitalID);
                if (model != null)
                {
                    return View["HospitalEdit", model];
                }
                ViewBag["permission"] = Session["permission"];
                return View["HospitalManage", this.Hospitals];
            };
            Post["HospitalEdit/"] = _ =>
            {
                HospitalManageViewModel.Update(Request.Form);
                this.Hospitals = HospitalManageViewModel.GetData();
                ViewBag["permission"] = Session["permission"];
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                var models =VersionManageViewModel.GetModels(p.HospitalID);
                Session["versionModels"] = models;
                return View["VersionManage", models];
            };
            Get["VersionEdit/{VersionID}"] = p =>
            {
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                var models = (List<VersionModel>)Session["versionModels"];
                var model =models==null?null: models.FirstOrDefault(s => s.ID==p.VersionID);
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
                ViewBag["permission"] = Session["permission"];
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
                if (Request.Session["userName"] == null)
                {
                    return View["Login"];
                }
                ViewBag["permission"] = Session["permission"];
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