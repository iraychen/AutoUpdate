using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class HospitalManageViewModel
    {
        public static List<HospitalModel> GetData(string name = null)
        {
            return SQLiteHelper.HospitalQuery(100, name);
        }

        public static bool Insert(int id, string name)
        {
            return SQLiteHelper.Insert<HospitalModel>(new HospitalModel { ID = id, Name = name });
        }

        public static bool Update(dynamic form)
        {
            var hospitalID = int.Parse(form["HospitalID"]);
            var model = SQLiteHelper.HospitalQuery(hospitalID)?[0];
            if (model != null)
            {
                model.ID = hospitalID;
                model.Name = form["HospitalName"];
                return SQLiteHelper.Update<HospitalModel>(model);
            }
            return false;
        }

        public static bool Delete(HospitalModel model)
        {
            return SQLiteHelper.Delete<HospitalModel>(model);
        }

    }
}