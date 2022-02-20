using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TopliBOT.Helpers;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Search;
namespace TopliBOT.Modules
{
    public class BotCommands : ModuleBase<SocketCommandContext>
    {

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

        [Command("sema")]
        public async Task SemaAsync([Remainder] string a)
        {
            var user = Context.Message.MentionedUsers;
            await Discord.UserExtensions.SendMessageAsync(user.FirstOrDefault(), "Hocel to rodjeni");
        }




        private async Task PlayFromFileAsync(string path, string name)
        {
            if (!_node.HasPlayer(Context.Guild))
            {
                var voiceState = Context.User as SocketGuildUser;
                if (voiceState?.VoiceChannel == null)
                {
                    await ReplyAsync("Moras bit u voice kanalu roki.");
                    return;
                }

                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

            var player = _node.GetPlayer(Context.Guild);
            var track = await _node.SearchAsync(SearchType.Direct, path);
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track.Tracks.FirstOrDefault());
                await ReplyAsync("`Emousnl demedz dodan u kvekve.`");
            }
            else
            {
                await player.PlayAsync(track.Tracks.FirstOrDefault());
                await ReplyAsync($"`Sada svira: {name}`");
            }
        }



        [Command("steta", RunMode = RunMode.Async)]
        public async Task StetaAsync()
        {
            string path = @"C:\Users\senad\source\repos\TopliBOT\TopliBOT\ed.mp3";
            await PlayFromFileAsync(path, "Emousnl demedz");
        }

        [Command("radio", RunMode = RunMode.Async)]
        public async Task RadioAsync()
        {
            string path = @"http://stream.rsg.ba:9000/;stream";
            await PlayFromFileAsync(path, "Radio Miljacka");
        }


        [Command("join")]
        public async Task JoinAsync()
        {
            if (_node.HasPlayer(Context.Guild))
            {
                await ReplyAsync("Vec sam tu brale.");
                return;
            }

            var voiceState = Context.User as SocketGuildUser;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Moras bit u voice kanalu roki.");
                return;
            }

            try
            {
                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"Usao u {voiceState.VoiceChannel.Name}!");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play")]
        public async Task PlayAsync([Remainder] string path)
        {
            var voiceState = Context.User as SocketGuildUser;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("Moras bit u voice kanalu roki.");
                return;
            }
            if (!_node.HasPlayer(Context.Guild))
            {
                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
            }

            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
            var botUser = bot as IGuildUser;

            if (botUser.VoiceChannel != null && (botUser.VoiceChannel.Id != voiceState.VoiceChannel.Id))
            {
                await ReplyAsync("Moras bit u voice kanalu roki.");
                return;
            }

            var search = await _node.SearchAsync(SearchType.YouTube, path);
            var player = _node.GetPlayer(Context.Guild);
            var track = search.Tracks.FirstOrDefault();
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track);
                await ReplyAsync($"`{track.Title} dodano u kvekve.`");
            }
            else
            {
                await player.PlayAsync(track);
                await ReplyAsync($"`Sada svira: {track.Title}`");
            }

        }

        [Command("stop")]
        public async Task StopAsync()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("Nisam konektovan.");
                return;
            }
            if (player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("Vec sam stopiran rodjak");
                return;
            }
            try
            {
                await player.StopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        [Command("skip")]
        public async Task SkipAsync()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("Nisam konektovan.");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("Vec si stopiran bajo moj.");
                return;
            }

            try
            {
                if (player.Queue.Count > 0)
                {
                    await player.SkipAsync();
                    await ReplyAsync($"`Pjesma preskocena.\nSada svira: {player.Track.Title}`");
                }
                else
                {
                    await ReplyAsync($"`Kvekve prazan.`");
                }

            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }

        }

    }
}
