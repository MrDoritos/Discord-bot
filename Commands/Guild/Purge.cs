using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;
using System.Threading.Tasks;

namespace MusicBot9001.Commands.Guild
{
    class Purge
    {
        public static Embed Help(string prefix)
        {
            return null;
        }
        
        public static async Task Purge_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { return; }

            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            int messagestoscan = 100;
            var splitargs = args.Split(' ');

            bool purgeuser = false;
            bool purgerole = false;
            bool purgecontents = false;
            bool purgeimages = false;
            bool purgefiles = false;
            bool purgeall = false;

            foreach (var a in splitargs)
            {
                try
                {
                    messagestoscan = Convert.ToInt16(a);
                    continue;
                }
                catch (Exception) { }
                switch (a.ToLower())
                {
                    case "image":
                        purgecontents = true;
                        purgeimages = true;
                        continue;
                    case "file":
                        purgecontents = true;
                        purgefiles = true;
                        continue;
                }
            }
            if (message.MentionedRoleIds != null && message.MentionedRoleIds.Count > 0)
            {
                purgerole = true;
            }
            if (message.MentionedUserIds != null && message.MentionedUserIds.Count > 0)
            {
                purgeuser = true;
            }
            if (!purgeuser && !purgerole && !purgecontents)
            {
                purgeall = true;
            }

            List<IAsyncEnumerable<IReadOnlyCollection<IMessage>>> messagesall = new List<IAsyncEnumerable<IReadOnlyCollection<IMessage>>>();
            List<IMessage> messages = new List<IMessage>();
            if (messagestoscan > 100)
            {
                var _messages_ = await (message.Channel.GetMessagesAsync(100).FlattenAsync());
                messages.AddRange(_messages_);
                for (int i = 0; i < messagestoscan; i += 100)
                {
                    var wha = (message.Channel.GetMessagesAsync(messages.Last(), Direction.Before, 100));
                    messages.AddRange(await wha.FlattenAsync());
                }
            }
            else
            {
                var messages_ = await message.Channel.GetMessagesAsync(messagestoscan).FlattenAsync();
                messages.AddRange(messages_);
            }
            messages.RemoveAll(n => n.Timestamp.Second < 1200000);
            if (purgeall)
            {
                await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.ToArray());
            } else if (purgecontents)
            {
                if (purgerole || purgeuser)
                {
                    if (purgerole && purgeuser)
                    {
                        if (purgefiles && !purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (message.MentionedUserIds.Any(n.Author.Id.Equals) || message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals)))));
                        }
                        else if (!purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".png") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".gif") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpeg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".tiff") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".bmp"))
                            && (message.MentionedUserIds.Any(n.Author.Id.Equals) || message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals)))));
                        }
                        else if (purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (message.MentionedUserIds.Any(n.Author.Id.Equals) || message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals)))));
                        }
                    } else if (purgerole && !purgeuser)
                    {
                        if (purgefiles && !purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals))));
                        }
                        else if (!purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".png") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".gif") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpeg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".tiff") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".bmp"))
                            && message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals))));
                        }
                        else if (purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals))));
                        }

                    } else if (!purgerole && purgeuser)
                    {
                        if (purgefiles && !purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && message.MentionedUserIds.Any(n.Author.Id.Equals)));
                        }
                        else if (!purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".png") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".gif") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".jpeg") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".tiff") ||
                            n.Attachments.FirstOrDefault().Filename.EndsWith(".bmp"))
                             && message.MentionedUserIds.Any(n.Author.Id.Equals)));
                        }
                        else if (purgefiles && purgeimages)
                        {
                            await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && message.MentionedUserIds.Any(n.Author.Id.Equals)));
                        }
                    }
                }
                else
                {
                    if (purgefiles && !purgeimages)
                    {
                        await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0));
                    }
                    else if (!purgefiles && purgeimages)
                    {
                        await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0 && (
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".png") ||
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".gif") ||
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".jpg") ||
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".jpeg") ||
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".tiff") ||
                        n.Attachments.FirstOrDefault().Filename.EndsWith(".bmp"))));
                    }
                    else if (purgefiles && purgeimages)
                    {
                        await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => n.Attachments.Count > 0));
                    }
                }
            } else if (purgerole || purgeuser)
            {
                if (purgerole && purgeuser)
                {
                    await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => message.MentionedUserIds.Any(n.Author.Id.Equals) || message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals))));
                } else if (!purgeuser && purgerole)
                {
                    await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => message.MentionedRoleIds.Any(m => (n.Author as IGuildUser).RoleIds.Any(m.Equals)))); 
                } else if (purgeuser && !purgerole)
                {
                    await (message.Channel as ITextChannel).DeleteMessagesAsync(messages.Where(n => message.MentionedUserIds.Any(n.Author.Id.Equals)));
                }
            }
        }
    }
}
