using AutoUpdateClient.Common;
using System;
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
        private static bool isFree;
        private static string terminalDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"../");
        private static string terminalEXEPath = Path.Combine(terminalDirectory, "Terminal.exe");
        internal static void checkClientAlive(object state)
        {
            LogHelper.instance.Logger.Info("【保护线程-启动】");
            if (Process.GetProcessesByName("Terminal").ToList().Count > 0|| Process.GetProcessesByName("Terminal.vshost").ToList().Count > 0)
            {
                LogHelper.instance.Logger.Info("【保护线程-退出】 客户端正在运行");
                CheckClientFree();
            }
            else
            {
                LogHelper.instance.Logger.Info("【保护线程-启动客户端】");
                if (!StartClient())
                {
                    LogHelper.instance.Logger.Info("【保护线程】启动客户端失败");
                }
                else
                {
                    LogHelper.instance.Logger.Info("【保护线程】启动客户端成功");
                    CheckClientFree();
                }
            }
        }

        private static bool StartClient()
        {
            Process p = new Process();
            p.StartInfo.FileName = terminalEXEPath;
            p.StartInfo.WorkingDirectory = terminalDirectory;
            return p.Start();
        }

        internal static void CheckClientFree()
        {
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
                    LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-结束】时间：{0}，", DateTime.Now));
                    return;
                }
                if (pipeClient.IsConnected)
                {
                    var ss = new StreamString(pipeClient);
                    if (ss.ReadString() == "I am the one true server!")
                    {
                        ss.WriteString("Are you Free?");
                        bool.TryParse(ss.ReadString(), out isFree);
                        LogHelper.instance.Logger.Info(string.Format("【保护线程检查空闲-结束】时间：{0}， 空闲状态：{1}", DateTime.Now, isFree));
                        if (isFree && !StateCenter.instance.HasRepalced)
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
                            if (ZipHelper.UnZipPath(packagePath, terminalDirectory))
                            {
                                File.Delete(packagePath);
                                StateCenter.instance.HasRepalced = true;
                                StartClient();
                                LogHelper.instance.Logger.Info(string.Format("【保护线程-更新成功{0}】", DateTime.Now));
                            }
                        }
                        else
                        {
                            LogHelper.instance.Logger.Info(string.Format("【保护线程-当前无需更新{0}】", DateTime.Now));
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
