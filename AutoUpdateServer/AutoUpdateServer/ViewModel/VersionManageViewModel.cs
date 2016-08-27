using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class VersionManageViewModel
    {
        public static List<VersionModel> GetModels(string HospitalID)
        {
            return SQLiteHelper.VersionQuery(1000, int.Parse(HospitalID));
        }

        internal static void Update(dynamic form)
        {
            throw new NotImplementedException();
        }
    }
}