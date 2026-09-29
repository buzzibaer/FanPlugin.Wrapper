using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class FanPlaybackTests
    {
        [TestMethod]
        public void Fan_PlayVideoWithId_RejectsInvalidIdBeforeNetworkAccess()
        {
            var fan = new Fan { ServerIp = "", ServerPort = 0 };

            Assert.AreEqual("Invalid videoID", fan.playVideoWithId("abc"));
        }

        [TestMethod]
        public void FanV3_PlayVideoWithId_RejectsInvalidIdBeforeNetworkAccess()
        {
            var fan = new FanV3 { ServerIp = "", ServerPort = 0 };

            Assert.AreEqual("Invalid videoID", fan.playVideoWithId("abc"));
        }

        [TestMethod]
        public void Fan_PlayVideoWithId_ReturnsConnectionFailure()
        {
            var fan = new Fan { ServerIp = "127.0.0.1", ServerPort = ClosedLoopbackPort.Get() };

            string result = fan.playVideoWithId("6");

            StringAssert.StartsWith(result, "Network error:");
        }

        [TestMethod]
        public void FanV3_PlayVideoWithId_ReturnsConnectionFailure()
        {
            var fan = new FanV3 { ServerIp = "127.0.0.1", ServerPort = ClosedLoopbackPort.Get() };

            string result = fan.playVideoWithId("6");

            StringAssert.StartsWith(result, "Network error:");
        }
    }

    internal static class ClosedLoopbackPort
    {
        internal static int Get()
        {
            var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
            listener.Start();
            int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
