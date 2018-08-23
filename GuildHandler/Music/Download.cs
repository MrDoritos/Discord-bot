using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading.Tasks;
using TagLib;
using SimpleYoutubePlaylist;
using Google;
using Google.Apis;
using Google.Apis.Services;
using Google.Apis.Auth;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube;
using System.Linq;

namespace MusicBot9001.GuildHandler.Music
{
    class Download
    {
        public static Regex getid = new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)");
        public static Regex id = new Regex(@"([a-zA-Z0-9-_]+)");
        public static Regex playList = new Regex(@"youtu(?:be\.com|.be)\/(?:watch\?v=(?:[a-zA-Z0-9_-]+)&list=([a-zA-Z0-9_-]+)|playlist\?list=([a-zA-Z0-9_-]+))");

        public static string ID(string url)
        {
            return getid.Match(url).Groups[1].Value;
        }

        public static string PlayListID(string url)
        {
            return playList.Match(url).Groups[1].Value;
        }

        public static List<ISong> Playlist(string url)
        {
            //            new SimpleYoutubePlaylist.YouTubePlayList(,
            //"AIzaSyDaWrh5nKLEt5ZZa68Q8sodZJzJJN9cemc"
            //, 100);
            List<ISong> songs = new List<ISong>();

            try
            {
                var PlayListRequest = Program.youtubeService.PlaylistItems.List("snippet");
                PlayListRequest.PlaylistId = (PlayListID(url));
                PlayListRequest.MaxResults = 50;
                var PlayListResponse = PlayListRequest.Execute();

                foreach (var vid in PlayListResponse.Items)
                {
                    try
                    {
                        if (vid.Snippet != null)
                        {
                            if (vid.Snippet.Title != null && vid.Snippet.Title != "Deleted video")
                            {
                                var song = (new ISong().WithTitle(vid.Snippet.Title).WithURL("https://www.youtube.com/watch?v=" + vid.Snippet.ResourceId.VideoId));
                                song.fileName = (vid.Snippet.ResourceId.VideoId) + ".m4a";                                
                                songs.Add(song);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Downloading playing data: " + url, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Obtaing Playlist items", ex);
            }
            //Obsolete
            //YouTubePlayList pl = new YouTubePlayList(playList.Match(url).Groups[1].Value, "AIzaSyDaWrh5nKLEt5ZZa68Q8sodZJzJJN9cemc", 100);
            return songs;
        }

        /// <summary>
        /// Manages the downloading of a video
        /// </summary>
        /// <param name="url">Video url</param>
        /// <param name="path">Path to music folder</param>
        /// <returns>Full path to song</returns>
        public static async Task<string> DownloadVid(string url, string path = null)
        {
            //Youtube link
            if (getid.IsMatch(url) || id.IsMatch(url))
            {
                //Tag tag = TagLib.File.Create(path).Tag;
                
                return await YouTube_DL(url, Program.config.musicPath + ID(url) + ".m4a");
            } else
            {
                return null;
            }
        }

        public static async Task<ISong> GetSongAsync(string url, string path = null)
        {
            try
            {
                //Youtube link
                if (getid.IsMatch(url) || id.IsMatch(url))
                {
                    ISong Song = new ISong();
                    Song.attemptedDownload = true;
                    //Tag tag = TagLib.File.Create(path).Tag;
                    if (System.IO.File.Exists(Program.config.musicPath + ID(url) + ".m4a"))
                    {

                    }
                    else
                    {
                        var song = await YouTube_DL(url, Program.config.musicPath + ID(url) + ".m4a");
                    }
                    Song.fileName = ID(url) + ".m4a";
                    File file = null;
                    int numoftries = 0;
                    while (numoftries < 5)
                    {
                        try
                        {
                            file = TagLib.File.Create(Program.config.musicPath + ID(url) + ".m4a");
                            break;
                        }
                        catch (Exception)
                        {
                            await Task.Delay(500);
                            numoftries++;
                        }
                    }
                    Song.title = System.Web.HttpUtility.HtmlDecode(file.Tag.Title);
                    Song.url = url;
                    Song.length = new TimeSpan(0,0,0,0, (int)(file.Properties.Duration.TotalMilliseconds));
                    return Song;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Downloads the file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="Path">Full path</param>
        /// <returns>Filename</returns>
        private static async Task<string> YouTube_DL(string url, string Path)
        {
            try
            {
                var YTDL = Process.Start(new ProcessStartInfo
                {
                    FileName = @"youtube-dl.exe",//old args: --extract-audio --audio-format mp3 -i -o D:\\BOTDL\\" + filename + "temp
                    Arguments = @" " + url + " -f 140 -o \"" + Program.config.musicPath + "%(id)s.m4a\" --add-metadata ",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Minimized
                });
                while (!(YTDL.HasExited))
                {
                    await Task.Delay(1000);
                }
                await Task.Delay(500);
                //while (!(System.IO.File.Exists(Path + ".m4a")))
                //{
                //    await Task.Delay(100);
                //}
            } catch(Exception e)
            {

            }
            return Path;
        }
    }
}
