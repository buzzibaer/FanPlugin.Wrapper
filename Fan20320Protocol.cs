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

            int remainder = command.Length;
            byte firstLengthByte = (byte)(98 + (remainder / 323));
            remainder %= 323;
            byte secondLengthByte = (byte)(99 + (remainder / 17));
            byte thirdLengthByte = (byte)(98 + (remainder % 17));
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

            if (frame[12] < 98 || frame[13] < 99 || frame[14] < 98)
            {
                return false;
            }

            int payloadLength = (frame[12] - 98) * 323
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
