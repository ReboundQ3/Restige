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
/// this one lets an admin name the pool explicitly.
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

    /// <summary>
    /// Pause between announcing a tie and opening the runoff, so players can read the result.
    /// </summary>
    private static readonly TimeSpan RunoffDelay = TimeSpan.FromSeconds(3);

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

        StartVote(shell.Player, poolId, maps, runoff: 0);

        shell.WriteLine(Loc.GetString("cmd-mapvotesv-started", ("pool", poolId), ("count", maps.Count)));
    }

    /// <summary>
    /// Opens a vote over <paramref name="maps"/>. <paramref name="runoff"/> is 0 for the initial
    /// vote and counts up once per tie-break re-run.
    /// </summary>
    private void StartVote(ICommonSession? initiator, string poolId, List<GameMapPrototype> maps, int runoff)
    {
        // A lone admin shouldn't have to sit out the full timer.
        var alone = _playerManager.PlayerCount == 1 && initiator != null;

        var options = new VoteOptions
        {
            Title = Loc.GetString(runoff > 0 ? "ui-vote-mapsv-title-runoff" : "ui-vote-mapsv-title"),
            Duration = TimeSpan.FromSeconds(alone
                ? _cfg.GetCVar(CCVars.VoteTimerAlone)
                : runoff > 0
                    ? _cfg.GetCVar(SVCCVars.MapVoteRunoffDuration)
                    : _cfg.GetCVar(CCVars.VoteTimerMap)),
        };

        foreach (var map in maps)
        {
            options.Options.Add((map.MapName, map));
        }

        options.SetInitiatorOrServer(initiator);

        var vote = _voteManager.CreateVote(options);
        vote.OnFinished += (_, args) => OnVoteFinished(args, initiator, poolId, runoff);

        var mapNames = string.Join("; ", maps.Select(map => map.MapName));
        var starter = runoff > 0
            ? $"Runoff {runoff} of the SV map vote"
            : $"{initiator?.ToString() ?? "The server"} started an SV map vote";
        _adminLogger.Add(LogType.Vote, LogImpact.Medium, $"{starter} for pool {poolId}: {mapNames}");
    }

    private void OnVoteFinished(VoteFinishedEventArgs args, ICommonSession? initiator, string poolId, int runoff)
    {
        if (args.Winner is GameMapPrototype outright)
        {
            ApplyResult(outright, Loc.GetString("ui-vote-mapsv-win", ("winner", outright.MapName)));
            return;
        }

        // Winner is null when several options tied for the top spot; they all land in Winners.
        var tied = args.Winners.Cast<GameMapPrototype>().ToList();

        // If nobody cast a vote, every option "tied" on zero and a runoff would just reopen the
        // exact same vote. That loops forever, so settle it here instead.
        var nobodyVoted = args.Votes.Count == 0 || args.Votes.Max() == 0;

        // Once the lobby is too far along to change the map, a runoff can't accomplish anything.
        // ApplyResult announces why below.
        if (nobodyVoted || runoff >= _cfg.GetCVar(SVCCVars.MapVoteMaxRunoffs) || !_gameTicker.CanUpdateMap())
        {
            var picked = (GameMapPrototype) _random.Pick(args.Winners);
            ApplyResult(picked, Loc.GetString("ui-vote-mapsv-tie", ("picked", picked.MapName)));
            return;
        }

        _chatManager.DispatchServerAnnouncement(Loc.GetString("ui-vote-mapsv-runoff",
            ("maps", string.Join(", ", tied.Select(map => map.MapName)))));
        _adminLogger.Add(LogType.Vote, LogImpact.Medium,
            $"SV map vote tied between {string.Join("; ", tied.Select(map => map.MapName))}, starting runoff {runoff + 1}");

        // VoteManager.Update() is enumerating its vote dictionary when it fires this callback, and
        // CreateVote adds to that same dictionary. Starting the runoff inline would throw, so it
        // has to happen on a later tick.
        Timer.Spawn(RunoffDelay, () => StartVote(initiator, poolId, tied, runoff + 1));
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

        // SelectMap only picks the map for the coming round. It deliberately does not write
        // CCVars.GameMap the way forcemapsv does, so a vote result doesn't stick forever.
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
