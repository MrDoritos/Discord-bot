using System;
using Discord;
using Discord.WebSocket;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.IO;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;

namespace MusicBot9001
{
    class Program
    {
        public static DiscordSocketClient client = new DiscordSocketClient();
        public static Preferences config = new Preferences();
        public static MainHandler mainHandler = new MainHandler();
        public static Logger logger = new Logger();
        public static string token;
        public static YouTubeService youtubeService;
        public static string helpText = "";

        static void Main(string[] args)
        {
            //Detect directories
            if (!Directory.Exists(".\\Guilds\\"))
            {
                Directory.CreateDirectory(".\\Guilds\\");
            }
            if (!Directory.Exists(config.musicPath))
            {
                config.musicPath = "Music\\";
                if (!Directory.Exists(".\\Music\\"))
                {
                    Directory.CreateDirectory(".\\Music\\");
                }
            }
            else
            {
                config.musicPath = config.musicPath.TrimEnd('\\') + "\\";
            }

            //Get a token
                if (args.Any(n => n == "-t") && args.Length >= args.ToList().IndexOf("-t"))
                {
                        token = args[args.ToList().IndexOf("-t") + 1];
                        Console.WriteLine("Token detected: " + token);
                    
                } else
                if (args.Any(n => n == "-v") && args.Length >= args.ToList().IndexOf("-v"))
                {
                        if (Environment.GetEnvironmentVariable(args[args.ToList().IndexOf("-v")]) != null)
                        {
                            token = Environment.GetEnvironmentVariable(args[args.ToList().IndexOf("-v") + 1]);
                            Console.WriteLine("Token detected: " + token);
                        }
                } else if (config.token.Length > 0)
                {
                    token = config.token;
                    Console.WriteLine("Token detected: " + token);
                } else
                {
                QueryToken();
                }
            //Info
                    //LogLevel
            Logger.Info("Log started with a level of: " + config.logLevel.ToString());
                    //Admins
            if (config.botAdmins.Length < 1)
            {
                Logger.Warning("No bot admins. It's dangerous to go alone!");
            }
            else
            {
                string admins = null;
                foreach (string admin__ in config.botAdmins)
                {
                    admins += "\n\t" + admin__;
                }
                Logger.Info("Running with admins: " + admins);
            }
            //Parse help

            if (File.Exists("help.txt"))
            {
                try
                {
                    helpText = File.ReadAllText("help.txt");
                }
                catch (Exception e) { Logger.Error("Could not open help.txt", e); }
            }
            else
            {
                try
                {
                    File.WriteAllText("help.txt", "");
                    helpText = "";
                }
                catch (Exception e) { Logger.Error("Could not create help.txt", e); }
            }

            //Task Task_ConsoleInput = new Task(ConsoleInput);
            //Main loop
            while (true)
            {
                //Try to log in
                while (client.LoginState == LoginState.LoggedOut)
                {
                    try
                    {
                        Console.WriteLine("User bot? [y/*]");
                        var key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Y)
                        {
                            client.LoginAsync(TokenType.User, token).GetAwaiter();
                        }
                        else
                        {
                            client.LoginAsync(TokenType.Bot, token).GetAwaiter();
                        }
                        StartAsync().GetAwaiter();
                        ConsoleInput();
                    }
                    catch (Exception)
                    {
                        Logger.Warning("Something went wrong! Wrong token?");
                        token = QueryToken().GetAwaiter().GetResult();
                    }
                }
                Task.Delay(1000).Wait();
                //Console.ReadKey();
            }
        }

        static public async Task StartAsync()
        {
            client.Ready += ReadyAsync;
            client.LoggedIn += LoggedInAsync;
            client.LoggedOut += LoggedOutAsync;
            
            
            client.StartAsync();
        }

        static public async Task ReadyAsync()
        {
            Logger.Info("Ready");
            string guilds__ = null;
            foreach (var a in client.Guilds)
            {
                guilds__ += "\n\t" + a.Name + " (" + a.Id + ") ";
                mainHandler.guildHandles.Add(a.Id, new GuildHandler.GuildHandle(a));
            }
            client.UserJoined += mainHandler.UserJoined;
            client.UserLeft += mainHandler.UserLeft;
            client.MessageReceived += mainHandler.MessageRecieved;
            client.ReactionAdded += mainHandler.ReactionAdded;
            client.ReactionRemoved += mainHandler.ReactionRemoved;
            
            Logger.Info("Current guilds: " + guilds__);

            await YoutubeAuth();
        }
        
        static public async Task LoggedOutAsync()
        {
            Logger.Info("Logged out");
        }

        static public async Task LoggedInAsync()
        {
            Logger.Info("Logged in");
        }
        
        static public async Task ExecTerminalCommand(string input)
        {
            var splitinput = input.Split(' ');
            switch (splitinput[0].ToLower())
            {
                case "logout":
                    await client.LogoutAsync();
                    client = new DiscordSocketClient();
                    Console.Clear();
                    token = await QueryToken();
                    break;
                case "exit":
                    await client.StopAsync();
                    Environment.Exit(0);
                    break;
                case "vcconnect":
                    if (splitinput.Length > 2)
                    {
                        try
                        {
                            ulong gid = Convert.ToUInt64(splitinput[1]);
                            ulong vid = Convert.ToUInt64(splitinput[2]);
                            client.GetGuild(gid).GetVoiceChannel(vid).ConnectAsync();
                        } catch (Exception)
                        {

                        }
                    }
                    else
                    {
                        Logger.Info("vcconnect <guild id> <vc id>");
                    }
                    break;
                case "help":
                    Logger.Info("Help\n\tlogout: log out\n\texit: exit\n\tvcconnect: vc connect");
                    break;
                case "reparse":
                    if (File.Exists("help.txt"))
                    {
                        try
                        {
                            helpText = File.ReadAllText("help.txt");
                        }
                        catch (Exception e) { Logger.Error("Could not open help.txt", e); }
                    }
                    else
                    {
                        try
                        {
                            File.WriteAllText("help.txt", "");
                            helpText = "";
                        } catch(Exception e) { Logger.Error("Could not create help.txt", e); }
                    }
                    if (mainHandler != null && mainHandler.guildHandles != null)
                    {
                        foreach (var handle in mainHandler.guildHandles)
                        {
                            handle.Value.database.ParseConfig();
                        }
                    }
                    break;
                case "save":
                    if (mainHandler != null && mainHandler.guildHandles != null)
                    {
                        foreach (var handle in mainHandler.guildHandles)
                        {
                            handle.Value.database.SaveConfig();
                        }
                    }
                    break;
            }
        }



