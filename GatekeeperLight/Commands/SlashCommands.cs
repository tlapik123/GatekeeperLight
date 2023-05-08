using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace GatekeeperLight.Commands; 

public sealed class SlashCommands : ApplicationCommandModule {
    private readonly RoleCheckAndGive _roleCheckAndGiveHandler;

    public SlashCommands(RoleCheckAndGive roleCheckAndGiveHandler) {
        _roleCheckAndGiveHandler = roleCheckAndGiveHandler;
    }

    [SlashCommand("verify", "Verify me based on a specific role (given by the admin) in a different server (given by the admin).")]
    public async Task VerifyCommand(InteractionContext ctx) {
        await _roleCheckAndGiveHandler.CheckAndGiveRole(ctx.User);
        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Executed the verify command."));
    }
}