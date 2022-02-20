using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TopliBOT.Helpers;
using Victoria;

namespace TopliBOT
{
    public class TopliBOTClient
    {
        private CommandService _commandService;
        private DiscordSocketClient _socketClient;
        private IServiceProvider _serviceProvider;
        private CommandHandler _commandHandler;
        public TopliBOTClient()
        {
            _commandService = new CommandService();
            _socketClient = new DiscordSocketClient();
        }

        public async Task InitializeAsync()
        {
            var token = File.ReadAllText(@"C:\Users\senad\source\repos\TopliBOT\TopliBOT\token.txt");

            await _socketClient.LoginAsync(TokenType.Bot, token);
            await _socketClient.StartAsync();
            _socketClient.Log += Log;


            _serviceProvider = SetupServices();
            _commandHandler = new CommandHandler(_socketClient, _commandService, _serviceProvider);
            await _commandHandler.InitializeAsync();
            await _serviceProvider.GetRequiredService<MusicHelper>().InitializeAsync();
            await Task.Delay(-1);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }



        private IServiceProvider SetupServices()
                => new ServiceCollection()
                .AddSingleton(_socketClient)
                .AddSingleton(_commandService)
                .AddLavaNode(x => { x.SelfDeaf = false;  })
                .AddSingleton<LavaConfig>()
                .AddSingleton<MusicHelper>()
                .BuildServiceProvider();

    }
}
