using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class Fan20320Tests
    {
        [TestMethod]
        public void PlayVideoWithId_MapsIdToReturnedFileListIndex()
        {
            using (var server = new Fan20320LoopbackServer(new[] { "000001.bin", "000005.bin" }))
            {
                var fan = CreateFan(server.Port);

                Assert.AreEqual("Command successfull", fan.playVideoWithId("5"));
                server.WaitForCompletion();
                CollectionAssert.AreEqual(new byte[] { (byte)'B', 1 }, server.SelectionCommand);
            }
        }

        [TestMethod]
        public void PlayVideoWithId_ReturnsNotFoundWithoutSendingSelection()
        {
            using (var server = new Fan20320LoopbackServer(new[] { "000001.bin" }))
            {
                var fan = CreateFan(server.Port);

                Assert.AreEqual("Video ID 5 not found on fan.", fan.playVideoWithId("5"));
                server.WaitForCompletion();
                Assert.IsNull(server.SelectionCommand);
            }
        }

        [TestMethod]
        public void PlayVideoWithId_RejectsInvalidIdBeforeNetworkAccess()
        {
            var fan = new Fan20320 { ServerIp = "", ServerPort = 0 };

            Assert.AreEqual("Invalid videoID", fan.playVideoWithId("abc"));
        }

        [TestMethod]
        public void Fan20320_DefaultsAreComConfigurable()
        {
            Type type = typeof(Fan20320);

            Assert.IsTrue(Marshal.IsTypeVisibleFromCom(type));
            object fan = Activator.CreateInstance(type);
            Assert.AreEqual("192.168.4.1", type.GetProperty("ServerIp").GetValue(fan, null));
            Assert.AreEqual(20320, type.GetProperty("ServerPort").GetValue(fan, null));
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

        private static Fan20320 CreateFan(int port)
        {
            return new Fan20320
            {
                ServerIp = "127.0.0.1",
                ServerPort = port,
                ConnectTimeoutMs = 1000,
                SocketTimeoutMs = 1000
            };
        }

        private sealed class Fan20320LoopbackServer : IDisposable
        {
            private readonly TcpListener listener;
            private readonly Task serverTask;

            internal Fan20320LoopbackServer(string[] files)
            {
                listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                Port = ((IPEndPoint)listener.LocalEndpoint).Port;
                serverTask = Task.Run(() => Serve(files));
            }

            internal int Port { get; private set; }
            internal byte[] SelectionCommand { get; private set; }

            internal void WaitForCompletion()
            {
                Assert.IsTrue(serverTask.Wait(3000), "The loopback server did not finish.");
                serverTask.GetAwaiter().GetResult();
            }

            public void Dispose()
            {
                listener.Stop();
                serverTask.Wait(3000);
            }

            private void Serve(string[] files)
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    CollectionAssert.AreEqual(Fan20320Protocol.BuildCommandFrame(new byte[0]), ReadFrame(stream));

                    byte[] response = BuildFileListResponse(files);
                    stream.Write(response, 0, 9);
                    stream.Write(response, 9, response.Length - 9);

                    stream.ReadTimeout = 500;
                    try
                    {
                        byte[] selectionFrame = ReadFrame(stream);
                        SelectionCommand = Copy(selectionFrame, 15, selectionFrame.Length - 27);
                    }
                    catch (IOException)
                    {
                        // No selection is expected when the requested file is absent.
                    }
                }
            }
        }

        private static byte[] ReadFrame(NetworkStream stream)
        {
            byte[] header = ReadExactly(stream, 15);
            int payloadLength = header[12] * 323 + (header[13] - 99) * 17 + (header[14] - 98);
            byte[] frame = new byte[15 + payloadLength + 12];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            byte[] remainder = ReadExactly(stream, frame.Length - header.Length);
            Buffer.BlockCopy(remainder, 0, frame, header.Length, remainder.Length);
            return frame;
        }

        private static byte[] ReadExactly(NetworkStream stream, int length)
        {
            var data = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int count = stream.Read(data, offset, length - offset);
                if (count == 0)
                {
                    throw new IOException("The client disconnected before its frame was complete.");
                }
                offset += count;
            }
            return data;
        }

        private static byte[] BuildFileListResponse(string[] names)
        {
            var payload = new List<byte> { (byte)'i' };
            Encoding encoding = Encoding.GetEncoding("GB2312");
            foreach (string name in names)
            {
                byte[] nameBytes = encoding.GetBytes(name);
                payload.Add((byte)nameBytes.Length);
                payload.AddRange(nameBytes);
            }
            payload.Add(1);
            payload.AddRange(new byte[15]);

            int length = payload.Count;
            var frame = new byte[15 + length + 12];
            Buffer.BlockCopy(Fan20320Protocol.StartMarker, 0, frame, 0, 12);
            frame[12] = (byte)(length / 323);
            frame[13] = (byte)(((length / 17) % 19) + 99);
            frame[14] = (byte)(((length % 323) % 17) + 98);
            payload.CopyTo(frame, 15);
            Buffer.BlockCopy(Fan20320Protocol.EndMarker, 0, frame, 15 + length, 12);
            return frame;
        }

        private static byte[] Copy(byte[] source, int offset, int length)
        {
            var copy = new byte[length];
            Buffer.BlockCopy(source, offset, copy, 0, length);
            return copy;
        }
    }
}
