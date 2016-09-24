using AutoUpdateServer.Common;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using Nancy;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AutoUpdateServer.ViewModel
{
    public class HospitalFileUpLoadViewModel
    {
        private static List<string> CreateFileOrDiretoryList = new List<string>();
        private static string currentFileName = string.Empty;
        private static string currentUser = string.Empty;
        public bool IsRuning = false;
        private HospitalFileUpLoadViewModel() { }

        public static readonly HospitalFileUpLoadViewModel instance = new HospitalFileUpLoadViewModel();
        public ResponseModel BatchFile(IEnumerable<HttpFile> files, string user)
        {
            var responseModel = new ResponseModel();
            var models = SQLiteHelper.VersionQuery("BaseModel");
            if (models == null || models.Count == 0)
            {
                responseModel.Success = false;
                responseModel.Msg = "服务端没有配置过模板文件，请联系管理员";
                this.IsRuning = false;
                return responseModel;
            }
            try
            {
                this.IsRuning = true;
                currentUser = user;
                if (files.Count() > 0)
                {
                    foreach (var file in files)
                    {
                        currentFileName = file.Name;
                        //1.检查文件中是否有命名不正确
                        int hospitalID;
                        HospitalModel hospitalModel;
                        if (!CheckFileName(responseModel, file, out hospitalID, out hospitalModel))
                        {
                            break;
                        }
                        //2.获取老版本文件目录和解压后的临时目录,压缩包地址存放地址
                        var oldFilesDirectoryPath = Path.Combine(ConstFile.WorkPath, hospitalID.ToString());
                        var tempDirectoryPath = Path.Combine(ConstFile.TempPath, DateTime.Now.ToString("yyyyMMddhhmmssffff"));
                        var zipPath = Path.Combine(tempDirectoryPath, file.Name);
                        //3.把文件存放到根目录下
                        SaveFile(file, zipPath);
                        //4.解压更新包到根目录。
                        if (ZipHelper.UnZipPath(zipPath, tempDirectoryPath))
                        {
                            CreateFileOrDiretoryList.Add(tempDirectoryPath);
                            //5.获取当前解压出来文件的文件目录地址
                            var newestFileDirectoryPath = Path.Combine(tempDirectoryPath, Path.GetFileNameWithoutExtension(zipPath));
                            //6.对比
                            CompareAction(oldFilesDirectoryPath, newestFileDirectoryPath, hospitalID, hospitalModel, zipPath);
                            continue;
                        }
                        responseModel.Success = false;
                        responseModel.Msg = file.Name + "处理失败";
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Success = false;
                responseModel.Msg = string.Format("{0}上传失败：服务端异常，{1}", currentFileName, ex.Message);
            }
            //删除临时文件和目录
            DeleteTempFileOrDirectory();
            this.IsRuning = false;
            return responseModel;
        }
        private static void CompareAction(string oldFilesDirectoryPath, string newestFileDirectoryPath, int hospitalID, HospitalModel hospitalModel, string zipPath)
        {
            //1.获取当前医院最新版本设置版本号
            //2.对比,管理文件
            //3.修改本地配置
            var lastVersionModel = new VersionModel();
            var number = ConstFile.BASEVERSION;
            var newestAllDLLVersionDictionary = new Dictionary<string, string>();
            var newestFileDirectoryInfo = new DirectoryInfo(newestFileDirectoryPath);
            GetFileNameDictionary(newestFileDirectoryInfo, newestAllDLLVersionDictionary, ConstFile.BASEVERSION, hospitalID.ToString());
            var newesterVsionModel = new VersionModel
            {
                ID = DateTime.Now.ToString("yyyyMMddhhmmssffff"),
                HospitalID = hospitalID,
                UpLoadTime = DateTime.Now,
                User = currentUser
            };
            var versionModels = SQLiteHelper.VersionQuery(1000, hospitalID);
            //对比逻辑：
            //1.第一次上传的时候或者当前没有出新文件的时候。和模板文件匹配.
            //2.第二次开始：
            //  A【修改】.上传文件和本地文件都存在，对比不同，则把最新的文件复制到work目录和仓库目录，并且设置上传文件version为最新version（存入数据库时）
            //  B【不变】.上传文件和本地文件都存在，对比相同，不复制文件，设置上传文件version为老版本文件的version（存入数据库时）
            //  C【新增】.上传文件存在，本地文件不存在，则把最新的文件复制到work目录和仓库目录，并且设置上传文件version为最新version（存入数据库时）
            //  D【删除】.上传文件不存在，本地文件存在。暂时不操作。
            var oldAllDLLVersionDictionary = new Dictionary<string, string>();
            var workPath = Path.Combine(ConstFile.WorkPath, hospitalID.ToString());

            if (versionModels.Count != 0)
            {
                lastVersionModel = versionModels.FirstOrDefault(p => p.ID == versionModels.Max(t => t.ID));
                number = AddVersion(lastVersionModel.Number);
                oldAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(lastVersionModel.AllDLLVersion);
            }
            else
            {
                var baseModel = SQLiteHelper.VersionQuery("BaseModel")[0];
                oldAllDLLVersionDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(baseModel.AllDLLVersion);
                number = AddVersion(baseModel.Number);
            }


            var tempdllVersionDictionary = new Dictionary<string, string>();
            foreach (var item in newestAllDLLVersionDictionary)
            {
                var newestFilePath = Path.Combine(newestFileDirectoryPath, item.Key);
                var newestFileInfo = new FileInfo(newestFilePath);
                if (oldAllDLLVersionDictionary.Keys.Contains(item.Key) && oldAllDLLVersionDictionary[item.Key] == ConstFile.BASEVERSION)
                {
                    oldFilesDirectoryPath = ConstFile.BaseFilePath;
                }
                var localFilePath = Path.Combine(oldFilesDirectoryPath, item.Key);
                if (File.Exists(localFilePath))
                {
                    var localFileInfo = new FileInfo(localFilePath);
                    if (!isTheSame(newestFileInfo, localFileInfo))
                    {
                        FileCopy(newestFileInfo, hospitalID.ToString(), number, workPath, item.Key);
                        tempdllVersionDictionary.Add(item.Key, number);
                    }
                    else
                    { 
                        tempdllVersionDictionary.Add(item.Key, oldAllDLLVersionDictionary[item.Key]);
                    }
                    oldAllDLLVersionDictionary.Remove(item.Key);
                }
                else
                {
                    FileCopy(newestFileInfo, hospitalID.ToString(), number, workPath, item.Key);
                    tempdllVersionDictionary.Add(item.Key, number);
                }
            }
            // 删除的文件
            var deleteKey = new List<string>();
            foreach (var item in oldAllDLLVersionDictionary)
            {
                if (item.Value != ConstFile.BASEVERSION)
                {
                    deleteKey.Add(item.Key);
                }
            }
            deleteKey.ForEach(p=>oldAllDLLVersionDictionary.Remove(p));

            foreach (var key in tempdllVersionDictionary.Keys)
            {
                if (oldAllDLLVersionDictionary.Keys.Contains(key))
                {
                    oldAllDLLVersionDictionary[key] = tempdllVersionDictionary[key];
                    continue;
                }
                oldAllDLLVersionDictionary.Add(key, tempdllVersionDictionary[key]);
            }


            hospitalModel.NewestVersion = number;
            newesterVsionModel.Number = number;
            newesterVsionModel.AllDLLVersion = JsonConvert.SerializeObject(oldAllDLLVersionDictionary);
            SQLiteHelper.Insert<VersionModel>(newesterVsionModel);
            SQLiteHelper.Update<HospitalModel>(hospitalModel);
        }
        internal ResponseModel BatchBaseModelFile(IEnumerable<HttpFile> files, string userName)
        {
            var responseModel = new ResponseModel();
            foreach (var file in files)
            {
                try
                {
                    var tempFileName = DateTime.Now.ToString("yyyyMMddhhmmssffff");
                    var zipPath = Path.Combine(ConstFile.TempPath, tempFileName);
                    SaveFile(file, zipPath);
                    if (Directory.Exists(ConstFile.BaseFilePath))
                    {
                        Directory.Delete(ConstFile.BaseFilePath, true);
                    }
                    ZipHelper.UnZipPath(zipPath, ConstFile.TempPath);
                    CreateFileOrDiretoryList.Add(zipPath);
                    var tempDirectoryPath = Path.Combine(ConstFile.TempPath, Path.GetFileNameWithoutExtension(file.Name));
                    CreateFileOrDiretoryList.Add(tempDirectoryPath);
                    var tempDirectoryInfo = new DirectoryInfo(tempDirectoryPath);
                    tempDirectoryInfo.MoveTo(ConstFile.BaseFilePath);
                    var newestAlldllVersionDictionary = new Dictionary<string, string>();
                    var newestFileDirectoryInfo = new DirectoryInfo(ConstFile.BaseFilePath);
                    GetFileNameDictionary(newestFileDirectoryInfo, newestAlldllVersionDictionary, ConstFile.BASEVERSION, "BaseModel");
                    var baseVersion = new VersionModel
                    {
                        ID = "BaseModel",
                        AllDLLVersion = JsonConvert.SerializeObject(newestAlldllVersionDictionary),
                        User = userName,
                        UpLoadTime = DateTime.Now,
                        HospitalID = -1,
                        Number = ConstFile.BASEVERSION,
                    };
                    var models = SQLiteHelper.VersionQuery(baseVersion.ID);
                    if ((models == null || models.Count == 0))
                    {
                        SQLiteHelper.Insert<VersionModel>(baseVersion);
                    }
                    else
                    {
                        SQLiteHelper.Update<VersionModel>(baseVersion);
                    }
                    responseModel.Success = true;
                }
                catch (Exception ex)
                {
                    CreateFileOrDiretoryList.Add(ConstFile.BaseFilePath);
                    responseModel.Success = false;
                    responseModel.Msg = string.Format("上传失败：原因{0}", ex.Message);
                    break;
                }

            }

            DeleteTempFileOrDirectory();
            return responseModel;

        }
        private static string AddVersion(string newestVersion)
        {
            //1.version 命名规则(2.0.0)2不变，递增
            var arr = newestVersion.Split('.');
            var second = int.Parse(arr[1]);
            var third = int.Parse(arr[2]);
            return third < 9 ? string.Format("2.{0}.{1}", second, ++third) : string.Format("2.{0}.0", ++second);
        }
        private static bool isTheSame(FileInfo f1, FileInfo f2)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash1, hash2;
                using (var stream = f1.OpenRead())
                    hash1 = md5.ComputeHash(stream);
                using (var stream = f2.OpenRead())
                    hash2 = md5.ComputeHash(stream);
                return hash1.SequenceEqual(hash2);
            }
        }
        private static void FileCopy(FileInfo newestFileInfo, string hospitalID, string version, string workPath, string path)
        {
            //复制成新的更新包文件(生成相对的文件目录，因为可能出现不同文件夹同名文件)，复制到WORK目录
            var newestPackageFileName = string.Format("{0}-{1}{2}", Path.GetFileNameWithoutExtension(newestFileInfo.Name), version, Path.GetExtension(newestFileInfo.Name));
            var newestPackageFileInfo = new FileInfo(Path.Combine(ConstFile.WareHousePath, hospitalID, Path.GetDirectoryName(path), newestPackageFileName));
            var newestFileDir = newestPackageFileInfo.Directory;
            if (!newestFileDir.Exists)
                newestFileDir.Create();
            newestFileInfo.CopyTo(newestPackageFileInfo.FullName, true);

            var workFilePath = Path.Combine(workPath, path);
            var workFileInfo = new FileInfo(workFilePath);
            var workFileDir = workFileInfo.Directory;
            if (!workFileDir.Exists)
                workFileDir.Create();
            newestFileInfo.CopyTo(workFileInfo.FullName, true);
        }
        private static void SaveFile(HttpFile file, string zipPath)
        {
            var fileInfo = new FileInfo(zipPath);
            if (!Directory.Exists(fileInfo.DirectoryName))
            {
                Directory.CreateDirectory(fileInfo.DirectoryName);
            }
            using (FileStream fileStream = new FileStream(zipPath, FileMode.Create))
            {
                file.Value.CopyTo(fileStream);
                CreateFileOrDiretoryList.Add(zipPath);
            }
        }
        private static void DeleteTempFileOrDirectory()
        {
            foreach (var path in CreateFileOrDiretoryList)
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    continue;
                }
                else if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }
        private static bool CheckFileName(ResponseModel responseModel, HttpFile file, out int hospitalID, out HospitalModel hospitalModel)
        {
            hospitalID = 0;
            hospitalModel = new HospitalModel();
            string pattern = @"^\d+$";
            var fileName = Path.GetFileNameWithoutExtension(file.Name);
            if (!Regex.IsMatch(fileName, pattern))
            {
                responseModel.Success = false;
                responseModel.Msg = "命名不正确,请看规则";
                return false;
            }
            else
            {
                var hospitalModels = SQLiteHelper.HospitalQuery(int.Parse(fileName));
                if (hospitalModels.Count == 0)
                {
                    responseModel.Success = false;
                    responseModel.Msg = string.Format("请先在网站创建医院ID为{0}的记录，看规则", fileName);
                    return false;
                }
                hospitalModel = hospitalModels[0];
                hospitalID = int.Parse(fileName);
            }
            return true;
        }
        private static void GetFileNameDictionary(DirectoryInfo info, Dictionary<string, string> fileNameDictionary, string version, string hospitalID)
        {
            var tag = string.Format(@"\{0}\", hospitalID);
            foreach (var item in info.GetFiles())
            {
                //因为不同的目录可能含有相同名称的文件。所以取路径作为KEY
                fileNameDictionary.Add(item.FullName.Substring(item.FullName.LastIndexOf(tag) + tag.Length), version);
            }
            foreach (var item in info.GetDirectories())
            {
                GetFileNameDictionary(item, fileNameDictionary, version, hospitalID);
            }
        }
    }
}