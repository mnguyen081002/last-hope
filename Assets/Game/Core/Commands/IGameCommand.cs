using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;

namespace LastHope.Core.Commands
{
    public enum CommandErrorCode
    {
        None,
        InvalidActor,
        InvalidTarget,
        ItemNotFound,
        InventoryFull,
        Overloaded,
        AlreadyActive,
        NotActive,
        RouteBlocked,
        InvalidState,
        InternalError,
        NotAtLocation,
        Incapacitated,
        SlotMismatch,
        NoMedicalItem,
        SlotOccupied,
        SlotLocked,
        MissingMaterials,
        TaskNotFound,
        TaskNotRunning,
        NoPower,
        NoFilter,
        NothingToPurify,
        NoBedAvailable,
        UnsafeToSleep,
        EventNotActive,
        ResponseUnavailable,
        EventNotDiscovered,
        NpcUnavailable,
        NpcNotRecruited,
        CapacityFull,
    }

    public readonly struct CommandResult
    {
        public bool Success { get; }
        public CommandErrorCode Code { get; }
        public string DebugMessage { get; }

        private CommandResult(bool success, CommandErrorCode code, string debugMessage)
        {
            Success = success;
            Code = code;
            DebugMessage = debugMessage;
        }

        public static CommandResult Ok() => new CommandResult(true, CommandErrorCode.None, null);
        public static CommandResult Fail(CommandErrorCode code, string debugMessage = null) =>
            new CommandResult(false, code, debugMessage);
    }

    /// <summary>
    /// The one dependency bundle every command/system needs. Built once at boot.
    /// </summary>
    public sealed class GameContext
    {
        public WorldState World { get; }
        public DefinitionRegistry Definitions { get; }
        public EventBus Events { get; }
        public RngService Rng { get; }
        public TickScheduler Clock { get; }

        public GameContext(WorldState world, DefinitionRegistry definitions, EventBus events, RngService rng, TickScheduler clock)
        {
            World = world;
            Definitions = definitions;
            Events = events;
            Rng = rng;
            Clock = clock;
        }
    }

    /// <summary>
    /// Every state mutation goes through a command (technical-specification.md mục 9/§35).
    /// ActorId/TargetId are explicit (no "the player" implicit state) so commands stay
    /// trivially serializable for a future network layer.
    /// </summary>
    public interface IGameCommand
    {
        string ActorId { get; }
        string TargetId { get; }
        long WorldTime { get; set; }

        /// <summary>Pure read — must not mutate GameContext.</summary>
        CommandResult Validate(GameContext ctx);

        /// <summary>Only called after Validate succeeds.</summary>
        void Execute(GameContext ctx);
    }
}
