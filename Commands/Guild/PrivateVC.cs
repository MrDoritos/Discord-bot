using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MusicBot9001.Commands.Guild
{
    class PrivateVC
    {
        /// <summary>
        /// Help context for PrivateVC
        /// </summary>
        /// <param name="prefix">Guild Prefix</param>
        /// <param name="timeout">Timeout for the voice channel in **seconds**</param>
        /// <returns>Finished Embed</returns>
        public static Embed Help(string prefix, int timeout)
        {
            var embed = new EmbedBuilder();
            embed.Title = "Private VC Help";
            embed.Description = $"```http\n" +
                $"{prefix}PrivateVC Help : This\n" +
                $"{prefix}PrivateVC Create : Create a private VC\n" +
                $"{prefix}PrivateVC Show : Show your current private VC info\n" +
                $"{prefix}PrivateVC (User Mention | Role Mention) : Add people or roles to your private VC\n" +
                $"{prefix}PrivateVC Remove (User Mention | Role Mention | All) : Remove people or roles from your private VC\n" +
                $"{prefix}PrivateVC Delete : Delete your Private VC\n" +
                $"{prefix}PrivateVC Setting : PrivateVC Settings (For Admins)" +
                $"```\n\n" +
                $"Create a private voice channel.\n" +
                $"Timeout is `{timeout}` seconds until a channel is deleted when not in use.\n";
            embed.Color = Color.Blue;
            return embed.Build();
        }

        public static async Task PrivateVC_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { return; }
            var guild = (message.Channel as IGuildChannel).Guild;
            var guildHandle = Program.mainHandler.guildHandles[guild.Id];
            var database = guildHandle.database;
            bool hasMentions = (message.MentionedUserIds != null && message.MentionedUserIds.Count > 0);
            bool hasRoleMention = (message.MentionedRoleIds != null && message.MentionedRoleIds.Count > 0);

            if (args == null || args.Length < 1)
            { await message.Channel.SendMessageAsync(null, false, Help(database.prefix, database.voiceChannelTimeOut)); }

            var splitargs = args.Split(' ');

            if (splitargs.Any(n => n.ToLower() == "show"))
            {
                if (guildHandle.privateVCs.ContainsKey(message.Author.Id))
                {
                    var privatevc = guildHandle.privateVCs[message.Author.Id];
                    var embedbuilder = new EmbedBuilder();
                    embedbuilder.Title = "Private VC";
                    embedbuilder.Color = Color.Blue;
                    embedbuilder.AddField("Owner", $"`{privatevc.Owner.Username}`", true);
                    embedbuilder.AddField("Channel", $"`{privatevc.VoiceChannel.Name}`", true);
                    bool somebool = !(await (privatevc.IsPopulated()));
                    embedbuilder.AddField("Can Timeout", $"`{somebool}`",true);
                    var users = (await guild.GetUsersAsync()).Where(n => privatevc.AllowedUsers.Any(n.Id.Equals)).ToArray();
                    var roles = guild.Roles.Where(n => privatevc.AllowedRoles.Any(n.Id.Equals)).ToArray();
                    EmbedFieldBuilder embedFieldBuilder = new EmbedFieldBuilder();
                    embedFieldBuilder.Name = "Allowed Users";
                    embedFieldBuilder.Value = "None";
                    int userCount = users.Length;
                    if (userCount > 0)
                    {
                        embedFieldBuilder.Value = " ";
                        for (int i = 0; i < userCount; i++)
                        {
                            if (i < 1)
                            {
                                embedFieldBuilder.Value += $"`{users[i].Username}`";
                            }
                            else
                            {
                                embedFieldBuilder.Value += $", `{users[i].Username}`";
                            }
                        }
                    }
                    embedFieldBuilder.IsInline = true;
                    if (embedFieldBuilder.Value.ToString().Length > 0)
                    {
                        embedbuilder.AddField(embedFieldBuilder);
                    }
                    else
                    {
                        embedFieldBuilder.Value = "None";
                        embedbuilder.AddField(embedFieldBuilder);
                    }
                    embedFieldBuilder = new EmbedFieldBuilder();
                    embedFieldBuilder.Name = "Allowed Roles";
                    embedFieldBuilder.Value = "None";
                    int roleCount = roles.Length;
                    if (roleCount > 0)
                    {
                        embedFieldBuilder.Value = " ";
                        for (int i = 0; i < roleCount; i++)
                        {
                            if (i < 1)
                            {
                                embedFieldBuilder.Value += $"`{roles[i].Name}`";
                            }
                            else
                            {
                                embedFieldBuilder.Value += $", `{roles[i].Name}`";
                            }
                        }
                    }
                    //embedbuilder.AddField(embedFieldBuilder);
                    embedFieldBuilder.IsInline = true;
                    if (embedFieldBuilder.Value.ToString().Length > 0)
                    {
                        embedbuilder.AddField(embedFieldBuilder);
                    }
                    else
                    {
                        embedFieldBuilder.Value = "None";
                        embedbuilder.AddField(embedFieldBuilder);
                    }
                    await message.Channel.SendMessageAsync(null, false, embedbuilder.Build());
                }
                else
                {
                    await message.Channel.SendMessageAsync("You don't have a private VC!");
                }
                return;
            }
            if (splitargs.Any(n => n.ToLower() == "create"))
            {
                if (guildHandle.privateVCs.ContainsKey(message.Author.Id))
                {
                    await message.Channel.SendMessageAsync("You already have a private VC!");
                }
                else
                {
                    var currentguilduser = await guild.GetCurrentUserAsync();
                    var roles = guild.Roles.Where(n => currentguilduser.RoleIds.Any(n.Id.Equals));
                    bool hasPermissions = roles.Any(n => (n.Permissions.ManageChannels && n.Permissions.ManageRoles || n.Permissions.Administrator));
                    if (hasPermissions)
                    {
                        try
                        {
                            var channel = await guild.CreateVoiceChannelAsync($"{message.Author.Username}'s PrivateVC");
                            var role = await guild.CreateRoleAsync($"PrivateVC Role");
                            await channel.RemovePermissionOverwriteAsync(guild.EveryoneRole);
                            var perms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);
                            var noperms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
                            await channel.AddPermissionOverwriteAsync(role, perms);
                            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, noperms);
                            guildHandle.privateVCs.Add(message.Author.Id, new GuildHandler.PrivateVC.PrivateVC((message.Author as IGuildUser), channel, role, database.voiceChannelTimeOut));
                            await message.Channel.SendMessageAsync($"Successfully created `{channel.Name}`");
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"Could not create private VC on {guild.Name} ({guild.Id})", e);
                            await message.Channel.SendMessageAsync($"Could not create VC");
                        }
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync("I don't have the appropriate permissions to manage channels!");
                    }
                }
            }
            if (splitargs.Any(n => n.ToLower() == "delete"))
            { }

            if (hasMentions || hasRoleMention)
            {
                if (!(guildHandle.privateVCs.ContainsKey(message.Author.Id)))
                {
                    await message.Channel.SendMessageAsync("You don't have a private VC!");
                }
                else
                {
                    var perms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Allow, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Allow, PermValue.Deny, PermValue.Deny);
                    var noperms = new OverwritePermissions(PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny, PermValue.Deny);
                    bool removeall = (splitargs.Any(n => n.ToLower() == "all"));
                    var privatevc = guildHandle.privateVCs[message.Author.Id];
                    IEnumerable<IRole> roles = null;
                    IEnumerable<IGuildUser> users = null;

                    if (hasRoleMention)
                    {
                        roles = guild.Roles.Where(n => message.MentionedRoleIds.Any(n.Id.Equals));
                    }
                    if (hasMentions)
                    {
                        users = (await guild.GetUsersAsync()).Where(n => message.MentionedUserIds.Any(n.Id.Equals));
                    }
                    if (splitargs.Any(n => n.ToLower() == "remove"))
                    {
                        if (removeall)
                        {
                            privatevc.AllowedRoles = new List<ulong>();
                            privatevc.AllowedUsers = new List<ulong>();
                            await message.Channel.SendMessageAsync("Successfully deleted all rules");
                            return;
                        }
                        if (hasMentions && !hasRoleMention)
                        {
                            var dat = users.Where(n => privatevc.AllowedUsers.Any(n.Id.Equals));
                            int count = dat.Count();
                            privatevc.AllowedUsers.RemoveAll(n => dat.Any(m => m.Id == n));
                            await message.Channel.SendMessageAsync($"Removed `{count}` users");
                        } else
                        if (hasRoleMention && !hasMentions)
                        {
                            var dat = roles.Where(n => privatevc.AllowedRoles.Any(n.Id.Equals));
                            int count = dat.Count();
                            privatevc.AllowedRoles.RemoveAll(n => dat.Any(m => m.Id == n));
                            await message.Channel.SendMessageAsync($"Removed `{count}` roles");
                        } else
                        {
                            var roledat = roles.Where(n => privatevc.AllowedRoles.Any(n.Id.Equals));
                            var userdat = users.Where(n => privatevc.AllowedUsers.Any(n.Id.Equals));
                            int usercount = userdat.Count();
                            int rolecount = roledat.Count();
                            privatevc.AllowedRoles.RemoveAll(n => roledat.Any(m => m.Id == n));
                            privatevc.AllowedUsers.RemoveAll(n => userdat.Any(m => m.Id == n));
                            await message.Channel.SendMessageAsync($"Removed `{usercount}` users and `{rolecount}` roles");
                        }
                        return;
                    }
                    else
                    {
                        if (hasMentions && !hasRoleMention)
                        {
                            var dat = users.Where(n => !(privatevc.AllowedUsers.Any(n.Id.Equals)));
                            int count = dat.Count();
                            privatevc.AllowedUsers.AddRange(dat.Select(n => n.Id));                            
                            foreach (var usr in dat)
                            {
                                await usr.AddRoleAsync(privatevc.Role);
                            }
                            await message.Channel.SendMessageAsync($"Added `{count}` users");
                        }
                        else
                        if (hasRoleMention && !hasMentions)
                        {
                            var dat = roles.Where(n => !(privatevc.AllowedRoles.Any(n.Id.Equals)));
                            int count = dat.Count();
                            privatevc.AllowedRoles.AddRange(dat.Select(n => n.Id));
                            foreach (var rol in dat)
                            {
                                await privatevc.VoiceChannel.AddPermissionOverwriteAsync(rol, perms);
                            }
                            await message.Channel.SendMessageAsync($"Added `{count}` roles");
                        }
                        else
                        {
                            var roledat = roles.Where(n => !(privatevc.AllowedRoles.Any(n.Id.Equals)));
                            var userdat = users.Where(n => !(privatevc.AllowedUsers.Any(n.Id.Equals)));
                            int usercount = userdat.Count();
                            int rolecount = roledat.Count();
                            privatevc.AllowedRoles.AddRange(roledat.Select(n => n.Id));
                            privatevc.AllowedUsers.AddRange(userdat.Select(n => n.Id));
                            foreach (var rol in roledat)
                            {
                                await privatevc.VoiceChannel.AddPermissionOverwriteAsync(rol, perms);
                            }
                            foreach (var usr in userdat)
                            {
                                await usr.AddRoleAsync(privatevc.Role);
                            }
                            await message.Channel.SendMessageAsync($"Added `{usercount}` users and `{rolecount}` roles");
                        }
                        return;
                    }

                }
            }
        }
    }
}
