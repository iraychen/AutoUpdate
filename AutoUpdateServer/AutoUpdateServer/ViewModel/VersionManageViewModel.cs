using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class VersionManageViewModel
    {
        public static List<VersionModel> GetModels(string HospitalID)
        {
            return SQLiteHelper.VersionQuery(1000, int.Parse(HospitalID));
        }

        public static VersionModel GetModel(string ID)
        {
            var models = SQLiteHelper.VersionQuery(ID);
            return (models != null && models.Count != 0) ? models[0] : null;
        }

        public static bool Update(dynamic form)
        {
            return SQLiteHelper.Update<VersionModel>(new VersionModel
            {
                ID = form["ID"],
                HospitalID = int.Parse(form["HospitalID"]),
                Number = form["Number"],
                User = form["User"],
                UpLoadTime = form["UpLoadTime"],
                Description = form["Description"]
            });
        }

        internal static string Delete(string versionID, int hospitalID)
        {
            var errorMsg = string.Empty;
            try
            {
                var versionModels = SQLiteHelper.VersionQuery(1000, hospitalID);
                var currentVersionModel = versionModels?.FirstOrDefault(p => p.ID == versionID);
                if (currentVersionModel != null)
                {
                    //删除仓库中无用文件
                    var nextNumber = AddVersion(currentVersionModel.Number);
                    var nextVersionModel = versionModels?.FirstOrDefault(p => p.Number == nextNumber && p.HospitalID == hospitalID);
                    versionModels.Remove(currentVersionModel);
                    var currentAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(currentVersionModel.AllDLLVersion);
                    if (nextVersionModel != null)
                    {
                        //删除本次更新中文件,如果下一个版本有用到，就不删除.
                        var nextAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(nextVersionModel.AllDLLVersion);
                        foreach (var item in currentAllDLLVersionDictionary)
                        {
                            if (!nextAllDLLVersionDictionary.Contains(item))
                            {
                                var destFileFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(item.Key),item.Value, Path.GetExtension(item.Key));
                                var destFilePath = Path.Combine(ConstFile.WareHousePath, hospitalID.ToString(), Path.GetDirectoryName(item.Key), destFileFileName); ;
                                if (File.Exists(destFilePath))
                                {
                                    File.Delete(destFilePath);
                                    DeleteDirectory(destFilePath);
                                }
                            }
                        }
                    }
                    else
                    {
                        string newestNumber = string.Empty;
                        var lastVersionModel = versionModels.FirstOrDefault(p => p.ID == versionModels.Max(s => s.ID));
                        if (lastVersionModel != null)
                        {
                            var lastAllDLLVersionDictionary = new Dictionary<string, string>();
                            lastAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(lastVersionModel.AllDLLVersion);
                            //当前为最新版本，删除本次更新，回滚到上一个版本,修改当前医院的最新版字段
                            foreach (var item in currentAllDLLVersionDictionary)
                            {
                                if (!lastAllDLLVersionDictionary.Contains(item))
                                {
                                    var deleteDestWareHouseFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(item.Key), currentVersionModel.Number, Path.GetExtension(item.Key));
                                    var deleteDestWareHouseFilePath = Path.Combine(ConstFile.WareHousePath, hospitalID.ToString(), Path.GetDirectoryName(item.Key), deleteDestWareHouseFileName);
                                    if (File.Exists(deleteDestWareHouseFilePath))
                                    {
                                        File.Delete(deleteDestWareHouseFilePath);
                                        DeleteDirectory(deleteDestWareHouseFilePath);
                                    }
                                    var deleteDestWorkFilePath = Path.Combine(ConstFile.WorkPath, hospitalID.ToString(), item.Key);
                                    if (File.Exists(deleteDestWorkFilePath))
                                    {
                                        File.Delete(deleteDestWorkFilePath);
                                        DeleteDirectory(deleteDestWorkFilePath);
                                    }
                                    if (lastAllDLLVersionDictionary.Keys.Contains(item.Key))
                                    {
                                        var number = lastAllDLLVersionDictionary[item.Key];
                                        var rollBackDestFilePath = Path.Combine(ConstFile.WorkPath, hospitalID.ToString(), item.Key);
                                        var rollBackSourceFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(item.Key), number, Path.GetExtension(item.Key));
                                        var rollBackSourceFilePath = string.Empty;
                                        rollBackSourceFilePath = Path.Combine(ConstFile.WareHousePath, hospitalID.ToString(), Path.GetDirectoryName(item.Key), rollBackSourceFileName);
                                        var rollBackSourceFileInfo = new FileInfo(rollBackSourceFilePath);
                                        rollBackSourceFileInfo.CopyTo(rollBackDestFilePath, true);
                                    }
                                }
                            }
                            newestNumber = lastVersionModel.Number;
                        }
                        else
                        {
                            //全部删除
                            Directory.Delete(Path.Combine(ConstFile.WorkPath, hospitalID.ToString()),true);
                            Directory.Delete(Path.Combine(ConstFile.WareHousePath, hospitalID.ToString()),true);
                        }
                        var hospitalModel = SQLiteHelper.HospitalQuery(currentVersionModel.HospitalID)?[0];
                        hospitalModel.NewestVersion = newestNumber;
                        SQLiteHelper.Update<HospitalModel>(hospitalModel);
                    }
                    //删除数据库表单数据
                    SQLiteHelper.Delete<VersionModel>(currentVersionModel);
                }
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
            }
            return errorMsg;
        }

        private static void DeleteDirectory(string filePath)
        {
            var destDirectoryInfo = new DirectoryInfo(Path.GetDirectoryName(filePath));
            if (destDirectoryInfo.GetFiles().Count() <= 0 && destDirectoryInfo.GetDirectories().Count() <= 0)
            {
                Directory.Delete(Path.GetDirectoryName(filePath));
            }
        }

        internal static bool Set(dynamic versionNumber, dynamic hospitalID)
        {
            var model = SQLiteHelper.HospitalQuery(hospitalID)?[0];
            if (model != null)
            {
                model.NewestVersion = versionNumber;
                return SQLiteHelper.Update<HospitalModel>(model);
            }
            return false;
        }

        private static string AddVersion(string version)
        {
            var arr = version.Split('.');
            var second = int.Parse(arr[1]);
            var third = int.Parse(arr[2]);
            return third < 9 ? string.Format("2.{0}.{1}", second, ++third) : string.Format("2.{0}.0", ++second);
        }
    }
}