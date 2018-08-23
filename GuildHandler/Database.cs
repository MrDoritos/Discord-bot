using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Discord;
using Discord.WebSocket;
using System.IO;

namespace MusicBot9001.GuildHandler
{
    class Database
    {
        private JObject _json;

        public IGuild guild;
        public string guildName;
        public ulong guildId;
        public string prefix;
        public string fileName;
        public bool sendWelcomeMessage;
        public bool sendLeaveMessage;
        public string welcomeMessage;
        public string leaveMessage;
        public ulong welcomeLeaveChannel;
        public ulong defaultRole;
        public bool giveDefaultRole;
        public bool useLevels;
        public bool showLevelUpMessage;
        public ulong musicChannel;
        public LoopType loop;
        public GuildHandler.Music.Queue queue;
        public PermissionsHandler permissions;
        public IDictionary<ulong, Levels> userlevels;
        public IDictionary<int, IRole> rolesPerLevel;
        public int voiceChannelTimeOut;

        public enum LoopType
        {
            NoLoop = 0,
            SingleLoop = 1,
            QueueLoop = 2
        }
                
        public Database(IGuild guild)
        {
            this.guild = guild;
            fileName = guild.Id.ToString() + ".json";
            guildId = guild.Id;

            if (File.Exists("Guilds\\" + fileName))
            {
                try
                {
                    _json = JObject.Parse(File.ReadAllText("Guilds\\" + fileName));
                    ParseConfig();
                }
                catch (Exception e)
                {
                    GenerateConfig();
                }
            }
            else
            {
                File.Create("Guilds\\" + fileName).Close();
                GenerateConfig();
            }
        }

        public void GenerateConfig()
        {
            _json = new JObject();
            guildName = guild.Name;
            guildId = guild.Id;
            prefix = "$";
            sendLeaveMessage = true;
            sendWelcomeMessage = true;
            defaultRole = 0;
            giveDefaultRole = false;
            welcomeMessage = "uh oh, $(user.Name) just joined";
            leaveMessage = "$(user.Name) just left, big sad";
            permissions = new PermissionsHandler();
            userlevels = new Dictionary<ulong, Levels>();
            rolesPerLevel = new Dictionary<int, IRole>();
            showLevelUpMessage = true;
            useLevels = true;
            voiceChannelTimeOut = 60;
            foreach (var user in guild.GetUsersAsync().GetAwaiter().GetResult())
            {
                userlevels.Add(user.Id, new Levels());
            }
            try
            {
                welcomeLeaveChannel = guild.GetDefaultChannelAsync().Result.Id;
            }
            catch (Exception)
            {
                welcomeLeaveChannel = 0;
            }
            try
            {
                musicChannel = guild.GetDefaultChannelAsync().Result.Id;
            }
            catch (Exception)
            {
                musicChannel = 0;
            }
            loop = 0;
            queue = new Music.Queue();

            SaveConfig();
        }

