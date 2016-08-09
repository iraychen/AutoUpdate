using AutoUpdateServer.Common;
using AutoUpdateServer.Config.Model;
using AutoUpdateServer.Enum;
using AutoUpdateServer.Model;
using AutoUpdateServer.Reponse.Model;
using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;

namespace AutoUpdateServer.ViewModel
{
    public class HospitalFileUpLoadViewModel
    {
        private static string uploadRootDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConstFile.CLIENTFILESDIRETORYNAME);
        private static IEnumerable<HttpFile> httpFiles;
        private static List<string> CreateFileOrDiretoryPathList = new List<string>();
        private static string currentFileName = string.Empty;
        public bool IsRuning = false;
        private HospitalFileUpLoadViewModel() { }

        public static readonly HospitalFileUpLoadViewModel instance = new HospitalFileUpLoadViewModel();
        public ResponseModel BatchFile(IEnumerable<HttpFile> files, string description)
        {
            ResponseModel responseModel = new ResponseModel(); ;
            try
            {
                this.IsRuning = true;
                httpFiles = files;
                if (httpFiles.Count() > 0)
                {
                    //1.快速检查文件中是否有命名不正确
                    var right = CheckFileName(responseModel);
                    if (right)
                    {
                        foreach (var file in httpFiles)
                        {
                            currentFileName = file.Name;
                            //2.拿到更新包名称里面包含的HospitalID
                            var hospitalID = int.Parse(file.Name.Substring(0, currentFileName.LastIndexOf(".")));
                            //3.生成上传的包的地址
                            var fileFullName = Path.Combine(uploadRootDirectory, file.Name);
                            //4.把文件存放到根目录下
                            SaveFile(file, fileFullName);
                            //5.解压更新包到根目录。代码注意点：一定要放到外面，等fileStream释放，不然进程被占用
                            if (ZipHelper.UnZipPath(fileFullName, uploadRootDirectory))
                            {
                                //6.获取当前解压出来文件的文件目录地址
                                var upLoadFileDirectoryPath = Path.Combine(uploadRootDirectory, Path.GetFileNameWithoutExtension(fileFullName));
                                CreateFileOrDiretoryPathList.Add(upLoadFileDirectoryPath);
                                //7.检查CompareInfoConfig{}配置文件信息.没有问题则拿到compareFileNames
                                List<string> compareFileNames;
                                if (!CheckCompareInfoConfig(responseModel, hospitalID, out compareFileNames))
                                {
                                    break;
                                }
                                //8.根据数据库信息判断是否第一次上传
                                var versionDataBase = SQLiteHelper.VersionQuery(1, hospitalID);
                                if (versionDataBase != null && versionDataBase.Count > 0)
                                {
                                    //}
                                    //var versionConfigsListPath = Path.Combine(uploadRootDirectory, ConstFile.VERSIONCONFIGDIRETORYNAME, string.Format(ConstFile.VERSIONCONFIGFILENAME, HospitalID));
                                    //if (!File.Exists(versionConfigsListPath))
                                    //{
                                    //第一次上传
                                    FirstCompareAction(upLoadFileDirectoryPath, compareFileNames, hospitalID, description);
                                }
                                else
                                {
                                    CompareAction(upLoadFileDirectoryPath, compareFileNames, hospitalID, description);
                                }
                                //9.添加信息到数据库表。HospitalVersion

                                continue;
                            }
                            responseModel.Success = false;
                            responseModel.Msg = file.Name + "处理失败";
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                responseModel.Success = false;
                responseModel.Msg = string.Format("{0}上传失败：服务端异常，{1}", currentFileName, ex.Message);
            }
            //删除临时文件和目录
            DeleteFileOrDirectory();
            this.IsRuning = false;
            return responseModel;
        }
        private static bool CheckCompareInfoConfig(ResponseModel responseModel, int hospitalID, out List<string> compareFileNames)
        {
            var compareInfoConfigPath = Path.Combine(uploadRootDirectory, ConstFile.COMPAREINFOCONFIGDIRETORYNAME, string.Format(ConstFile.COMPAREINFOCONFIGFILENAME, hospitalID));
            compareFileNames = new List<string>();
            var model = FileUtil.XMLLoadData<CompareInfoConfig>(compareInfoConfigPath);
            if (model != null && model.Names.Count > 0)
            {
                compareFileNames = model.Names;
                return true;
            }
            responseModel.Success = false;
            responseModel.Msg = string.Format("未配置：{0}", string.Format(ConstFile.COMPAREINFOCONFIGFILENAME, hospitalID));
            return false;
        }
        private static void FirstCompareAction(string upLoadFileDirectoryPath, List<string> compareFileNames, int hospitalID, string description)
        {
            var versionModel = new VersionModel();
            versionModel.Number = ConstFile.BASEVERSION;
            versionModel.HospitalID = hospitalID;
            versionModel.UpLoadTime = DateTime.Now;
            versionModel.Description = description;
            versionModel.ID = DateTime.Now.ToString("yyyyMMddhhmmssffff");
            var dllInfos = new List<DLLModel>();
            compareFileNames.ForEach(p =>
            {
                var uploadFilePath = Path.Combine(upLoadFileDirectoryPath, p);
                if (File.Exists(uploadFilePath))
                {
                    var uploadFileInfo = new FileInfo(uploadFilePath);
                    var newFileInfo = FileCopy(p, uploadFileInfo, ConstFile.BASEVERSION);
                    dllInfos.Add(new DLLModel
                    {
                        Name = newFileInfo.Name,
                        Size = newFileInfo.Length,
                        UpdateStatus = UpdateStatus.ADD,
                        VersionID = versionModel.ID
                    });
                }
            });
            SQLiteHelper.Insert<VersionModel>(versionModel);
            SQLiteHelper.Insert<DLLModel>(dllInfos);

        }
        private static void CompareAction(string upLoadFileDirectoryPath, List<string> compareFileNames, int hospitalID, string description)
        {
            //1.找到对应hospitalid的最新版本2.对比3.修改本地配置
            var dataBase = SQLiteHelper.VersionQuery(1000, hospitalID);
            var newestVersionModel = dataBase.FirstOrDefault(p => p.ID == dataBase.Max(t => t.ID));
            var newestDLLModel = SQLiteHelper.DLLQuery(1000, newestVersionModel.ID);
            var number = CreateVersion(newestVersionModel.Number);
            var versionModel = new VersionModel();
            versionModel.ID = DateTime.Now.ToString("yyyyMMddhhmmssffff");
            versionModel.Number = number;
            versionModel.HospitalID = hospitalID;
            versionModel.UpLoadTime = DateTime.Now;
            versionModel.Description = description;
            var dllInfos = new List<DLLModel>();
            compareFileNames.ForEach(p =>
            {
                var uploadFilePath = Path.Combine(upLoadFileDirectoryPath, p);
                var localFilePath = Path.Combine(uploadRootDirectory, GetDLLName(newestDLLModel, p));
                var uploadFileInfo = new FileInfo(uploadFilePath);
                var localFileInfo = new FileInfo(localFilePath);
                //对比规则：
                //1.根据配置去找上传文件中对应的文件，如果不存在。则表示当前DLL删除（即用户修改了compareConfig 删除了一项）
                //2.如果对应的文件存在，但是本地不存在。则表示当前DLL是新增（即用户修改了compareConfig 新增了一项）
                //3.如果对应的文件存在，本地也存在，对比hash值。如果不同，则表示当前DLL是修改；反之，则为保持KEEP
                if (File.Exists(uploadFilePath))
                {
                    if (!File.Exists(localFilePath))
                    {
                        var newFileInfo = FileCopy(p, uploadFileInfo, number);
                        dllInfos.Add(new DLLModel
                        {
                            Name = newFileInfo.Name,
                            Size = newFileInfo.Length,
                            UpdateStatus = UpdateStatus.ADD,
                            VersionID = versionModel.ID
                        });
                    }
                    else
                    {
                        if (!isTheSame(new FileInfo(uploadFilePath), new FileInfo(localFilePath)))
                        {
                            var newFileInfo = FileCopy(p, uploadFileInfo, number);
                            dllInfos.Add(new DLLModel
                            {
                                Name = newFileInfo.Name,
                                Size = newFileInfo.Length,
                                UpdateStatus = UpdateStatus.UPDATE,
                                VersionID = versionModel.ID
                            });
                        }
                        else
                        {
                            dllInfos.Add(new DLLModel
                            {
                                Name = localFileInfo.Name,
                                Size = localFileInfo.Length,
                                UpdateStatus = UpdateStatus.KEEP,
                                VersionID = versionModel.ID
                            });
                        }
                    }
                }
                else
                {
                    dllInfos.Add(new DLLModel
                    {
                        Name = localFileInfo.Name,
                        Size = localFileInfo.Length,
                        UpdateStatus = UpdateStatus.DELETE,
                        VersionID = versionModel.ID
                    });
                }
            });
            SQLiteHelper.Insert<VersionModel>(versionModel);
            SQLiteHelper.Insert<DLLModel>(dllInfos);

        }
        private static string CreateVersion(string newestVersion)
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

        private static FileInfo FileCopy(string compareFileName, FileInfo uploadFileInfo, string version)
        {
            var name = version + "-" + compareFileName;
            var index = compareFileName.LastIndexOf('.');
            if (index > 0)
            {
                name = compareFileName.Substring(0, index) + "-" + version + compareFileName.Substring(index);
            }
            var newFileInfo = new FileInfo(Path.Combine(uploadRootDirectory, name));
            var dir = newFileInfo.Directory;
            if (!dir.Exists)
                dir.Create();
            uploadFileInfo.CopyTo(newFileInfo.FullName);
            return newFileInfo;
        }
        private static void SaveFile(HttpFile file, string fileFullName)
        {
            if (!Directory.Exists(uploadRootDirectory))
            {
                Directory.CreateDirectory(uploadRootDirectory);
            }
            using (FileStream fileStream = new FileStream(fileFullName, FileMode.Create))
            {
                file.Value.CopyTo(fileStream);
                CreateFileOrDiretoryPathList.Add(fileFullName);
            }
        }
        private static void DeleteFileOrDirectory()
        {
            foreach (var path in CreateFileOrDiretoryPathList)
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
        private static bool CheckFileName(ResponseModel responseModel)
        {
            string pattern = @"^\d+$";
            foreach (var file in httpFiles)
            {
                var index = file.Name.LastIndexOf(".");
                if (!Regex.IsMatch(file.Name.Substring(0, index), pattern))
                {
                    responseModel.Success = false;
                    responseModel.Msg = "命名不正确,请看规则";
                    return false;
                }
            }
            return true;

        }
        private static string GetDLLName(List<DLLModel> newestDLLModel, string name)
        {
            string dLLName = string.Empty;
            foreach (var p in newestDLLModel)
            {
                if (p.Name.Substring(0, p.Name.LastIndexOf(".")).Contains(name.Substring(0, name.LastIndexOf("."))) &&
                p.Name.Substring(p.Name.LastIndexOf(".")) == name.Substring(name.LastIndexOf(".")))
                {
                    dLLName = p.Name;
                    break;
                }
            }
            return dLLName;
        }
    }
}