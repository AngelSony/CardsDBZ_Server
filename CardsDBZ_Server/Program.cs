using CardsDBZ_Server;
using CardsDBZ_Server.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddSingleton<Lobby>();

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var Lobby = app.Services.GetRequiredService<Lobby>();
Lobby.Start();

app.MapHub<LobbyHub>("/lobby");

app.Run();