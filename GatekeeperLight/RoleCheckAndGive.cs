using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace GatekeeperLight;

public class RoleCheckAndGive {
    private readonly ulong _guildFromId;
    private readonly ulong _guildFromRoleId;
    private readonly ulong _guildToId;
    private readonly ulong _guildToRoleId;

    private DiscordGuild? _guildFrom;
    private DiscordRole? _guildFromRole;
    private DiscordGuild? _guildTo;
    private DiscordRole? _guildToRole;

    public RoleCheckAndGive(ulong guildFromId, ulong guildFromRoleId, ulong guildToId, ulong guildToRoleId) {
        _guildFromId = guildFromId;
        _guildFromRoleId = guildFromRoleId;
        _guildToId = guildToId;
        _guildToRoleId = guildToRoleId;
    }
    
    /// <summary>
    /// When the guilds are downloaded populate the guild and role fields.
    /// </summary>
    /// <param name="_">DC client (unused)</param>
    /// <param name="e">Guilds that the bot is in.</param>
    /// <returns><see cref="Task"/>Completed task.</returns>
    public Task GuildsDownloadCompletedHandler(DiscordClient _, GuildDownloadCompletedEventArgs e) {
        _guildFrom = e.Guilds[_guildFromId];
        _guildTo = e.Guilds[_guildToId];

        _guildFromRole = _guildFrom.GetRole(_guildFromRoleId);
        _guildToRole = _guildTo.GetRole(_guildToRoleId);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Check if the user has a configured role in a configured server and give him a configured role in configured server.  
    /// </summary>
    /// <param name="user">User to check and give a role to.</param>
    public async Task<bool> CheckAndGiveRole(DiscordUser user) {
        if (_guildFromRole is null || _guildToRole is null || _guildFrom is null || _guildTo is null) {
            // Object has not yet been initialized
            return false;
        }

        var guildFromMember = await _guildFrom.GetMemberAsync(user.Id);
        var guildToMember = await _guildTo.GetMemberAsync(user.Id);
        if (user.IsBot || guildFromMember is null || guildToMember is null) {
            // TODO: somehow do the error handling
            return false;
        }

        if (!guildFromMember.Roles.Contains(_guildFromRole)) {
            // TODO: user doesnt have the required role
            return false;
        }

        await guildToMember.GrantRoleAsync(_guildToRole,
            $"Also has role: {_guildFromRole.Name} in {_guildFrom.Name} server.");
        return true;
    }
}