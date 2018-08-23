using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace MusicBot9001
{
    class CommandHandler
    {
        /*
        public static bool HasPermission(string CommandTree, IGuildUser user)
        {
            string[] permissions = Program.mainHandler.guildHandles[user.GuildId].permissionsHandler.getPerms(user).perms;
            string[] permissionsExclusion = Program.mainHandler.guildHandles[user.GuildId].permissionsHandler.getPerms(user).permsExclude;
            string command = CommandTree.Split('\'').Last();
            string commandFamily = CommandTree.Split('\'')[0];
            if (Program.config.botAdmins.Any(user.Id.ToString().Equals))
            { return true; }

            //if (permissions.Any(command.Equals))
            //{
            //    return true;
            //}

            //Return true if it is the guild owner, but not if they sent a botadmin command
            if (user.Guild.OwnerId == user.Id)
            {
                if (commandFamily == "Commands.BotAdmin")
                {
                    if (permissions.Any(commandFamily.Equals))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            if (permissions.Any(commandFamily.Equals) && !permissionsExclusion.Any(CommandTree.Equals))
            {
                return true;
            }
            return false;
        }
        */
        public static async Task HandleCommand(SocketMessage message)
        {
            string content = null;
            string args = null;
            if (message.Channel is IGuildChannel)
            {
                content = message.Content.Remove(0, Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].database.prefix.Length);
                args = content.Remove(0, content.Split(' ')[0].Length).Trim();
            }
            else
            {
                content = message.Content.Remove(0, 1);
                args = content.Remove(0, content.Split(' ')[0].Length).Trim();
            }
            string[] splitmessage = content.Split(' ');
            IGuildChannel guildChannel = (message.Channel as IGuildChannel);
            IGuildUser guildUser = (message.Author as IGuildUser);
            
            //Guild only commands
            if (message.Channel is IGuildChannel)
            {
                switch (splitmessage[0].ToLower())
                {
                    case "prefix":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'Prefix", message.Author as IGuildUser))
                        {
                            await Commands.Guild.Prefix.Prefix_(args, message);
                        }
                        return;
                    case "welcomemessage":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'WelcomeMessage", message.Author as IGuildUser))
                        {
                            await Commands.Guild.WelcomeMessage.WelcomeMessage_(args, message);
                        }
                        return;
                    case "leavemessage":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'WelcomeMessage", message.Author as IGuildUser))
                        {
                            await Commands.Guild.LeaveMessage.LeaveMessage_(args, message);
                        }
                        return;
                    case "whoami":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.User'WhoAmI", message.Author as IGuildUser))
                        {
                            await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "WhoAmI?", Description = $"You are {message.Author.Username}" }.Build());
                        }
                        return;
                    case "testing":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.BotAdmin'Testing", message.Author as IGuildUser))
                        {
                            await message.Channel.SendMessageAsync("perms");
                        }
                        return;
                    case "permissions":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'Permissions", message.Author as IGuildUser))
                        {
                            await message.Channel.SendMessageAsync(Commands.Guild.Permissions.Permissions_(args, message));
                        }
                        return;
                    case "avatar":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.User'Avatar", message.Author as IGuildUser))
                        {
                            await Commands.User.Avatar.Avatar_(args, message);
                        }
                        return;
                    case "defaultrole":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'DefaultRole", message.Author as IGuildUser))
                        {
                            await Commands.Guild.DefaultRole.DefaultRole_(args, message);
                        }
                        return;
                    case "level":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'Level", message.Author as IGuildUser))
                        {
                            await Commands.Guild.Level.Level_(args, message);
                        }
                        return;
                    case "privatevc":
                        if (Program.mainHandler.guildHandles[guildChannel.GuildId].permissionsHandler.HasPermission("Commands.Guild'PrivateVC", message.Author as IGuildUser))
                        {
                            await Commands.Guild.PrivateVC.PrivateVC_(args, message);
                        }
                        return;


                    //Remove these
                    case "purge":
                        await Commands.Guild.Purge.Purge_(args, message);
                        return;
                    case "loop":
                        await Commands.Music.Loop.Loop_(args, message);
                        return;
                    case "nowplaying": case "np":
                        await Commands.Music.NowPlaying.NowPlaying_(null, message);
                        return;
                    case "dequeue":
                        var musichandle = Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle;
                        if ((message.MentionedRoles != null && message.MentionedRoles.Count > 0) || (message.MentionedUsers != null && message.MentionedUsers.Count > 0))
                        {
                            if (message.MentionedRoles != null && message.MentionedRoles.Count > 0)
                            {
                                await message.Channel.SendMessageAsync(musichandle.ClearQueue(musichandle.guild.Roles.Where(n => message.MentionedRoles.Any(m => m.Id == n.Id))));
                            }
                            if (message.MentionedUsers != null && message.MentionedUsers.Count > 0)
                            {
                                var users = await musichandle.guild.GetUsersAsync();
                                await message.Channel.SendMessageAsync(musichandle.ClearQueue(users.Where(n => message.MentionedUsers.Any(m => m.Id == n.Id))));
                            }
                        }
                        else
                        if (args.Length > 0)
                        {
                            try
                            {
                                await message.Channel.SendMessageAsync(musichandle.ClearQueue(Convert.ToInt16(args)));
                            }
                            catch (Exception)
                            {
                                await message.Channel.SendMessageAsync(musichandle.ClearQueue(GuildHandler.Music.Search.SearchYTGetSong(args)));
                            }
                        }
                        return;
                    case "ping":
                        await message.Channel.SendMessageAsync("pong!");
                        return;
                    case "pause":
                        await message.Channel.SendMessageAsync(Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Pause());
                        return;
                    case "unpause":
                        await message.Channel.SendMessageAsync(Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Pause(true));
                        return;
                    case "play":
                        if (splitmessage.Length > 1)
                        {
                            //var song = await GuildHandler.Music.Download.GetSongAsync(splitmessage[1]);
                            if (!GuildHandler.Music.Download.playList.IsMatch(splitmessage[1]) && GuildHandler.Music.Download.getid.IsMatch(splitmessage[1]))
                            {
                                var song = new GuildHandler.Music.ISong().WithURL(splitmessage[1]);
                                song.requester = message.Author as IGuildUser;
                                if (song.FileExists())
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                    {
                                        await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                    else
                                    {
                                        if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                        {
                                            await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                            await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                        }
                                        else
                                        {
                                            await message.Channel.SendMessageAsync("Downloading `" + song.title + "`");
                                            await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                        }
                                    }
                                }
                                else
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                    {
                                        if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.songs.Count > 0)
                                        {
                                            await message.Channel.SendMessageAsync("Added `" + song.url + "` to the queue");
                                        }
                                        else
                                        {
                                            await message.Channel.SendMessageAsync("Downloading `" + song.url + "`");
                                        }
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                    else
                                    {
                                        if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                        {
                                            await message.Channel.SendMessageAsync("Added `" + song.url + "` to the queue");
                                            await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                        }
                                        else
                                        {
                                            await message.Channel.SendMessageAsync("Downloading `" + song.url + "`");
                                            await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                        }
                                    }
                                }
                            }
                            else if (!GuildHandler.Music.Download.getid.IsMatch(splitmessage[1]))
                            {
                                var song = GuildHandler.Music.Search.SearchYTGetSong(message.Content.Substring(splitmessage[0].Length, message.Content.Length - splitmessage[0].Length));
                                song.requester = message.Author as IGuildUser;
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                {
                                    await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                }
                                else
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                    {
                                        await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                    }
                                    else
                                    {
                                        await message.Channel.SendMessageAsync("Now playing `" + song.title + "`");
                                    }
                                    await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                }
                            }
                            else if (GuildHandler.Music.Download.playList.IsMatch(splitmessage[1]))
                            {
                                var songs = GuildHandler.Music.Download.Playlist(splitmessage[1]);
                                songs.ForEach(n => n.requester = message.Author as IGuildUser);
                                await message.Channel.SendMessageAsync("Added playlist");
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                {
                                    //Add to queue
                                    //foreach (var song in songs)
                                    //{
                                    //    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    //}
                                    Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                                }
                                else
                                {
                                    await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                    {
                                        //Queue contains items, but nobody is in vc
                                        Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                                    }
                                    else
                                    {
                                        //Nothing in the queue, play first song
                                        Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                                    }
                                }
                            }
                        }
                        else if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.songs.Count > 0)
                        {
                            await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                        }
                        return;
                    case "connect":
                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                        return;
                    case "queue":
                        await message.Channel.SendMessageAsync(null, false, Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.GetQueueEmbed());
                        return;
                    case "stop":
                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Disconnect();
                        await message.Channel.SendMessageAsync("Audio stopped");
                        return;
                    case "skip":
                        if (splitmessage.Count() > 1)
                            try
                            {

                                int num = Convert.ToInt32(splitmessage[1]);
                                await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Skip(num);
                            }
                            catch (Exception) { }
                        else
                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Skip(1);
                        return;
                
                
            }
                
            }

            //User Commands
            switch (splitmessage[0].ToLower())
            {
                case "help":
                    await Commands.User.Help.Help_(args, message);
                    return;
            }
            
        }

        public static async Task HandleCommand_OLD(SocketMessage message)
        {
            var Iguild = (message.Author as IGuildUser)?.Guild;
            string[] splitmessage;

            if (Iguild != null)
            {
                splitmessage = message.Content.Remove(0, Program.mainHandler.guildHandles[Iguild.Id].database.prefix.Length).Split(' ');
            }
            else
            {
                splitmessage = message.Content.TrimStart('$').Split(' ');
            }

            //Any user
            switch (splitmessage[0])
            {
                case "ping":
                    await message.Channel.SendMessageAsync("pong!");
                    return;
                case "play":
                    if (splitmessage.Length > 1)
                    {
                        //var song = await GuildHandler.Music.Download.GetSongAsync(splitmessage[1]);
                        if (!GuildHandler.Music.Download.playList.IsMatch(splitmessage[1]) && GuildHandler.Music.Download.getid.IsMatch(splitmessage[1]))
                        {
                            var song = new GuildHandler.Music.ISong().WithURL(splitmessage[1]);
                            if (song.FileExists())
                            {
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                {
                                    await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                }
                                else
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                    {
                                        await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                    else
                                    {
                                        await message.Channel.SendMessageAsync("Downloading `" + song.title + "`");
                                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                }
                            }
                            else
                            {
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.songs.Count > 0)
                                    {
                                        await message.Channel.SendMessageAsync("Added `" + song.url + "` to the queue");
                                    }
                                    else
                                    {
                                        await message.Channel.SendMessageAsync("Downloading `" + song.url + "`");
                                    }
                                    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                }
                                else
                                {
                                    if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                    {
                                        await message.Channel.SendMessageAsync("Added `" + song.url + "` to the queue");
                                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                    else
                                    {
                                        await message.Channel.SendMessageAsync("Downloading `" + song.url + "`");
                                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                    }
                                }
                            }
                        }
                        else if (!GuildHandler.Music.Download.getid.IsMatch(splitmessage[1]))
                        {
                            var song = GuildHandler.Music.Search.SearchYTGetSong(message.Content.Substring(splitmessage[0].Length, message.Content.Length - splitmessage[0].Length));

                            if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                            {
                                await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                            }
                            else
                            {
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                {
                                    await message.Channel.SendMessageAsync("Added `" + song.title + "` to the queue");
                                }
                                else
                                {
                                    await message.Channel.SendMessageAsync("Now playing `" + song.title + "`");
                                }
                                await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                            }
                        } else if (GuildHandler.Music.Download.playList.IsMatch(splitmessage[1]))
                        {
                            var songs = GuildHandler.Music.Download.Playlist(splitmessage[1]);
                            await message.Channel.SendMessageAsync("Added playlist");
                            if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.audioClient != null)
                            {
                                //Add to queue
                                //foreach (var song in songs)
                                //{
                                //    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.AddSong(song);
                                //}
                                Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                                if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.NowPlaying != null)
                                {
                                    //Queue contains items, but nobody is in vc
                                        Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                                }
                                else
                                {
                                    //Nothing in the queue, play first song
                                    Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.AddMany(songs);
                                }
                            }
                        }
                    } else if (Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.queue.songs.Count > 0)
                    {
                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));
                    }
                    return;
                case "connect":
                        await message.Channel.SendMessageAsync(await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Connect((message.Author as IGuildUser)));                    
                    return;
                case "queue":
                    await message.Channel.SendMessageAsync(null, false, Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.GetQueueEmbed());
                    return;
                case "stop":
                    await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Disconnect();
                    return;
                case "skip":
                    if (splitmessage.Count() > 1)
                        try
                        {

                            int num = Convert.ToInt32(splitmessage[1]);
                            await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Skip(num);
                        }
                        catch (Exception) { }
                    else
                        await Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].musicHandle.Skip(1);
                    return;
            }

            //Bot admin
            if (Program.config.botAdmins.Any(message.Author.Id.ToString().Contains))
            {
                switch (splitmessage[0])
                {
                    case "reparse":
                        foreach (var ghandle in Program.mainHandler.guildHandles)
                        {
                            ghandle.Value.database.ParseConfig();
                        }
                        await message.Channel.SendMessageAsync("Reparsed all");
                        return;
                    case "relay":
                        if (TCPChatRelay.open)
                        {
                            await message.Channel.SendMessageAsync("Relay is already open!");
                        }
                        else
                        {
                            Task.Run(() => TCPChatRelay.Relay(message.Channel as ITextChannel));
                            await message.Channel.SendMessageAsync("Relay is now open");
                        }
                        return;
                }
            }
        }
    }
}
