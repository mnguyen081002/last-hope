using LastHope.Core.Commands;
using LastHope.Systems.Shelter;
using UnityEngine;

namespace LastHope.Systems.Commands
{
    /// <summary>Sửa Pump Jam (Active Task, BL-P3-16) — tốn thời gian thật tại Shelter.</summary>
    public class RepairPumpJamCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public CommandResult Validate(GameContext context)
        {
            if (!AtShelter(context))
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ sửa được khi ở Shelter.");

            var pump = ShelterWaterSystem.FindModule(context.World.Shelter, ShelterModuleIds.Pump);
            if (pump == null || !pump.IsJammed)
                return CommandResult.Fail(CommandErrorCode.NotAllowedNow, "Pump không bị kẹt.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            int minutes = Mathf.RoundToInt(context.Definitions.Balance.Shelter.PumpJamResolveMinutes);
            context.Ticks.FastForward(minutes);

            var pump = ShelterWaterSystem.FindModule(context.World.Shelter, ShelterModuleIds.Pump);
            if (pump != null) pump.IsJammed = false;
        }

        static bool AtShelter(GameContext context) =>
            context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
            && location.IsShelter;
    }
}
