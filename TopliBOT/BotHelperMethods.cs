using CliWrap;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos.Streams;

namespace TopliBOT
{
    public static class BotHelperMethods
    {
           
        private static Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            });
        }

        public static async Task SendAsync(IAudioClient client, string path)
        {

            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }
        }

        public static async Task PlayYoutubeAsync(IAudioClient client, YoutubeClient youtubeClient, string link, MemoryStream memoryStream)
        {
            var streamManifest = await youtubeClient.Videos.Streams.GetManifestAsync(link);
            var StreamInfo = streamManifest.GetAudioOnlyStreams().TryGetWithHighestBitrate();
            var stream = await youtubeClient.Videos.Streams.GetAsync(StreamInfo);

            await Cli.Wrap("ffmpeg")
                .WithArguments(" -hide_banner -loglevel panic -i pipe:0 -ac 2 -f s16le -ar 48000 pipe:1")
                .WithStandardInputPipe(PipeSource.FromStream(stream))
                .WithStandardOutputPipe(PipeTarget.ToStream(memoryStream))
                .ExecuteAsync();

            using (var discord = client.CreatePCMStream(AudioApplication.Mixed))
            {
                try { await discord.WriteAsync(memoryStream.ToArray(), 0, (int)memoryStream.Length); }
                finally { await discord.FlushAsync(); }
            }

        }
    }
}
