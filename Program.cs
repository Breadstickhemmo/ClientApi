class Program
{
    public static async Task Main(string[] args)
    {
        var serverTask = Server.Run();
        var clientTask = Client.Run();

        await Task.WhenAll(serverTask, clientTask);
    }
}