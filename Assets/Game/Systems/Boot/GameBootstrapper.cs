using System;
using System.IO;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Logging;
using LastHope.Core.Random;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Data;
using LastHope.Systems.Condition;
using LastHope.Systems.Disaster;
using LastHope.Systems.Events;
using LastHope.Systems.Hazard;
using LastHope.Systems.Inventory;
using LastHope.Systems.Registry;
using LastHope.Systems.Shelter;
using LastHope.Systems.Tasks;
using LastHope.Systems.Telemetry;
using UnityEngine;

namespace LastHope.Systems.Boot
{
    /// <summary>
    /// Composition root, lives in 10_GamePersistent (technical-specification.md mục 9/§14):
    /// loads Definitions, builds a new WorldState, constructs every Core service via constructor
    /// injection, and registers them in GameServiceRegistry. Fails fast on Definition errors —
    /// does not fall back to a partially-loaded registry.
    /// </summary>
    public sealed class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private ulong newGameSeed;

        private void Awake()
        {
            GameLog.Info(LogCategory.Boot, "GameBootstrapper: loading definitions...");

            string definitionsPath = Path.Combine(Application.streamingAssetsPath, "Definitions");
            DefinitionLoadResult loadResult = DefinitionLoader.Load(definitionsPath);

            if (!loadResult.Success)
            {
                GameLog.Error(LogCategory.Boot,
                    "GameBootstrapper: definition load failed, boot halted:\n" + string.Join("\n", loadResult.Errors));
                enabled = false;
                return;
            }

            var world = new WorldState
            {
                RandomSeed = newGameSeed != 0 ? newGameSeed : (ulong)DateTime.UtcNow.Ticks,
                PlaythroughId = Guid.NewGuid().ToString("N"),
            };
            world.Player.CurrentLocationId = loadResult.Registry.Balance.NewGame.StartLocationId;

            var bus = new EventBus();
            var tickScheduler = new TickScheduler(world, bus);
            var clock = new SimulationClock();
            var rng = new RngService(world);
            var ctx = new GameContext(world, loadResult.Registry, bus, rng, tickScheduler);
            var processor = new CommandProcessor(ctx);
            var saveService = new SaveService(
                Path.Combine(Application.persistentDataPath, "Saves"), loadResult.Registry.DefinitionVersion);
            var inventorySystem = new InventorySystem(ctx);
            inventorySystem.RecomputeAll();
            var telemetry = new TelemetryLogger(
                Path.Combine(Application.persistentDataPath, "Telemetry"), ctx, Guid.NewGuid().ToString("N"));
            var disasterPhaseSystem = new DisasterPhaseSystem(ctx);
            var conditionSystem = new ConditionSystem(ctx);
            var hazardSystem = new HazardSystem(ctx);
            // Construction order = long-tick subscriber order: tasks complete builds, power
            // allocates to the new modules, THEN water intrusion reads pump Active state — all
            // within the same tick. (P3 review 2026-07-25: WaterIntrusionSystem used to subscribe
            // first, so a freshly powered pump only took effect one long-tick late.)
            var taskSystem = new TaskSystem(ctx);
            var powerSystem = new PowerSystem(ctx);
            var waterIntrusionSystem = new WaterIntrusionSystem(ctx);
            var waterSystem = new WaterSystem(ctx);
            var eventSystem = new EventSystem(ctx);

            GameServiceRegistry.Register(ctx);
            GameServiceRegistry.Register(tickScheduler);
            GameServiceRegistry.Register(clock);
            GameServiceRegistry.Register(processor);
            GameServiceRegistry.Register(saveService);
            GameServiceRegistry.Register(inventorySystem);
            GameServiceRegistry.Register(telemetry);
            GameServiceRegistry.Register(disasterPhaseSystem);
            GameServiceRegistry.Register(conditionSystem);
            GameServiceRegistry.Register(hazardSystem);
            GameServiceRegistry.Register(waterIntrusionSystem);
            GameServiceRegistry.Register(taskSystem);
            GameServiceRegistry.Register(powerSystem);
            GameServiceRegistry.Register(waterSystem);
            GameServiceRegistry.Register(eventSystem);

            GameLog.Info(LogCategory.Boot,
                $"GameBootstrapper: ready. Seed={world.RandomSeed}, DefinitionVersion={loadResult.Registry.DefinitionVersion}.");
        }
    }
}
