using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using System.Threading.Tasks;
using Discord.WebSocket;
using System.Linq;

namespace MusicBot9001.GuildHandler
{
    class GuildHandle
    {
        public IGuild guild;  
        public Database database;
        public MusicHandler musicHandle;
        public PermissionsHandler permissionsHandler;
        public LevelHandler levelHandler;
        public Embed[] helpEmbeds = new Embed[0];
        /// <summary>
        /// Each help message per channel
        /// </summary>
        public IDictionary<ulong, Commands.User.HelpActions> channelHelps = new Dictionary<ulong, Commands.User.HelpActions>();
        public IDictionary<ulong, PrivateVC.PrivateVC> privateVCs = new Dictionary<ulong, PrivateVC.PrivateVC>();

        public GuildHandle(IGuild guild)
        {
            this.guild = guild;
            database = new Database(guild);
            musicHandle = new MusicHandler(guild, database);
            levelHandler = new LevelHandler(OnUserLevelUp, database);            
            //permissionsHandler = new PermissionsHandler();
            if (database.queue != null)
            {
                musicHandle.queue = database.queue;
            }
            foreach (var channel in guild.GetChannelsAsync().GetAwaiter().GetResult())
            {
                channelHelps.Add(channel.Id, null);
            }
            permissionsHandler = database.permissions;
            permissionsHandler.SetPerms(guild.OwnerId, new Permissions() { perms = new string[] { "Commands.Music", "Commands.User", "Commands.GuildOwner", "Commands.Guild" } });
            helpEmbeds = BuildEmbed(database.prefix);
            Task.Run(GuildThread);
            //levelHandler.levelup += OnUserLevelUp();
        }

        public async Task GuildThread()
        {
            while (true)
            {
                await Task.Delay(5000);
                for (int i = 0; i < privateVCs.Count; i++)
                {
                    if (i < privateVCs.Count)
                    {
                        var vc = privateVCs.ElementAt(i);
                        if (vc.Value.HasTimedOut)
                        {
                            try
                            {
                                var channel = vc.Value.VoiceChannel;
                                var role = vc.Value.Role;
                                var currentguilduser = await guild.GetCurrentUserAsync();
                                var roles = guild.Roles.Where(n => currentguilduser.RoleIds.Any(n.Id.Equals));
                                bool hasPermissions = roles.Any(n => (n.Permissions.ManageChannels || n.Permissions.Administrator));
                                if (hasPermissions)
                                {
                                    try
                                    {
                                        await channel.DeleteAsync();
                                        await role.DeleteAsync();
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error($"Could not delete VC {channel.Name}");
                                    }
                                }
                                else
                                {
                                    Logger.Warning($"I don't have appropriate permissions to manage channels on {guild.Name} ({guild.Id})");
                                }
                                vc.Value.Dispose();

                            }
                            catch (Exception e)
                            {
                                Logger.Error("Failed to cleanup timed out VC", e);
                            }
                            privateVCs.Remove(vc.Key);
                        }
                    }
                }
            }
        }

