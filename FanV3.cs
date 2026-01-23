using System;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;
using System.Text;

namespace FanPlugin.Wrapper
{
    public class FanV3
    {
        // Version 3 FAN

        private static String playModelSingle = "36";
        private static String playLoop = "37";
        private static String playFile = "40";
        private static String DEFAULT_NO_DATA_LENTH = "abc";
        private static String DEFAULT_HAS_2_DATA_LENTH = "abe";
        private static String end = "a4a8c2e3";
        private static String sendAPInfo = "99";
        private static  String contentDefaultValue = "00";

        // Configurables (public pour permettre override depuis l'extérieur)
        public static string ServerIp { get; set; } = "192.168.4.1";
        public static int ServerPort { get; set; } = 5233;
        /// <summary>Timeout de connexion en millisecondes</summary>
        public static int ConnectTimeoutMs { get; set; } = 3000;
        /// <summary>Timeout de lecture/écriture socket en millisecondes</summary>
        public static int SocketTimeoutMs { get; set; } = 3000;

        private static String getFileList = "38";
        private static String playlast = "33";
        private static String powerOff = "94";
        private static String powerOn = "95";

        public struct SharedData { 
            public int actual { get; set; }
            public int last { get; set; }
        }
            

        public string playVideoWithId(String videoID)
        {
            if (!int.TryParse(videoID, out int vid))
            {
                return "Invalid videoID";
            }
        
            SharedMemory<SharedData> shmem = new SharedMemory<SharedData>("Shem",32);

            if (!shmem.Open()) return "fail";

            SharedData data = new SharedData();

            // Read from shared memory
            data = shmem.Data;

            // Change some data
            data.last = data.actual;
            data.actual = vid;
             
            // Write back to shared memory
            shmem.Data = data;            

            // Close shared memory
            shmem.Close();

            String command = "c31c" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(vid) + end;
            connect(command);

            return "NEW ID = " + data.actual + " Old ID = " + data.last;
        }

        public String selectSingleVideoPlaybackMode() { 
            String command = "c31c" + playModelSingle + DEFAULT_NO_DATA_LENTH + end;            
            return connect(command);
        }

        public String selectLoopVideoPlaybackMode()
        {
            String command = "c31c" + playLoop + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);
        }

        public String getFileListFromFan() {
            String command =  "c31c" + getFileList + DEFAULT_NO_DATA_LENTH + end;
            return connectRead(command);
        }

        public String getApiVersionInfo() {
            String command = "c31c" + sendAPInfo + DEFAULT_NO_DATA_LENTH + end;
            return connectRead(command);
        }

        public String playLastFromFan() {
            String command = "c31c" + playlast + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);            
        }

        public String sendPowerOn()
        {
            String command = "c31c" + powerOn + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);
        }

        public String sendPowerOff()
        {
            String command = "c31c" + powerOff + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);
        }

        public String playOldFromFan() {
            SharedMemory<SharedData> shmem = new SharedMemory<SharedData>("Shem", 32);

            if (!shmem.Open()) return "fail";

            SharedData data = new SharedData();

            // Read from shared memory
            data = shmem.Data;
      
            // Write back to shared memory
            shmem.Data = data;

            // Close shared memory
            shmem.Close();

            String command = "c31c" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(data.last) + end;
            connect(command);
            return connect("Old ID = " + data.last);
        }

        private static String intTo2Str(int i)
        {
            if (i >= 0 && i < 10)
            {
                return "0" + i;
            }
            else if (i < 10 || i >= 100)
            {
                return i >= 100 ? sendAPInfo : contentDefaultValue;
            }
            else
            {
                return i.ToString();             
            }
        }

        private static String connect(String message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ServerIp, ServerPort);
                    bool connected = connectTask.Wait(ConnectTimeoutMs);
                    if (!connected || !client.Connected)
                    {
                        return "Connection timeout or refused";
                    }

                    client.SendTimeout = SocketTimeoutMs;
                    client.ReceiveTimeout = SocketTimeoutMs;

                    using (NetworkStream stream = client.GetStream())
                    {
                        Byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);
                        // Not reading response here
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                return e.Message;
            }
            catch (SocketException e)
            {
                return e.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return "Command successful";
        }

        private static String connectRead(String message)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync(ServerIp, ServerPort);
                    bool connected = connectTask.Wait(ConnectTimeoutMs);
                    if (!connected || !client.Connected)
                    {
                        return "Connection timeout or refused";
                    }

                    client.SendTimeout = SocketTimeoutMs;
                    client.ReceiveTimeout = SocketTimeoutMs;

                    using (NetworkStream stream = client.GetStream())
                    {
                        Byte[] data = Encoding.ASCII.GetBytes(message);
                        stream.Write(data, 0, data.Length);

                        data = new Byte[1024];
                        int bytes = stream.Read(data, 0, data.Length);
                        String responseData = Encoding.ASCII.GetString(data, 0, bytes);
                        return responseData;
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                return e.Message;
            }
            catch (SocketException e)
            {
                return e.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}