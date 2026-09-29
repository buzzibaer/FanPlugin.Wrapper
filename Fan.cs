using System;
using System.IO.MemoryMappedFiles;
using System.Net.Sockets;

namespace FanPlugin.Wrapper
{
    public class Fan
    {
        /*
           
        // Version 3 FAN

        private static String playModelSingle = "36";
        private static String playLoop = "37";
        private static String playFile = "40";
        private static String DEFAULT_NO_DATA_LENTH = "abc";
        private static String DEFAULT_HAS_2_DATA_LENTH = "abe";
        private static String end = "a4a8c2e3";
        private static String sendAPInfo = "99";
        private static  String contentDefaultValue = "00";
        public static String DEFUALT_SERVER_IP = "192.168.4.1";
	    public static int DEFUALT_SERVER_PORT = 5233;
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
            SharedMemory<SharedData> shmem = new SharedMemory<SharedData>("Shem",32);

            if (!shmem.Open()) return "fail";

            SharedData data = new SharedData();

            // Read from shared memory
            data = shmem.Data;


            // Change some data
            data.last = data.actual;
            data.actual = int.Parse(videoID);
             

            // Write back to shared memory
            shmem.Data = data;            

            // Close shared memory
            shmem.Close();

            String command = "c31c" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(int.Parse(videoID)) + end;
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
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = DEFUALT_SERVER_PORT;
                System.Net.Sockets.TcpClient client = new TcpClient(DEFUALT_SERVER_IP, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                // Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                //data = new Byte[256];

                // String to store the response ASCII representation.
                //String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                //Int32 bytes = stream.Read(data, 0, data.Length);
                //responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                //Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                // Console.WriteLine("ArgumentNullException: {0}", e);
                return e.Message;
            }
            catch (SocketException e)
            {
                // Console.WriteLine("SocketException: {0}", e);
                return e.Message;
            }

           // Console.WriteLine("\n Press Enter to continue...");
           // Console.Read();

            return "Command successfull";
        }


        private static String connectRead(String message)
        {
            try
            {
                // Create a TcpClient.
                // Note, for this client to work you need to have a TcpServer
                // connected to the same address as specified by the server, port
                // combination.
                Int32 port = DEFUALT_SERVER_PORT;
                System.Net.Sockets.TcpClient client = new TcpClient(DEFUALT_SERVER_IP, port);

                // Translate the passed message into ASCII and store it as a Byte array.
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

                // Get a client stream for reading and writing.
                //  Stream stream = client.GetStream();

                NetworkStream stream = client.GetStream();

                // Send the message to the connected TcpServer.
                stream.Write(data, 0, data.Length);

                // Console.WriteLine("Sent: {0}", message);

                // Receive the TcpServer.response.

                // Buffer to store the response bytes.
                data = new Byte[256];

                // String to store the response ASCII representation.
                String responseData = String.Empty;

                // Read the first batch of the TcpServer response bytes.
                Int32 bytes = stream.Read(data, 0, data.Length);
                responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
                // Console.WriteLine("Received: {0}", responseData);

                // Close everything.
                stream.Close();
                client.Close();

                return responseData;
            }
            catch (ArgumentNullException e)
            {
                // Console.WriteLine("ArgumentNullException: {0}", e);
                return e.Message;
            }
            catch (SocketException e)
            {
                // Console.WriteLine("SocketException: {0}", e);
                return e.Message;
            }
            
        }

        */



        // Version 2 FAN
        
        private static String playModelSingle = "36";
        private static String playLoop = "37";
        private static String playFile = "40";
        private static String DEFAULT_NO_DATA_LENTH = "lfh";
        private static String DEFAULT_HAS_2_DATA_LENTH = "lfj";
        private static String end = "bfb5d2a2";
        private static String sendAPInfo = "99";
        private static  String contentDefaultValue = "00";
        /// <summary>Default address for Version 2 fan hardware.</summary>
        public const string DefaultServerIp = "192.168.4.1";
        /// <summary>Default TCP port for Version 2 fan hardware.</summary>
        public const int DefaultServerPort = 5233;

        /// <summary>Server address used by this Version 2 fan instance.</summary>
        public string ServerIp { get; set; } = DefaultServerIp;
        /// <summary>TCP port used by this Version 2 fan instance.</summary>
        public int ServerPort { get; set; } = DefaultServerPort;
        /// <summary>Maximum time to establish a TCP connection, in milliseconds.</summary>
        public int ConnectTimeoutMs { get; set; } = 3000;
        /// <summary>Maximum time for a TCP read or write, in milliseconds.</summary>
        public int SocketTimeoutMs { get; set; } = 3000;
        private static String getFileList = "38";
        private static String playlast = "33";
        //private static String powerOff = "94";
        //private static String powerOn = "95";

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

            String command = "c0eeb7c9baa3020000000014cc" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(parsedVideoId) + end;
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

            String command = "c0eeb7c9baa3020000000014cc" + playModelSingle + DEFAULT_NO_DATA_LENTH + end;            
            return connect(command);
        }

        public String selectLoopVideoPlaybackMode()
        {

            String command = "c0eeb7c9baa3020000000014cc" + playLoop + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);
        }

        public String getFileListFromFan() {
            String command = "c0eeb7c9baa3020000000014cc" + getFileList + DEFAULT_NO_DATA_LENTH + end;
            return connectRead(command);
        }

        public String getApiVersionInfo() {
           String command = "c31c" + sendAPInfo + DEFAULT_NO_DATA_LENTH + end;
            return connect(command);
        }

        public String playLastFromFan() {

            String command = "c0eeb7c9baa3020000000014cc" + playlast + DEFAULT_NO_DATA_LENTH + end;
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

            String command = "c0eeb7c9baa3020000000014cc" + playFile + DEFAULT_HAS_2_DATA_LENTH + intTo2Str(data.last) + end;
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
