using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
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
        internal static DownLoadNewestResponseModel DownLoadNewest(dynamic hospitalID, dynamic VersionID)
        {
            var res = new DownLoadNewestResponseModel();
            var model = SQLiteHelper.HospitalQuery(hospitalID)?[0];
            if (model?.NewestVersion == VersionID)
            {
                res.Success = true;
                return res;
            }
            try
            {
                var newestVersion = (SQLiteHelper.HospitalQuery(hospitalID)?[0])?.NewestVersion;
                List<VersionModel> versionList = SQLiteHelper.VersionQuery(1000, hospitalID);
                var newestVersionID = (versionList.FirstOrDefault(p => p.HospitalID == hospitalID && p.Number == newestVersion))?.ID;
                var dllList = SQLiteHelper.DLLQuery(newestVersionID);
                if (versionList == null || string.IsNullOrEmpty(newestVersion) || string.IsNullOrEmpty(newestVersionID) || dllList == null)
                {
                    //医院给删除或者版本信息已经被人为删除情况下
                    res.Success = false;
                    return res;
                }
                //Nancy的下载文件只能放在Content静态文件夹里面才可以访问
                var tempName = string.Format("{0}_{1}", hospitalID, DateTime.Now.ToString("yyyyMMddhhmmssffff"));
                var zipDirectoryPath = Path.Combine(downLoadDiretory, tempName);
                if (!Directory.Exists(zipDirectoryPath))
                {
                    Directory.CreateDirectory(zipDirectoryPath);
                }
                var dllPathList = new ArrayList();
                dllList.ForEach((p) =>
                {
                    //做压缩包之前，先把文件重命名
                    var sourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.CLIENTFILESDIRETORYNAME, p.Name);
                    var info = new FileInfo(sourcePath);
                    var newName = string.Format("{0}{1}", p.Name.Split('-')[0],info.Extension);
                    var destPah = Path.Combine(zipDirectoryPath, newName);
                    info.CopyTo(destPah);
                    dllPathList.Add(destPah);
                });
                if (ZipHelper.ZipFileMore(dllPathList, zipDirectoryPath))
                {
                    res.Success = false;
                    res.Data.Version = newestVersion;
                    res.Data.FilePath = Path.Combine(ConstFile.CONTENTFILEDIRECTORY, "DownLoad", tempName);
                    Directory.Delete(zipDirectoryPath, true);
                }
                else
                {
                    res.Success = false;
                }
                return res;

            }
            catch (Exception)
            {
                res.Success = false;
                return res;
            }
        }

        internal static ResponseModel DeletePackage(dynamic packagename)
        {
            var res = new ResponseModel();
            try
            {
                var path = Path.Combine(downLoadDiretory, packagename);
                File.Delete(path);
                res.Success = true;
            }
            catch (Exception ex)
            {
                res.Success = false;
                res.Msg = ex.Message;
            }
            return res;
        }
    }
}