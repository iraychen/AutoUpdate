using AutoUpdateClient.Common;
using AutoUpdateClient.Model;
using System;
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
            var AutoUpdateTimer = new Timer(new TimerCallback(Modifier.CheckUpdate), null, 0, 20000);//一小时
            var ClientMonitorTimer = new Timer(new TimerCallback(Monitor.checkClientAlive), null, 0, 10000);
            Console.ReadKey();
        }
    }

}