        private async Task OnUserLevelUp(ulong user, ulong channel = 0)
        {
            Logger.Info("Someone leveled up");
            if (database.rolesPerLevel.ContainsKey(levelHandler.GetUserLevels(user)))
            {
                var userToRole = await guild.GetUserAsync(user);
                var role = database.rolesPerLevel[levelHandler.GetUserLevels(user)];
                var currentguilduser = await guild.GetCurrentUserAsync();
                var roles = guild.Roles.Where(n => currentguilduser.RoleIds.Any(n.Id.Equals));
                bool hasPermissions = roles.Any(n => ((n.Permissions.ManageRoles || n.Permissions.Administrator) && n.Position >= role.Position) || n.Permissions.Administrator);

                if (hasPermissions)
                {
                    if (!(userToRole.RoleIds.Any(n => n.Equals(role.Id))))
                    {
                        try
                        {
                            await userToRole.AddRoleAsync(role);
                        }
                        catch (Exception)
                        {
                            Logger.Warning($"Could not set default role for {userToRole.Username} ({userToRole.Id}) on guild {guild.Name} ({guild.Id})");
                        }
                    }
                }
                else
                {
                    Logger.Warning("I don't have the permissions to set the role on guild " + guild.Name + " (" + guild.Id + ")");
                }
            }
            if (database.showLevelUpMessage)
            {
                if (channel != 0)
                {
                    var channe = await guild.GetChannelAsync(channel);
                    if (channe != null)
                    {
                        if (channe is ITextChannel)
                        {
                            var luser = (await guild.GetUserAsync(user));
                            if (luser.Nickname != null && luser.Nickname.Length > 0)
                            {
                                if (database.rolesPerLevel.ContainsKey(levelHandler.GetUserLevels(user)))
                                {
                                    await (channe as ITextChannel).SendMessageAsync($"`{luser.Nickname}` leveled up to level `{levelHandler.GetUserLevels(user)}` and earned the role `{database.rolesPerLevel[levelHandler.GetUserLevels(user)].Name}`");
                                }
                                else
                                {
                                    await (channe as ITextChannel).SendMessageAsync($"`{luser.Nickname}` leveled up to level `{levelHandler.GetUserLevels(user)}`");
                                }
                            }
                            else
                            {
                                if (database.rolesPerLevel.ContainsKey(levelHandler.GetUserLevels(user)))
                                {
                                    await (channe as ITextChannel).SendMessageAsync($"`{luser.Username}` leveled up to level `{levelHandler.GetUserLevels(user)}` and earned the role `{database.rolesPerLevel[levelHandler.GetUserLevels(user)].Name}`");
                                }
                                else
                                {
                                    await (channe as ITextChannel).SendMessageAsync($"`{luser.Username}` leveled up to level `{levelHandler.GetUserLevels(user)}`");
                                }
                            }
                        }
                    }
                }
            }
            return;
        }

        public async Task ChannelAdded()
        {
        }

        public async Task ChannelRemoved()
        {

        }

        public async Task ChannelUpdated()
        {

        }

        public async Task UserAdded(IUser user)
        {
            if (database.sendWelcomeMessage && database.welcomeLeaveChannel > 0)
            {
                if (database.giveDefaultRole)
                {
                    if (database.defaultRole != 0)
                    {
                        if (guild.Roles.Any(n => n.Id == database.defaultRole))
                        {
                            IRole role = guild.GetRole(database.defaultRole);
                            if (role != null)
                            {
                                bool hasPermissions = false;
                                var currentguilduser = await guild.GetCurrentUserAsync();
                                var roles = guild.Roles.Where(n => currentguilduser.RoleIds.Any(n.Id.Equals));
                                //hasPermissions = (roles.Any(n => n.Position <= role.Position) && roles.Any(n => n.Permissions.ManageRoles));
                                hasPermissions = roles.Any(n => ((n.Permissions.ManageRoles || n.Permissions.Administrator) && n.Position >= role.Position) || n.Permissions.Administrator);
                                if ((await guild.GetUserAsync(user.Id)).RoleIds.Any(n => !(n.Equals(database.defaultRole))) && hasPermissions)
                                {
                                    try
                                    {
                                        await (await guild.GetUserAsync(user.Id)).AddRoleAsync(role);
                                    }
                                    catch (Exception)
                                    {
                                        Logger.Warning($"Could not set default role for {user.Username} ({user.Id}) on guild {guild.Name} ({guild.Id})");
                                    }
                                }
                                else
                                {
                                    Logger.Warning("I don't have the permissions to set the role on guild " + guild.Name + " (" + guild.Id + ")");
                                    database.giveDefaultRole = false;
                                }
                            }
                        }
                        else
                        {
                            try
                            {
                                database.giveDefaultRole = false;
                                database.defaultRole = guild.GetRole(database.defaultRole).Id;
                            }
                            catch (Exception) { }
                        }
                    }
                }
                try
                {
                    var channel = await guild.GetChannelAsync(database.welcomeLeaveChannel);
                    ITextChannel textChannel = (channel as ITextChannel);
                    if (textChannel == null)
                    {
                        return;
                    }
                    else
                    {
                        await textChannel.SendMessageAsync(ParseVariablesInScript(database.welcomeMessage, user, guild));
                    }
                }
                catch (Exception)
                {

                }
            }
            if (!(database.userlevels.ContainsKey(user.Id)))
            {
                database.userlevels.Add(user.Id, new Levels());
            }
            //await (await guild.GetDefaultChannelAsync()).SendMessageAsync(ParseVariablesInScript(database.welcomeMessage, user, guild));
        }

