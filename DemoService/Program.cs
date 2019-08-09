using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using XingKongHttpServer;

namespace DemoService
{
    class Program
    {
        static void Main(string[] args)
        {
            int port = 8123;
            HttpServer httpServer = new HttpServer(port);
            httpServer.AddPath(new RootController());

            httpServer.Start();

            Thread.CurrentThread.Suspend();
        }
    }
}
