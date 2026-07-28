using LastHope.Core.Commands;
using UnityEngine;

namespace LastHope.Systems.Commands
{
    /// <summary>Sleep Simulation (BL-P3-13) — bơm thời gian qua TickScheduler như Travel, cộng thêm hồi Fatigue.</summary>
    public class SleepCommand : IGameCommand
    {
        public long WorldTime { get; set; }

        public float Hours;

        public SleepCommand(float hours) => Hours = hours;

        public CommandResult Validate(GameContext context)
        {
            if (!AtShelter(context))
                return CommandResult.Fail(CommandErrorCode.WrongLocation, "Chỉ ngủ được ở Shelter.");

            var balance = context.Definitions.Balance.Shelter;
            if (Hours < balance.SleepMinHours || Hours > balance.SleepMaxHours)
                return CommandResult.Fail(CommandErrorCode.InvalidTarget,
                    $"Chỉ ngủ được {balance.SleepMinHours}-{balance.SleepMaxHours} giờ.");

            return CommandResult.Ok();
        }

        public void Execute(GameContext context)
        {
            var balance = context.Definitions.Balance.Shelter;
            int minutes = Mathf.RoundToInt(Hours * 60f);
            context.Ticks.FastForward(minutes);

            var player = context.World.Player;
            player.Fatigue = Mathf.Max(0f, player.Fatigue - balance.SleepFatigueRecoveryPerHour * Hours);
        }

        static bool AtShelter(GameContext context) =>
            context.Definitions.TryGetLocation(context.World.Player.CurrentLocationId, out var location)
            && location.IsShelter;
    }
}