        public async Task UserRemoved(IUser user)
        {
            if (database.sendLeaveMessage && database.welcomeLeaveChannel > 0)
            {
                try
                {
                    var channel = await guild.GetChannelAsync(database.welcomeLeaveChannel);
                    ITextChannel textChannel = (channel as ITextChannel);
                    if (textChannel == null)
                    {
                        return;
                    }
                    else
                    {
                        await textChannel.SendMessageAsync(ParseVariablesInScript(database.leaveMessage, user, guild));

                    }
                } catch (Exception)
                {

                }
            }
            //await (await guild.GetDefaultChannelAsync()).SendMessageAsync(ParseVariablesInScript(database.leaveMessage, user, guild));
        }

        public async Task UserUpdated()
        {

        }

        public async Task ReactionAddded(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {

            //Help context
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            { return; }
            switch (reaction.Emote.Name)
            {//⏪⏩⏮⏭
                case "⏪":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.back);
                    }
                    return;
                case "⏩":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.next);
                    }
                    return;
                case "⏮":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.first);
                    }
                    return;
                case "⏭":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.last);
                    }
                    return;
            }
        }

        public async Task ReactionRemoved(Cacheable<IUserMessage, ulong> cacheable, ISocketMessageChannel messageChannel, SocketReaction reaction)
        {

            //Help context
            if (reaction.User.IsSpecified && reaction.User.Value.IsBot)
            { return; }
            switch (reaction.Emote.Name)
            {
                case "⏪":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.back);
                    }
                    return;
                case "⏩":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.next);
                    }
                    return;
                case "⏮":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.first);
                    }
                    return;
                case "⏭":
                    if (channelHelps.ContainsKey(messageChannel.Id))
                    {
                        channelHelps[messageChannel.Id].seek(Commands.User.HelpActions.Seek.last);
                    }
                    return;
            }
        }
        
        public static Embed[] BuildEmbed(string prefix)
        {
            string helptext = Program.helpText;
            string[] helppages = helptext.Split("new-page");
            List<Embed> embeds = new List<Embed>();
            for (int i = 0; i < helppages.Length; i++)
            //foreach (string helptex in helppages)
            {
                helppages[i] = helppages[i].Replace("$(prefix)", prefix);
                helppages[i] = helppages[i].Replace("$(page)", (i + 1).ToString());
                helppages[i] = helppages[i].Replace("$(totalpagenum)", helppages.Length.ToString());
                EmbedBuilder embedBuilder = new EmbedBuilder();
                string title = "Help";
                string description = "Empty";
                string[] split = helppages[i].Split(new string[] { "<description>", "</description>" }, StringSplitOptions.None);
                if (split.Length > 1)
                    description = split[1];
                string[] splittitle = helppages[i].Split(new string[] { "<title>", "</title>" }, StringSplitOptions.None);
                if (split.Length > 1)
                    title = splittitle[1];
                embedBuilder.Title = title;
                embedBuilder.Color = Color.Blue;
                embedBuilder.Description = description;
                embeds.Add(embedBuilder.Build());
            }
            return embeds.ToArray();
        }

        public string ParseVariablesInScript(string message, IUser user = null, IGuild guild = null)
        {
            if (!(user == null))
            {
                message = message.Replace("$(user.Name)", user.Username);
                message = message.Replace("$(user.Id)", user.Id.ToString());
            }
            if (!(guild == null))
            {
                message = message.Replace("$(guild.Name)", guild.Name);
                message = message.Replace("$(guild.Id)", guild.Id.ToString());
            }
            return message;
        }
    }
}
