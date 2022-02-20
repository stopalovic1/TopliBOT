using Discord;
using Discord.WebSocket;
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
                return;
            }

            if (queueable is not LavaTrack track)
            {
                await player.TextChannel.SendMessageAsync("Next item in queue is not a track.");
                return;
            }

            await args.Player.PlayAsync(track);
            await args.Player.TextChannel.SendMessageAsync($"`Sada svira: {track.Title}`");
        }


        private async Task OnReadyAsync()
        {

            if (!_node.IsConnected)
            {
                try
                {
                    await _node.ConnectAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }
        }

    }
}
