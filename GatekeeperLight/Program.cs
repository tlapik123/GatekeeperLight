using DSharpPlus;
using DSharpPlus.SlashCommands;
using GatekeeperLight;
using Microsoft.Extensions.Logging;
using GatekeeperLight.Commands;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Starting the bot!");

// token as the first argument to the program
var token = args[0];

var argException = new ArgumentException("One of the arguments provided to the program was not convertible to Id");

if (!ulong.TryParse(args[1], out var guildFromId)) throw argException;
if (!ulong.TryParse(args[2], out var guildFromRoleId)) throw argException;
if (!ulong.TryParse(args[3], out var guildToId)) throw argException;

var guildToRoleIds = new List<ulong>();

for (var i = 4; i < args.Length; i++) {
    if (ulong.TryParse(args[i], out var guildToRoleId)) {
        guildToRoleIds.Add(guildToRoleId);
    } else {
        throw argException;
    }
}


// initialize the discord client
var discord = new DiscordClient(new DiscordConfiguration {
    Token = token,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.GuildMembers | DiscordIntents.Guilds,
    MinimumLogLevel = LogLevel.Debug,
});

var services = new ServiceCollection().AddSingleton<RoleCheckAndGive>(_ =>
    new RoleCheckAndGive(guildFromId, guildFromRoleId, guildToId, guildToRoleIds));
var serviceProvider = services.BuildServiceProvider();

var slash = discord.UseSlashCommands(new SlashCommandsConfiguration {
    Services = serviceProvider,
});

slash.RegisterCommands<SlashCommands>();
slash.SlashCommandErrored += (_, e) => {
    discord.Logger.LogError(new(800, "SlashCommand"), e.Exception, $"The {e.Exception.Message} exception was thrown in one of the Slash commands.");
    return Task.CompletedTask;
};

discord.GuildDownloadCompleted += serviceProvider.GetRequiredService<RoleCheckAndGive>().GuildsDownloadCompletedHandler;

discord.GuildMemberAdded += async (_, eArgs) => {
    await serviceProvider.GetRequiredService<RoleCheckAndGive>().CheckAndGiveRole(eArgs.Member);
};

// connect client
await discord.ConnectAsync();

// sleep main forever
await Task.Delay(-1);