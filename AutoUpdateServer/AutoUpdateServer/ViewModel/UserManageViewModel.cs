using AutoUpdateServer.Common;
using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class UserManageViewModel
    {
        private static SQLiteHelper SQLiteHelper = new SQLiteHelper(new DirectoryInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataBase")));
        public UserManageViewModel()
        {
        }

        public static List<UserModel> GetData(string name = null)
        {
            return SQLiteHelper.Query<UserModel>(100, name);
        }

        public static bool Insert(string name,string pwd,int status)
        {
            var passWord = MD5Helper.MD5Encode(pwd);
            var per = new Permission();
            switch (status)
            {
                case 1:
                    per = Permission.Coder;
                    break;
                case 2:
                    per = Permission.Admin;
                    break;
                default:
                    per = Permission.Default;
                    break;
            }
            var model = new UserModel
            {
                Name = name,
                PassWord = passWord,
                Permission = per
            };
            return SQLiteHelper.Insert<UserModel>(model);
        }

        public static bool Update(dynamic form)
        {
            var passWord = MD5Helper.MD5Encode(form["firstPassword"]);
            int status = int.Parse(form["status"]);
            var per = new Permission();
            switch (status)
            {
                case 1:
                    per = Permission.Coder;
                    break;
                case 2:
                    per = Permission.Admin;
                    break;
                default:
                    per = Permission.Default;
                    break;
            }
            var model = new UserModel
            {
                Name = form["name"],
                PassWord = passWord,
                Permission = per
            };
            return SQLiteHelper.Update<UserModel>(model);
        }

        public static bool Delete(UserModel model)
        {
            return SQLiteHelper.Delete<UserModel>(model);
        }
    }
}