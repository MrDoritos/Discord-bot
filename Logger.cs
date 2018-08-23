using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Discord.WebSocket;
using Discord;

namespace MusicBot9001
{
    internal class Logger
    {
        static public ConsoleColor defaultcolor = ConsoleColor.Gray;
        static private ConsoleColor infofrgd = ConsoleColor.Blue;
        static private ConsoleColor warningfrgd = ConsoleColor.Yellow;
        static private ConsoleColor errorfrgd = ConsoleColor.Red;
        static private ConsoleColor chatfrgd = ConsoleColor.Green;
        static private ConsoleColor commandfrgd = ConsoleColor.Cyan;
        static public bool write = false;
        static public StreamWriter stream = GetStream();

        static public void Info(string info)
        {
            string time = DateTime.Now.ToString("[MM-dd-yyyy hh:mm:ss]");
            Console.ForegroundColor = infofrgd;
            Console.Write(time + " [INFO] ");
            Console.ForegroundColor = defaultcolor;
            Console.WriteLine(info);
            if (Program.config.logLevel == Preferences.LogLevel.debug)
                WriteLog(time + " [INFO] " + info);
        }

        static public void Warning(string warning)
        {
            string time = DateTime.Now.ToString("[MM-dd-yyyy hh:mm:ss]");
            Console.ForegroundColor = warningfrgd;
            Console.Write(time + " [WARNING] ");
            Console.ForegroundColor = defaultcolor;
            Console.WriteLine(warning);
            if (Program.config.logLevel == Preferences.LogLevel.debug)
                WriteLog(time + " [WARNING] " + warning);
        }

        static public void Error(string message, Exception e = null)
        {
            string time = DateTime.Now.ToString("[MM-dd-yyyy hh:mm:ss]");
            Console.ForegroundColor = errorfrgd;
            Console.Write(time + " [ERROR] ");
            Console.ForegroundColor = defaultcolor;
            if (e != null)
            {
                Console.Write("{0} ", message);
                Console.WriteLine(e.Message);
                if (Program.config.logLevel == Preferences.LogLevel.debug)
                    WriteLog(time + " [ERROR] " + message + ", " + e.Message);
            } else
            {
                Console.WriteLine(message);
                if (Program.config.logLevel == Preferences.LogLevel.debug)
                    WriteLog(time + " [ERROR] " + message);
            }
        }

        static public void Command(string message)
        {
            string time = DateTime.Now.ToString("[MM-dd-yyyy hh:mm:ss]");
            Console.ForegroundColor = commandfrgd;
            Console.Write(time + " [COMMAND] ");
            Console.ForegroundColor = defaultcolor;
            Console.WriteLine("( " + message + " ) Recieved");
            if (Program.config.logLevel == Preferences.LogLevel.debug)
                WriteLog(time + " [COMMAND] ( " + message + " ) Recieved");
        }

        static public void Chat(SocketMessage message)
        {
            string collect = message.Content;
            string time = DateTime.Now.ToString("[MM-dd-yyyy hh:mm:ss]");
            Console.ForegroundColor = chatfrgd;
            Console.Write(time + " [CHAT] ");
            Console.ForegroundColor = defaultcolor;
            if (message.Attachments == null)
            {
                if (message.Channel as SocketDMChannel == null)
                {
                    collect = time + " [CHAT] " + "[ " + message.Author.Username + " (" + message.Author.Id + ")" + " ]"
                        + "" + "[ " + (message.Channel as SocketGuildChannel).Guild.Name + " (" + (message.Channel as SocketGuildChannel).Guild.Id + ") ]"
                        + "" + "[ " + message.Channel.Name + " (" + message.Channel.Id + ") ]"
                        + "" + "[ \"" + message.Content + "\" (" + message.Id + ") ]";
                    Console.WriteLine(collect);
                    if ((int)Program.config.logLevel > 0)
                        WriteLog(collect);
                }
                else
                {
                    var messag = message.Channel as IDMChannel;
                    collect = time + " [CHAT] " + "[ " + message.Author + " (" + message.Author.Id + ")" + " ]"
                        + "" + "[ " + messag.Name + " (" + messag.Id + ") ]"
                        + "" + "[ \"" + message.Content + "\" (" + message.Id + ") ]";
                    Console.WriteLine(collect);
                    if ((int)Program.config.logLevel > 0)
                        WriteLog(collect);
                }
            } else
            {
                if (message.Channel as IDMChannel == null)
                {
                    List<string> stringlist = new List<string>();
                    message.Attachments.ToList().ForEach(n => stringlist.Add(n.Url));
                    string attachments = "";
                    foreach (string a in stringlist)
                    {
                        attachments = attachments + "\n" + a;
                    }
                    collect = time + " [CHAT] " + "[ " + message.Author + " (" + message.Author.Id + ")" + " ]"
                        + " " + "[ " + (message.Channel as SocketGuildChannel).Guild.Name + " (" + (message.Channel as SocketGuildChannel).Guild.Id + ") ]"
                        + " " + "[ " + message.Channel.Name + " (" + message.Channel.Id + ") ]"
                        + " " + "[ \"" + message.Content + "\" " + attachments + " (" + message.Id + ") ]";
                    Console.WriteLine(collect);
                    if ((int)Program.config.logLevel > 0)
                        WriteLog(collect);
                }
                else
                {
                    var messag = message.Channel as IDMChannel;
                    List<string> stringlist = new List<string>();
                    message.Attachments.ToList().ForEach(n => stringlist.Add(n.Url));
                    string attachments = "";
                    foreach (string a in stringlist)
                    {
                        attachments = attachments + "\n" + a;
                    }
                    collect = time + " [CHAT] " + "[ " + message.Author + " (" + message.Author.Id + ")" + " ]"
                        + "" + "[ " + messag.Name + " (" + messag.Id + ") ]"
                        + "" + "[ \"" + message.Content + "\" " + attachments + " (" + message.Id + ") ]";
                    Console.WriteLine(collect);
                    if ((int)Program.config.logLevel > 0)
                        WriteLog(collect);

                }
            }
        }

        static public StreamWriter GetStream()
        {
            return File.AppendText("log.txt");
        }
        
        static public void WriteLog(string Data)
        {
            try
            {
                if (stream != null)
                {
                    stream.WriteLineAsync(Data);
                    stream.Flush();
                }
                return;
            } catch (Exception e)
            {
                //Error("at WriteLog()", e);
            }
        }
    }
}
