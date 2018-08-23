using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Linq;

namespace MusicBot9001.Commands.Guild
{
    class Permissions
    {
        public static Embed Help(string prefix)
        {
            var embed = new EmbedBuilder();
            embed.Color = Color.Blue;
            embed.Title = "Permissions Help";
            embed.Description = "```http\n" +
                $"{prefix}Permissions View : View Permissions\n" +
                $"{prefix}Permissions View All : View all permissions\n" +
                $"{prefix}Permissions View (User Mention) : View user permissions\n" +
                $"{prefix}Permissions View (Role Mention) : View role permissions\n" +
                $"{prefix}Permissions View Available : View all available permissions\n" +
                $"{prefix}Permissions Set (User Mention | Role Mention) (Permission) ... : Set Permissions\n" +
                $"{prefix}Permissions Revoke (User Mention | Role Mention) (Permission) ... : Revoke Permissions\n" +
                $"```\n" +
                $"\n" +
                $"There are many permissions you can set, use `{prefix}Permissions View Available` to see what you can assign\n";
            return embed.Build();
        }

        public static Embed ViewPermissions(object permissionTarget, GuildHandler.GuildHandle guildHandle)
        {
            if (permissionTarget is IUser)
            {
                if (guildHandle.permissionsHandler.UserExists(permissionTarget as IUser))
                {

                }
                else
                {

                }
            } else
                if (permissionTarget is IRole)
            {
                if (guildHandle.permissionsHandler.RoleExists(permissionTarget as IRole))
                {
                    EmbedBuilder embedBuilder = new EmbedBuilder();
                    embedBuilder.Color = Color.Blue;
                    embedBuilder.Title = "Permissions";
                    embedBuilder.Description = "";
                }
                else
                {
                    return new EmbedBuilder() { Color = Color.Blue, Title = "Permissions", Description = "No permissions set for `" + (permissionTarget as IRole).Name + "`"}.Build(); 
                }
            }
            else
            {
                return Help(guildHandle.database.prefix);
            }
            return null;
        }

