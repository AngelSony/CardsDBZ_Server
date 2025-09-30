using CardsDBZ_Server;
using CardsDBZ_Server.Hubs;
using Microsoft.Extensions.Logging.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<Lobby>();

var logFilePath = Path.Combine(AppContext.BaseDirectory, "console-log.txt");
var fileStream = new FileStream(logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read);
var streamWriter = new StreamWriter(fileStream) { AutoFlush = true };
var app = builder.Build();

Console.SetOut(streamWriter);

app.MapGet("/", () => "Hello World!");

var Lobby = app.Services.GetRequiredService<Lobby>();
Lobby.Start();
app.MapHub<LobbyHub>("/lobby");


app.Run();