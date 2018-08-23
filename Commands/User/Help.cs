using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace MusicBot9001.Commands.User
{
    class Help
    {
        public static async Task Help_(string args, IMessage message)
        {
            string prefix = "$";
            if (message.Channel is IGuildChannel)
            {
                prefix = Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].database.prefix; 
            }
            if (args.Length > 0 && args.Split(' ').Length > 0)
            {
                switch (args.Split(' ')[0].ToLower())
                {
                    case "welcomemessage":
                        await message.Channel.SendMessageAsync(null, false, Commands.Guild.WelcomeMessage.Help(prefix));
                        return;
                    case "leavemessage":
                        await message.Channel.SendMessageAsync(null, false, Commands.Guild.LeaveMessage.Help(prefix));
                        return;
                    case "permissions":
                        return;
                    case "prefix":
                        await message.Channel.SendMessageAsync(null, false, Commands.Guild.Prefix.Help(prefix));
                        return;
                    case "avatar":
                        await message.Channel.SendMessageAsync(null, false, Commands.User.Avatar.Help(prefix));
                        return;
                    case "defaultrole":
                        await message.Channel.SendMessageAsync(null, false, Commands.Guild.DefaultRole.Help(prefix));
                        return;
                }
            }
            else
            {
                if (!(message.Channel is IGuildChannel))
                {
                    await GetGlobalHelp(args, message);
                    return;
                }
                var guild = (message.Channel as IGuildChannel).Guild;
                var guildHandle = Program.mainHandler.guildHandles[guild.Id];

                if (guildHandle.channelHelps.ContainsKey(message.Channel.Id))
                {
                    if (guildHandle.channelHelps[message.Channel.Id] != null)
                    {
                        guildHandle.channelHelps[message.Channel.Id].delete();
                        guildHandle.channelHelps[message.Channel.Id] = new HelpActions(guildHandle.helpEmbeds, message.Channel as ITextChannel);
                        await guildHandle.channelHelps[message.Channel.Id].SendMessage();
                        await guildHandle.channelHelps[message.Channel.Id].AddReactions();
                    }
                    else
                    {
                        guildHandle.channelHelps[message.Channel.Id] = new HelpActions(guildHandle.helpEmbeds, message.Channel as ITextChannel);
                        await guildHandle.channelHelps[message.Channel.Id].SendMessage();
                        await guildHandle.channelHelps[message.Channel.Id].AddReactions();

                    }
                }
                else
                {
                    guildHandle.channelHelps.Add(message.Channel.Id, new HelpActions(guildHandle.helpEmbeds, message.Channel as ITextChannel));
                    await guildHandle.channelHelps[message.Channel.Id].SendMessage();
                    await guildHandle.channelHelps[message.Channel.Id].AddReactions();
                }
            }
        }

        private static async Task GetGlobalHelp(string args, IMessage message)
        {

        }

    }
    class HelpActions
    {
        public ITextChannel channel;
        public IUserMessage message;
        
        Embed[] embeds;
        /// <summary>
        /// Not zero based page number
        /// </summary>
        int currentpage = 1;

        public enum Seek
        {
            last,
            next,
            back,
            first,
        }

        public void seek(Seek seek)
        {
            if (message == null)
            { return; }
           if (embeds != null)
            {
                switch (seek)
                {
                    case Seek.next:
                        //
                        if (currentpage < embeds.Length)
                        {
                            currentpage++;
                        }
                        else
                            //
                            if (currentpage > embeds.Length - 1)
                        {
                            currentpage = 1;
                        }
                        break;
                    case Seek.back:
                        if (currentpage > 1)
                        {
                            currentpage--;
                        }
                        else if (currentpage < 2)
                        {
                            currentpage = embeds.Length;
                        }
                        break;
                    case Seek.first:
                        currentpage = 1;
                        break;
                    case Seek.last:
                        currentpage = embeds.Length;
                        break;
                }
                message.ModifyAsync(n => n.Embed = embeds[currentpage - 1]);
            }
        }

        public void delete()
        {
            if (message == null)
            { return; }
            message.DeleteAsync();
        }

        public HelpActions(Embed[] embeds, ITextChannel channel, IUserMessage message = null,  int page = 1, bool sendinitial = true)
        {
            if (sendinitial == true|| message == null && channel != null && embeds.Length > 0)
            {
               
                this.message = message;
                this.embeds = embeds;
                this.channel = channel;
                currentpage = page;
                ////⏪⏩⏮⏭
                //message.AddReactionAsync(new Emoji("⏮"));
                //message.AddReactionAsync(new Emoji("⏪"));
                //message.AddReactionAsync(new Emoji("⏩"));
                //message.AddReactionAsync(new Emoji("⏭"));
            }
        }

        public async Task SendMessage()
        {
            message = await channel.SendMessageAsync(null, false, embeds[0]);
        }

        public async Task AddReactions()
        {
            try
            {
                await message.AddReactionAsync(new Emoji("⏮"));
                await message.AddReactionAsync(new Emoji("⏪"));
                await message.AddReactionAsync(new Emoji("⏩"));
                await message.AddReactionAsync(new Emoji("⏭"));
            }
            catch (Exception) { }
        }
    }
}
