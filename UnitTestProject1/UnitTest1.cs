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
            Srvany.InstallService("XkedTestService", @"D:\test.exe", "-s", "XkedTestServiceDisplayName", "This is a test Desc");
        }

        [TestMethod]
        public void TestRemoveService()
        {
            Srvany.RemoveService("XkedTestService");
        }
    }
}
