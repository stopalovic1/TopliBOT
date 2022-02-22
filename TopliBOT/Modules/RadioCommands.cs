using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TopliBOT.Helpers;
using Victoria;
using Victoria.Enums;

namespace TopliBOT.Modules
{
    public class RadioCommands : ModuleBase<SocketCommandContext>
    {
        private readonly LavaNode _node;
        private readonly MusicHelper _musicHelper;
        private readonly HelperMethods _helperMethods;

        public RadioCommands(LavaNode node, MusicHelper musicHelper, HelperMethods helperMethods)
        {
            _node = node;
            _musicHelper = musicHelper;
            _helperMethods = helperMethods;

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
                var embed = _helperMethods.BuildEmbed($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}", "Sada svira: ", name, "", "", Context.User);
                await ReplyAsync(embed: embed.Build());
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


    }
}
