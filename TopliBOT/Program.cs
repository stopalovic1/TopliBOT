using TopliBOT;

public class Program
{
    public static async Task Main(string[] args)
    {
        await new TopliBOTClient().InitializeAsync();
    }

}