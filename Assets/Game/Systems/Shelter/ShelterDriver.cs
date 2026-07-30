using LastHope.Core.Events;
using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Hazard;

namespace LastHope.Systems.Shelter
{
    /// <summary>
    /// Nối các hệ thống Shelter (P3) vào tick, giống <see cref="Condition.ConditionDriver"/>.
    /// Dựng lại mỗi lần <c>GameServices.BindWorld</c>.
    /// </summary>
    public class ShelterDriver
    {
        readonly WorldState world;
        readonly DefinitionRegistry definitions;
        readonly EventBus events;
        readonly RngService rng;

        public ShelterDriver(WorldState world, DefinitionRegistry definitions, TickScheduler ticks, EventBus events, RngService rng)
        {
            this.world = world;
            this.definitions = definitions;
            this.events = events;
            this.rng = rng;

            ticks.ShortTick += OnShortTick;
            ticks.LongTick += OnLongTick;
        }

        void OnShortTick(long worldTimeMinutes)
        {
            string completedModuleId = BuildSystem.ApplyShortTick(world);
            if (completedModuleId != null)
            {
                events?.Publish(new ConstructionCompleted(completedModuleId));
            }
        }

        void OnLongTick(long worldTimeMinutes)
        {
            var phase = DisasterPhaseSystem.CurrentPhase(worldTimeMinutes, definitions.Balance.DisasterPhase);

            PowerSystem.Allocate(world.Shelter, definitions, phase);
            ShelterWaterSystem.ApplyLongTick(world.Shelter, definitions, phase);
            ShelterEventSystem.ApplyLongTick(world, definitions, rng.Stream(RngService.Events), events, phase);
        }
    }
}
