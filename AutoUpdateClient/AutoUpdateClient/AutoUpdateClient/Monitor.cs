using AutoUpdateClient.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;

namespace AutoUpdateClient
{
    public class Monitor
    {
        private static bool _isFree;
        private static string _terminalDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"../");
        private static string _terminalEXEPath = Path.Combine(_terminalDirectory, "Terminal.exe");
        internal static void CheckClientAlive(object state)
        {
            LogHelper.instance.Logger.Info("");
            if (Process.GetProcessesByName("Terminal").ToList().Count > 0|| Process.GetProcessesByName("Terminal.vshost").ToList().Count > 0)
            {
                LogHelper.instance.Logger.Info("【保护线程】 客户端正在运行");
                CheckClientFree();
            }
            else
            {
                if (StartClient())
                {
                    LogHelper.instance.Logger.Info("【保护线程】启动客户端成功");
                    CheckClientFree();
                }
            }
        }

        private static bool StartClient()
        {
            if (!File.Exists(_terminalEXEPath))
            {
                LogHelper.instance.Logger.Info("【保护线程】启动客户端失败,找不到客户端exe文件");
                return false;
            }
            Process p = new Process();
            p.StartInfo.FileName = _terminalEXEPath;
            p.StartInfo.WorkingDirectory = _terminalDirectory;
            return p.Start();
        }

        internal static void CheckClientFree()
        {
            if (StateCenter.instance.HasRepalced)
            {
                LogHelper.instance.Logger.Info(string.Format("【保护线程-当前客户端已经是最新版本】", DateTime.Now));
                return;
            }
            LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-开始】时间：{0}，", DateTime.Now));
            using (var pipeClient = new NamedPipeClientStream(".", "AutoUpdatePipe", PipeDirection.InOut,
               PipeOptions.None, TokenImpersonationLevel.Impersonation))
            {
                try
                {
                    pipeClient.Connect(500);
                    Thread.Sleep(500);
                }
                catch (TimeoutException)
                {
                    LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-超时】时间：{0},", DateTime.Now));
                    return;
                }
                if (pipeClient.IsConnected)
                {
                    var ss = new StreamString(pipeClient);
                    if (ss.ReadString() == "I am the one true server!")
                    {
                        ss.WriteString("Are you Free?");
                        bool.TryParse(ss.ReadString(), out _isFree);
                        LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-结束】时间：{0}， 空闲状态：{1}", DateTime.Now, _isFree));
                        if (_isFree)
                        {
                            //关闭客户端，文件替换，重启客户端
                            LogHelper.instance.Logger.Info(string.Format("【保护线程更新-开始】时间：{0} ", DateTime.Now));
                            Process[] myproc = Process.GetProcesses();
                            foreach (Process item in myproc)
                            {
                                if (item.ProcessName == "Terminal")
                                {
                                    item.Kill();
                                }
                            }
                            var packagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserData", "Package.7z");
                            if (ZipHelper.UnZipPath(packagePath, _terminalDirectory))
                            {
                                File.Delete(packagePath);
                                LogHelper.instance.Logger.Info(string.Format("【保护线程-更新成功{0}】", DateTime.Now));
                                var deleteFileConfigPath = Path.Combine(_terminalDirectory, "DeleteFileConfig.xml");
                                if (File.Exists(deleteFileConfigPath))
                                {
                                    var deleteFileList = FileUtil.XMLLoadData<List<string>>(deleteFileConfigPath);
                                    deleteFileList.ForEach((p) =>
                                    {
                                        var deleteFilePath = Path.Combine(_terminalDirectory, p);
                                        if (File.Exists(deleteFilePath))
                                        {
                                            File.Delete(deleteFilePath);
                                        }
                                        if (Directory.Exists(Path.GetDirectoryName(deleteFilePath)))
                                        {
                                            var deleteFileDrectoryInfo = new DirectoryInfo(Path.GetDirectoryName(deleteFilePath));
                                            if (deleteFileDrectoryInfo.GetFiles().Length == 0)
                                            {
                                                Directory.Delete(deleteFileDrectoryInfo.FullName);
                                            }
                                        }
                                    });
                                }
                                var batFilePath= Path.Combine(_terminalDirectory, "config.bat");
                                if (File.Exists(batFilePath))
                                {
                                    var proc = new Process();
                                    proc.StartInfo.FileName = batFilePath;
                                    proc.StartInfo.CreateNoWindow = false;
                                    proc.Start();
                                    proc.WaitForExit();
                                }
                                StateCenter.instance.HasRepalced = true;
                                StartClient();
                            }
                            else
                            {
                                LogHelper.instance.Logger.Info(string.Format("【保护线程-解压失败,退出】时间：{0}", DateTime.Now));
                            }
                        }
                        else
                        {
                            LogHelper.instance.Logger.Info(string.Format("【保护线程-当前客户端忙碌】", DateTime.Now));
                        }
                    }
                }
                else
                {
                    LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-结束：超时】时间：{0}，命名管道连接异常", DateTime.Now));
                }
            }

        }
    }
}
