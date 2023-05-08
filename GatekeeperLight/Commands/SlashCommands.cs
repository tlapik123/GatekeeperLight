using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace GatekeeperLight.Commands;

public sealed class SlashCommands : ApplicationCommandModule {
    private readonly RoleCheckAndGive _roleCheckAndGiveHandler;

    public SlashCommands(RoleCheckAndGive roleCheckAndGiveHandler) {
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
    public async Task VerifyAllCommand(InteractionContext ctx) {
        await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

        foreach (var member in await ctx.Guild.GetAllMembersAsync()) {
            if (member.IsBot) continue;
            await _roleCheckAndGiveHandler.CheckAndGiveRole(member);
        }
        await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Done. Tried to verify all the users."));
    }
}