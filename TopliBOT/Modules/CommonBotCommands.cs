using Dapper;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopliBOT.Models;

namespace TopliBOT.Modules
{
    public class CommonBotCommands : ModuleBase<SocketCommandContext>
    {


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

            embed.WithFooter(footer => footer.WithIconUrl(Context.User.GetAvatarUrl()).WithText($"Zatrazeno od: {(Context.User as SocketGuildUser).Username}"))
                .WithColor(Color.Blue)
                .WithTitle("Komande")
                .WithDescription(help)
                .WithCurrentTimestamp();
            await ReplyAsync(embed: embed.Build());

        }

        [Command("setprefix")]
        public async Task SetPrefixAsync([Remainder] string prefix)
        {
            if (prefix == null)
            {
                await ReplyAsync("Nisi unio novi prefix rodjak.");
                return;
            }

            if (prefix.Length != 1)
            {
                await ReplyAsync("Prefix se mora sastojat od jednog karaktera.");
                return;
            }

            var dbPath = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "/Tokens/databaseToken.txt");
            Prefixes data = null;
            using (IDbConnection connection = new SqlConnection(dbPath))
            {
                data = (await connection.QueryAsync<Prefixes>("select * from Prefixes where GuildId = @GuildId;", new { GuildId = Context.Guild.Id.ToString() })).FirstOrDefault();

                if (data == null)
                {
                    await connection.ExecuteAsync("insert into Prefixes(GuildId,Prefix) values(@GuildId,@Prefix);", new { GuildId = Context.Guild.Id.ToString(), Prefix = prefix[0] });

                }
                else
                {
                    await connection.ExecuteAsync("update Prefixes set Prefix = @Prefix where GuildId = @GuildId;", new { GuildId = Context.Guild.Id.ToString(), prefix = prefix[0] });
                }
            }
            await ReplyAsync($"Novi prefiks je {prefix[0]}");
        }


    }
}
