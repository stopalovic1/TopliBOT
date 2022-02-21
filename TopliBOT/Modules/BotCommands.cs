using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Text;
using TopliBOT.Helpers;
using Victoria;
using Victoria.Enums;
using Victoria.Responses.Rest;
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

        [Command("ping")]
        public async Task Pong([Remainder] string path)
        {

            try
            {
                await ReplyAsync("PONG!");
            }
            catch (Exception ex)
            {
                await ReplyAsync(ex.Message);
            }
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
            await UserExtensions.SendMessageAsync(user.FirstOrDefault(), "Hocel to rodjeni");
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
            var track = await _node.SearchAsync(path);
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {
                player.Queue.Enqueue(track.Tracks.FirstOrDefault());
                await ReplyAsync($"`{name} dodan u kvekve.`");
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
            string path = @"C:\Users\senad\source\repos\TopliBOT\TopliBOT\MusicFiles\ed.mp3";
            await PlayFromFileAsync(path, "Emousnl demedz");
        }

        [Command("miljacka", RunMode = RunMode.Async)]
        public async Task MiljackaAsync()
        {
            //var pathOfFiles = AppDomain.CurrentDomain.BaseDirectory;
            string path = @"https://radiomiljacka-bhcloud.radioca.st/stream.mp3";
            await PlayFromFileAsync(path, "Radio Miljacka");
        }

        [Command("rsg", RunMode = RunMode.Async)]
        public async Task RsgAsync()
        {
            //var pathOfFiles = AppDomain.CurrentDomain.BaseDirectory;
            string path = @"http://stream.rsg.ba:9000/;stream";
            await PlayFromFileAsync(path, "Radio RSG");
        }



        [Command("join")]
        public async Task JoinAsync()
        {
            if (_node.HasPlayer(Context.Guild))
            {
                await ReplyAsync("`Vec sam tu brale.`");
                return;
            }

            var voiceState = Context.User as SocketGuildUser;
            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("`Moras bit u voice kanalu roki.`");
                return;
            }

            try
            {
                await _musicHelper.ConnectAsync(voiceState.VoiceChannel, Context.Channel as ITextChannel);
                await ReplyAsync($"`Usao u {voiceState.VoiceChannel.Name}!`");
            }
            catch (Exception exception)
            {
                await ReplyAsync(exception.Message);
            }
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task PlayAsync([Remainder] string path)
        {
            var voiceState = Context.User as SocketGuildUser;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("`Moras bit u voice kanalu roki.`");
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
                await ReplyAsync("`Moras bit u voice kanalu roki.`");
                return;
            }
            SearchResponse search;

            Uri uriResult;
            bool isUrl = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

            if (isUrl)
            {
                search = await _node.SearchAsync(path);
            }
            else
            {
                search = await _node.SearchYouTubeAsync(path);
            }



            /*if (path.Contains("music.youtube.com"))   Leaving this here beacuse of victoria bug ??
            {
                var playlistPath = path.Substring(path.IndexOf('?') + 1);
                var parsedPlaylistPath = @"https://youtube.com/playlist?" + playlistPath;
                search = await _node.SearchAsync(SearchType.Direct, parsedPlaylistPath);

            }
            else if (path.Contains("youtube.com"))
            {
                if (path.Contains("list"))
                {
                    var playlistPath = path.Substring(path.IndexOf('&') + 1);
                    var parsedPlaylist = @"https://youtube.com/playlist?" + playlistPath;
                    search = await _node.SearchAsync(SearchType.Direct, parsedPlaylist);
                }
                else
                {
                    search = await _node.SearchAsync(SearchType.Direct, path);
                }
            }

            else
            {
                search = await _node.SearchAsync(SearchType.YouTube, path);
            }*/


            if (search.LoadStatus == LoadStatus.NoMatches || search.LoadStatus == LoadStatus.LoadFailed)
            {
                await ReplyAsync("`Nisam nista nasao braco.`");
                return;
            }

            var player = _node.GetPlayer(Context.Guild);
            if (player.PlayerState == PlayerState.Playing || player.PlayerState == PlayerState.Paused)
            {

                if (search.Playlist.Name != null)
                {
                    foreach (var track in search.Tracks)
                    {
                        player.Queue.Enqueue(track);
                    }

                    await ReplyAsync($"`Dodano {search.Tracks.Count} pjesama`");
                }
                else
                {
                    var track = search.Tracks.FirstOrDefault();
                    player.Queue.Enqueue(track);
                    await ReplyAsync($"`{track.Title} dodano u kvekve.`");
                }

            }
            else
            {
                var track = search.Tracks.FirstOrDefault();
                if (search.Playlist.Name != null)
                {
                    var parsedSearch = search.Tracks.ToList();
                    for (var i = 0; i < parsedSearch.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);
                            await ReplyAsync($"`Sada svira: {track.Title}`");
                        }
                        else
                        {
                            player.Queue.Enqueue(parsedSearch[i]);
                        }
                    }
                    await ReplyAsync($"`Dodano {parsedSearch.Count} pjesama`");
                }
                else
                {
                    await player.PlayAsync(track);
                    await ReplyAsync($"`Sada svira: {track.Title}`");
                }
            }


        }

        [Command("stop")]
        public async Task StopAsync()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("`Nisam konektovan.`");
                return;
            }
            if (player.PlayerState == PlayerState.Stopped)
            {
                await ReplyAsync("`Vec sam stopiran rodjak.`");
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
                await ReplyAsync("`Nisam konektovan.`");
                return;
            }

            if (player.PlayerState != PlayerState.Playing)
            {
                await ReplyAsync("`Vec si stopiran bajo moj.`");
                return;
            }

            try
            {
                if (player.Queue.Count > 0)
                {
                    var currentTrack = await player.SkipAsync();
                    await ReplyAsync($"`Pjesma preskocena.\nSada svira: {currentTrack.Title}`");
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


        [Command("clear", RunMode = RunMode.Async)]
        public async Task ClearAsync()
        {
            var voiceState = Context.User as SocketGuildUser;

            if (voiceState?.VoiceChannel == null)
            {
                await ReplyAsync("`Moras bit u voice kanalu roki.`");
                return;
            }

            if (!_node.HasPlayer(Context.Guild))
            {
                await ReplyAsync("`Nisam konektovan.`");
                return;
            }

            var bot = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id);
            var botUser = bot as IGuildUser;

            if (botUser.VoiceChannel != null && (botUser.VoiceChannel.Id != voiceState.VoiceChannel.Id))
            {
                await ReplyAsync("`Moras bit u voice kanalu roki.`");
                return;
            }

            _node.TryGetPlayer(Context.Guild, out var player);
            player.Queue.Clear();
            await ReplyAsync("`Kvekve ociscen.`");
        }



        [Command("queue")]
        public async Task GetSongsFromQueueAsync()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("`Nisam konektovan.`");
                return;
            }

            var tracks = player.Queue;
            var stringBuilder = new StringBuilder();
            if (tracks.Count > 0)
            {
                foreach (var track in tracks)
                {
                    stringBuilder.AppendLine(track.Title);
                }
                try
                {
                    await ReplyAsync("```" + stringBuilder.ToString() + "```");
                }
                catch (Exception ex)
                {
                    await ReplyAsync(ex.Message);
                }
            }
            else
            {
                await ReplyAsync("`Kvekve prazan.`");
            }
        }

        [Command("help")]
        public async Task ShowHelpAsync()
        {

            string help = "!play - Pusta pjesmu\n";
            help += "!stop - Zaustavlja pjesmu\n";
            help += "!skip - Skipa pjesmu\n";
            help += "!queue - Trenutni kvekve pjesama\n";
            help += "!clear - Cisti kvekve\n";
            help += "!miljacka - Pusta radio Miljacka\n";
            help += "!rsg - Pusta radio RSG\n";
            help += "!steta - Emousnl demedz\n";
            help += "!bing - Bing Chilling";

            var embed = new EmbedBuilder();

            embed.WithAuthor(Context.Client.CurrentUser)
                .WithFooter(footer => footer.Text = $"Zatrazeno od: {(Context.User as SocketGuildUser).Username}")
                .WithColor(Color.Blue)
                .WithTitle("Komande")
                .WithDescription(help)
                .WithCurrentTimestamp();
            await ReplyAsync(embed: embed.Build());

        }

    }
}
