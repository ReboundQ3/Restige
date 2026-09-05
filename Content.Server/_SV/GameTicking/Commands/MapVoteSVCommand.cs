using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Voting;
using Content.Server.Voting.Managers;
using Content.Shared._SV.CCVar;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Database;
using Content.Shared.Maps;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Console;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._SV.GameTicking.Commands;

/// <summary>
/// SV - Starts a map vote between every map in a given <see cref="GameMapPoolPrototype"/>.
/// The upstream map vote derives its options from the active preset or the game.map_pool CVar;
/// this one lets an admin name the pool explicitly. Which is nice!
/// </summary>
[AdminCommand(AdminFlags.Round)]
public sealed partial class MapVoteSVCommand : LocalizedEntityCommands
{
    [Dependency] private IAdminLogManager _adminLogger = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private IGameMapManager _gameMapManager = default!;
    [Dependency] private IPlayerManager _playerManager = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private IVoteManager _voteManager = default!;
    [Dependency] private GameTicker _gameTicker = default!;

    public override string Command => "mapvotesv";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-need-exactly-one-argument"));
            return;
        }

        var poolId = args[0];

        if (!_prototypeManager.TryIndex<GameMapPoolPrototype>(poolId, out var pool))
        {
            shell.WriteError(Loc.GetString("cmd-mapvotesv-pool-not-found", ("pool", poolId)));
            return;
        }

        var maps = new List<GameMapPrototype>();
        foreach (var mapId in pool.Maps)
        {
            if (!_prototypeManager.TryIndex<GameMapPrototype>(mapId, out var mapProto))
            {
                shell.WriteError(Loc.GetString("cmd-mapvotesv-map-not-found", ("map", mapId), ("pool", poolId)));
                continue;
            }

            maps.Add(mapProto);
        }

        if (maps.Count == 0)
        {
            shell.WriteError(Loc.GetString("cmd-mapvotesv-pool-empty", ("pool", poolId)));
            return;
        }

        // GameMapPoolPrototype.Maps is a HashSet, so its iteration order isn't stable.
        // Sort by display name so the vote buttons don't shuffle between rounds.
        maps.Sort((a, b) => string.Compare(a.MapName, b.MapName, StringComparison.OrdinalIgnoreCase));

        StartVote(shell.Player, poolId, maps);

        shell.WriteLine(Loc.GetString("cmd-mapvotesv-started", ("pool", poolId), ("count", maps.Count)));
    }

    /// <summary>
    /// Opens a vote over <paramref name="maps"/>. <paramref name="runoff"/> is 0 for the initial
    /// vote and counts up once per tie-break re-run.
    /// </summary>
    private void StartVote(ICommonSession? initiator, string poolId, List<GameMapPrototype> maps)
    {
        var options = new VoteOptions
        {
            Title = Loc.GetString("ui-vote-mapsv-title"),
            Duration = TimeSpan.FromSeconds(_cfg.GetCVar(SVCCVars.MapVoteDuration))
        };

        foreach (var map in maps)
        {
            options.Options.Add((map.MapName, map));
        }

        options.SetInitiatorOrServer(initiator);

        var vote = _voteManager.CreateVote(options);
        vote.OnFinished += (_, args) => OnVoteFinished(args, initiator, poolId);

        var mapNames = string.Join("; ", maps.Select(map => map.MapName));
        var starter = $"{initiator?.ToString() ?? "The server"} started an SV map vote";
        _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"{starter} for pool {poolId}: {mapNames}");
    }

    private void OnVoteFinished(VoteFinishedEventArgs args, ICommonSession? initiator, string poolId)
    {
        if (args.Winner is GameMapPrototype map)
        {
            ApplyResult(map, Loc.GetString("ui-vote-mapsv-win", ("winner", map.MapName)));
            return;
        }

        var tied = args.Winners.Cast<GameMapPrototype>().ToList();
        var nobodyVoted = args.Votes.Count == 0 || args.Votes.Max() == 0;

        if (nobodyVoted || !_gameTicker.CanUpdateMap())
        {
            Timer.Spawn(10, () => StartVote(initiator, poolId, tied));
            return;
        }

        _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"SV map vote tied between {string.Join("; ", tied.Select(map => map.MapName))}, retrying vote");

        Timer.Spawn(10, () => StartVote(initiator, poolId, tied));

    }

    private void ApplyResult(GameMapPrototype picked, string announcement)
    {
        _chatManager.DispatchServerAnnouncement(announcement);
        _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"SV map vote finished: {picked.MapName}");

        if (!_gameTicker.CanUpdateMap())
        {
            if (_gameTicker.RoundPreloadTime <= TimeSpan.Zero)
            {
                _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-mapsv-notlobby"));
            }
            else
            {
                var timeString = $"{_gameTicker.RoundPreloadTime.Minutes:0}:{_gameTicker.RoundPreloadTime.Seconds:00}";
                _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-mapsv-notlobby-time", ("time", timeString)));
            }

            return;
        }

        if (!_gameMapManager.CheckMapExists(picked.ID))
        {
            _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-mapsv-invalid", ("winner", picked.MapName)));
            return;
        }

        _gameMapManager.SelectMap(picked.ID);
        _gameTicker.UpdateInfoText();
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length != 1)
            return CompletionResult.Empty;

        var options = _prototypeManager.EnumeratePrototypes<GameMapPoolPrototype>()
            .Select(pool => pool.ID)
            .OrderBy(id => id);

        return CompletionResult.FromHintOptions(options, Loc.GetString("cmd-mapvotesv-hint"));
    }
}
