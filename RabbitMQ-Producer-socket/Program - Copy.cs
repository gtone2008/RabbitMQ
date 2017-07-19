using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ConsoleApplication1
{
    internal class Program
    {
        private static Socket clentSocket;

        private static void Main(string[] args)
        {
            //connectServer("10.136.180.21");
            //sendCommand("ccmclist\0");
            //sendCommand("Selectevent\0");
            //sendCommand("moduleevent\0");
            //sendCommand("GetModuleSelectEvent\0");
            //sendCommand("GetSelectEvent\0");
            //sendCommand("CCEVENT\0");

            var clientHostList = new string[] { "10.136.180.21", "10.136.180.211" };

            foreach (string host in clientHostList)
            {
                ThreadPool.QueueUserWorkItem(start => startWork(host, "moduleevent\0"));
            }

            Console.ReadLine();
        }

        private static void startWork(string host, string strCommand)
        {
            try
            {
                connectServer(host);
                sendCommand(strCommand);
                receiveMessage();
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
            }
        }

        private static void receiveMessage()
        {
            string msg = "";
            byte[] recBytes = new byte[4096];
            int bytes = 0;
            Regex rg = new Regex(@"\[BAY.{5,15}PDCOUNT2([\s\S]*?)*?,");
            do
            {
                bytes = clentSocket.Receive(recBytes, recBytes.Length, 0);
                msg = Encoding.ASCII.GetString(recBytes, 0, bytes);
                var collMsg = rg.Matches(msg);
                foreach (var m in collMsg)
                {
                    string[] x = m.ToString().Split('\t');
                    writeFile(m.ToString());
                    Console.WriteLine(x[0] + "\t" + x[2]);
                }

                //Console.WriteLine("-------------------------" + bytes);
            } while (msg!=null);
        }

        private static void sendCommand(string strCommand)
        {
            byte[] sendBytes = Encoding.ASCII.GetBytes(strCommand);
            clentSocket.Send(sendBytes);
        }

        private static void connectServer(string host)
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, 53341);
            clentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clentSocket.Connect(ipe);
        }

        private static void writeFile(string msg)
        {
            using (StreamWriter sw = File.AppendText("PDCOUNT2.txt"))
            {
                sw.Write(msg);
            }
        }

        public static void testService()
        {
            const int BufferSize = 8192;
            Console.WriteLine("service run*********");

            IPAddress ip = IPAddress.Parse("127.0.0.1");
            TcpListener listener = new TcpListener(ip, 8500);

            Console.WriteLine("service listening*********");

            TcpClient remoteClient = listener.AcceptTcpClient();

            Console.WriteLine("Client connected! {0}<-{1}", remoteClient.Client.LocalEndPoint, remoteClient.Client.RemoteEndPoint);

            NetworkStream streamToClient = remoteClient.GetStream();
            do
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead = streamToClient.Read(buffer, 0, BufferSize);
                Console.WriteLine("Reading data,{0} bytes", bytesRead);

                string msg = Encoding.Unicode.GetString(buffer, 0, bytesRead);
                Console.WriteLine("Received:{0}", msg);
            } while (true);
        }
    }
}