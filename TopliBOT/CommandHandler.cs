using Discord.Commands;
using Discord.WebSocket;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using TopliBOT.Models;

namespace TopliBOT
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider)
        {
            _client = client;
            _commands = commands;
            _serviceProvider = serviceProvider;
        }

        public async Task InitializeAsync()
        {

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage MessageParam)
        {
            var message = MessageParam as SocketUserMessage;
            if (message == null) return;
            int ArgPos = 0;
            Prefixes data;
            var author = message.Author as SocketGuildUser;
            var guildId = author.Guild.Id;
            var dbPath = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "/Tokens/databaseToken.txt");
            using (IDbConnection connection = new SqlConnection(dbPath))
            {
                data = (await connection.QueryAsync<Prefixes>("select * from dbo.Prefixes where GuildId=@GuildId;", new { GuildId = guildId.ToString() })).FirstOrDefault();
            }

            var prefix = '!';

            if (data != null)
            {
                prefix = data.Prefix[0];
            }

            if (!(message.HasCharPrefix(prefix, ref ArgPos) || message.HasMentionPrefix(_client.CurrentUser, ref ArgPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(context: context, argPos: ArgPos, services: _serviceProvider);
        }






    }
}
