using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace FanPlugin.Wrapper
{
    public class Fan20320
    {
        public const string DefaultServerIp = "192.168.4.1";
        public const int DefaultServerPort = 20320;
        private const int MaximumFrameSize = 64 * 1024;

        private TcpClient client;
        private NetworkStream stream;
        private string[] files;

        public string ServerIp { get; set; } = DefaultServerIp;
        public int ServerPort { get; set; } = DefaultServerPort;
        public int ConnectTimeoutMs { get; set; } = 3000;
        public int SocketTimeoutMs { get; set; } = 3000;

        public string playVideoWithId(string videoID)
        {
            int videoId;
            if (!FanVideoId.TryParse(videoID, out videoId))
            {
                return "Invalid videoID";
            }

            string connectionError = EnsureConnected();
            if (connectionError != null)
            {
                return connectionError;
            }

            int index;
            if (!Fan20320Protocol.TryFindFileIndex(files, videoId, out index))
            {
                if (Fan20320Protocol.ContainsFile(files, videoId))
                {
                    return "Video ID " + videoId + " is beyond the fan selection index range.";
                }
                return "Video ID " + videoId + " not found on fan.";
            }

            try
            {
                byte[] command = Fan20320Protocol.BuildCommandFrame(new[] { (byte)'B', (byte)index });
                stream.Write(command, 0, command.Length);
                return "Command successfull";
            }
            catch (Exception exception) when (IsNetworkException(exception))
            {
                CloseSession();
                return NetworkError(exception);
            }
        }

        private string EnsureConnected()
        {
            if (client != null && stream != null && files != null)
            {
                return null;
            }

            if (ConnectTimeoutMs <= 0 || SocketTimeoutMs <= 0)
            {
                return "Network error: Timeout values must be positive.";
            }

            try
            {
                client = new TcpClient();
                IAsyncResult connectAttempt = client.BeginConnect(ServerIp, ServerPort, null, null);
                WaitHandle connectWaitHandle = connectAttempt.AsyncWaitHandle;
                try
                {
                    if (!connectWaitHandle.WaitOne(ConnectTimeoutMs))
                    {
                        CloseSession();
                        try
                        {
                            client.EndConnect(connectAttempt);
                        }
                        catch
                        {
                            // Closing the client cancels the pending connect attempt.
                        }
                        return "Network timeout: connect";
                    }

                    client.EndConnect(connectAttempt);
                }
                finally
                {
                    connectWaitHandle.Close();
                }

                stream = client.GetStream();
                stream.ReadTimeout = SocketTimeoutMs;
                stream.WriteTimeout = SocketTimeoutMs;
                byte[] request = Fan20320Protocol.BuildCommandFrame(new byte[0]);
                stream.Write(request, 0, request.Length);

                while (true)
                {
                    byte[] response = ReadFrame();
                    string[] receivedFiles;
                    if (Fan20320Protocol.TryParseFileList(response, out receivedFiles))
                    {
                        files = receivedFiles;
                        return null;
                    }
                    if (response[15] == (byte)'i')
                    {
                        throw new InvalidDataException("Invalid response frame.");
                    }
                }
            }
            catch (Exception exception) when (IsNetworkException(exception))
            {
                CloseSession();
                return NetworkError(exception);
            }
        }

        private byte[] ReadFrame()
        {
            byte[] header = ReadExactly(15);
            if (!HasMarker(header, 0, Fan20320Protocol.StartMarker)
                || header[13] < 99 || header[13] > 117
                || header[14] < 98 || header[14] > 114)
            {
                throw new InvalidDataException("Invalid response frame.");
            }

            int payloadLength = header[12] * 323
                + (header[13] - 99) * 17
                + (header[14] - 98);
            int frameLength = 15 + payloadLength + Fan20320Protocol.EndMarker.Length;
            if (frameLength > MaximumFrameSize)
            {
                throw new InvalidDataException("Response frame exceeds 64 KiB.");
            }

            byte[] frame = new byte[frameLength];
            Buffer.BlockCopy(header, 0, frame, 0, header.Length);
            byte[] remainder = ReadExactly(frameLength - header.Length);
            Buffer.BlockCopy(remainder, 0, frame, header.Length, remainder.Length);
            if (!HasMarker(frame, frameLength - Fan20320Protocol.EndMarker.Length, Fan20320Protocol.EndMarker))
            {
                throw new InvalidDataException("Invalid response frame.");
            }
            return frame;
        }

        private byte[] ReadExactly(int length)
        {
            var data = new byte[length];
            int offset = 0;
            while (offset < length)
            {
                int count = stream.Read(data, offset, length - offset);
                if (count == 0)
                {
                    throw new IOException("The fan disconnected.");
                }
                offset += count;
            }
            return data;
        }

        private void CloseSession()
        {
            files = null;
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }
            if (client != null)
            {
                client.Close();
                client = null;
            }
        }

        private static bool HasMarker(byte[] value, int offset, byte[] marker)
        {
            for (int index = 0; index < marker.Length; index++)
            {
                if (value[offset + index] != marker[index])
                {
                    return false;
                }
            }
            return true;
        }

        private static bool IsNetworkException(Exception exception)
        {
            return exception is ArgumentException
                || exception is IOException
                || exception is ObjectDisposedException
                || exception is SocketException
                || exception is InvalidDataException;
        }

        private static string NetworkError(Exception exception)
        {
            SocketException socketException = FindSocketException(exception);
            if (socketException != null && socketException.SocketErrorCode == SocketError.TimedOut)
            {
                return "Network timeout: read/write";
            }
            return "Network error: " + (socketException == null ? exception.Message : socketException.SocketErrorCode.ToString());
        }

        private static SocketException FindSocketException(Exception exception)
        {
            while (exception != null)
            {
                var socketException = exception as SocketException;
                if (socketException != null)
                {
                    return socketException;
                }
                exception = exception.InnerException;
            }
            return null;
        }
    }
}
