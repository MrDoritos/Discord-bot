using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Net;

namespace MusicBot9001.Commands.User
{
    class Avatar
    {
        public static Embed Help(string prefix)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder();
            embedBuilder.Title = "Avatar Help";
            embedBuilder.Color = Color.Blue;
            embedBuilder.Description = "```http\n" +
                prefix + "Avatar (User Mention | User Id) : Get the avatar of a user\n" +
                "```";
            return embedBuilder.Build();
        }

        public static async Task Avatar_(string args, IMessage message)
        {
            string prefix = "$";
            IUser usertoget;
            if (message.Channel is IGuildChannel)
            {
                prefix = Program.mainHandler.guildHandles[(message.Channel as IGuildChannel).GuildId].database.prefix;
            }

            ulong useridtoget = 0;
            if (message.MentionedUserIds != null && message.MentionedUserIds.Count > 0)
            {
                useridtoget = message.MentionedUserIds.First();
            }
            else
            {
                if (args != null && args.Length > 0)
                {
                    ulong.TryParse(args.Split(' ')[0].Trim(), out useridtoget);
                }
            }
            if (useridtoget == 0)
            {
                await message.Channel.SendMessageAsync(null, false, Help(prefix));
                return;
            }
            else
            {
                try
                {
                    usertoget = Program.client.GetUser(useridtoget);
                } catch(Exception e)
                {
                    Logger.Error("Retrieving avatar for " + useridtoget, e);
                    await message.Channel.SendMessageAsync(null, false, Help(prefix));
                    return;
                }
            }
            if (usertoget != null)
            {
                string url = usertoget.GetAvatarUrl(ImageFormat.Auto, 1024);
                using (MemoryStream bytestream = new MemoryStream(new WebClient().DownloadData(url)))                    
                await message.Channel.SendFileAsync(bytestream, url.Split('/', '?').Where(n => n.Contains(".")).Last());
            }
        }
    }
}
