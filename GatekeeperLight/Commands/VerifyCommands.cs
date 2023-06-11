using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;

namespace GatekeeperLight.Commands;

public sealed class VerifyCommands : ApplicationCommandModule {
    private readonly RoleCheckAndGive _roleCheckAndGiveHandler;

    public VerifyCommands(RoleCheckAndGive roleCheckAndGiveHandler) {
        _roleCheckAndGiveHandler = roleCheckAndGiveHandler;
    }

    [SlashCommand("verify",
        "Verify me based on a specific role (given by the admin) in a different server (given by the admin).")]
    public async Task VerifyCommand(InteractionContext ctx) {
        var result = await _roleCheckAndGiveHandler.CheckAndGiveRole(ctx.User);
        var message = result
            ? "Executed the verify command. Successfully."
            : "There was an error with the verify command. Do you have required role in the other Discord?";
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
            new DiscordInteractionResponseBuilder().WithContent(message));
    }
    
    [SlashCommand("verifyAll",
        "Verify all the people on the server")]
    [SlashRequireUserPermissions(Permissions.Administrator,false)]
    public async Task VerifyAllCommand(InteractionContext ctx) {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        
        Console.WriteLine("Starting the verify all process");

        var members = await ctx.Guild.GetAllMembersAsync();
        
        Console.WriteLine($"Number of members in guild: {members.Count}");
        foreach (var member in members) {
            Console.WriteLine($"Processing member: {member.Username}@{member.Id}");
            if (member.IsBot) continue;
            await _roleCheckAndGiveHandler.CheckAndGiveRole(member);
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done. Tried to verify all the users."));
    }
}