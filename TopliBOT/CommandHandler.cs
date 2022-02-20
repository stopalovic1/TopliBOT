using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

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

            if (!(message.HasCharPrefix('!', ref ArgPos) || message.HasMentionPrefix(_client.CurrentUser, ref ArgPos)) || message.Author.IsBot) return;

            var context = new SocketCommandContext(_client, message);

            await _commands.ExecuteAsync(context: context, argPos: ArgPos, services: _serviceProvider);
        }






    }
}
