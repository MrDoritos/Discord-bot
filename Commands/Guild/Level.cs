using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MusicBot9001.Commands.Guild
{
    class Level
    {
        public static char[] numbers = new char[] {'0','1','2','3','4','5','6','7','8', '9'};

        public static Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Color = Color.Blue;
            embed.Title = "Level Help";
            embed.Description = $"```http\n" +
                $"{prefix}Level (User Mention) : View level of a user\n" +
                $"{prefix}Level Set (User Mention) (Level): Set a user's level\n" +
                $"{prefix}Level Up (User Mention): Level up a user\n" +
                $"{prefix}Level Enable : Enable level module\n" +
                $"{prefix}Level Disable : Disable level module\n" +
                $"{prefix}Level (Role Mention) (Level) : Role to give a user when they reach a certain level\n" +
                $"{prefix}Level (Role Mention) Remove : Remove a role from being given to a user\n" +
                $"{prefix}Level List : List role levels\n" +
                $"{prefix}Level Help : Show level help\n" +
                $"```";
            return embed.Build();
        }

        public static async Task Level_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
                return;
            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            var database = guildHandle.database;

            if (args == null || args.Length < 1)
            {
                await message.Channel.SendMessageAsync(null, false, Help(database.prefix));
                return;
            }

            if (message.MentionedUserIds != null && message.MentionedUserIds.Count > 0)
            {
                var tosend = new EmbedBuilder();
                tosend.Color = Color.Blue;
                tosend.Title = "Levels";
                var users = (await guild.GetUsersAsync()).Where(n => message.MentionedUserIds.Any(n.Id.Equals));
                var userlevels = guildHandle.levelHandler.GetUsersLevels(users.Select(n => n.Id).ToArray());
                foreach (var a in userlevels)
                {
                    tosend.AddField(new EmbedFieldBuilder() { IsInline = false, Name = users.Where(n => n.Id == a.Key).First().Nickname, Value = userlevels[a.Key].level });
                }
                await message.Channel.SendMessageAsync(null, false, tosend.Build());
                return;
            }

            var splitargs = args.Split(' ');
            bool hasMention = (message.MentionedUserIds != null && message.MentionedUserIds.Count > 0);
            bool hasRoleMention = (message.MentionedRoleIds != null && message.MentionedRoleIds.Count > 0);
            if (splitargs.Any(n => n.ToLower() == "up"))
            {
                if (hasMention)
                {
                    var users = (await guild.GetUsersAsync()).Where(n => message.MentionedUserIds.Any(n.Id.Equals));
                    foreach (var usr in users)
                    {
                        
                    }
                }
                else
                {
                    await message.Channel.SendMessageAsync(null, false, Help(database.prefix));
                }
                return;
            }
             if (hasRoleMention)
            {
                var roles = guild.Roles;
                if (splitargs.Any(n => n.ToLower() == "remove"))
                {
                    if (database.rolesPerLevel.Values.Any(n => roles.Any(m => n.Id == m.Id)))
                    {
                        int removed = 0;
                        var rolesToRemove = database.rolesPerLevel.Values.Where(n => roles.Any(m => n.Id == m.Id));
                        var keyvalue = database.rolesPerLevel.Where(n => rolesToRemove.Select(m => m.Id).Any(n.Value.Id.Equals));
                        foreach (var role in rolesToRemove)
                        {
                            removed++;
                            database.rolesPerLevel.Remove(keyvalue.First(n => n.Value.Id == role.Id).Key);
                        }
                        if (removed == 1)
                        {
                            await message.Channel.SendMessageAsync($"Removed `1` role");
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync($"Removed `{removed}` roles");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("Removed `0` roles");
                    }
                }
                else
                {
                    if (splitargs.Any(n => n.All(o => numbers.Any(m => o == m))))
                    {
                        try
                        {
                            int num = Convert.ToInt32(new string(splitargs.First(n => n.Length > 0 && n.All(o => numbers.Any(m => o == m))).Take(8).ToArray()));
                            if (num != 0)
                            {
                                var role = roles.Where(n => message.MentionedRoleIds.Any(n.Id.Equals)).First();
                                if (role != null)
                                {
                                    if (database.rolesPerLevel.ContainsKey(num))
                                    {
                                        await message.Channel.SendMessageAsync($"Role `{database.rolesPerLevel[num].Name}` is already set for level `{num}`");
                                        return;
                                    }
                                    else
                                    {
                                        database.rolesPerLevel.Add(num, role);
                                        await message.Channel.SendMessageAsync($"Role `{role.Name}` is now set for level `{num}`");
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("Don't set a role for level `0`, that's bad");
                                return;
                            }
                        } catch (Exception e)
                        {
                            Logger.Error("Setting rolelevel", e);
                            await message.Channel.SendMessageAsync(null, false, Help(database.prefix));
                            return;
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync(null, false, Help(database.prefix));
                        return;
                    }
                }
            }

             if (splitargs.Any(n => n.ToLower() == "enable" || n.ToLower() == "disable"))
            {
                if (splitargs.Any(n => n.ToLower() == "enable"))
                {
                    database.useLevels = true;
                    await message.Channel.SendMessageAsync("Enabled level up");
                } else
                if (splitargs.Any(n => n.ToLower() == "disable"))
                {
                    database.useLevels = false;
                    await message.Channel.SendMessageAsync("Disabled level up");
                }
                return;
            }
             if (splitargs.Any(n => n.ToLower() == "list"))
            {
                if (database.rolesPerLevel.Count > 0)
                {
                    var embed = new EmbedBuilder();
                    embed.Color = Color.Blue;
                    embed.Title = "Role Levels";
                    foreach (var keyvalue in database.rolesPerLevel)
                    {
                        embed.AddField($"Level `{keyvalue.Key}`", $"`{keyvalue.Value.Name}`");
                    }
                    await message.Channel.SendMessageAsync(null, false, embed.Build());
                    return;
                }
                else
                {
                    await message.Channel.SendMessageAsync("No level roles to list");
                    return;
                }
            }
             if (hasMention)
            {

            }
        }
    }
}
