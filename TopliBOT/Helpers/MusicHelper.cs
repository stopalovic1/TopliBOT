using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.EventArgs;

namespace TopliBOT.Helpers
{
    public class MusicHelper
    {
        private LavaNode _node;
        private readonly DiscordSocketClient _discordSocketClient;
        public MusicHelper(LavaNode node, DiscordSocketClient discordSocketClient)
        {
            _node = node;
            _discordSocketClient = discordSocketClient;
        }



        public Task InitializeAsync()
        {
            _discordSocketClient.Ready += OnReadyAsync;
            _node.OnTrackEnded += OnTrackEnded;
            return Task.CompletedTask;
        }

        public async Task ConnectAsync(SocketVoiceChannel channel, ITextChannel textChannel)
        {
            await _node.JoinAsync(channel, textChannel);

        }

        private async Task OnTrackEnded(TrackEndedEventArgs args)
        {

            var player = args.Player;
            if (!player.Queue.TryDequeue(out var queueable))
            {
                await player.TextChannel.SendMessageAsync("Queue completed!");
                return;
            }

            if (!(queueable is LavaTrack track))
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"```Now playing: {track.Title}```");
        }


        private async Task OnReadyAsync()
        {

            if (!_node.IsConnected)
            {
                try
                {
                    await _node.ConnectAsync();
                    var s = _node.IsConnected;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

    }
}
