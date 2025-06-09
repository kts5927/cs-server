using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var host = new ChatServerHost();
        await host.RunAsync();
    }
}
