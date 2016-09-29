using AutoUpdateClient.Common;
using AutoUpdateClient.Model;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.ComponentModel;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Collections.Generic;

namespace AutoUpdateClient
{
    public class Modifier
    {
        private static string _newestVersion = string.Empty;
        private static string _packagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "Package.7z");
        public static async void CheckUpdate(object state)
        {
            try
            {
                if (!StateCenter.instance.HasRepalced)
                {
                    LogHelper.instance.Logger.Info(string.Format("【修改线程-等待客户端空闲替换下载好的文件】"));
                    return;
                }
                LogHelper.instance.Logger.Info(string.Format("【修改线程检查-开始】时间：{0}", DateTime.Now));
                using (HttpClient client = new HttpClient())
                {
                    var url = string.Format("{0}/api/RequestNewestPackageUrl/{1}/{2}", UserConfigInstance.instance.UserConfig.ServerUrl, UserConfigInstance.instance.UserConfig.HospitalID, UserConfigInstance.instance.UserConfig.Version);
                    var httpResponse = await client.GetAsync(url);
                    if (httpResponse.StatusCode != HttpStatusCode.OK)
                    {
                        LogHelper.instance.Logger.Info(string.Format("【修改线程检查-结束】时间：{0}，状态码：{1}", DateTime.Now, httpResponse.StatusCode));
                        return;
                    }
                    var res = JsonConvert.DeserializeObject<ResponseModel>(httpResponse.Content.ReadAsStringAsync().Result);
                    if (res.Success)
                    {
                        _newestVersion = res.Data.Version;
                        DownLoad(res.Data.FilePath);
                    }
                    else
                    {
                        LogHelper.instance.Logger.Info(string.Format("【修改线程检查-结束服务端返回】时间：{0},原因：{1}", DateTime.Now,res.Msg));
                        return;
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.instance.Logger.Info(string.Format("【修改线程检查-结束客户端出错】时间：{0}，原因：{1}", DateTime.Now, ex.Message));
                return;
            }
        }


        private static void DownLoad(string filePath)
        {
            using (var webClient = new WebClient())
            {
                try
                {
                    var url = new Uri(string.Format("{0}/{1}", UserConfigInstance.instance.UserConfig.ServerUrl, filePath));
                    if (File.Exists(_packagePath))
                    {
                        File.Delete(_packagePath);
                    }
                    var token = "AutoUpdate";
                    webClient.Proxy.Credentials = CredentialCache.DefaultCredentials;
                    webClient.DownloadFileCompleted += DownloadFileCompleted;
                    webClient.Encoding = Encoding.UTF8;
                    LogHelper.instance.Logger.Info(string.Format("【修改线程下载-开始】时间：{0}", DateTime.Now));
                    webClient.DownloadFileAsync(url, _packagePath, token);
                }
                catch (WebException e)
                {
                    LogHelper.instance.Logger.Info(string.Format("【修改线程下载-失败】原因：网络异常{0}", e));
                    return;
                }
                catch (Exception ex)
                {
                    LogHelper.instance.Logger.Info(string.Format("【修改线程下载-失败】原因：{0}", ex));
                    return;
                }
            }
        }

        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.UserState.ToString() == "AutoUpdate")
            {
                var terminalDirectory= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../");
                UserConfigInstance.instance.UserConfig.Version = _newestVersion;
                FileUtil.XMLSave<UserConfig>(UserConfigInstance.instance.UserConfig, UserConfigInstance.instance.ConfigPath);
                StateCenter.instance.HasRepalced = false;
                LogHelper.instance.Logger.Info(string.Format("【修改线程下载-文件成功】时间：{0}", DateTime.Now));

                #region TestCode
                //if (ZipHelper.UnZipPath(packagePath, terminalDirectory))
                //{
                //    File.Delete(packagePath);
                //    LogHelper.instance.Logger.Info(string.Format("【修改线程下载-替换成功{0}】", DateTime.Now));
                //    var deleteFileConfigPath = Path.Combine(terminalDirectory, "DeleteFileConfig.xml");
                //    if (File.Exists(deleteFileConfigPath))
                //    {
                //        var deleteFileList = FileUtil.XMLLoadData<List<string>>(deleteFileConfigPath);
                //        deleteFileList.ForEach((p) =>
                //        {
                //            var deleteFilePath = Path.Combine(terminalDirectory, p);
                //            if (File.Exists(deleteFilePath))
                //            {
                //                File.Delete(deleteFilePath);
                //            }
                //            var deleteFileDrectoryInfo = new DirectoryInfo(Path.GetDirectoryName(deleteFilePath));
                //            if (deleteFileDrectoryInfo.GetFiles().Length == 0)
                //            {
                //                Directory.Delete(deleteFileDrectoryInfo.FullName);
                //            }
                //        });
                //    }
                //    StateCenter.instance.HasRepalced = true;
                //    //StartClient();
                //}
                //else
                //{
                //    LogHelper.instance.Logger.Info(string.Format("【保护线程-解压失败,退出】时间：{0}", DateTime.Now));
                //}
                #endregion
            }
        }
    }
}
