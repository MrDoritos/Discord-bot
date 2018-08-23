using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using Discord;

namespace MusicBot9001.GuildHandler
{
    class LevelHandler
    {
        private IDictionary<ulong, Levels> userlevels = new Dictionary<ulong, Levels>();
        private Func<ulong, ulong, Task> _levelUpMessageMethod;
        
        public LevelHandler(Func<ulong, ulong, Task> onUserLevelUp, Database database)
        {
            userlevels = database.userlevels;
            //this.userlevels = userlevels;
            _levelUpMessageMethod = onUserLevelUp ?? throw new ArgumentNullException("onUserLevelUp cannot be null");
        }

        public int GetUserLevels(ulong user)
        {
            if (userlevels.ContainsKey(user))
            {
                return userlevels[user].level;
            }
            else
            {
                userlevels.Add(user, new Levels());
                return userlevels[user].level;
            }
        }

        public IDictionary<ulong, Levels> GetUsersLevels(ulong[] users)
        {
            return userlevels.Where(n => users.Any(n.Key.Equals)) as Dictionary<ulong, Levels>;
        }

        public void AddUser(ulong user)
        {
            if (!(userlevels.ContainsKey(user)))
            {
                userlevels.Add(user, new Levels());
            }
        }

        public bool UserExists(ulong user)
        {
            return (userlevels.ContainsKey(user));
        }

        /// <summary>
        /// Set a user's level, if they don't exist in the dictionary they will be added to the dictionary
        /// </summary>
        /// <param name="user">The user's Id</param>
        /// <param name="level">The level to set the user to</param>
        /// <returns>The current level of the user</returns>
        public int SetUserLevels(ulong user, int level)
        {
            if (userlevels.ContainsKey(user))
            {
                userlevels[user].level = level;
                return level;
            }
            else
            {
                userlevels.Add(user, new Levels() { level = level });
                return level;
            }
        }

        /// <summary>
        /// Advance a user's level, if they don't exist in the dictionary they will be added to the dictionary
        /// </summary>
        /// <param name="user">The user's Id</param>
        /// <param name="levels">The number of levels to advance the user</param>
        /// <returns>The current level of the user</returns>
        public int AdvanceUserLevel(ulong user, int levels, ulong channel)
        {
            if (userlevels.ContainsKey(user))
            {                
                userlevels[user].level += levels;
                _levelUpMessageMethod(user, channel);
                userlevels[user].messagesSentSinceLevelUp = 0;
                return userlevels[user].level;
            }
            else
            {
                userlevels.Add(user, new Levels() { level = levels });
                _levelUpMessageMethod(user, channel);
                return userlevels[user].level;
            }
        }

        public void ResetUser(ulong user)
        {
            if (userlevels.ContainsKey(user))
            {
                userlevels[user] = new Levels();
            }
        }
        
        public void MessageSent(ulong user, ulong channel)
        {
            if (userlevels.ContainsKey(user))
            {
                userlevels[user].messagesSentSinceLevelUp++;
                userlevels[user].totalMessageSent++;
                if (LevelUp(userlevels[user].messagesSentSinceLevelUp, userlevels[user].level))
                {
                    AdvanceUserLevel(user, 1, channel);
                }
            }
            else
            {
                userlevels.Add(user, new Levels() { totalMessageSent = 1, messagesSentSinceLevelUp = 1 });
            }
        }

        /// <summary>
        /// Determines if a user is ready to level up
        /// </summary>
        /// <param name="messages">Amount of messages sent since last level up</param>
        /// <param name="level">Current level</param>
        /// <returns>If to level up</returns>
        public bool LevelUp(int messages, int level)
        {
            if (level < 1)
                return true;
            if (messages == 0)
                return false;
            //return (messages < ((messages / level) * 10));
            return (messages > level);
        }
    }

    class Levels
    {
        public int level = 0;
        public int messagesSentSinceLevelUp = 0;
        public int totalMessageSent = 0;
    }
}
