using Discord;
using Discord.Commands;
using Discord.WebSocket;
using TopliBOT;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();
    DiscordSocketClient _client;
    private static CommandService _commands;

    private CommandHandler commandHandler;
    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }

    public async Task MainAsync()
    {
        _commands = new CommandService();
        _client = new DiscordSocketClient();
        commandHandler = new CommandHandler(_client, _commands);

        _client.Log += Log;

        var token = File.ReadAllText(@"C:\Users\senad\source\repos\TopliBOT\TopliBOT\token.txt");

        await commandHandler.InitializeAsync();
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        await Task.Delay(-1);
    }





}