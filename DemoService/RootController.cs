using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using XingKongHttpServer;

namespace DemoService
{
    class RootController : HttpRequestHandlerBase
    {
        public RootController()
        {
            Path = "/";
        }

        public override void Handler(HttpListenerRequest request, HttpListenerResponse response)
        {
            string html = "<html><head><title>Welcome</title></head><body>Welcome to EasyDeploy.</body></html>";
            Html(response, html);
        }
    }
}
