using System;
using System.Collections.Generic;
using System.Text;
using Discord;
using Discord.Audio;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicBot9001.GuildHandler
{
    class MusicHandler
    {
        public Music.Queue queue = new Music.Queue();
        private Music.ISong _nowPlaying = null;
        public Music.Play _currentFFMpegProc = new Music.Play();
        private Task MusicThreadTask_ = null;
        private bool RunMusicThread = false;
        private bool AudioStopped = false;
        public Database.LoopType loopType = Database.LoopType.NoLoop;

        /// <summary>
        /// Access this to change the song?
        /// </summary>
        public Music.ISong NowPlaying
        {
            get { return _nowPlaying; }
        }
        public IGuild guild = null;
        public IAudioChannel audioChannel = null;
        public IAudioClient audioClient = null;
        public AudioOutStream audioStream = null;
        public IGuildChannel musicChannel = null;
        
        public bool AudioPlaying = false;
        public bool AudioPaused = false;

        public MusicHandler(IGuild guild, Database database)
        {
            this.guild = guild;
            musicChannel = guild.GetChannelAsync(database.musicChannel).GetAwaiter().GetResult();
            Task.Run(MusicThread);
        }

        public async Task SendMessage(string message)
        {
            if (Program.mainHandler.guildHandles[guild.Id].database.musicChannel != 0)
            {
                var musicChannel = guild.GetChannelAsync(Program.mainHandler.guildHandles[guild.Id].database.musicChannel) as ITextChannel;
                if (musicChannel != null)
                {
                    await (musicChannel.SendMessageAsync(message));
                }
            }
        }

        /// <summary>
        /// Clear everything from the queue
        /// </summary>
        /// <returns></returns>
        public string ClearQueue()
        {
            if (queue.songs.Count > 0)
            {
                int count = queue.songs.Count;
                queue.songs = new List<Music.ISong>();
                if (count > 1)
                    return "Cleared `" + count + "` songs from the queue";
                else
                    return "Cleared 1 song from the queue";
            }
            else
            {
                return "Queue is empty";
            }
        }

        public string ClearQueue(int count)
        {
            int queuenum = queue.songs.Count;
            if (queuenum > 0 && count > 0)
            {
                if (count > queuenum)
                {
                    queue.songs = new List<Music.ISong>();
                    return "Cleared all songs";
                }
                else if (count <= queuenum)
                {
                    queue.songs.RemoveRange(0, count);
                    if (count > 1)
                        return "Cleared `" + count + "` songs from the queue";
                    else
                        return "Cleared 1 song from the queue";
                }
                else
                {
                    return "Cleared no songs";
                }
                //queue.songs.RemoveRange(0, count);
            }
            else if (queuenum > 0)
            {
                return "Cleared no songs";
            }
            else if (count > 0)
            {
                return "Queue is empty";
            }
            return "Cleared no songs and the queue is empty";
        }

        //public string ClearQueue(int index, int count)
        //{
            
        //}

        public string ClearQueue(IEnumerable<IUser> user)
        {
            if (user != null)
            {
                int count = queue.songs.Count(n => user.Any(m => m.Id == n.requester.Id));
                queue.songs.RemoveAll(n => user.Any(m => m.Id == n.requester.Id));
                if (count > 1)
                {
                    return "Cleared `" + count + "` songs";
                }
                else if (count == 1)
                {
                    return "Cleared `1` song";
                }
                else
                {
                    return "Cleared no songs";
                }
            }
            else
            {
                return "Cleared no songs";
            }
        }

        public string ClearQueue(IEnumerable<IRole> role)
        {
            if (role != null)
            {
                int count = queue.songs.Count(n => n.requester.RoleIds.Any(m => role.Any(m.Equals)));
                queue.songs.RemoveAll(n => n.requester.RoleIds.Any(m => role.Any(m.Equals)));
                if (count > 1)
                {
                    return "Cleared `" + count + "` songs";
                } else if (count == 1)
                {
                    return "Cleared `1` song";
                }
                else
                {
                    return "Cleared no songs";
                }
            }
            else
            {
                return "Cleared no songs";
            }
        }

        public string ClearQueue(Music.ISong song, int count = Int32.MaxValue)
        {
            if (count > 0)
            {
                int queuenum = queue.songs.Count(n => n == song);
                int actnum = 0;
                for (int i = 0; i < count || i < queuenum; i++)
                {
                    queue.songs.Remove(song);

                    actnum++;
                }
                if (actnum > 1)
                {
                    return "Cleared `" + actnum + "` songs";
                }
                else if (queuenum == 1)
                {
                    return "Cleared `1` song";
                }
                else
                {
                    return "Cleared no songs";
                }
            }
            else
            {
                return "Cleared no songs";
            }
        }
        
        //public async Task MusicThread()
        //{
        //    while (true)
        //    {                
        //        while (queue.songs.Count > 0 && audioClient != null && audioChannel != null)
        //        {
        //            if (audioClient != null)
        //            {
        //                _nowPlaying = queue.songs[0];
        //                //Consume a song
        //                queue.songs.RemoveAt(0);
        //                if (_nowPlaying.attemptedDownload || _nowPlaying.FileExists())
        //                {
        //                    try
        //                    {
        //                        _currentFFMpegProc = new Music.Play();
        //                        if (_nowPlaying.HasTitle())
        //                        {
        //                            await SendMessage("Now playing `" + _nowPlaying.title + "`");
        //                        }
        //                        else
        //                        {
        //                            await SendMessage("Now playing `" + _nowPlaying.url + "`");
        //                        }
        //                        await _currentFFMpegProc.PlaySongNAudio(0, _nowPlaying.fileName, audioClient);
        //                    } catch(Exception e)
        //                    {
        //                        Logger.Error("Playing song, maybe track skip?");
        //                    }
        //                }
        //                else
        //                {
        //                    try
        //                    {
        //                        //We need to download the song
        //                        _nowPlaying = await GuildHandler.Music.Download.GetSongAsync(_nowPlaying.url);
        //                        if (_nowPlaying.FileExists())
        //                        {
        //                            _currentFFMpegProc = new Music.Play();
        //                            if (_nowPlaying.HasTitle())
        //                            {
        //                                await SendMessage("Now playing `" + _nowPlaying.title + "`");
        //                            }
        //                            else
        //                            {
        //                                await SendMessage("Now playing `" + _nowPlaying.url + "`");
        //                            }
        //                            await _currentFFMpegProc.PlaySongNAudio(0, _nowPlaying.fileName, audioClient);
        //                        }
        //                        else
        //                        {
        //                            //Video was unavailable, notify
        //                            //Logger.Warning("Video: " + _nowPlaying.url + " was unnavailable");
        //                            if (Program.mainHandler.guildHandles[guild.Id].database.musicChannel != 0)
        //                            {
        //                                var musicChannel = guild.GetChannelAsync(Program.mainHandler.guildHandles[guild.Id].database.musicChannel) as ITextChannel;
        //                                if (musicChannel != null)
        //                                {
        //                                    if (_nowPlaying.HasTitle())
        //                                    {
        //                                        await musicChannel.SendMessageAsync("Song `" + _nowPlaying.title + "` is unavailable");
        //                                    }
        //                                    else
        //                                    {
        //                                        await musicChannel.SendMessageAsync("Song `" + _nowPlaying.url + "` is unavailable");
        //                                    }
        //                                }
        //                            }
        //                        }

        //                        /*
        //                        await new Music.Play().PlaySong(0,
        //                        (await GuildHandler.Music.Download.GetSongAsync(_nowPlaying.url)).fileName, audioClient);
        //                        */
        //                    } catch(Exception e)
        //                    {
        //                        Logger.Error("Playing song, maybe track skip?");
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                if (!queue.songs[0].FileExists())
        //                {
        //                    queue.songs[0] = await GuildHandler.Music.Download.GetSongAsync(queue.songs[0].url);
        //                }
        //            }
        //        }
        //        await Task.Delay(1000);
        //    }
        //}

        public async Task MusicThread()
        {
            while (true)
            {
                await Task.Delay(1000);
                if (audioClient != null)
                {
                    if (audioStream == null)
                    {
                        audioStream = audioClient.CreatePCMStream(AudioApplication.Music);
                    }
                    Music.ISong song = _nowPlaying;

                    while (queue.songs.Count > 0 && song == null && _nowPlaying == null)
                    {
                        song = queue.Dequeue();
                    }
                    if (song == null && queue.songs.Count < 1)
                    {
                        try
                        {
                            audioStream.Close();
                            await audioClient.StopAsync();
                            audioClient = null;
                            audioStream = null;
                            AudioPlaying = false;
                            AudioStopped = false;
                        }
                        catch (Exception)
                        {
                            audioClient = null;
                            audioStream = null;
                            AudioPlaying = false;
                            AudioStopped = false;
                        }
                        if (musicChannel is ITextChannel)
                        {
                            await (musicChannel as ITextChannel).SendMessageAsync("Queue finished playing");
                        }
                    }
                    else
                    {
                        if (song != null)
                        {
                            if (_nowPlaying == null)
                            {
                                _nowPlaying = song;
                            }
                            if (_nowPlaying.FileExists())
                            {
                                try
                                {
                                    try
                                    {
                                        AudioPlaying = true;
                                        if (loopType != Database.LoopType.SingleLoop)
                                        await SayNowPlaying(_nowPlaying);
                                        var time = _currentFFMpegProc.PlaySongNAudio(_nowPlaying.currentTrackTimeOffset.Seconds, _nowPlaying.fileName, audioStream);
                                        if (!AudioPaused)
                                        {
                                            if (loopType == Database.LoopType.NoLoop)
                                            {
                                                _nowPlaying = null;
                                            }
                                            else if (loopType == Database.LoopType.SingleLoop)
                                            {
                                                _nowPlaying.currentTrackTimeOffset = new TimeSpan(0, 0, 0, 0, 0);
                                            } else if (loopType == Database.LoopType.QueueLoop)
                                            {
                                                queue.Enqueue(_nowPlaying);
                                            }
                                        }
                                        else
                                        {
                                            _nowPlaying.currentTrackTimeOffset = time;
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Logger.Error("Playing music: ", e);
                                        if (!AudioPaused)
                                        {
                                            _nowPlaying = null;
                                        }
                                        else
                                        {
                                            _nowPlaying.currentTrackTimeOffset = new TimeSpan(0, 0, 0, 0, 0);
                                        }
                                    }

                                }
                                catch (Exception) { }
                                AudioPlaying = false;
                                if (AudioStopped)
                                {
                                    try
                                    {
                                        audioStream.Close();
                                        await audioClient.StopAsync();
                                        audioClient = null;
                                        audioStream = null;
                                    }
                                    catch (Exception)
                                    {
                                        audioClient = null;
                                        audioStream = null;
                                    }
                                }
                            }
                            else
                            {
                                if (_nowPlaying.url != null)
                                {
                                    await SayDownloading(_nowPlaying);
                                    _nowPlaying = await Music.Download.GetSongAsync(_nowPlaying.url);
                                }
                                else
                                {
                                    _nowPlaying = null;
                                }
                            }
                        }
                    }
                }

                while (AudioPaused && audioClient != null)
                {
                    await Task.Delay(1000);
                }
            }
        }
        
        public async Task SayNowPlaying(Music.ISong song)
        {
            if (song != null)
            {
                if (musicChannel is ITextChannel)
                {
                    if (song.HasTitle())
                    {
                        await (musicChannel as ITextChannel).SendMessageAsync("Now playing `" + song.title + "`");
                    }
                    else
                    {
                        await (musicChannel as ITextChannel).SendMessageAsync("Now playing `" + song.url + "`");
                    }
                }
            }
        }

        public async Task SayDownloading(Music.ISong song)
        {
            IUserMessage userMessage = null;
            if (song != null)
            {
                if (musicChannel is ITextChannel)
                {
                    if (song.HasTitle())
                    {
                        userMessage = await (musicChannel as ITextChannel).SendMessageAsync("Downloading `" + song.title + "`");
                        await Task.Run(() => UpdateSaidDownloading(song, userMessage));
                    }
                    else
                    {
                        userMessage = await (musicChannel as ITextChannel).SendMessageAsync("Downloading `" + song.url + "`");
                        await Task.Run(() => UpdateSaidDownloading(song, userMessage));
                    }                    
                }
            }
        }

        /// <summary>
        /// Do NOT run on main thread or syncronously
        /// </summary>
        /// <param name="song"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task UpdateSaidDownloading(Music.ISong song, IUserMessage message)
        {
            TimeSpan timeSpan = new TimeSpan(0, 0, 1, 0);
            var seconds = timeSpan.Seconds;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            while (stopwatch.Elapsed.Seconds < seconds)
            {
                if (song.FileExists())
                {
                    try
                    {
                        if (song.HasTitle())
                        {
                            await message.ModifyAsync(n => n.Content = "Now playing `" + song.title + "`");
                        }
                        else
                        {
                            await message.ModifyAsync(n => n.Content = "Now playing `" + song.url + "`");
                        }                       
                    }
                    catch (Exception) { }
                    return;
                }
                await Task.Delay(5000);
            }
        }

        public async Task AddSong(Music.ISong song)
        {
            //queue.AddSong(song);
            if (!song.FileExists())
            {
                var song_ = (await GuildHandler.Music.Download.GetSongAsync(song.url));
                song_.requester = song.requester;
                queue.Enqueue(song_);
            }
            else
            {
                queue.Enqueue(song);
            }
        }

        public async Task StopFFMpegAsync()
        {
            if (_currentFFMpegProc != null)
                await _currentFFMpegProc.StopAsync();
        }

        public async Task RemoveSong(Music.ISong song)
        {
            queue.RemoveSong(queue.songs.Where(n => n.fileName == song.fileName).First());
        }

        public Embed GetQueueEmbed()
        {
            if (_nowPlaying != null)
            {
                if (_nowPlaying.HasTitle())
                {
                    var time = new TimeSpan(0, 0, Convert.ToInt32(_currentFFMpegProc.mediaFoundationReader.Position / _currentFFMpegProc.mediaFoundationReader.WaveFormat.AverageBytesPerSecond));
                    string loop = "";
                    switch (loopType)
                    {
                        case Database.LoopType.NoLoop:
                            loop = "No loop";
                            break;
                        case Database.LoopType.QueueLoop:
                            loop = "Looping queue";
                            break;
                        case Database.LoopType.SingleLoop:
                            loop = "Looping song";
                            break;
                    }
                    return queue.GetQueueEmbed().WithDescription("Loop: " + loop + "\n[NP] " + _nowPlaying.title + " `[" + new DateTime(time.Ticks).ToString("mm:ss") + "/" + new DateTime(_currentFFMpegProc.mediaFoundationReader.TotalTime.Ticks).ToString("mm:ss") + "]`\n" + queue.GetQueueEmbed().Description).Build();
                }
                else
                {
                    return queue.GetQueueEmbed().WithDescription("[NP]" + _nowPlaying.fileName + "\n" + queue.GetQueueEmbed().Description).Build();
                }
            }
            else
            {
                return new EmbedBuilder().WithTitle("Queue").WithDescription("Nothing in the queue").WithColor(Color.Blue).Build();

            }
        }

        /// <summary>
        /// Connects to a VC
        /// </summary>
        /// <param name="user">User</param>
        /// <returns>Method return response</returns>
        public async Task<string> Connect(IGuildUser user)
        {
            if (AudioStopped)
            {
                AudioPaused = false;
                AudioStopped = false;
            }
            var vcchannel = (user?.VoiceChannel);
            //if audioClient isnt null we need to change channels
            if (audioClient != null)
            {
                if (vcchannel != null && vcchannel != audioChannel)
                {
                    //User in vc
                    if (audioChannel != null)
                    {
                        if (audioChannel != vcchannel)
                        {                            
                            return "I'm already connected in a different channel!";
                        }
                        else
                        {
                            return "I'm already connected!";
                        }
                    }
                    else
                    {
                        if (vcchannel != audioChannel)
                        {
                            try
                            {
                                await audioClient.StopAsync();
                                audioChannel = vcchannel;
                                audioClient = await vcchannel.ConnectAsync();
                                return "Connected to `" + audioChannel.Name + "`";
                            }
                            catch (Exception)
                            {
                                return "Could not connect.";
                            }
                        }
                        else
                        {
                            return "I'm already connected.";
                        }
                    }
                }
                else
                {
                    //User not in vc
                    if (audioChannel != null)
                    {
                        return "You aren't in a voice channel! Besides, I'm already in one.";
                    }
                    else
                    {
                        return "Connect to a voice channel then try again.";
                    }
                }
            }
            else
            {
                if (vcchannel != null)
                {
                    try
                    {
                        audioChannel = vcchannel;
                        Logger.Info("Connecting to: " + audioChannel.Name);
                        audioClient = await vcchannel.ConnectAsync();
                        return "Connected to `" + audioChannel.Name + "`";
                    }
                    catch (Exception e)
                    {
                        Logger.Error("Could not connect", e);
                        return "Could not connect.";
                    }
                }
                else
                {
                    return "Connect to a voice channel then try again";
                }
            }
        }

        /// <summary>
        /// Disconnects current VC instance
        /// </summary>
        /// <returns></returns>
        public async Task Disconnect()
        {
            //RunMusicThread = false;
                if (audioClient != null)
                {
                AudioPaused = true;
                _currentFFMpegProc.playing = false;
                AudioStopped = true;
                    //await audioClient.StopAsync();
                }
                if (audioChannel != null)
                {
                    audioChannel = null;
                }            
        }

        /// <summary>
        /// If unpause is true it will force an attempt to unpause, else it will pause
        /// </summary>
        /// <param name="unpause"></param>
        /// <returns></returns>
        public string Pause(bool unpause = false)
        {
            if (audioClient != null)
            {
                if (_currentFFMpegProc.playing && !unpause)
                {
                    AudioPaused = true;
                    _currentFFMpegProc.playing = false;
                    Task.Delay(100).Wait();
                    return $"Paused at `{new DateTime(_nowPlaying.currentTrackTimeOffset.Ticks).ToString("mm:ss")}`";
                }
                else
                {
                    if (_nowPlaying != null)
                    {
                        AudioPaused = false;
                        return $"Unpaused, playing from `{new DateTime(_nowPlaying.currentTrackTimeOffset.Ticks).ToString("mm:ss")}`";
                    }
                    else
                    {
                        return "Nothing is playing";
                    }
                }
            }
            else
            {
                return "Nothing is playing";
            }
        }
        

        /// <summary>
        /// Skips songs
        /// </summary>
        /// <param name="count">1 for nowplaying, more to skip queue</param>
        /// <returns></returns>
        public async Task Skip(int count)
        {
            if (count == 1)
            {
                _currentFFMpegProc.playing = false;
            } else
            if (count > queue.songs.Count)
            {
                queue.songs = new List<Music.ISong>();
                _currentFFMpegProc.playing = false;
            } else if (count > 1 && count < queue.songs.Count)
            {
                queue.songs.RemoveRange(0, count - 1);
                _currentFFMpegProc.playing = false;
            }
        }
    }
}
