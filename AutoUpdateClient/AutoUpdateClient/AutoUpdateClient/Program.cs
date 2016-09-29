using AutoUpdateClient.Common;
using AutoUpdateClient.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutoUpdateClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WindowWidth = 150;
            var AutoUpdateTimer = new Timer(new TimerCallback(Modifier.CheckUpdate), null, 0, int.Parse(UserConfigInstance.instance.UserConfig.CheckUpdateTime));
            var ClientMonitorTimer = new Timer(new TimerCallback(Monitor.CheckClientAlive), null, 0, int.Parse(UserConfigInstance.instance.UserConfig.CheckClientAliveTime));
            Console.ReadKey();
        }
    }

}
