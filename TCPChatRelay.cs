using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace MusicBot9001
{
    class TCPChatRelay
    {
        static public TcpClient clientSocket;
        static public NetworkStream clientStream;
        static public IPAddress bindAddress = Dns.GetHostAddresses("iansweb.org")[0];
        static public short bindPort = 1200;
        static public bool open = false;
        static public ITextChannel channelBind;

        public static async Task Relay(ITextChannel channelBind_)
        {
            if (channelBind_ != null)
            {
                channelBind = channelBind_;
            }
            else
            {
                return;
            }
            try
            {
                clientSocket = new TcpClient(bindAddress.ToString(), bindPort);
                clientStream = clientSocket.GetStream();
                open = true;
                //Thread drawThread = new Thread(Draw);
                //drawThread.Start();
                //Thread clientThread = new Thread(Client);
                //clientThread.Start();
                //Thread.Sleep(500);
                //Thread receiveThread = new Thread(Receive);
                Logger.Info("TCPChat client bound!");
                Receive();
            }
            catch (Exception e)
            {
                //Console.WriteLine("Could not create client on {0}:{1}", bindAddress.ToString(), bindPort);
                //Console.WriteLine(e.Message);
                //Console.WriteLine(e.StackTrace);
                Logger.Error("Could not create client on " + bindAddress.ToString() + ":" + bindPort, e);
                open = false;
            }
        }

        public static async Task Receive()
        {
            using (StreamReader reader = new StreamReader(clientStream))
            {
                while (true)
                {
                    string recieved = reader.ReadLine().TrimEnd('\n', '\r');
                    Console.WriteLine(recieved);
                    await channelBind.SendMessageAsync(recieved);
                }
            }
        }

        public static void SendMessage(string message)
        {
            //using (StreamWriter writer = new StreamWriter(clientStream))
            //{
                
                clientStream.Write(Encoding.UTF8.GetBytes(message += "\n"), 0, Encoding.UTF8.GetByteCount(message += "\n"));
                //writer.Flush();
            //}
        }

        public static void SendMessage(SocketMessage message)
        {
            if (message.Author == Program.client.CurrentUser)
            { return; }
            //using (StreamWriter writer = new StreamWriter(clientStream))
            //{
                clientStream.Write(Encoding.UTF8.GetBytes("[" + message.Author.Username + "] " + message.Content), 0, Encoding.UTF8.GetByteCount("[" + message.Author.Username + "] " + message.Content));
            //}
        }
    }
}
