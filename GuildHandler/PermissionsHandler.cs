using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;


namespace MusicBot9001.GuildHandler
{
    class PermissionsHandler
    {
        public IDictionary<ulong, Permissions> userPerms = new Dictionary<ulong, Permissions>();
        public IDictionary<ulong, Permissions> rolePerms = new Dictionary<ulong, Permissions>();

        public Permissions getRolePerms(IRole role)
        {
            if (role != null)
            {
                if (rolePerms.ContainsKey(role.Id))
                {
                    return rolePerms[role.Id];
                }
            }
            return new Permissions() { perms = new string[]{"" } };
        }



        public bool RoleExists(object role)
        {
            if (role is IRole)
            {
                if ((role as IRole) == null)
                { return false;
                }
                else
                {
                    if (rolePerms.ContainsKey((role as IRole).Id))
                    { 
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            } else
                if (role is ulong)
            { 
                if (rolePerms.ContainsKey(Convert.ToUInt64(role)))
                {
                    return true;
                }
                else
                {
                    return false;
                }                    
            }
            else
            {
                return false;
            }                
        }

        public bool UserExists(object user)
        {
            if (user is IUser)
            {
                if (user != null)
                {
                    if (userPerms.ContainsKey((user as IUser).Id))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            } else
                if (user is ulong)
            {
                if (userPerms.ContainsKey(Convert.ToUInt64(user)))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Revokes multiple users of permissions
        /// </summary>
        /// <param name="users">The users to revoke</param>
        /// <param name="perms">The perms to revoke</param>
        public void RevokeUsersPerms(ulong[] users, Permissions perms)
        {
            foreach (ulong id in users)
            {
                if (userPerms.ContainsKey(id))
                {
                    string[] commandFamiliesRevoke = perms.perms.Where(n => !(n.Contains('\''))).ToArray();
                    string[] commandTreesRevoke = perms.perms.Where(n => n.Contains('\'')).ToArray();

                    //string[] commandFamilies = getPerms(id).perms.Where(n => !(n.Contains('\''))).ToArray();
                    //string[] commandTrees = getPerms(id).perms.Where(n => n.Contains('\'')).ToArray();

                    if (userPerms[id].perms.Any(n => commandFamiliesRevoke.Any(n.Equals)))
                    {
                        var old = userPerms[id].perms.ToList();
                        old.RemoveAll(n => commandFamiliesRevoke.Any(n.Contains));
                        userPerms[id].perms = old.ToArray();
                    }
                    else
                    {
                        var old_ = userPerms[id].perms.ToList();
                        old_.RemoveAll(n => commandTreesRevoke.Any(n.Equals));
                        userPerms[id].perms = old_.ToArray();
                    }                    
                }
            }
        }

        public void RevokeRolesPerms(ulong[] roles, Permissions perms)
        {
            foreach (ulong id in roles)
            {
                if (rolePerms.ContainsKey(id))
                {
                    string[] commandFamiliesRevoke = perms.perms.Where(n => !(n.Contains('\''))).ToArray();
                    string[] commandTreesRevoke = perms.perms.Where(n => n.Contains('\'')).ToArray();

                    if (rolePerms[id].perms.Any(n => commandFamiliesRevoke.Any(n.Equals)))
                    {
                        var old = rolePerms[id].perms.ToList();
                        old.RemoveAll(n => commandFamiliesRevoke.Any(n.Contains));
                        rolePerms[id].perms = old.ToArray();
                    }
                    else
                    {
                        var old_ = rolePerms[id].perms.ToList();
                        old_.RemoveAll(n => commandFamiliesRevoke.Any(n.Equals));
                        rolePerms[id].perms = old_.ToArray();
                    }
                }
            }
        }

        public Permissions getRolePerms(ulong role)
        {
            if (rolePerms.ContainsKey(role))
            {
                return rolePerms[role];
            }
            return new Permissions() { perms = new string[]{"" } };
        }

        public void SetRolePerms(IRole role, Permissions perms)
        {
            if (role != null)
            {
                if (rolePerms.ContainsKey(role.Id))
                {                    
                    rolePerms[role.Id] = perms;
                }
                else
                {
                    rolePerms.Add(role.Id, perms);
                }
            }
        }

        public Permissions getPerms(IUser user)
        {
            if (user != null)
            {
                if (userPerms.ContainsKey(user.Id))
                {
                    return userPerms[user.Id];
                }
            }
            return new Permissions();
        }

        public bool HasPermission(string CommandTree, IGuildUser user)
        {
            string[] permissions = getPerms(user).perms;
            //string[] permissionsExclusion = getPerms(user).permsExclude;
            string command = CommandTree.Split('\'').Last();
            string commandFamily = CommandTree.Split('\'')[0];
            if (Program.config.botAdmins.Any(user.Id.ToString().Equals))
            { return true; }

            //if (permissions.Any(command.Equals))
            //{
            //    return true;
            //}

            //Return true if it is the guild owner, but not if they sent a botadmin command
            if (user.Guild.OwnerId == user.Id)
            {
                if (commandFamily == "Commands.BotAdmin")
                {
                    if (permissions.Any(commandFamily.Equals))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            if (permissions.Any(commandFamily.Equals)/* && !permissionsExclusion.Any(CommandTree.Equals)*/)
            {                
                return true;
            }
            if (permissions.Any(CommandTree.Equals))
            {
                return true;
            }
            if (user.RoleIds != null)
            {
                foreach (var role in user.RoleIds)
                {
                    if (rolePerms.ContainsKey(role))
                    if (rolePerms[role].perms.Any(commandFamily.Equals)/* && !permissionsExclusion.Any(CommandTree.Equals)*/)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public Permissions getPerms(ulong id)
        {
            if (userPerms.ContainsKey(id))
            {
                return userPerms[id];
            }
            return new Permissions();
        }

        /// <summary>
        /// Set the permissions manually
        /// </summary>
        /// <param name="id">User</param>
        /// <param name="perms">Permissions</param>
        /// <param name="addNew">If the user doesn't exist, add the user</param>
        public void SetPerms(ulong id, Permissions perms, bool addNew = false)
            
        {
            if (userPerms.ContainsKey(id))
            {
                    userPerms[id] = perms;
                
            }
            else if (addNew)
            {
                userPerms.Add(id, perms);
            }
        }

        /// <summary>
        /// Add users
        /// </summary>
        /// <param name="ids">User Ids</param>
        /// <param name="perms">Permissions</param>
        /// <param name="overWrite">Overwrite permissions if the user already exists</param>
        public void AddUsers(ulong[] ids, Permissions perms, bool overWrite = false)
        {
            bool setPerms = (perms != null);
            foreach (var id in ids)
            {
                if (userPerms.ContainsKey(id))
                {
                    if (setPerms && overWrite)
                    {
                        userPerms[id] = perms;
                    } else if (!setPerms && overWrite)
                    {
                        userPerms[id] = new Permissions();
                    }
                } else
                {
                    if (setPerms)
                    {
                        userPerms.Add(id, perms);
                    } else
                    {
                        userPerms.Add(id, new Permissions());
                    }
                }
            }
        }


        public string[] GetPermissionsByFriendlyName(IUser user)
        {
            return new string[] { "" };
        }

        /// <summary>
        /// Returns all permissions by friendly names for a specific role. If the object has permissions for Commands.Guild'Permissions and Commands.Guild, it will only return Commands.Guild instead of the command tree and command family. It will convert the actual names to friendly names such as Commands.Guild -> Admin, Commands.Guild'Permissions -> Edit Permissions, Commands.BotAdmin'Save -> Save Bot Config.
        /// </summary>
        /// <param name="role">Role</param>
        /// <returns></returns>
        public string[] GetPermissionsByFriendlyName(IRole role)
        {
            if (RoleExists(role))
            {
                List<string> toReturn = new List<string>();
                string[] perms = rolePerms[role.Id].perms;
                foreach (string str in perms)
                {
                    string commandFamily = str.Split('\'')[0];
                    string commandTree = str;
                    switch (commandFamily)
                    {
                        case "Commands.User":
                            if (perms.Any(commandFamily.Equals))
                            {
                                if (!(toReturn.Any((Permissions.permfamilies[0, 1].Equals))))
                                {
                                    toReturn.RemoveAll(n =>
                                    n.Equals(Permissions.commandsuseravatar[1])||
                                    n.Equals(Permissions.commandsuserhelp[1]));
                                    toReturn.Add(Permissions.permfamilies[0, 1]);
                                }
                                else
                                {
                                    toReturn.Add(Permissions.permfamilies[0, 1]);
                                }
                            }
                            else
                            {

                            }
                            continue;
                        case "Commands.Music":
                            if (perms.Any(commandFamily.Equals))
                            {

                            }
                            else
                            {

                            }
                            continue;
                        case "Commands.Guild":
                            if (perms.Any(commandFamily.Equals))
                            {

                            }
                            else
                            {

                            }
                            continue;
                        case "Commands.GuildOwner":
                            if (perms.Any(commandFamily.Equals))
                            {

                            }
                            else
                            {

                            }
                            continue;
                        case "Commands.BotAdmin":
                            if (perms.Any(commandFamily.Equals))
                            {
                                if (!(toReturn.Any((Permissions.permfamilies[2, 1]).Equals)))
                                {
                                    toReturn.Add(Permissions.permfamilies[2, 1]);
                                    toReturn.RemoveAll(n => n.Equals(Permissions.availablePerms));
                                }
                            }
                            else
                            {

                            }
                            continue;
                    }
                }
            }
            else
            {
                return new string[] { "" };
            }
            return new string[] { "" };
        }


        public PermissionsHandler()
        {

        }
    }

    class Permissions
    {
        public static string[] availablePerms = new string[] { "Commands.User", "Commands.Music", "Commands.BotAdmin", "Commands.GuildOwner", "Commands.Guild" };
        public static string[,] permfamilies = new string[5, 3] {
            { "Commands.User", "User Commands", "Access to all the standard user commands" },
            { "Commands.Music", "Music Commands", "Access to all the music commands" },
            { "Commands.BotAdmin", "Bot Admin Commands", "Access to all commands, including bot administration commands" },
            { "Commands.GuildOwner", "Server Owner Commands", "Access to all the guild owner commands" },
            { "Commands.Guild", "Server Admin Commands", "Access to all the guild admin commands" }
        };
        public static string[] commandsuser = new string[] { "Commands.User", "User Commands", "Access to all the standard user commands" };
        public static string[] commandsmusic = new string[] { "Commands.Music", "Music Commands", "Access to all the music commands" };
        public static string[] commandsbotadmin = new string[] { "Commands.BotAdmin", "Bot Admin Commands", "Access to all commands, including bot administration commands" };
        public static string[] commandsguildowner = new string[] { "Commands.GuildOwner", "Server Owner Commands", "Access to all the guild owner commands" };
        public static string[] commandsguild = new string[] { "Commands.Guild", "Server Admin Commands", "Access to all the guild admin commands" };

        public static string[,] guildperms = new string[5, 3] {
            { "Commands.Guild'Permissions", "Permissions", "Ability to change permissions" },
            { "Commands.Guild'WelcomeMessage", "WelcomeMessage", "Ability to change the welcome message"  },
            { "Commands.Guild'LeaveMessage", "LeaveMessage", "Ability to change the leave message"},
            { "Commands.Guild'DefaultRole", "DefaultRole", "Ability to modify the default role" },
            { "Commands.Guild'Prefix", "Prefix", "Ability to change the prefix" } };

        public static string[] commandsguildpermissions = new string[] { "Commands.Guild'Permissions", "Permissions", "Ability to change permissions" };
        public static string[] commandsguildwelcomemessage = new string[] { "Commands.Guild'WelcomeMessage", "WelcomeMessage", "Ability to change the welcome message" };
        public static string[] commandsguildleavemessage = new string[] { "Commands.Guild'LeaveMessage", "LeaveMessage", "Ability to change the leave message" };
        public static string[] commandsguilddefaultrole = new string[] { "Commands.Guild'DefaultRole", "DefaultRole", "Ability to modify the default role" };
        public static string[] commandsguildprefix = new string[] { "Commands.Guild'Prefix", "Prefix", "Ability to change the prefix" };

        public static string[,] userperms = new string[,]
        {
            { "Commands.User'Avatar", "Avatar", "Ability to grab a user's avatar" },
            { "Commands.User'Help", "Help", "Access to the help command" },
        };

        public static string[] commandsuseravatar = new string[] { "Commands.User'Avatar", "Avatar", "Ability to grab a user's avatar" };
        public static string[] commandsuserhelp = new string[] { "Commands.User'Help", "Help", "Access to the help command" };

        public static string[,] musicperms = new string[,]
        {
            { "Commands.Music'Connect", "Connect", "Connect the bot to a voice channel" },
            { "Commands.Music'Disconnect", "Disconnect", "Disconnect the bot from a voice channel" },
            { "Commands.Music'Play", "Play", "Play music, can be used to connect the bot to a voice channel" },
            { "Commands.Music'Stop", "Stop", "Stop music, can be used to disconnect the bot from a voice channel" },
            { "Commands.Music'Pause", "Pause", "Pause/Unpause music" }
        };

        public static string[] commandsmusicconnect = new string[] { "Commands.Music'Connect", "Connect", "Connect the bot to a voice channel" };
        public static string[] commandsmusicdisconnect = new string[] { "Commands.Music'Disconnect", "Disconnect", "Disconnect the bot from a voice channel" };
        public static string[] commandsmusicplay = new string[] { "Commands.Music'Play", "Play", "Play music, can be used to connect the bot to a voice channel" };
        public static string[] commandsmusicstop = new string[] { "Commands.Music'Stop", "Stop", "Stop music, can be used to disconnect the bot from a voice channel" };
        public static string[] commandsmusicpause = new string[] { "Commands.Music'Pause", "Pause", "Pause/Unpause music" };


        public string[] perms = new string[] { "Commands.User" };
    }
}
