using LastHope.Core.Random;
using LastHope.Core.State;
using LastHope.Data;

namespace LastHope.Systems.Boot
{
    /// <summary>Dựng WorldState ban đầu từ balance.json — không hard-code số trong code.</summary>
    public static class NewGameFactory
    {
        public static WorldState Create(DefinitionRegistry definitions, ulong masterSeed)
        {
            var balance = definitions.Balance;

            var world = new WorldState
            {
                WorldTimeMinutes = 0,
                MasterSeed = masterSeed,
                Player = new PlayerState
                {
                    CurrentLocationId = balance.NewGame.StartLocationId,
                    Inventory = new InventoryState
                    {
                        CapacityKg = balance.Inventory.BackpackCapacityKg,
                        CapacityLiters = balance.Inventory.BackpackCapacityLiters,
                    },
                },
                Shelter = new ShelterState
                {
                    StructuralIntegrity = balance.Shelter.InitialStructuralIntegrity,
                    CleanWater = balance.Shelter.InitialCleanWater,
                    UntreatedWater = balance.Shelter.InitialUntreatedWater,
                },
            };

            // Ghi seed khởi điểm của từng stream để save đầu tiên đã có state đầy đủ.
            foreach (string name in new[] { RngService.Loot, RngService.Events, RngService.Npc })
            {
                world.RngStreams[name] = RngService.DeriveSeed(masterSeed, name);
            }

            return world;
        }
    }
}
