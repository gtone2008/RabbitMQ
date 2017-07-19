using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using RabbitMQ.Client;

namespace ConsoleApplication1
{
    public class Program
    {
        private static void Main(string[] args)
        {
            //connectServer("10.136.180.21");
            //sendCommand("ccmclist\0");
            //sendCommand("Selectevent\0");
            //sendCommand("moduleevent\0");
            //sendCommand("GetModuleSelectEvent\0");
            //sendCommand("GetSelectEvent\0");
            //sendCommand("CCEVENT\0");

            var clientHostList = new string[] { "10.136.180.21" };
            //var clientHostList = new string[] { "10.136.180.21","10.136.183.57" };
            foreach (string host in clientHostList)
            {
                ThreadPool.QueueUserWorkItem(start => startWork(host, "moduleevent\0"));
            }

            Console.ReadLine();
        }

        private static void RabbitMQFactory(byte[] msg)
        {
            ConnectionFactory factory = new ConnectionFactory();
            factory.Uri = "amqp://test:test@WUXENGSER04:5672";
            using (IConnection conn = factory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    //channel.QueueDeclare("MyTest", false, false, false, null);
                    channel.BasicPublish("", "MyTest", null, msg);
                }
            }

        }

        private static void startWork(string host, string strCommand)
        {
            try
            {
                IPEndPoint ipe = new IPEndPoint(IPAddress.Parse(host), 53341);
                Socket clentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clentSocket.BeginConnect(ipe, result =>
                {
                    if (result.IsCompleted)
                    {
                        sendCommand(strCommand, clentSocket);
                        receiveMessage(clentSocket);
                    }
                    else
                        return;
                }, null);

            }
            catch (Exception ex)
            {
                Console.WriteLine("error: " + ex.Message);
            }
        }

        private static void receiveMessage(Socket clentSocket)
        {
            if (clentSocket == null || !clentSocket.Connected) return;
            byte[] recBytes = new byte[4096];
            clentSocket.BeginReceive(recBytes, 0, recBytes.Length, SocketFlags.None, asyncResult =>
            {
                int length = clentSocket.EndReceive(asyncResult);
                string msg = Encoding.ASCII.GetString(recBytes, 0, length);
                //Regex rg = new Regex(@"\[BAY.{5,15}RECIPECHG:([\s\S]*?)*?,");
                //var collMsg1 = rg.Matches(msg);
                //string msg1 = Encoding.ASCII.GetString(recBytes);
                //Console.WriteLine(msg+"--"+ length);
                //writeFile(msg1 +"-----" +length+"\r\n",msg1.Substring(8,msg1.IndexOf('#')));
                //Console.WriteLine(msg1.Substring(8, msg1.IndexOf('#')));
                //Console.WriteLine(msg1.IndexOf('#'));
                //Console.WriteLine(msg1.IndexOf('['));
                //foreach (var m in collMsg1)
                //{
                //    writeFile(m+"\r\n", "xxx");
                //    Console.WriteLine(m);
                //}
                //writeFile(msg1, msg1.Substring(msg1.IndexOf('[')+1, msg1.IndexOf('#')-1- msg1.IndexOf('[')));
                RabbitMQFactory(recBytes);
                writeFile(msg + "\r\n", "abc");
                receiveMessage(clentSocket);
            }, null);

        }

        //private static void receiveMessage(Socket clentSocket)
        //{
        //    if (clentSocket == null || !clentSocket.Connected) return;
        //    string msg = "";
        //    byte[] recBytes = new byte[4096];
        //    int bytes = 0;
        //    Regex rg = new Regex(@"\[BAY.{5,15}PDCOUNT2([\s\S]*?)*?,");
        //    do
        //    {
        //        bytes = clentSocket.Receive(recBytes, recBytes.Length, 0);
        //        msg = Encoding.ASCII.GetString(recBytes, 0, bytes);

        //        var collMsg1 = rg.Matches(msg);
        //        //var collMsg = rg.Matches(msg);
        //        //foreach (var m in collMsg)
        //        //{
        //        //    string[] x = m.ToString().Split('\t');
        //        //    writeFile(m.ToString());
        //        //    Console.WriteLine(x[0] + "\t" + x[2]);
        //        //}
        //        int x = msg.IndexOf(",",1);
        //        Console.WriteLine(x);
        //        //foreach (var m in collMsg1)
        //        //{
        //        //    writeFile(m + "\r\n");
        //        //}
        //        writeFile(msg + "\r\n");
        //    } while (clentSocket.ReceiveBufferSize>0);
        //}

        private static void sendCommand(string strCommand, Socket clentSocket)
        {
            if (clentSocket == null || !clentSocket.Connected) return;
            byte[] sendBytes = Encoding.ASCII.GetBytes(strCommand);
            clentSocket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, result =>
                {
                    clentSocket.EndSend(result);
                }, null);
        }

        private static void connectServer(string host)
        {
            IPAddress ip = IPAddress.Parse(host);
            IPEndPoint ipe = new IPEndPoint(ip, 53341);
            Socket clentSocket;
            clentSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clentSocket.Connect(ipe);
        }

        private static void writeFile(string msg, string name)
        {
            //if(File.Exists("PDCOUNT2.txt"))
            //File.Move("PDCOUNT2.txt", "X/aaa.x");

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