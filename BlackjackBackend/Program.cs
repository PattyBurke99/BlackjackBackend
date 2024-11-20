using BlackjackBackend.Services;

namespace BlackjackBackend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add CORS service and allow all origins, methods, and headers
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("Everything", policy =>
                    policy.AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .WithOrigins("http://localhost:5173"));
            });

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IPlayerStateService, PlayerStateService>();

            builder.Logging.ClearProviders(); // Optional: Clear default providers
            builder.Logging.AddConsole();    // Add console logging
            builder.Logging.AddDebug();      // Add debug output logging

            var app = builder.Build();

            app.UseCors("Everything");

            app.MapGet("/", () => "Hello World!");

            app.MapHub<BlackjackHub>("/signalr");

            app.Run();
        }
    }
}
