using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace MusicBot9001
{
    class MainHandler
    {
        public IDictionary<ulong, GuildHandler.GuildHandle> guildHandles = new Dictionary<ulong, GuildHandler.GuildHandle>();
        public static Random random = new Random();

        public MainHandler()
        {
            Task.Run(autosaveThread);
        }
        
        public async Task GuildAdded(SocketGuild guild)
        {
            if (!guildHandles.ContainsKey(guild.Id))
            {
                guildHandles.Add(guild.Id, new GuildHandler.GuildHandle(guild));
            }
        }

        public async Task GuildRemoved(SocketGuild guild)
        {
            if (guildHandles.ContainsKey(guild.Id))
            {
                guildHandles.Remove(guild.Id);
            }
        }

        public async Task MessageRecieved(SocketMessage message)
        {
            Logger.Chat(message);
            //if (TCPChatRelay.open && message.Author.Id != Program.client.CurrentUser.Id && message.Author.Id != 477874907758460932)
            //{
            //    TCPChatRelay.SendMessage(message);
            //}
            var guild = (message.Author as IGuildUser)?.Guild;
            if (guild == null)
            {
                if (message.Content.StartsWith('$'))
                {
                    await Task.Run(() => CommandHandler.HandleCommand(message));
                }
            }
            else
            {
                int num = random.Next(0, 2);
                if (message.Author.Id != Program.client.CurrentUser.Id &&  num == 1)
                {
                    guildHandles[guild.Id].levelHandler.MessageSent(message.Author.Id, message.Channel.Id);
                }
                if (message.Content.StartsWith(guildHandles[guild.Id].database.prefix))
                {
                    await Task.Run(() => CommandHandler.HandleCommand(message));
                }
            }

            //non-commands & non-responses
            if (guild == null)
            {
                if (message.Content == "prefix")
                {
                    await message.Channel.SendMessageAsync("My prefix is `$`");
                }
            }
            else
            {
                if (message.Content == "prefix")
                {
                    await message.Channel.SendMessageAsync("My prefix on this guild is `" + guildHandles[guild.Id].database.prefix +"`");
                }
            }

            //reponses
        }

        public async Task ReactionAdded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            if (messageChannel is IGuildChannel)
            {
                if (guildHandles.ContainsKey((messageChannel as IGuildChannel).GuildId))
                {
                    await guildHandles[(messageChannel as IGuildChannel).GuildId].ReactionAddded(cacheable, messageChannel, reaction);
                }
            }
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {
            if (messageChannel is IGuildChannel)
            {
                if (guildHandles.ContainsKey((messageChannel as IGuildChannel).GuildId))
                {
                    await guildHandles[(messageChannel as IGuildChannel).GuildId].ReactionRemoved(cacheable, messageChannel, reaction);
                }
            }
        }

        public async Task UserJoined(SocketUser user)
        {
            if ((user as IGuildUser)?.Guild != null)
            {
                if (guildHandles.ContainsKey((user as IGuildUser).GuildId))
                {
                    guildHandles[(user as IGuildUser).GuildId].UserAdded(user);
                }
            }
        }

        public async Task UserLeft(SocketUser user)
        {
            if ((user as IGuildUser)?.Guild != null)
            {
                if (guildHandles.ContainsKey((user as IGuildUser).GuildId))
                {
                    guildHandles[(user as IGuildUser).GuildId].UserRemoved(user);
                }
            }
        }

        public async Task autosaveThread()
        {

            await Task.Delay(20000);
            while (true)
            {
                if (guildHandles != null)
                {
                    Logger.Info("Autosaving databases...");
                    foreach (var handle in guildHandles.Values)
                    {
                        try
                        {
                            handle.database.SaveConfig();
                            handle.database.queue = handle.musicHandle.queue;
                            Logger.Info("Database saved for " + handle.guild.Name + " (" + handle.guild.Id + ")");
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Could not save database for " + handle.guild.Name + " (" + handle.guild.Id + ")", e);
                        }
                    }
                }

                await Task.Delay(500000);
            }
        }
    }
}