        static public async void ConsoleInput()
        {
            int lastCursorTop = 0;
            int lastCursorLeft = 0;
            string buffer = "";

            //Read ConsoleInput while we haven't been disposed
            while (client.LoginState == LoginState.LoggedIn || client.LoginState == LoginState.LoggingIn)
            {
                //clear the top
                lastCursorTop = Console.CursorTop;
                lastCursorLeft = Console.CursorLeft;
                Console.SetCursorPosition(0, Console.WindowTop);
                for (int i = 0; i < Console.WindowWidth; i++)
                { Console.Write(' '); }
                Console.SetCursorPosition(0, Console.WindowTop);
                Console.ForegroundColor = ConsoleColor.Green;
                if (buffer.Length > Console.WindowWidth - 1)
                {
                    Console.Write(buffer.Substring(0, Console.WindowWidth - 1));
                }
                else
                {
                    Console.Write(buffer);
                }
                Console.SetCursorPosition(lastCursorLeft, lastCursorTop);
                Console.ForegroundColor = Logger.defaultcolor;

                var key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        lastCursorTop = Console.CursorTop;
                        lastCursorLeft = Console.CursorLeft;
                        Console.SetCursorPosition(0, Console.WindowTop);
                        if (buffer.Length > 0)
                        {
                            ExecTerminalCommand(buffer);
                        }
                        buffer = "";
                        for (int i = 0; i < Console.WindowWidth; i++)
                        { Console.Write(' '); }
                        Console.SetCursorPosition(lastCursorLeft, lastCursorTop);
                        continue;
                    case ConsoleKey.Backspace:
                        if (buffer.Length > 0)
                        {
                            buffer = buffer.Substring(0, buffer.Length - 1);
                            lastCursorTop = Console.CursorTop;
                            lastCursorLeft = Console.CursorLeft;
                            Console.SetCursorPosition(0, Console.WindowTop);
                            for (int i = 0; i < Console.WindowWidth; i++)
                            { Console.Write(' '); }
                            Console.SetCursorPosition(0, Console.WindowTop);
                            Console.ForegroundColor = ConsoleColor.Green;
                            if (buffer.Length > Console.WindowWidth - 1)
                            {
                                Console.Write(buffer.Substring(0, Console.WindowWidth - 1));
                            }
                            else
                            {
                                Console.Write(buffer);
                            }
                            Console.SetCursorPosition(lastCursorLeft, lastCursorTop);
                            Console.ForegroundColor = Logger.defaultcolor;
                        }
                        continue;
                    case ConsoleKey.Escape:
                        continue;
                    default:
                        try
                        {
                            char ch = key.KeyChar;
                            buffer = buffer + ch;
                            lastCursorTop = Console.CursorTop;
                            lastCursorLeft = Console.CursorLeft;
                            Console.SetCursorPosition(0, Console.WindowTop);
                            Console.ForegroundColor = ConsoleColor.Green;
                            if (buffer.Length > Console.WindowWidth - 1)
                            {
                                Console.Write(buffer.Substring(0, Console.WindowWidth - 1));
                            }
                            else
                            {
                                Console.Write(buffer);
                            }
                            Console.SetCursorPosition(lastCursorLeft, lastCursorTop);
                            Console.ForegroundColor = Logger.defaultcolor;
                        }
                        catch (Exception)
                        {

                        }
                        continue;
                }
            }
        }


        static public Task<string> QueryToken()
        {
            string chosenVar = null;
            Console.WriteLine("Which environment variable contains your token?");

            foreach (DictionaryEntry a in Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User))
            {
                Console.WriteLine("{0}: {1}", a.Key, a.Value);
            }

            while (chosenVar == null)
            {
                chosenVar = Console.ReadLine();

                if (Environment.GetEnvironmentVariable(chosenVar) != null)
                {
                    token = Environment.GetEnvironmentVariable(chosenVar);
                    Console.WriteLine("Token: {0}", token);
                }
                else
                {
                    Console.WriteLine("Non-existant variable");
                    chosenVar = null;
                }
            }
            return Task.Run(() => string.Copy(token));
        }

        static private async Task YoutubeAuth()
        {
            Logger.Info("Logging into the YouTube API");
            /*
            UserCredential credential;
            using (var stream = new FileStream("client_secrets.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(YoutubeAuth().GetType().ToString())
                );
            }
            Logger.Info("Creating the YouTube Service");
            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = YoutubeAuth().GetType().ToString()
            });
            */
            youtubeService = new YouTubeService(new BaseClientService.Initializer() { ApiKey = "AIzaSyDaWrh5nKLEt5ZZa68Q8sodZJzJJN9cemc", ApplicationName = "My Project 40576" });

            Console.WriteLine("[SECRET] YouTube Service created: " + youtubeService.ApiKey);
        }
        }
    }
