using System.IO;
using LastHope.Core.Events;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Travel;
using NUnit.Framework;
using UnityEngine;

namespace LastHope.Tests.EditMode
{
    public class TravelSystemTests
    {
        const string RouteShelterStore = "route_shelter_store"; // 25 phút, nối location_shelter <-> location_convenience_store

        DefinitionRegistry definitions;
        WorldState world;
        EventBus events;
        TickScheduler ticks;

        [SetUp]
        public void SetUp()
        {
            definitions = DefinitionLoader.LoadFromDirectory(
                Path.Combine(Application.streamingAssetsPath, "Definitions"));

            world = new WorldState();
            world.Player.CurrentLocationId = "location_shelter";
            world.Player.Inventory.CapacityKg = definitions.Balance.Inventory.BackpackCapacityKg;
            world.Player.Inventory.CapacityLiters = definitions.Balance.Inventory.BackpackCapacityLiters;

            events = new EventBus();
            ticks = new TickScheduler(world, events);
        }

        [Test]
        public void ComputeTravelMinutes_Normal_MatchesRouteDefinition()
        {
            int minutes = TravelSystem.ComputeTravelMinutes(world, definitions, RouteShelterStore);

            Assert.AreEqual(25, minutes); // loadFactorNormal = 1.0
        }

        [Test]
        public void ComputeTravelMinutes_Heavy_AppliesLoadFactor()
        {
            // Backpack cap 15kg: 2 toolbox (16kg) + 5 water bottle (4kg) = 20kg = 133% -> Heavy [130%,150%).
            world.Player.Inventory.Slots.Add(new ItemInstanceState { ItemId = "item_toolbox", Quantity = 2 });
            world.Player.Inventory.Slots.Add(new ItemInstanceState { ItemId = "item_water_bottle", Quantity = 5 });

            int minutes = TravelSystem.ComputeTravelMinutes(world, definitions, RouteShelterStore);

            int expected = Mathf.RoundToInt(25 * definitions.Balance.Travel.LoadFactorHeavy);
            Assert.AreEqual(expected, minutes);
        }

        [Test]
        public void Travel_AdvancesWorldTime_ByComputedMinutes()
        {
            long before = world.WorldTimeMinutes;
            int expectedMinutes = TravelSystem.ComputeTravelMinutes(world, definitions, RouteShelterStore);

            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);

            Assert.AreEqual(before + expectedMinutes, world.WorldTimeMinutes);
        }

        [Test]
        public void Travel_ChangesCurrentLocation_ToOtherEnd()
        {
            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);

            Assert.AreEqual("location_convenience_store", world.Player.CurrentLocationId);
        }

        [Test]
        public void Travel_AddsFatiguePerTravel()
        {
            // TickScheduler ở đây không gắn ConditionDriver (test dựng thẳng, không qua
            // GameServices) nên fatigue theo tick không tự chạy — chỉ có cộng một lần của
            // TravelSystem.Travel. ConditionDriver được test riêng ở ConditionDriverTests.
            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);

            Assert.AreEqual(definitions.Balance.Condition.FatiguePerTravel, world.Player.Fatigue, 0.0001f);
        }

        [Test]
        public void Travel_RoundTrip_ReturnsToOriginalLocation()
        {
            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);
            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);

            Assert.AreEqual("location_shelter", world.Player.CurrentLocationId);
        }

        [Test]
        public void Travel_FiresShortTicksForEveryMinute()
        {
            int shortTickCount = 0;
            ticks.ShortTick += _ => shortTickCount++;

            TravelSystem.Travel(world, definitions, ticks, RouteShelterStore);

            Assert.AreEqual(25, shortTickCount, "FastForward phải chạy từng phút, không nhảy cộc.");
        }
    }
}
