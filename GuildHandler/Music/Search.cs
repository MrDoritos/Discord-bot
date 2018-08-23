using System;
using System.Collections.Generic;
using System.Text;
using YoutubeSearch;

namespace MusicBot9001.GuildHandler.Music
{
    class Search
    {
        static public string SearchYTTitle(string query)
        {
            var search = new VideoSearch().SearchQuery(query, 1);
            return search[0].Title;
        }

        static public string SearchYTURL(string query)
        {
            var search = new VideoSearch().SearchQuery(query, 1);
            return search[0].Url;
        }

        static public ISong SearchYTGetSong(string query)
        {
            var search = new VideoSearch().SearchQuery(query, 1);
            TimeSpan duration = TimeSpan.FromMilliseconds(0);
            try
            {
                //duration = search[0].Duration
            }
            catch (Exception) { }            
            return new ISong().WithDuration(duration).WithTitle(search[0].Title).WithURL(search[0].Url);
        }

        static public List<ISong> SearchYTGetSongs(string query)
        {
            List<ISong> songs = new List<ISong>();
            var search = new VideoSearch().SearchQuery(query, 1);
            foreach (var result in search)
            {
                try
                {
                    TimeSpan duration = TimeSpan.FromMilliseconds(0);
                    var song = new ISong().WithURL(result.Url).WithTitle(result.Title).WithDuration(duration);
                    song.fileName = Download.ID(result.Url) + ".m4a";
                    songs.Add(song);
                }
                catch (Exception) { }
            }
            return songs;
        }
    }
}
