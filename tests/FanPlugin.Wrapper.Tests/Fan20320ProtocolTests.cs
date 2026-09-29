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

        [DataTestMethod]
        [DataRow(17, 0, 100, 98)]
        [DataRow(323, 1, 99, 98)]
        [DataRow(324, 1, 99, 99)]
        public void BuildCommandFrame_UsesExpectedLengthBytesAtBoundaries(
            int payloadLength,
            int firstLengthByte,
            int secondLengthByte,
            int thirdLengthByte)
        {
            byte[] frame = Fan20320Protocol.BuildCommandFrame(new byte[payloadLength]);

            CollectionAssert.AreEqual(
                new[] { (byte)firstLengthByte, (byte)secondLengthByte, (byte)thirdLengthByte },
                Copy(frame, 12, 3));
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

        [TestMethod]
        public void TryFindFileIndex_UsesPaddedFilenameRatherThanIdAsIndex()
        {
            int index;

            bool found = Fan20320Protocol.TryFindFileIndex(
                new[] { "000001.bin", "000005.bin", "000010.bin" },
                5,
                out index);

            Assert.IsTrue(found);
            Assert.AreEqual(1, index);
        }

        [TestMethod]
        public void TryFindFileIndex_ReturnsFalseForAbsentFile()
        {
            int index;

            bool found = Fan20320Protocol.TryFindFileIndex(new[] { "000001.bin" }, 5, out index);

            Assert.IsFalse(found);
            Assert.AreEqual(-1, index);
        }

        [TestMethod]
        public void TryFindFileIndex_RejectsIndexOutsideSingleByteRange()
        {
            var files = new string[257];
            int index;

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = i.ToString("D6") + ".bin";
            }

            bool found = Fan20320Protocol.TryFindFileIndex(files, 256, out index);

            Assert.IsFalse(found);
            Assert.AreEqual(-1, index);
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
