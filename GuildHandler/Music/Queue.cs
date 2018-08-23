using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace MusicBot9001.GuildHandler.Music
{
    class Queue
    {
        public List<ISong> songs = new List<ISong>();
        
        public Queue() {

            Task.Run(DownloaderThread);
        }

        public void Enqueue(ISong song)
        {
            if (song != null)
            {
                songs.Add(song);
            }
        }

        public ISong Dequeue()
        {
            if (songs.Count > 0)
            {
                var song = songs[0];
                songs.RemoveAt(0);
                return song;
            }
            else
            {
                return null;
            }
        }

        public ISong Peek()
        {
            if (songs.Count > 0)
            {
                return songs[0];
            }
            else
            {
                return null;
            }
        }

        public async Task DownloaderThread()
        {
            while (true)
            {
                try
                {
                    if (songs.Count > 0)
                    {
                        if (songs[0] == null)
                        {
                            songs.RemoveAt(0);
                        }
                        if (!songs[0].FileExists() && !songs[0].attemptedDownload)
                        {
                            songs[0] = await Download.GetSongAsync(songs[0].url);
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }
                    }
                } catch(Exception e)
                {
                    Logger.Error("Downloading song", e);
                }
                await Task.Delay(1000);
            }
        }

        public Queue AddMany(List<ISong> songs)
        {
            this.songs.AddRange(songs);
            return this;
        }
        
        public void RemoveSong(ISong song)
        {
            if (song != null && songs.Contains(song))
            songs.Remove(song);
        }

        public EmbedBuilder GetQueueEmbed()
        {
            if (songs != null)
            {
                string wrapper = "";
                for (int i = 0; i < songs.Count; i++)
                {
                        if (songs[i].HasTitle())
                        {
                            wrapper += "[" + (i + 1).ToString() + "] " + songs[i].title + "\n";
                        }
                        else
                        {
                            wrapper += "[" + (i + 1).ToString() + "] " + songs[i].fileName + "\n";
                        }
                    
                }
                var pieceOFSHIT = new EmbedBuilder().WithTitle("Queue").WithColor(Color.Blue).WithDescription(wrapper);
                return pieceOFSHIT;
            }
            else
            {
                return new EmbedBuilder().WithTitle("Queue").WithColor(Color.Orange).WithDescription("Nothing in queue");
            }
        } 
    }
}
