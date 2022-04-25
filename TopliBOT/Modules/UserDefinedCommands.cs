using Discord;
using Discord.Commands;

namespace TopliBOT.Modules
{
    public class UserDefinedCommands : ModuleBase<SocketCommandContext>
    {

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


        [Command("tag")]
        public async Task SemaAsync([Remainder] string a)
        {
            var user = Context.Message.MentionedUsers;
            await UserExtensions.SendMessageAsync(user.FirstOrDefault(), "Hocel to rodjeni");
        }





    }
}
