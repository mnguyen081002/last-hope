using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;

namespace LastHope.Systems.Condition
{
    /// <summary>
    /// Nối <see cref="ConditionSystem"/> vào tick. Dựng lại mỗi lần
    /// <c>GameServices.BindWorld</c> (giống <c>TickScheduler</c>) — instance cũ cùng
    /// subscription của nó bị bỏ theo, không cần tự huỷ đăng ký thủ công.
    /// </summary>
    public class ConditionDriver
    {
        readonly WorldState world;
        readonly DefinitionRegistry definitions;

        public ConditionDriver(WorldState world, DefinitionRegistry definitions, TickScheduler ticks)
        {
            this.world = world;
            this.definitions = definitions;

            ticks.ShortTick += OnShortTick;
            ticks.LongTick += OnLongTick;
        }

        void OnShortTick(long worldTimeMinutes) =>
            ConditionSystem.ApplyShortTick(world.Player, definitions.Balance.Condition, IsAtShelter());

        void OnLongTick(long worldTimeMinutes) =>
            ConditionSystem.ApplyLongTick(world.Player, definitions.Balance.Condition);

        bool IsAtShelter() =>
            definitions.TryGetLocation(world.Player.CurrentLocationId, out var location) && location.IsShelter;
    }
}
