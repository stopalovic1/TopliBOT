using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
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
            await StartLavalinkAsync();
            var token = await File.ReadAllTextAsync(AppDomain.CurrentDomain.BaseDirectory + "/Tokens/botToken.txt");

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

        private static async Task StartLavalinkAsync()
        {
            var process = new ProcessStartInfo
            {
                FileName = "java",
                Arguments = $"-jar \"{Path.Combine(AppContext.BaseDirectory, "Lavalink")}/Lavalink.jar\"",
                WorkingDirectory = Path.Combine(AppContext.BaseDirectory, "Lavalink"),
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Minimized
            };

            Process.Start(process);
            await Task.Delay(2000);
        }

        private IServiceProvider SetupServices()
                => new ServiceCollection()
                .AddSingleton(_socketClient)
                .AddSingleton(_commandService)
                .AddLavaNode(x => { x.SelfDeaf = false; })
                .AddSingleton<LavaConfig>()
                .AddSingleton<MusicHelper>()
                .AddTransient<HelperMethods>()
                .BuildServiceProvider();

    }
}
