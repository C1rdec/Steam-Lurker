using Lurker.Steam.Services;

var service = new SteamService();
var exe = await service.InitializeAsync();

//var game = service.FindGames();

Console.WriteLine(service.FindUsername());