        public static string Permissions_(string args, IMessage message)
        {
            if (!(message.Channel is IGuildChannel))
            { return "This is not a server!"; }
            var guild = (message.Channel as IGuildChannel).Guild;
            var userperms = Program.mainHandler.guildHandles[guild.Id].permissionsHandler;
            if (args == null)
            { return "No arguments for `Permissions`"; }
            string[] splitargs = args.Split(' ');





            bool set = true;
            if (splitargs.Length > 0)
                set = (splitargs[0].ToLower() == "set");

            bool revoke = false;
            if (splitargs.Length > 0)
            {
                revoke = (splitargs[0].ToLower() == "revoke");
            }

            GuildHandler.Permissions perms = new GuildHandler.Permissions();
            if (revoke)
                perms.perms = new string[] { "" };

            foreach (var a in splitargs)
            {
                switch (a.ToLower())
                {
                    case "botadmin":
                        if (userperms.HasPermission("Commands.BotAdmin", (message.Author as IGuildUser)) || Program.config.botAdmins.Any(message.Author.Id.ToString().Equals))
                        {
                            if (!(perms.perms.Any("Commands.BotAdmin".Equals)))
                            {
                                var asdasdsdasd = new List<string>(perms.perms);
                                asdasdsdasd.Add("Commands.BotAdmin");
                                perms.perms = asdasdsdasd.ToArray();
                            }
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("You can't set permissions for `BotAdmin`");
                        }
                        continue;
                    case "permissions":
                        if (userperms.HasPermission("Commands.Guild'Permissions", (message.Author as IGuildUser)))
                        {
                            if (!(perms.perms.Any("Commands.Guild'Permissions".Equals)))
                            {
                                var asa = new List<string>(perms.perms);
                                asa.Add("Commands.Guild'Permissions");
                                perms.perms = asa.ToArray();
                            }
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("You can't set permissions for `Permissions`");
                        }                        
                        continue;
                    case "none":
                        if (userperms.HasPermission("Commands.Guild'Permissions", (message.Author as IGuildUser)))
                        {
                            perms = new GuildHandler.Permissions() { perms = new string[] { "" } };
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("You can't set permissions to `none`");
                        }
                        continue;
                    case "admin":
                        if (userperms.HasPermission("Commands.Guild", (message.Author as IGuildUser)))
                        {
                            if (!(perms.perms.Any("Commands.Guild".Equals)))
                            {
                                var asdasdasdasd = new List<string>(perms.perms);
                                asdasdasdasd.Add("Commands.Guild");
                                perms.perms = asdasdasdasd.ToArray();
                            }
                        }
                        else
                        {
                            message.Channel.SendMessageAsync("You can't set permissions for `Admin`");
                        }
                        continue;
                    case "avatar":
                        if (userperms.HasPermission("Commands.User'Avatar", (message.Author as IGuildUser)))
                        {
                            if (!(perms.perms.Any("Commands.User'Avatar".Equals)))
                            {
                                var asna = new List<string>(perms.perms);
                                asna.Add("Commands.User'Avatar");
                                perms.perms = asna.ToArray();
                            }
                        }
                        continue;
                }
            }

            int setusers = 0;
            int setroles = 0;

            if (revoke)
            {
                if (message.Tags.Any(n => n.Type == TagType.EveryoneMention))
                {
                    var everyone = (message.Channel as IGuildChannel).Guild.GetUsersAsync().GetAwaiter().GetResult().Where(n => n.Id != message.Author.Id && n.Id != guild.OwnerId);
                    setusers += everyone.Count();

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.RevokeUsersPerms(everyone.Select(n => n.Id).ToArray(), perms);
                } else
                if (message.Tags.Any(n => n.Type == TagType.HereMention))
                {
                    var here = guild.GetUsersAsync().GetAwaiter().GetResult().Where(n => !(n.Status == UserStatus.Offline) && n.Id != message.Author.Id && n.Id != guild.OwnerId);
                    setusers += here.Count();

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.RevokeUsersPerms(here.Select(n => n.Id).ToArray(), perms);
                } else
                if (message.MentionedUserIds != null)
                {
                    setusers += message.MentionedUserIds.Count;

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.RevokeUsersPerms(message.MentionedUserIds.ToArray(), perms);
                }
            }
            else
            {
                if (message.Tags.Any(n => n.Type == TagType.EveryoneMention))
                {
                    var everyone = (message.Channel as IGuildChannel).Guild.GetUsersAsync().GetAwaiter().GetResult().Where(n => n.Id != message.Author.Id && n.Id != guild.OwnerId);
                    setusers += everyone.Count();

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.AddUsers(everyone.Select(n => n.Id).ToArray(), perms, true);
                }
                else
                if (message.Tags.Any(n => n.Type == TagType.HereMention))
                {
                    var here = guild.GetUsersAsync().GetAwaiter().GetResult().Where(n => !(n.Status == UserStatus.Offline) && n.Id != message.Author.Id && n.Id != guild.OwnerId);
                    setusers += here.Count();

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.AddUsers(here.Select(n => n.Id).ToArray(), perms, true);
                }
                else
                if (message.MentionedUserIds != null)
                {
                    setusers += message.MentionedUserIds.Count;

                    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.AddUsers(message.MentionedUserIds.ToArray(), perms, true);
                }
                ////
                //if (message.MentionedUserIds != null)
                //{
                //    setusers += message.MentionedUserIds.Count;

                //    Program.mainHandler.guildHandles[guild.Id].permissionsHandler.AddUsers(message.MentionedUserIds.ToArray(), perms, set);
                //}
            }

            if (message.MentionedRoleIds != null)
            {
                if (revoke)
                {
                    setroles += message.MentionedRoleIds.Count;
                    foreach (ulong role in message.MentionedRoleIds)
                        Program.mainHandler.guildHandles[guild.Id].permissionsHandler.RevokeRolesPerms(message.MentionedRoleIds.ToArray(), perms);
                }
                else
                {
                    setroles += message.MentionedRoleIds.Count;
                    foreach (ulong role in message.MentionedRoleIds)
                        Program.mainHandler.guildHandles[guild.Id].permissionsHandler.SetRolePerms(guild.GetRole(role), perms);
                }
            }
            if (revoke)
            {
                if (setroles == 0 && setusers == 0)
                {
                    return "Revoked permissions for nothing";
                }
                else if (setroles != 0 && setusers == 0)
                {
                    return "Revoked permissions for `" + setroles + "` roles";
                }
                else if (setroles == 0 && setusers != 0)
                {
                    return "Revoked permissions for `" + setusers + "` users";
                }
                else if (setroles != 0 && setusers != 0)
                {
                    return "Revoked permissions for `" + setroles + "` roles and `" + setusers + "` users";
                }
            }
            else
            {
                if (setroles == 0 && setusers == 0)
                {
                    return "Set permissions for nothing";
                }
                else if (setroles != 0 && setusers == 0)
                {
                    return "Set permissions for `" + setroles + "` roles";
                }
                else if (setroles == 0 && setusers != 0)
                {
                    return "Set permissions for `" + setusers + "` users";
                }
                else if (setroles != 0 && setusers != 0)
                {
                    return "Set permissions for `" + setroles + "` roles and `" + setusers + "` users";
                }
            }
            return "";
        }
    }
}
