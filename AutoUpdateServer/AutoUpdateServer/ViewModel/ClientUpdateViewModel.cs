using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace AutoUpdateServer.ViewModel
{
    public class ClientUpdateViewModel
    {
        private static string downLoadDiretory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.CONTENTFILEDIRECTORY, "DownLoad");
        internal static RequestNewestPackageUrlResponseModel RequestNewestPackageUrl(dynamic hospitalID, dynamic oldNumber)
        {
            var res = new RequestNewestPackageUrlResponseModel();
            var model = SQLiteHelper.HospitalQuery(hospitalID)?[0];
            if (model?.NewestVersion == oldNumber)
            {
                res.Success = false;
                res.Msg = "已经是最新版本";
                res.Data = null;
                return res;
            }
            try
            {
                var newestVersion = (SQLiteHelper.HospitalQuery(hospitalID)?[0])?.NewestVersion;
                #region 判断最近一小时有没有打过包
                var packageName = string.Format("{0}_{1}", oldNumber, newestVersion);
                var packageDirectoryPath = Path.Combine(downLoadDiretory,hospitalID, packageName);
                string packagePath = string.Format("{0}.7z", packageDirectoryPath);
                if (File.Exists(packagePath))
                {
                    var fileInfo = new FileInfo(packagePath);
                    if (ExecDateDiff(DateTime.Now, fileInfo.LastWriteTime) < 3600000)
                    {
                        res.Success = true;
                        res.Msg = "最近一小时已经打过包,传回历史包";
                        res.Data.Version = newestVersion;
                        res.Data.FilePath = packagePath.Replace(AppDomain.CurrentDomain.BaseDirectory,"");
                        return res;
                    }
                    File.Delete(packagePath);
                }
                #endregion
                if (!Directory.Exists(packageDirectoryPath))
                {
                    Directory.CreateDirectory(packageDirectoryPath);
                }
                List<VersionModel> versionList = SQLiteHelper.VersionQuery(1000, hospitalID);
                var newestAllDLLVersionDic = JsonConvert.DeserializeObject<Dictionary<string, string>>(versionList.FirstOrDefault(p => p.Number == newestVersion)?.AllDLLVersion);
                var oldAllDLLVersionDic = (oldNumber == ConstFile.BASEVERSION)?
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(SQLiteHelper.VersionQuery("BaseModel")[0].AllDLLVersion): 
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(versionList.FirstOrDefault(p => p.Number == oldNumber)?.AllDLLVersion);
                foreach (var item in newestAllDLLVersionDic)
                {
                    if (!oldAllDLLVersionDic.Contains(item))
                    {
                        //如果版本是2.0.0就去模板文件中拿，其他区仓库拿，copy到包目录。
                        var number = newestAllDLLVersionDic[item.Key];
                        var rollBackSourceFilePath = string.Empty;
                        if (number == ConstFile.BASEVERSION)
                        {
                            rollBackSourceFilePath = Path.Combine(ConstFile.BaseFilePath, item.Key);
                        }
                        else
                        {
                            var rollBackSourceFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(item.Key), number, Path.GetExtension(item.Key));
                            rollBackSourceFilePath = Path.Combine(ConstFile.WareHousePath, hospitalID.ToString(), Path.GetDirectoryName(item.Key), rollBackSourceFileName);
                        }
                        var rollBackSourceFileInfo = new FileInfo(rollBackSourceFilePath);
                        var destFilePath = Path.Combine(packageDirectoryPath, item.Key);
                        if (!Directory.Exists(Path.GetDirectoryName(destFilePath)))
                        {
                            Directory.CreateDirectory(Path.GetDirectoryName(destFilePath));
                        }
                        rollBackSourceFileInfo.CopyTo(destFilePath, true);

                    }
                    if (oldAllDLLVersionDic.Keys.Contains(item.Key))
                    {
                        oldAllDLLVersionDic.Remove(item.Key);
                    }
                }
                //需要删除的部分存到xml传送到客户端，让客户端去删除
                if (oldAllDLLVersionDic.Count > 0)
                {
                    var keyList = new List<string>();
                    var deleteFileConfigPath = Path.Combine(packageDirectoryPath, "DeleteFileConfig.xml");
                    foreach (var key in oldAllDLLVersionDic.Keys)
                    {
                        keyList.Add(key);
                    }
                    FileUtil.XMLSaveData<List<string>>(keyList, deleteFileConfigPath);
                }
                //Nancy的下载文件只能放在Content静态文件夹里面才可以访问(不包含目录的压缩包，方便客户端直接解压替换)
                var zipDllPathList = new ArrayList();
                var packageDirectoryInfo = new DirectoryInfo(packageDirectoryPath);
                foreach (var item in packageDirectoryInfo.GetDirectories())
                {
                    zipDllPathList.Add(item.FullName);
                }
                foreach (var item in packageDirectoryInfo.GetFiles())
                {
                    zipDllPathList.Add(item.FullName);
                }
                if (ZipHelper.ZipFileMore(zipDllPathList, packagePath))
                {
                    res.Success = true;
                    res.Data.Version = newestVersion;
                    res.Data.FilePath = packagePath.Replace(AppDomain.CurrentDomain.BaseDirectory, "");
                    Directory.Delete(packageDirectoryPath, true);
                }
                else
                {
                    res.Success = false;
                    res.Msg = "服务端出错压缩失败";
                }
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Msg ="服务端出错："+ex.Message;
            }
            return res;
        }


        public static double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            return ts3.TotalMilliseconds;
        }
    }
}