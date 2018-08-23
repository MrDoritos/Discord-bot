using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace MusicBot9001.Commands.Guild
{
    class LeaveMessage
    {
        static public Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Title = "LeaveMessage Help";
            embed.Description = "```http" + "\n" +
                prefix + "LeaveMessage Enable : Enable leave messages" + "\n" +
                prefix + "LeaveMessage Disable : Disable leave messages" + "\n" +
                prefix + "LeaveMessage View : View current config\n" +
                prefix + "LeaveMessage Channel (Channel Mention) : Set Welcome/Leave Channel\n" +
                prefix + "LeaveMessage (Message) : Set leave message\n" +
                "```\n" +
                "Current message variables\n" +
                "$(user.Name) : Old user's name\n" +
                "$(user.Id) : Old user's Id\n" +
                "$(guild.Name) : Server's name\n" +
                "$(guild.Id) : Server's Id";
            embed.Color = Color.Blue;
            return embed.Build();
        }

        static public async Task LeaveMessage_(string args, IMessage message)
        {
            //We dont want any indirect nullrefs
            if (!(message.Channel is IGuildChannel))
            { return; }


            var guildChannel = (message.Channel as IGuildChannel);
            var guild = guildChannel.Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];

            string channelName = "undefined";
            try
            {
                channelName = (await guild.GetChannelAsync(guildHandle.database.welcomeLeaveChannel)).Name;
            }
            catch (Exception) { }



            if (args.Length < 1)
            { await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "Leave Message", Description = "Enabled: `" + guildHandle.database.sendLeaveMessage.ToString() + "`\nCurrent Message:\n`" + guildHandle.database.leaveMessage + "`\nChannel: `" + channelName + "`", Color = Color.Blue }.Build()); return; }

            switch (args.Split(' ')[0].ToLower())
            {
                case "enable":
                    guildHandle.database.sendLeaveMessage = true;
                    await message.Channel.SendMessageAsync("Enabled leave messages");
                    return;
                case "disable":
                    guildHandle.database.sendLeaveMessage = false;
                    await message.Channel.SendMessageAsync("Disabled leave messages");
                    return;
                case "view":
                    await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "Leave Message", Description = "Enabled: `" + guildHandle.database.sendLeaveMessage.ToString() + "`\nCurrent Message:\n`" + guildHandle.database.leaveMessage + "`\nChannel: `" + channelName + "`", Color = Color.Blue }.Build());
                    return;
                case "channel":
                    if (message.MentionedChannelIds != null && message.MentionedChannelIds.Count > 0)
                    {
                        guildHandle.database.welcomeLeaveChannel = (await guild.GetChannelAsync(message.MentionedChannelIds.First())).Id;
                        await message.Channel.SendMessageAsync("Welcome/Leave channel set to `" + (await guild.GetChannelAsync(message.MentionedChannelIds.First())).Name + "`");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Mention a channel");
                    }
                    return;
                default:
                    guildHandle.database.leaveMessage = args;
                    await message.Channel.SendMessageAsync("Leave message set to `" + args + "`");
                    return;
            }
        }
    }
}
