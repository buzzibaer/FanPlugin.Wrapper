using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FanPlugin.Wrapper
{
    internal sealed class FanTcpResult
    {
        private FanTcpResult(bool succeeded, string value, string error)
        {
            Succeeded = succeeded;
            Value = value;
            Error = error;
        }

        internal bool Succeeded { get; private set; }
        internal string Value { get; private set; }
        internal string Error { get; private set; }

        internal static FanTcpResult Success(string value)
        {
            return new FanTcpResult(true, value, null);
        }

        internal static FanTcpResult Failure(string error)
        {
            return new FanTcpResult(false, null, error);
        }
    }

    internal static class FanTcpTransport
    {
        internal static FanTcpResult Send(
            string serverIp,
            int serverPort,
            int connectTimeoutMs,
            int socketTimeoutMs,
            string message,
            bool readResponse)
        {
            if (connectTimeoutMs <= 0 || socketTimeoutMs <= 0)
            {
                return FanTcpResult.Failure("Network error: Timeout values must be positive.");
            }

            using (var client = new TcpClient())
            {
                IAsyncResult connectAttempt = null;
                WaitHandle connectWaitHandle = null;

                try
                {
                    connectAttempt = client.BeginConnect(serverIp, serverPort, null, null);
                    connectWaitHandle = connectAttempt.AsyncWaitHandle;
                    if (!connectWaitHandle.WaitOne(connectTimeoutMs))
                    {
                        client.Close();
                        try
                        {
                            client.EndConnect(connectAttempt);
                        }
                        catch
                        {
                            // Closing the client cancels the pending connect attempt.
                        }

                        return FanTcpResult.Failure("Network timeout: connect");
                    }

                    client.EndConnect(connectAttempt);
                    using (NetworkStream stream = client.GetStream())
                    {
                        stream.ReadTimeout = socketTimeoutMs;
                        stream.WriteTimeout = socketTimeoutMs;

                        byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);

                        if (!readResponse)
                        {
                            return FanTcpResult.Success("Command successfull");
                        }

                        data = new byte[1024];
                        int bytes = stream.Read(data, 0, data.Length);
                        return FanTcpResult.Success(Encoding.ASCII.GetString(data, 0, bytes));
                    }
                }
                catch (ArgumentNullException e)
                {
                    return FanTcpResult.Failure("Network error: " + e.Message);
                }
                catch (ArgumentException e)
                {
                    return FanTcpResult.Failure("Network error: " + e.Message);
                }
                catch (SocketException e)
                {
                    if (e.SocketErrorCode == SocketError.TimedOut)
                    {
                        return FanTcpResult.Failure("Network timeout: " + e.SocketErrorCode);
                    }
                    return FanTcpResult.Failure("Network error: " + e.SocketErrorCode);
                }
                catch (IOException e)
                {
                    SocketException socketException = FindSocketException(e);
                    if (socketException != null && socketException.SocketErrorCode == SocketError.TimedOut)
                    {
                        return FanTcpResult.Failure("Network timeout: read/write");
                    }
                    return FanTcpResult.Failure("Network error: " + e.Message);
                }
                catch (ObjectDisposedException e)
                {
                    return FanTcpResult.Failure("Network error: " + e.Message);
                }
                finally
                {
                    if (connectWaitHandle != null)
                    {
                        connectWaitHandle.Close();
                    }
                }
            }
        }

        private static SocketException FindSocketException(Exception exception)
        {
            while (exception != null)
            {
                SocketException socketException = exception as SocketException;
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
