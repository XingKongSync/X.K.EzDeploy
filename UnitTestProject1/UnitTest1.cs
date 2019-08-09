using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Xked.ServiceHelper;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestInstallService()
        {
            Srvany.InstallService("XkedTestService", @"D:\Program Files\n2n\edge_v2.exe", "-r -a dhcp:0.0.0.0 -c xingkong -k xingkong -l 65.49.192.196:6900", "XkedTestServiceDisplayName", "This is a test Desc");
        }

        [TestMethod]
        public void TestRemoveService()
        {
            Srvany.RemoveService("XkedTestService");
        }
    }
}
