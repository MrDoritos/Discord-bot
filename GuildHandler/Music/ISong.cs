using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Discord;

namespace MusicBot9001.GuildHandler.Music
{
    class ISong : IDisposable
    {
        public TimeSpan length = TimeSpan.FromMilliseconds(0);
        public TimeSpan currentTrackTimeOffset = TimeSpan.FromMilliseconds(0);
        public string fileName = null;
        public string title = null;
        public string url = null;
        public bool attemptedDownload = false;
        public bool played = false;
        public IGuildUser requester = null;
                
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public ISong WithURL(string url)
        {
            this.url = url;
            return this;
        }

        public ISong WithTitle(string title)
        {
            this.title = title;
            return this;
        }

        public ISong WithDuration(TimeSpan duration)
        {
            length = duration;
            return this;
        }

        public bool HasTitle()
        {
            return (title != null);
        }

        public bool HasLength()
        {
            return (length.Milliseconds != 0);
        }

        public bool FileExists()
        {
            if (fileName != null)
            {
                if (File.Exists(Program.config.musicPath + fileName))
                {
                    if (!HasTitle())
                    {
                        title = TagLib.File.Create(Program.config.musicPath + fileName).Tag.Title;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (File.Exists(Program.config.musicPath + Download.ID(url) + ".m4a"))
                {
                    fileName = Download.ID(url) + ".m4a";
                    if (!HasTitle())
                    {
                        title = TagLib.File.Create(Program.config.musicPath + fileName).Tag.Title;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // get rid of managed resources
            }
            // get rid of unmanaged resources
        }
    }
}
