using LastHope.Core.Commands;
using UnityEngine;

namespace LastHope.Systems.Commands
{
    /// <summary>Xử lý Drain Backflow (Active Task, BL-P3-14) — tốn thời gian thật tại Shelter.</summary>
    public class ResolveDrainBackflowCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public CommandResult Validate(GameContext context)
        {
            if (!AtShelter(context))
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ xử lý được khi ở Shelter.");

            if (!context.World.Shelter.DrainBackflowActive)
                return CommandResult.Fail(CommandErrorCode.NotAllowedNow, "Không có Drain Backflow đang xảy ra.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            int minutes = Mathf.RoundToInt(context.Definitions.Balance.Shelter.DrainBackflowResolveMinutes);
            context.Ticks.FastForward(minutes);
            context.World.Shelter.DrainBackflowActive = false;
        }

        static bool AtShelter(GameContext context) =>
            context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
            && location.IsShelter;
    }
}
