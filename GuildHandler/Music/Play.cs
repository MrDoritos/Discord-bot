using System;
using System.Diagnostics;
using Discord.Audio;
using System.Threading.Tasks;
using NAudio.Wave;


namespace MusicBot9001.GuildHandler.Music
{
    class Play 
    {
        public Process musicproc;
        public IWavePlayer wavePlayer;
        public AudioFileReader audioFileReader;
        public MediaFoundationReader mediaFoundationReader = null;
        public bool playing = false;
        public int bps = 0;
        public TimeSpan CurrentTime
        {
            get
            {
                if (mediaFoundationReader != null)
                    return mediaFoundationReader.CurrentTime;
                else
                    return new TimeSpan(0, 0, 0, 0, 0);
            }
            set
            {
                if (mediaFoundationReader != null)
                mediaFoundationReader.CurrentTime = value;
            }
        }

        public async Task PlaySong(int seconds, string path, AudioOutStream aclient)
        {
            musicproc = Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = " -loglevel quiet" +
            " -ss " + seconds +
                           $" -i " + " \"" + Program.config.musicPath + path.Trim() + "\" " + //strip.wma
                           "-f s16le -ar 48000 -ac 2 pipe:1",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = false
            });
            try
            {
                while (true)
                {
                    if (musicproc.HasExited)
                        break;

                    await musicproc.StandardOutput.BaseStream.CopyToAsync(aclient);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Playing song", e);
            }
            await aclient.FlushAsync();
        }
        
        public TimeSpan PlaySongNAudio(int seconds, string filename, AudioOutStream aclient)
        {
            //path = "kraft.mp3";
            

            //await mediaFoundationReader.CopyToAsync(aclient);
            //WaveStream reader = new AudioFileReader(Program.config.musicPath + path.Trim());
            //var what = reader.ToSampleProvider();
            //what = what.ToStereo();
            //reader = what.WaveFormat.r;
            //var reader = new Mp3FileReader(Program.config.musicPath + path);
            //var channelCount = aclient.GetService<AudioService>().Config.Channels; // Get the number of AudioChannels our AudioService has been configured to use.
            var OutFormat = new WaveFormat(48000, 16, 2); // Create a new Output Format, using the spec that Discord will accept, and with the number of channels that our client supports.
            TimeSpan timetoreturn = new TimeSpan();
            //var stream = WaveFormatConversionStream.CreatePcmStream(mediaFoundationReader);
            using (mediaFoundationReader = new MediaFoundationReader(Program.config.musicPath + filename.Trim()))
            using (var resampler = new MediaFoundationResampler(mediaFoundationReader, OutFormat))
            {
                //await resampler.CopyToAsync(aclient);
                mediaFoundationReader.CurrentTime = new TimeSpan(0, 0, 0, seconds, 0);
                resampler.ResamplerQuality = 60; // Set the quality of the resampler to 60, the highest quality
                bps = OutFormat.AverageBytesPerSecond;
                int blockSize = bps / 50; // Establish the size of our AudioBuffer                
                byte[] buffer = new byte[blockSize];
                int byteCount;
                playing = true;
                while ((byteCount = resampler.Read(buffer, 0, blockSize)) > 0 && playing) // Read audio into our buffer, and keep a loop open while data is present
                {
                    if (byteCount < blockSize)
                    {
                        // Incomplete Frame
                        for (int i = byteCount; i < blockSize; i++)
                            buffer[i] = 0;
                    }
                    aclient.Write(buffer, 0, blockSize);
                }
                timetoreturn = mediaFoundationReader.CurrentTime;
                playing = false;
            }
            //mediaFoundationReader = null;

            //aclient.Dispose();
            //aclient.Close();
            return timetoreturn;
        }

        //Works for MP3
        //var reader = new Mp3FileReader(Program.config.musicPath + path);
        //var stream = WaveFormatConversionStream.CreatePcmStream(reader);
        //await stream.CopyToAsync(aclient);

        public async Task StopAsync()
        {
            musicproc.Close();
        }
    }
}
