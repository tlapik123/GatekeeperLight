using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;

namespace GatekeeperLight;

public class RoleCheckAndGive {
    private readonly ulong _guildFromId;
    private readonly ulong _guildFromRoleId;
    private readonly ulong _guildToId;
    private readonly List<ulong> _guildToRoleIds;
    private DiscordGuild? _guildFrom;
    private DiscordRole? _guildFromRole;
    private DiscordGuild? _guildTo;
    private readonly List<DiscordRole?> _guildToRoles = new();

    public RoleCheckAndGive(ulong guildFromId, ulong guildFromRoleId, ulong guildToId, List<ulong> guildToRoleIds) {
        _guildFromId = guildFromId;
        _guildFromRoleId = guildFromRoleId;
        _guildToId = guildToId;
        _guildToRoleIds = guildToRoleIds;
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
        foreach (var toRoleId in _guildToRoleIds) {
            _guildToRoles.Add(_guildTo.GetRole(toRoleId));
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Check if the user has a configured role in a configured server and give him a configured role in configured server.  
    /// </summary>
    /// <param name="user">User to check and give a role to.</param>
    public async Task<bool> CheckAndGiveRole(DiscordUser user) {
        if (_guildFromRole is null || _guildToRoles.Count == 0 || _guildFrom is null || _guildTo is null) {
            // Object has not yet been initialized
            return false;
        }

        if (user.IsBot) {
            // TODO: somehow do the error handling
            return false;
        }

        // TODO there is some issue with getting members - why? 

        /*if (!_guildFrom.Members.TryGetValue(user.Id, out var guildFromMember)) {
            // TODO: user is not on the from server.
            Console.WriteLine("User not in from server");
            return false;
        }

        if (!_guildTo.Members.TryGetValue(user.Id, out var guildToMember)) {
            // TODO: user is not on the "to" server
            Console.WriteLine("User not in to server");
            return false;
        }*/

        DiscordMember guildFromMember;
        DiscordMember guildToMember;
        try {
            guildFromMember = await _guildFrom.GetMemberAsync(user.Id);
        } catch (NotFoundException) {
            return false;
        }
        try {
            guildToMember = await _guildTo.GetMemberAsync(user.Id);
        } catch (NotFoundException) {
            return false;
        }

        if (_guildToRoles.TrueForAll(toRole => guildToMember.Roles.Contains(toRole))) return true;

        if (!guildFromMember.Roles.Contains(_guildFromRole)) {
            // TODO: user doesnt have the required role
            return false;
        }

        foreach (var toRole in _guildToRoles) {
            await guildToMember.GrantRoleAsync(toRole,
                $"Also has role: {_guildFromRole.Name} in {_guildFrom.Name} server.");
        }

        Console.WriteLine($"Gave role to: {guildToMember.Username}: {guildToMember.Id}");
        return true;
    }
}