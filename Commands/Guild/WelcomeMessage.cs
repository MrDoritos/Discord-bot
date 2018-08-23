using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace MusicBot9001.Commands.Guild
{
    class WelcomeMessage
    {
        static public Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Title = "WelcomeMessage Help";
            embed.Description = "```http" + "\n" +
                prefix + "WelcomeMessage Enable : Enable welcome messages" + "\n" +
                prefix + "WelcomeMessage Disable : Disable welcome messages" + "\n" +
                prefix + "WelcomeMessage View : View current config\n" +
                prefix + "WelcomeMessage Channel (Channel Mention) : Set Welcome/Leave Channel\n" +
                prefix + "WelcomeMessage (Message) : Set welcome message\n" +
                "```\n" +
                "Current message variables\n" +
                "$(user.Name) : New user's name\n" +
                "$(user.Id) : New user's Id\n" +
                "$(guild.Name) : Server's name\n" +
                "$(guild.Id) : Server's Id";
            embed.Color = Color.Blue;
            return embed.Build();
        }

        static public async Task WelcomeMessage_(string args, IMessage message)
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
            { await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "Welcome Message", Description = "Enabled: `" + guildHandle.database.sendWelcomeMessage.ToString() + "`\nCurrent Message:\n`" + guildHandle.database.welcomeMessage + "`\nChannel: `" + channelName + "`", Color = Color.Blue}.Build()); return; }

            switch (args.Split(' ')[0].ToLower())
            {
                case "enable":
                    guildHandle.database.sendWelcomeMessage = true;
                    await message.Channel.SendMessageAsync("Enabled welcome messages");
                    return;
                case "disable":
                    guildHandle.database.sendWelcomeMessage = false;
                    await message.Channel.SendMessageAsync("Disabled welcome messages");
                    return;
                case "view":
                    await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "Welcome Message", Description = "Enabled: `" + guildHandle.database.sendWelcomeMessage.ToString() + "`\nCurrent Message:\n`" + guildHandle.database.welcomeMessage + "`\nChannel: `" + channelName + "`", Color = Color.Blue }.Build());
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
                    guildHandle.database.welcomeMessage = args;
                    await message.Channel.SendMessageAsync("Welcome message set to `" + args + "`");
                    return;
            }
        }
    }
}
