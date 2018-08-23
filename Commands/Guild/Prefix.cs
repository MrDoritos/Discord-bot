using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace MusicBot9001.Commands.Guild
{
    class Prefix
    {
        public static Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Title = "Prefix Help";
            embed.Description = "```http\n" +
                prefix + "Prefix (New Prefix) : Sets a new prefix (Spaces allowed)\n" +
                "Prefix : View the prefix\n" +
                "```";
            embed.Color = Color.Blue;
            return embed.Build();
        }

        public static async Task Prefix_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { await message.Channel.SendMessageAsync("This isn't a guild!"); return; }

            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            var badchars = new char[] { ' ', ':', '\'', '"', '\n', '\r', '`' };

            if (args.Length > 0)
            {
                if (args.Any(n => badchars.Any(n.Equals)))
                {
                    await message.Channel.SendMessageAsync("There are some unrecommended characters, if the bot no longer is able to take commands, pass `$reset` to reset the prefix");
                    guildHandle.database.prefix = args;
                    guildHandle.helpEmbeds = GuildHandler.GuildHandle.BuildEmbed(guildHandle.database.prefix);
                    await message.Channel.SendMessageAsync("Prefix set to `" + guildHandle.database.prefix + "`");
                }
                else
                {
                    guildHandle.database.prefix = args;
                    guildHandle.helpEmbeds = GuildHandler.GuildHandle.BuildEmbed(guildHandle.database.prefix);
                    await message.Channel.SendMessageAsync("Prefix set to `" + guildHandle.database.prefix + "`");

                }
            }
            else
            {
                await message.Channel.SendMessageAsync("My prefix on this guild is `" + guildHandle.database.prefix + "`");
            }
        }
    }
}
