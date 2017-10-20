using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Server servidor = new Server(12303, "127.0.0.1");
            servidor.StartListening();
        }
    }
}