        public void ParseConfig()
        {
            if (_json.ContainsKey("guildName"))
            { guildName = (string)_json["guildName"]; }
            if (_json.ContainsKey("guildId"))
            { guildId = (ulong)_json["guildId"]; }
            if (_json.ContainsKey("prefix"))
            { prefix = (string)_json["prefix"]; }
            if (_json.ContainsKey("sendLeaveMessage"))
            { sendLeaveMessage = (bool)_json["sendLeaveMessage"]; }
            if (_json.ContainsKey("sendWelcomeMessage"))
            { sendWelcomeMessage = (bool)_json["sendWelcomeMessage"]; }
            if (_json.ContainsKey("welcomeMessage"))
            { welcomeMessage = (string)_json["welcomeMessage"]; }
            if (_json.ContainsKey("leaveMessage"))
            { leaveMessage = (string)_json["leaveMessage"]; }
            if (_json.ContainsKey("welcomeLeaveChannel"))
            { welcomeLeaveChannel = (ulong)_json["welcomeLeaveChannel"]; }
            if (_json.ContainsKey("musicChannel"))
            { musicChannel = (ulong)_json["musicChannel"]; }
            if (_json.ContainsKey("giveDefaultRole"))
            { giveDefaultRole = (bool)_json["giveDefaultRole"]; }
            if (_json.ContainsKey("useLevels"))
            { useLevels = (bool)_json["useLevels"]; }
            if (_json.ContainsKey("showLevelUpMessage"))
            { showLevelUpMessage = (bool)_json["showLevelUpMessage"]; }
            if (_json.ContainsKey("voiceChannelTimeOut"))
            { voiceChannelTimeOut = (int)_json["voiceChannelTimeOut"]; }
            if (_json.ContainsKey("defaultRole"))
            {
                if (guild.Roles.Any(n => n.Id == (ulong)_json["defaultRole"]))
                {
                    defaultRole = (ulong)_json["defaultRole"];
                }
                else
                {
                    giveDefaultRole = false;
                }
            }
            if (_json.ContainsKey("loop"))
            { loop = (LoopType)(int)_json["loop"]; }
            if (_json.ContainsKey("queue"))
            { queue = new Music.Queue();
                foreach (var song in _json.Properties().Where(n => n.Name == "queue").First().Values())
                {
                    var so = new Music.ISong();
                    so.fileName = (string)song;
                    if (so.FileExists())
                    { queue.Enqueue(so); }
                }
            }
            permissions = new PermissionsHandler();
            if (_json.ContainsKey("permissions"))
            {
                try
                {
                    foreach (var perm in _json["permissions"].Children<JProperty>())
                    {
                        ulong.TryParse(perm.Name, out ulong id);
                        if (!permissions.userPerms.ContainsKey(id))
                        {
                            permissions.userPerms.Add(id, new Permissions() { perms = new string[] { "" } });
                            List<string> permstoadd = new List<string>(new string[] { "" });
                            if (perm.HasValues)
                                foreach (var pee in perm.Values())
                                {
                                    permstoadd.Add((string)pee);
                                }
                            permissions.userPerms[id].perms = permstoadd.ToArray();
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Could not load old permissions for guild " + guild.Name + " (" + guild.Id + ")" + e);
                }
            }
            if (_json.ContainsKey("rolepermissions"))
            {
                try
                {
                    foreach (var roleperm in _json["rolepermissions"].Children<JProperty>())
                    {
                        ulong.TryParse(roleperm.Name, out ulong roleid);
                        if (!permissions.rolePerms.ContainsKey(roleid))
                        {
                            permissions.rolePerms.Add(roleid, new Permissions() { perms = new string[] { "" } });
                            List<string> rolestoadd = new List<string>(new string[] { "" });
                            if (roleperm.HasValues)
                            {
                                foreach (var rolepee in roleperm.Values())
                                {
                                    rolestoadd.Add((string)rolepee);
                                }
                            }
                            permissions.rolePerms[roleid].perms = rolestoadd.ToArray();
                        }
                    }
                } catch(Exception e)
                {
                    Logger.Error("Could not load old role permissions for guild " + guild.Name + " (" + guild.Id + ")" + e);
                }
            }
            userlevels = new Dictionary<ulong, Levels>();
            foreach (var user in guild.GetUsersAsync().GetAwaiter().GetResult())
            {
                userlevels.Add(user.Id, new Levels());
            }
            if (_json.ContainsKey("userLevels"))
            {
                try
                {
                    foreach (var userlevel in _json["userLevels"].Children<JProperty>())
                    {
                        ulong.TryParse(userlevel.Name, out ulong userid);
                        if (!userlevels.ContainsKey(userid))
                        {
                            userlevels.Add(userid, new Levels() { level = (int)userlevel.Value["level"], messagesSentSinceLevelUp = (int)userlevel.Value["messagesSentSinceLevelUp"], totalMessageSent = (int)userlevel.Value["totalMessagesSent"] });
                        }
                        else
                        {
                            userlevels[userid].level = (int)userlevel.Value["level"];
                            userlevels[userid].messagesSentSinceLevelUp = (int)userlevel.Value["messagesSentSinceLevelUp"];
                            userlevels[userid].totalMessageSent = (int)userlevel.Value["totalMessagesSent"];
                        }
                    }
                } catch (Exception e)
                {
                    Logger.Error($"Could not load old userLevels for guild {guild.Name} ({guild.Id})" + e);
                }
            }
            rolesPerLevel = new Dictionary<int, IRole>();
            if (_json.ContainsKey("roleLevels"))
            {
                try
                {
                    var roleids = guild.Roles;
                    foreach (var rolelevel in _json["roleLevels"].Children<JProperty>())
                    {
                        ulong.TryParse(rolelevel.Name, out ulong roleid);
                        int key = (int)rolelevel.Values().First();
                        if (roleids.Select(n => n.Id).Any(n => n.Equals(roleid)))
                        {
                            rolesPerLevel.Add(key, roleids.First(n => n.Id == roleid));
                        }                        
                    }
                } catch(Exception e)
                {
                    Logger.Error($"Could not load old roleLevels for guild {guild.Name} ({guild.Id})", e);
                }
            }
        }

        public void SaveConfig()
        {
            _json = new JObject();
            _json.Add("guildName", guildName);
            _json.Add("guildId", guildId);
            _json.Add("prefix", prefix);
            _json.Add("sendWelcomeMessage", sendWelcomeMessage);
            _json.Add("sendLeaveMessage", sendLeaveMessage);
            _json.Add("welcomeMessage", welcomeMessage);
            _json.Add("leaveMessage", leaveMessage);
            _json.Add("welcomeLeaveChannel", welcomeLeaveChannel);
            _json.Add("musicChannel", musicChannel);
            _json.Add("loop", (int)loop);
            _json.Add("queue", new JArray(queue.songs.Select(n => n.fileName)));
            _json.Add("giveDefaultRole", giveDefaultRole);
            _json.Add("defaultRole", defaultRole);
            _json.Add("showLevelUpMessage", showLevelUpMessage);
            _json.Add("useLevels", useLevels);
            _json.Add("voiceChannelTimeOut", voiceChannelTimeOut);
            //_json.Add("permissions", new JObject(permissions.userPerms.Select(n => new JObject(n.Key)))
            try
            {
                JObject permission = new JObject();
                foreach (var a in permissions.userPerms)
                {
                    permission.Add(a.Key.ToString(), new JArray(a.Value.perms));
                }
                _json.Add("permissions", permission);
            } catch(Exception e)
            {
                Logger.Error("Writing permissions to " + guild.Name + " (" + guild.Id + ")'s database", e);
            }
            try
            {
                JObject rolepermission = new JObject();
                foreach (var a in permissions.rolePerms)
                {
                    rolepermission.Add(a.Key.ToString(), new JArray(a.Value.perms));
                }
                _json.Add("rolepermissions", rolepermission);
            } catch(Exception e)
            {
                Logger.Error("Writing role permissions to " + guild.Name + " (" + guild.Id + ")'s database", e);
            }
            try
            {
                JObject levels = new JObject();
                foreach (var a in userlevels)
                {
                    JObject user = new JObject();
                    user.Add("level", a.Value.level);
                    user.Add("messagesSentSinceLevelUp", a.Value.messagesSentSinceLevelUp);
                    user.Add("totalMessagesSent", a.Value.totalMessageSent);
                    levels.Add(a.Key.ToString(), user);
                }
                _json.Add("userLevels", levels);
            }
            catch (Exception e)
            {
                Logger.Error($"Writing user levels to {guild.Name} ({guild.Id})'s database");
            }
            try
            {
                JObject roleLevels = new JObject();
                foreach (var a in rolesPerLevel)
                {
                    //JObject rolelevel = new JObject();
                    //rolelevel.Add("level", a.Key);
                    roleLevels.Add(a.Value.ToString(), a.Key);
                }
                _json.Add("roleLevels", roleLevels);
            } catch(Exception e)
            {
                Logger.Error($"Writing roleLevels to {guild.Name} ({guild.Id})'s database");
            }

            try
            {
                File.WriteAllText("Guilds\\" + fileName, JsonConvert.SerializeObject(_json));
            }
            catch (Exception)
            {

            }
        }
    }
}
