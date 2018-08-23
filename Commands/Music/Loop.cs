using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace MusicBot9001.Commands.Music
{
    class Loop
    {
        public static Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Title = "Loop Help";
            embed.Description = "```http\n" +
                $"{prefix}Loop Song : Loop a single song\n" +
                $"{prefix}Loop Queue : Loop the queue\n" +
                $"{prefix}Loop None : Loop nothing\n" +
                $"```";
            embed.Color = Color.Blue;
            return embed.Build();
        }

        public static async Task Loop_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { return; }

            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            var musicHandle = guildHandle.musicHandle;

            if (args.Length < 1)
            {
                string loop = "";
                switch (musicHandle.loopType)
                {
                    case GuildHandler.Database.LoopType.NoLoop:
                        await message.Channel.SendMessageAsync("Loop: `No Loop`");
                        return;
                    case GuildHandler.Database.LoopType.QueueLoop:
                        await message.Channel.SendMessageAsync("Loop: `Queue Loop`");
                        return;
                    case GuildHandler.Database.LoopType.SingleLoop:
                        await message.Channel.SendMessageAsync("Loop: `Single Song`");
                        return;
                }
            }
            switch (args.ToLower())
            {
                case "queue":
                    await message.Channel.SendMessageAsync("Looping the queue");
                    musicHandle.loopType = GuildHandler.Database.LoopType.QueueLoop;
                    return;
                case "song":
                    await message.Channel.SendMessageAsync("Looping a single song");
                    musicHandle.loopType = GuildHandler.Database.LoopType.SingleLoop;
                    return;
                case "none":
                    await message.Channel.SendMessageAsync("No longer looping");
                    musicHandle.loopType = GuildHandler.Database.LoopType.NoLoop;
                    return;
                default:
                    await message.Channel.SendMessageAsync(null, false, Help(guildHandle.database.prefix));
                    return;
            }

        }
    }
}
