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
    public class MusicCommands : ModuleBase<SocketCommandContext>
    {

        private MusicHelper _musicHelper;
        private LavaNode _node;
        private readonly HelperMethods _helperMethods;

        public MusicCommands(MusicHelper musicHelper, LavaNode node, HelperMethods helperMethods)
        {
            _musicHelper = musicHelper;
            _node = node;
            _helperMethods = helperMethods;
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
                    var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Info: ", $"Dodano {search.Tracks.Count} pjesama", "", "", Context.User);
                    await ReplyAsync(embed: embed.Build());
                }
                else
                {
                    var track = search.Tracks.FirstOrDefault();
                    var thumbUrl = $"https://i.ytimg.com/vi/{track.Id}/mqdefault.jpg";
                    player.Queue.Enqueue(track);
                    var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Dodano u kvekve: ", track.Title, track.Url, thumbUrl, Context.User);
                    await ReplyAsync($"`{track.Title} dodano u kvekve.`");
                }

            }
            else
            {
                var track = search.Tracks.FirstOrDefault();
                var thumbUrl = $"https://i.ytimg.com/vi/{track.Id}/mqdefault.jpg";
                if (search.Playlist.Name != null)
                {
                    var parsedSearch = search.Tracks.ToList();
                    for (var i = 0; i < parsedSearch.Count; i++)
                    {
                        if (i == 0)
                        {
                            await player.PlayAsync(track);

                            var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Sada svira: ", track.Title, track.Url, thumbUrl, Context.User);
                            await ReplyAsync(embed: embed.Build());
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
                    var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Sada svira: ", track.Title, track.Url, thumbUrl, Context.User);
                    await ReplyAsync(embed: embed.Build());
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
                    var thumbUrl = $"https://i.ytimg.com/vi/{currentTrack.Id}/mqdefault.jpg";
                    var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Pjesma preskocena.\nSada svira: ", currentTrack.Title, currentTrack.Url, thumbUrl, Context.User);
                    await ReplyAsync(embed: embed.Build());
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



        [Command("queue", RunMode = RunMode.Async)]
        public async Task GetSongsFromQueueAsync()
        {
            if (!_node.TryGetPlayer(Context.Guild, out var player))
            {
                await ReplyAsync("`Nisam konektovan.`");
                return;
            }

            var tracks = player.Queue;
            var stringBuilder = new StringBuilder();
            var tasks = new List<Task>();
            if (tracks.Count > 0)
            {
                foreach (var track in tracks)
                {
                    if (stringBuilder.Length + track.Title.Length >= 2000)
                    {
                        var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Trenutni kvekve: ", stringBuilder.ToString(), "", "", Context.User);
                        tasks.Add(ReplyAsync(embed: embed.Build()));
                        stringBuilder.Clear();
                    }
                    stringBuilder.AppendLine(track.Title);
                }
                await Task.WhenAll(tasks);
                try
                {
                    if (stringBuilder.Length != 0)
                    {
                        var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Trenutni kvekve: ", stringBuilder.ToString(), "", "", Context.User);
                        await ReplyAsync(embed: embed.Build());
                    }

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
    }
}
