using System;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;

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
        /// <summary>Default address for Version 3 fan hardware.</summary>
        public const string DefaultServerIp = "192.168.4.1";
        /// <summary>Default TCP port for Version 3 fan hardware.</summary>
        public const int DefaultServerPort = 5233;

        /// <summary>Server address used by this Version 3 fan instance.</summary>
        public string ServerIp { get; set; } = DefaultServerIp;
        /// <summary>TCP port used by this Version 3 fan instance.</summary>
        public int ServerPort { get; set; } = DefaultServerPort;
        /// <summary>Maximum time to establish a TCP connection, in milliseconds.</summary>
        public int ConnectTimeoutMs { get; set; } = 3000;
        /// <summary>Maximum time for a TCP read or write, in milliseconds.</summary>
        public int SocketTimeoutMs { get; set; } = 3000;
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
            int parsedVideoId;
            if (!FanVideoId.TryParse(videoID, out parsedVideoId))
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
            data.actual = parsedVideoId;

            String command = "c31c" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(parsedVideoId) + end;
            String result = connect(command);
            if (result.StartsWith("Network error:"))
            {
                shmem.Close();
                return result;
            }

            // Record playback history only after the command was sent.
            shmem.Data = data;
            shmem.Close();

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

        private String connect(String message)
        {
            FanTcpResult result = FanTcpTransport.Send(
                ServerIp, ServerPort, ConnectTimeoutMs, SocketTimeoutMs, message, false);
            return result.Succeeded ? result.Value : result.Error;
        }


        private String connectRead(String message)
        {
            FanTcpResult result = FanTcpTransport.Send(
                ServerIp, ServerPort, ConnectTimeoutMs, SocketTimeoutMs, message, true);
            return result.Succeeded ? result.Value : result.Error;
        }

        



    }
        

}
