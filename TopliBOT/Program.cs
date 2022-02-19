using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TopliBOT;
using Victoria;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();


    public  async Task MainAsync()
    {
        await new TopliBOTClient().InitializeAsync();
    }





}