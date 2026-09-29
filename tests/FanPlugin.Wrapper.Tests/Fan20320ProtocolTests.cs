using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FanPlugin.Wrapper.Tests
{
    [TestClass]
    public class Fan20320ProtocolTests
    {
        [TestMethod]
        public void BuildCommandFrame_UsesMarkersAndEncodedLength()
        {
            byte[] frame = Fan20320Protocol.BuildCommandFrame(new[] { (byte)'e' });

            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("C0EEB7C9BAA3"), Copy(frame, 0, 12));
            CollectionAssert.AreEqual(new byte[] { 0, 99, 99 }, Copy(frame, 12, 3));
            Assert.AreEqual((byte)'e', frame[15]);
            CollectionAssert.AreEqual(Encoding.ASCII.GetBytes("C0EEBDF9E5B7"), Copy(frame, 16, 12));
        }

        [TestMethod]
        public void BuildCommandFrame_WritesFileIndexAsRawByte()
        {
            byte[] frame = Fan20320Protocol.BuildCommandFrame(new[] { (byte)'B', (byte)1 });

            Assert.AreEqual((byte)1, frame[16]);
        }

        [TestMethod]
        public void TryParseFileList_ParsesOrderedNamesAndStatusSuffix()
        {
            byte[] frame = BuildFileListResponse("000001.bin", "000005.bin");
            string[] files;

            Assert.AreEqual((byte)0, frame[12]);
            Assert.IsTrue(Fan20320Protocol.TryParseFileList(frame, out files));
            CollectionAssert.AreEqual(new[] { "000001.bin", "000005.bin" }, files);
        }

        [TestMethod]
        public void TryParseFileList_RejectsInvalidFrame()
        {
            string[] files;

            Assert.IsFalse(Fan20320Protocol.TryParseFileList(new byte[0], out files));
        }

        private static byte[] BuildFileListResponse(params string[] names)
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
            int payloadLength = payload.Count;
            byte firstLengthByte = (byte)(payloadLength / 323);
            byte secondLengthByte = (byte)(((payloadLength / 17) % 19) + 99);
            byte thirdLengthByte = (byte)(((payloadLength % 323) % 17) + 98);
            byte[] frame = new byte[12 + 3 + payloadLength + 12];

            Buffer.BlockCopy(Encoding.ASCII.GetBytes("C0EEB7C9BAA3"), 0, frame, 0, 12);
            frame[12] = firstLengthByte;
            frame[13] = secondLengthByte;
            frame[14] = thirdLengthByte;
            payload.CopyTo(frame, 15);
            Buffer.BlockCopy(Encoding.ASCII.GetBytes("C0EEBDF9E5B7"), 0, frame, 15 + payloadLength, 12);
            return frame;
        }

        private static byte[] Copy(byte[] source, int offset, int length)
        {
            var result = new byte[length];
            Buffer.BlockCopy(source, offset, result, 0, length);
            return result;
        }
    }
}
