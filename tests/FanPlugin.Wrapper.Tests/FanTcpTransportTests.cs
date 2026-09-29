using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class FanTcpTransportTests
    {
        [TestMethod]
        public void Fan_GetFileList_ReturnsReadTimeoutFromLoopbackServer()
        {
            AssertReadTimeout((ip, port, connectTimeout, socketTimeout) => new Fan
            {
                ServerIp = ip,
                ServerPort = port,
                ConnectTimeoutMs = connectTimeout,
                SocketTimeoutMs = socketTimeout
            }, fan => fan.getFileListFromFan());
        }

        [TestMethod]
        public void FanV3_GetFileList_ReturnsReadTimeoutFromLoopbackServer()
        {
            AssertReadTimeout((ip, port, connectTimeout, socketTimeout) => new FanV3
            {
                ServerIp = ip,
                ServerPort = port,
                ConnectTimeoutMs = connectTimeout,
                SocketTimeoutMs = socketTimeout
            }, fan => fan.getFileListFromFan());
        }

        [TestMethod]
        public void BothHardwareVariantsDefaultToThreeSecondTimeouts()
        {
            AssertComConfiguration(typeof(Fan));
            AssertComConfiguration(typeof(FanV3));
        }

        [TestMethod]
        public void InvalidEndpointIsReturnedAsConfigurationError()
        {
            var fan = new Fan { ServerIp = "127.0.0.1", ServerPort = 70000 };

            string result = fan.getFileListFromFan();

            StringAssert.StartsWith(result, "Network error:");
        }

        private static void AssertComConfiguration(Type type)
        {
            Assert.IsTrue(Marshal.IsTypeVisibleFromCom(type));
            object fan = Activator.CreateInstance(type);
            Assert.AreEqual("192.168.4.1", type.GetProperty("ServerIp").GetValue(fan, null));
            Assert.AreEqual(5233, type.GetProperty("ServerPort").GetValue(fan, null));
            Assert.AreEqual(3000, type.GetProperty("ConnectTimeoutMs").GetValue(fan, null));
            Assert.AreEqual(3000, type.GetProperty("SocketTimeoutMs").GetValue(fan, null));

            foreach (string propertyName in new[] { "ServerIp", "ServerPort", "ConnectTimeoutMs", "SocketTimeoutMs" })
            {
                var property = type.GetProperty(propertyName);
                Assert.IsNotNull(property);
                Assert.IsTrue(property.CanRead && property.CanWrite);
                Assert.IsFalse(property.GetMethod.IsStatic);
                Assert.IsFalse(property.SetMethod.IsStatic);
            }
        }

        private static void AssertReadTimeout<TFan>(
            Func<string, int, int, int, TFan> createFan,
            Func<TFan, string> getFileList)
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;

            Task server = Task.Run(() =>
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] request = new byte[256];
                    stream.Read(request, 0, request.Length);
                    Thread.Sleep(1500);
                }
            });

            try
            {
                TFan fan = createFan("127.0.0.1", port, 1000, 200);
                var stopwatch = Stopwatch.StartNew();
                string result = getFileList(fan);
                stopwatch.Stop();

                StringAssert.StartsWith(result, "Network timeout:");
                Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
                    "The read should stop at its configured timeout rather than waiting for the server to close.");
            }
            finally
            {
                listener.Stop();
                server.Wait(2000);
            }
        }
    }
}
