using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopliBOT.Helpers;
using Victoria;
using YoutubeExplode;
using Victoria.Enums;
using System.Linq;
using Victoria.Responses.Search;
namespace TopliBOT.Modules
{
    public class BotCommands : ModuleBase<SocketCommandContext>
    {

        private IAudioClient _client;
        private static YoutubeClient _youtubeClient;
        private static MemoryStream _stream;
        private MusicHelper _musicHelper;
        private LavaNode _node;
        public BotCommands(MusicHelper musicHelper, LavaNode node)
        {
            _musicHelper = musicHelper;
            _node = node;
        }

        [Command("Ping")]
        public async Task Pong()
        {
            await ReplyAsync("PONG!");
        }

        [Command("edis")]
        public async Task EdisAsync()
        {
            await ReplyAsync("Djes edise");
        }

        [Command("keno")]
        public async Task KenoAsync()
        {
            await ReplyAsync("EEEE OKO IZO JE");
        }

        private async Task PlayAudioFromFileAsync(string path)
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            if (_client == null)
            {
                _client = await channel.ConnectAsync();
            }

            try
            {
                await BotHelperMethods.SendAsync(_client, path);
            }
            catch (Exception ex)
            {

            }
        }



        private async Task PlayYoutubeAsync(string path)
        {
            var channel = (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null) { await Context.Channel.SendMessageAsync("User must be in a voice channel, or a voice channel must be passed as an argument."); return; }

            if (_youtubeClient == null)
            {
                _youtubeClient = new YoutubeClient();

            }
            if (_client == null)
            {
                _client = await channel.ConnectAsync();
            }
            if (_stream == null)
            {
                _stream = new MemoryStream();
            }
            try
            {
                await BotHelperMethods.PlayYoutubeAsync(_client, _youtubeClient, path, _stream);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }

        }

        [Command("bing", RunMode = RunMode.Async)]
        public async Task JoinChannel()
        {
            string p = AppDomain.CurrentDomain.BaseDirectory;
            string path = @"C:\Users\senad\source\repos\TopliBOT\TopliBOT\bing.mp3";
            await PlayAudioFromFileAsync(path);

        }

        [Command("steta", RunMode = RunMode.Async)]
        public async Task Steta()
        {
            string path = @"C:\Users\senad\source\repos\TopliBOT\TopliBOT\ed.mp3";
            await PlayAudioFromFileAsync(path);
        }

        [Command("radio", RunMode = RunMode.Async)]
        public async Task Radio([Remainder] string path)
        {
            await Context.Channel.SendMessageAsync("Svira Radio miljacka");
            await PlayAudioFromFileAsync(path);
        }


        [Command("stop", RunMode = RunMode.Async)]
        public async Task Stop()
        {
            if (_client.ConnectionState == ConnectionState.Connected) //NAPRAVITI
                await _client.StopAsync();
        }

        [Command("p", RunMode = RunMode.Async)]
        public async Task PlayYtAsync([Remainder] string path)
        {
            // await Context.Channel.SendMessageAsync("Svira Radio miljacka");
            await PlayYoutubeAsync(path);
        }

        [Command("join")]
        public async Task JoinAsync()
        {
            if (_node.HasPlayer(Context.Guild))
            {
                await ReplyAsync("I'm already connected to a voice channel!");
                return;
            }

            var voiceState = Context.User as SocketGuildUser;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("You must be connected to a voice channel!");
                return;
            }

            try
            {
                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Joined {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string path)
        {
            if(!_node.HasPlayer(Context.Guild))
            {
                var voiceState = Context.User as SocketGuildUser;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("You must be connected to a voice channel!");
                    return;
                }

                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }
            string p = @"C:\Users\senad\source\repos\TopliBOT\TopliBOT\ed.mp3";
            var search = await _node.SearchAsync(SearchType.Direct,p);

            var player = _node.GetPlayer(Context.Guild);
            var track = search.Tracks.FirstOrDefault();
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync($"```Added {track.Title} to queue.```");
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync($"Now Playing: {track.Title}");
            }
        }   
    }
}
