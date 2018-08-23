using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Discord;
using System.Threading.Tasks;

namespace MusicBot9001.GuildHandler.PrivateVC
{
    class PrivateVC : IDisposable
    {
        public List<ulong> AllowedRoles;
        public List<ulong> AllowedUsers;
        public IVoiceChannel VoiceChannel { get; private set; }
        public IRole Role { get; private set; }
        public IGuildUser Owner { get; private set; }
        //private List<ulong> _joinedUsers = new List<ulong>();
        private TimeSpan _timeSinceLastUsed = new TimeSpan(0, 0, 0, 0);
        private DateTime TimeLastUsed = DateTime.Now;
        private int _timeout;
        
        public TimeSpan TimeSinceLastUsed
        {
            get { return new TimeSpan(DateTime.Now.Ticks - TimeLastUsed.Ticks); }
        }

        public async Task<bool> IsPopulated()
        {   
            var users = (await ((VoiceChannel as IAudioChannel).GetUsersAsync()).FlattenAsync());            
            return users.Count() > 0;            
        }

        public bool HasTimedOut
        {
            get { return (!IsPopulated().GetAwaiter().GetResult() && _timeout < TimeSinceLastUsed.TotalSeconds); }
        }

        public PrivateVC(IGuildUser owner, IVoiceChannel voiceChannel, IRole role, int timeout = 60)
        {
            Role = role;
            AllowedRoles = new List<ulong>();
            AllowedUsers = new List<ulong>();
            VoiceChannel = voiceChannel;
            _timeout = timeout;
            Owner = owner;
        }

        //public void UserJoined(ulong user)
        //{
        //    if (!(_joinedUsers.Contains(user)))
        //    {
        //        _joinedUsers.Add(user);
        //    }
        //}

        //public void UserLeft(ulong user)
        //{
        //    if (_joinedUsers.Contains(user))
        //    {
        //        _joinedUsers.RemoveAll(n => n == user);
        //    }
        //    if (!IsPopulated)
        //    {
        //        TimeLastUsed = DateTime.Now;
        //    }
        //}

        public void AddRole(ulong role)
        {
            if (!AllowedRoles.Any(role.Equals))
            {
                AllowedRoles.Add(role);
            }
        }

        public void AddRoles(IEnumerable<ulong> roles)
        {
            var toAdd = roles.Where(n => !(AllowedRoles.Any(n.Equals)));
            AllowedRoles.AddRange(toAdd);
        }

        public void AddUser(ulong user)
        {
            if (!AllowedUsers.Any(user.Equals))
            {
                AllowedUsers.Add(user);
            }
        }

        public void AddUsers(IEnumerable<ulong> users)
        {
            var toAdd = users.Where(n => !(AllowedUsers.Any(n.Equals)));
            AllowedUsers.AddRange(toAdd);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
