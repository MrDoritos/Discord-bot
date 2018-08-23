using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MusicBot9001.Commands.Guild
{
    class DefaultRole
    {
        public static Embed Help(string prefix)
        {
            var Embed = new EmbedBuilder();
            Embed.Color = Color.Blue;
            Embed.Title = "DefaultRole Help";
            Embed.Description = "```http\n" +
                prefix + "DefaultRole Enable : Enable the default role\n" +
                prefix + "DefaultRole Disable : Disable the default role\n" +
                prefix + "DefaultRole (Role Mention) : Set the default role\n" +
                prefix + "DefaultRole Show : Show default role status\n" +
                prefix + "DefaultRole Clear : Clear default role\n" +
                "```";
            return Embed.Build();
        }

        private static Embed Show(GuildHandler.GuildHandle handle)
        {
            string name = "invalid";
            if (handle.database.defaultRole == 0)
            {
                name = "undefined";
            }
            else
            {
                var role = handle.guild.GetRole(handle.database.defaultRole);
                if (role != null)
                {
                    name = role.Name;
                }
            }
            var Embed = new EmbedBuilder();
            Embed.Color = Color.Blue;
            Embed.Title = "DefaultRole";
            Embed.Description = "Enabled: `" + handle.database.giveDefaultRole.ToString() + "`\n"
                + "DefaultRole: `" + name + "`\n";
          
               
            return Embed.Build();
        }

        public static async Task DefaultRole_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { return; }
            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            if (args.Length < 1 && message.MentionedRoleIds.Count < 1)
            {
                await message.Channel.SendMessageAsync(null, false, Show(guildHandle));
                return;
            }

            if (args.Length > 0)
            {
                if (message.MentionedRoleIds.Count > 0)
                {
                    var role = guild.GetRole(message.MentionedRoleIds.First());
                    bool hasPermissions = false;
                    var currentguilduser = await guild.GetCurrentUserAsync();
                    var roles = guild.Roles.Where(n => currentguilduser.RoleIds.Any(n.Id.Equals));
                    bool temp1 = roles.Any(n => ((n.Permissions.ManageRoles || n.Permissions.Administrator) && n.Position >= role.Position) || n.Permissions.Administrator);
                    hasPermissions = (temp1);
                    if (hasPermissions)
                    {
                        guildHandle.database.defaultRole = role.Id;
                        await message.Channel.SendMessageAsync("Default role set to `" + role.Name + "`");
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync($"None of my roles allow me to set permissions for `{role.Name}`");
                    }
                }
                else
                {
                    switch (args.Split(' ')[0].ToLower())
                    {
                        case "enable":
                            //guildHandle.database.giveDefaultRole = true;
                            if (guild.GetRole(guildHandle.database.defaultRole) != null)
                            {
                                guildHandle.database.giveDefaultRole = true;
                                await message.Channel.SendMessageAsync("Now giving out the default role of `" + guild.GetRole(guildHandle.database.defaultRole).Name + "` when a user joins the server");
                            }
                            else
                            {
                                await message.Channel.SendMessageAsync("You must use `" + guildHandle.database.prefix + "DefaultRole (Role Mention)` prior to enabling the default role");
                            }
                            break;
                        case "disable":
                            guildHandle.database.giveDefaultRole = false;
                            break;
                        case "show":
                            await message.Channel.SendMessageAsync(null, false, Show(guildHandle));
                            break;
                        case "clear":
                            await message.Channel.SendMessageAsync("Cleared default role, and turned off default role");
                            guildHandle.database.giveDefaultRole = false;
                            guildHandle.database.defaultRole = 0;
                            break;
                        default:
                            await message.Channel.SendMessageAsync(null, false, Help(guildHandle.database.prefix));
                            break;
                    }
                }
            }            
        }
    }
}
