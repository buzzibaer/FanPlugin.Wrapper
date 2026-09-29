using System;
using System.Collections.Generic;
using System.Text;

namespace FanPlugin.Wrapper
{
    internal static class Fan20320Protocol
    {
        internal static readonly byte[] StartMarker = Encoding.ASCII.GetBytes("C0EEB7C9BAA3");
        internal static readonly byte[] EndMarker = Encoding.ASCII.GetBytes("C0EEBDF9E5B7");

        internal static byte[] BuildCommandFrame(byte[] command)
        {
            if (command == null || command.Length > 65534)
            {
                throw new ArgumentOutOfRangeException("command");
            }

            byte firstLengthByte = (byte)(command.Length / 323);
            byte secondLengthByte = (byte)(((command.Length / 17) % 19) + 99);
            byte thirdLengthByte = (byte)(((command.Length % 323) % 17) + 98);
            byte[] frame = new byte[StartMarker.Length + 3 + command.Length + EndMarker.Length];

            Buffer.BlockCopy(StartMarker, 0, frame, 0, StartMarker.Length);
            frame[12] = firstLengthByte;
            frame[13] = secondLengthByte;
            frame[14] = thirdLengthByte;
            Buffer.BlockCopy(command, 0, frame, 15, command.Length);
            Buffer.BlockCopy(EndMarker, 0, frame, 15 + command.Length, EndMarker.Length);
            return frame;
        }

        internal static bool TryParseFileList(byte[] frame, out string[] files)
        {
            files = new string[0];

            if (frame == null || frame.Length < 44 || !HasMarker(frame, 0, StartMarker))
            {
                return false;
            }

            if (frame[13] < 99 || frame[13] > 117 || frame[14] < 98 || frame[14] > 114)
            {
                return false;
            }

            int payloadLength = frame[12] * 323
                + (frame[13] - 99) * 17
                + (frame[14] - 98);
            int expectedLength = StartMarker.Length + 3 + payloadLength + EndMarker.Length;

            if (payloadLength < 17 || frame.Length != expectedLength || !HasMarker(frame, 15 + payloadLength, EndMarker))
            {
                return false;
            }

            if (frame[15] != (byte)'i')
            {
                return false;
            }

            int position = 16;
            int statusStart = 15 + payloadLength - 16;
            var parsedFiles = new List<string>();
            Encoding encoding = Encoding.GetEncoding("GB2312");

            while (position < statusStart)
            {
                int nameLength = frame[position++];
                if (nameLength > statusStart - position)
                {
                    return false;
                }

                parsedFiles.Add(encoding.GetString(frame, position, nameLength));
                position += nameLength;
            }

            files = parsedFiles.ToArray();
            return true;
        }

        internal static bool TryFindFileIndex(string[] files, int videoId, out int index)
        {
            index = -1;
            string expectedName = videoId.ToString("D6") + ".bin";

            for (int i = 0; i < files.Length; i++)
            {
                if (string.Equals(files[i], expectedName, StringComparison.OrdinalIgnoreCase))
                {
                    if (i > byte.MaxValue)
                    {
                        return false;
                    }

                    index = i;
                    return true;
                }
            }

            return false;
        }

        private static bool HasMarker(byte[] frame, int offset, byte[] marker)
        {
            for (int index = 0; index < marker.Length; index++)
            {
                if (frame[offset + index] != marker[index])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
