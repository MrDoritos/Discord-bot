using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Linq;

namespace MusicBot9001.Commands.Music
{
    class NowPlaying
    {
        static public async Task NowPlaying_(string args, IMessage message)
        {
            //We dont want any indirect nullrefs
            if (!(message.Channel is IGuildChannel))
            { return; }
            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            var musicHandle = guildHandle.musicHandle;

            if (musicHandle.NowPlaying != null)
            {
                var embed = new EmbedBuilder();
                embed.Title = "Now Playing";
                embed.Fields.Add(new EmbedFieldBuilder() { IsInline = false, Name = "Title", Value = musicHandle.NowPlaying.title });
                embed.Fields.Add(new EmbedFieldBuilder() { IsInline = true, Name = "Requested By", Value = musicHandle.NowPlaying.requester.Username });
                embed.Fields.Add(new EmbedFieldBuilder() { IsInline = true, Name = "Time", Value = new DateTime(musicHandle._currentFFMpegProc.CurrentTime.Ticks).ToString("mm:ss") });
                embed.Fields.Add(new EmbedFieldBuilder() { IsInline = true, Name = "Length", Value = new DateTime(musicHandle._currentFFMpegProc.mediaFoundationReader.TotalTime.Ticks).ToString("mm:ss") });
                embed.Fields.Add(new EmbedFieldBuilder() { IsInline = false, Name = "Url", Value = musicHandle.NowPlaying.url });
                embed.Description = "`" + GetProgBar(musicHandle._currentFFMpegProc.CurrentTime.Ticks, musicHandle._currentFFMpegProc.mediaFoundationReader.TotalTime.Ticks) + "`";
                embed.Color = Color.Blue;
                await message.Channel.SendMessageAsync(null, false, embed.Build());
            }
            else
            {
                await message.Channel.SendMessageAsync(null, false, new EmbedBuilder() { Title = "Now Playing", Description = "Nothing is playing!", Color = Color.Blue }.Build());
            }
        }

        static public string GetProgBar(long per, long tot)
        {
            if (per < tot && per > 0)
            {
                var toreturn = new char[50];
                for (int i = 0; i < toreturn.Length; i++)
                {
                    toreturn[i] = '/';
                }
                var chunk = (50.0f / tot);
                for (int i = 0; i < chunk * per; i++)
                {
                    toreturn[i] = '#';
                }
                string tore = "";
                foreach (char a in toreturn)
                {
                    tore += a;
                }
                return tore;
            } else if (per < 0)
            {
                return "//////////////////////////////////////////////////";
            } else if (per < tot)
            {
                return "##################################################";
            }
            return "//////////////////////////////////////////////////";
        }
    }
}
