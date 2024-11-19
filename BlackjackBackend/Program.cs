namespace BlackjackBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IPlayerManager, PlayerManager>();

            builder.Logging.ClearProviders(); // Optional: Clear default providers
            builder.Logging.AddConsole();    // Add console logging
            builder.Logging.AddDebug();      // Add debug output logging

            var app = builder.Build();

            app.MapGet("/", () => "Hello World!");

            app.MapHub<BlackjackHub>("/signalr");

            app.Run();
        }
    }
}
