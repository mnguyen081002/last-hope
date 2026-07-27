using System.IO;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Condition;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class ConditionDriverTests
    {
        DefinitionRegistry definitions;
        WorldState world;
        TickScheduler ticks;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));
            world = new WorldState();
            ticks = new TickScheduler(world, null);
        }

        [Test]
        public void ShortTick_AppliesConditionAutomatically_OnceWired()
        {
            new ConditionDriver(world, definitions, ticks);

            ticks.Advance(1, maxCatchUpMinutes: 1);

            Assert.AreEqual(
                definitions.Balance.Condition.ThirstPerHour / 60f, world.Player.Thirst, 0.0001f);
        }

        [Test]
        public void LongTick_AppliesFatigue_OnceWired()
        {
            new ConditionDriver(world, definitions, ticks);
            world.Player.CurrentLocationId = "location_convenience_store"; // không phải shelter

            ticks.Advance(10, maxCatchUpMinutes: 10);

            Assert.AreEqual(definitions.Balance.Condition.FatiguePerLongTick, world.Player.Fatigue, 0.0001f);
        }

        [Test]
        public void IsAtShelter_ReadsFromCurrentLocationDefinition()
        {
            new ConditionDriver(world, definitions, ticks);
            world.Player.CurrentLocationId = "location_shelter"; // IsShelter = true trong content
            world.Player.Wet = 50f; // dưới ngưỡng drift nên không tụt, chỉ test đường Wet dry

            ticks.Advance(1, maxCatchUpMinutes: 1);

            Assert.AreEqual(50f - definitions.Balance.Condition.WetDryPerMinuteAtShelter, world.Player.Wet, 0.0001f);
        }

        [Test]
        public void WithoutDriver_TickDoesNotTouchCondition()
        {
            // Không new ConditionDriver — tick chạy nhưng Condition phải đứng yên.
            ticks.Advance(60, maxCatchUpMinutes: 60);

            Assert.AreEqual(0f, world.Player.Thirst);
            Assert.AreEqual(0f, world.Player.Fatigue);
        }
    }
}
