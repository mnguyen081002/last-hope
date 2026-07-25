using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Rules;
using LastHope.Core.Save;
using LastHope.Core.State;
using LastHope.Core.Time;
using LastHope.Systems.Boot;
using LastHope.Systems.Registry;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.DebugTools.Panel
{
    /// <summary>
    /// Debug Panel v1 (technical-specification.md mục 9/§38, BL-P1-12). Toggle F2 (F1 is the
    /// always-on DebugOverlay). "Add Item" and the Condition stat cheat are the sanctioned
    /// Command Layer bypasses — explicit, clearly-labeled cheats, not a pattern for gameplay
    /// code to follow.
    /// </summary>
    public sealed class DebugPanel : MonoBehaviour
    {
        private bool _visible;
        private Vector2 _panelScroll;
        private Vector2 _stateScroll;
        private string _addItemId = "item_test";
        private string _addItemQty = "1";
        private string _fastForwardMinutes = "60";
        private string _saveSlotId = "manual_0";
        private string _statusMessage = "";
        private string _conditionStatName = "health";
        private string _conditionStatDelta = "10";
        private string _equipItemInstanceId = "";
        private string _equipSlot = "body";
        private string _shelterWaterDelta = "10";
        private string _sleepMinutes = "480";

        private string _travelRouteId = "route_shelter_store";

        private GameContext _ctx;
        private TickScheduler _scheduler;
        private SimulationDriver _driver;
        private SaveService _saveService;
        private CommandProcessor _processor;

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _scheduler);
            GameServiceRegistry.TryGet(out _saveService);
            GameServiceRegistry.TryGet(out _processor);
            _driver = GetComponent<SimulationDriver>();
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f2Key.wasPressedThisFrame)
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;

            if (_ctx == null)
            {
                GUI.Box(new Rect(280, 10, 320, 40), "Debug Panel: services not ready (boot failed?)");
                return;
            }

            GUILayout.BeginArea(new Rect(280, 10, 380, Mathf.Min(640, Screen.height - 20)), GUI.skin.box);
            _panelScroll = GUILayout.BeginScrollView(_panelScroll);
            GUILayout.Label("Last Hope — Debug Panel (F2)");
            GUILayout.Label($"World time: {GameTimeUtil.Format(_ctx.World.WorldTimeMinutes)} (minute {_ctx.World.WorldTimeMinutes})");

            GUILayout.Space(6);
            GUILayout.Label("Clock");
            GUILayout.BeginHorizontal();
            _fastForwardMinutes = GUILayout.TextField(_fastForwardMinutes, GUILayout.Width(60));
            if (GUILayout.Button("Fast-forward") && int.TryParse(_fastForwardMinutes, out int minutes) && minutes > 0)
                _scheduler?.FastForward(minutes);
            GUILayout.EndHorizontal();

            if (_driver != null)
            {
                _driver.DebugPaused = GUILayout.Toggle(_driver.DebugPaused, "Pause simulation");
                GUILayout.Label($"Time scale: {_driver.DebugTimeScale:0.0}x");
                _driver.DebugTimeScale = GUILayout.HorizontalSlider(_driver.DebugTimeScale, 0f, 10f);
            }

            GUILayout.Space(6);
            GUILayout.Label("Condition");
            var condition = _ctx.World.Player.Condition;
            GUILayout.Label($"Health {condition.Health:0.0}  Stamina {condition.Stamina:0.0}  Fatigue {condition.Fatigue:0.0}");
            GUILayout.Label($"Hunger {condition.Hunger:0.0}  Thirst {condition.Thirst:0.0}  BodyTemp {condition.BodyTemperatureC:0.0}C  ({condition.Incapacitation})");
            if (condition.StatusEffects.Count > 0)
            {
                var statuses = new List<string>();
                foreach (var kvp in condition.StatusEffects) statuses.Add($"{kvp.Key}={kvp.Value.Severity:0}");
                GUILayout.Label("Status: " + string.Join(", ", statuses));
            }
            GUILayout.BeginHorizontal();
            _conditionStatName = GUILayout.TextField(_conditionStatName, GUILayout.Width(140));
            _conditionStatDelta = GUILayout.TextField(_conditionStatDelta, GUILayout.Width(60));
            if (GUILayout.Button("Apply delta") && float.TryParse(_conditionStatDelta, out float delta))
                ApplyConditionCheat(condition, _conditionStatName, delta);
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Equipment");
            var inv = _ctx.World.Player.Inventory;
            if (inv.EquipmentSlots.Count > 0)
            {
                var equipped = new List<string>();
                foreach (var kvp in inv.EquipmentSlots) equipped.Add($"{kvp.Key}={kvp.Value}");
                GUILayout.Label(string.Join(", ", equipped));
            }
            GUILayout.BeginHorizontal();
            _equipItemInstanceId = GUILayout.TextField(_equipItemInstanceId, GUILayout.Width(180));
            _equipSlot = GUILayout.TextField(_equipSlot, GUILayout.Width(80));
            if (GUILayout.Button("Equip") && _processor != null)
            {
                var result = _processor.Submit(new EquipItemCommand(_ctx.World.Player.ActorId, _equipItemInstanceId, _equipSlot));
                _statusMessage = result.Success ? $"Equipped to '{_equipSlot}'." : $"Equip failed: {result.Code}";
            }
            if (GUILayout.Button("Unequip") && _processor != null)
            {
                var result = _processor.Submit(new UnequipItemCommand(_ctx.World.Player.ActorId, _equipSlot));
                _statusMessage = result.Success ? $"Unequipped '{_equipSlot}'." : $"Unequip failed: {result.Code}";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Rest at Shelter (must be at a shelter location)");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Rest") && _processor != null)
            {
                var result = _processor.Submit(new RestAtShelterCommand(_ctx.World.Player.ActorId, RestMode.Rest));
                _statusMessage = result.Success ? "Rested." : $"Rest failed: {result.Code}";
            }
            if (GUILayout.Button("Treat Exposure") && _processor != null)
            {
                var result = _processor.Submit(new RestAtShelterCommand(_ctx.World.Player.ActorId, RestMode.TreatExposure));
                _statusMessage = result.Success ? "Treated exposure." : $"Treat failed: {result.Code}";
            }
            if (GUILayout.Button("Dry Off") && _processor != null)
            {
                var result = _processor.Submit(new RestAtShelterCommand(_ctx.World.Player.ActorId, RestMode.DryOff));
                _statusMessage = result.Success ? "Dried off." : $"Dry off failed: {result.Code}";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Sleep (must be at a shelter, S12 wakes early if it floods to Deep+)");
            GUILayout.BeginHorizontal();
            _sleepMinutes = GUILayout.TextField(_sleepMinutes, GUILayout.Width(60));
            if (GUILayout.Button("Sleep") && _processor != null && int.TryParse(_sleepMinutes, out int sleepMinutes) && sleepMinutes > 0)
            {
                var result = _processor.Submit(new StartSleepCommand(_ctx.World.Player.ActorId, sleepMinutes));
                _statusMessage = result.Success ? "Slept (see log for SleepEnded/SleepInterrupted)." : $"Sleep failed: {result.Code}";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Shelter (Water Intrusion)");
            string mainShelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (_ctx.World.ShelterStates.TryGetValue(mainShelterId, out var shelter))
            {
                GUILayout.Label($"Structural {shelter.StructuralIntegrity:0}  Water {shelter.WaterIntrusion.Level} ({shelter.WaterIntrusion.Units:0}/100)");
                GUILayout.Label($"Clean Water {shelter.WaterStocks.Clean:0}  Untreated {shelter.WaterStocks.Untreated:0}  Living {shelter.Occupants}/{shelter.LivingCapacity}");
                GUILayout.Label($"Modules built: {shelter.Modules.Count}  Active build tasks: {_ctx.World.ActiveTasks.Count}");
                if (shelter.EventFlags.Count > 0)
                    GUILayout.Label("Flags: " + string.Join(", ", shelter.EventFlags));
                GUILayout.BeginHorizontal();
                _shelterWaterDelta = GUILayout.TextField(_shelterWaterDelta, GUILayout.Width(60));
                if (GUILayout.Button("Add Water Units") && float.TryParse(_shelterWaterDelta, out float waterDelta))
                    ApplyShelterWaterCheat(shelter, waterDelta);
                GUILayout.EndHorizontal();
            }
            else
            {
                GUILayout.Label("(shelter state not initialized yet)");
            }

            GUILayout.Space(6);
            GUILayout.Label("Events (trigger cheat bypasses EventTriggerRules — testing only)");
            foreach (var eventDef in _ctx.Definitions.Events.Values)
            {
                if (GUILayout.Button($"Force-trigger '{eventDef.Id}'"))
                    ForceTriggerEventCheat(eventDef.Id);
            }
            foreach (var activeEvent in _ctx.World.ActiveEvents)
            {
                // Debug tool shows Undiscovered too (player UI hides them) — resolve buttons on an
                // Undiscovered instance fail with EventNotDiscovered, which is itself useful to see.
                if (activeEvent.State != EventLifecycleState.Active && activeEvent.State != EventLifecycleState.Undiscovered) continue;
                GUILayout.BeginHorizontal();
                GUILayout.Label($"{activeEvent.EventId} [{activeEvent.State}]", GUILayout.Width(220));
                if (_ctx.Definitions.TryGetEvent(activeEvent.EventId, out var def))
                {
                    foreach (string responseId in def.AvailableResponses)
                    {
                        if (GUILayout.Button(responseId) && _processor != null)
                        {
                            var result = _processor.Submit(new ResolveEventCommand(_ctx.World.Player.ActorId, activeEvent.EventInstanceId, responseId));
                            _statusMessage = result.Success ? $"Resolved '{activeEvent.EventId}' via '{responseId}'." : $"Resolve failed: {result.Code}";
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(6);
            GUILayout.Label("Phase jump (cheat)");
            foreach (var phase in _ctx.Definitions.DisasterPhasesSorted)
            {
                if (GUILayout.Button($"Jump to '{phase.Id}' (minute {phase.StartMinute})"))
                {
                    _ctx.World.WorldTimeMinutes = phase.StartMinute;
                    _ctx.Events.Publish(new WorldStateReloaded());
                    _statusMessage = $"Jumped to '{phase.Id}'.";
                }
            }

            GUILayout.Space(6);
            GUILayout.Label("Add Item (bypasses Command Layer — debug only)");
            GUILayout.BeginHorizontal();
            _addItemId = GUILayout.TextField(_addItemId, GUILayout.Width(180));
            _addItemQty = GUILayout.TextField(_addItemQty, GUILayout.Width(40));
            if (GUILayout.Button("Add") && int.TryParse(_addItemQty, out int qty) && qty > 0)
            {
                if (_ctx.Definitions.TryGetItem(_addItemId, out var addedDef))
                {
                    var instance = InventoryOps.AddItem(_ctx.World.Player.Inventory, _ctx.Definitions, _addItemId, qty,
                        () => System.Guid.NewGuid().ToString("N"));
                    _equipItemInstanceId = instance.InstanceId;
                    if (!string.IsNullOrEmpty(addedDef.EquipSlot))
                        _equipSlot = addedDef.EquipSlot;
                    _statusMessage = $"Added {qty}x {_addItemId} (instance '{instance.InstanceId}', pre-filled into Equip above).";
                }
                else
                {
                    _statusMessage = $"Unknown item id '{_addItemId}'.";
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label($"Travel (at '{_ctx.World.Player.CurrentLocationId}')");
            GUILayout.BeginHorizontal();
            _travelRouteId = GUILayout.TextField(_travelRouteId, GUILayout.Width(180));
            if (GUILayout.Button("Travel") && _processor != null)
            {
                CommandResult result = _processor.Submit(new BeginTravelCommand(_ctx.World.Player.ActorId, _travelRouteId));
                _statusMessage = result.Success ? $"Arrived at '{_ctx.World.Player.CurrentLocationId}'." : $"Travel failed: {result.Code}";
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.Label("Save / Load");
            GUILayout.BeginHorizontal();
            _saveSlotId = GUILayout.TextField(_saveSlotId, GUILayout.Width(180));
            if (GUILayout.Button("Save") && _saveService != null)
            {
                SaveResult result = _saveService.SaveToSlot(_ctx.World, _saveSlotId);
                _statusMessage = result.Success ? $"Saved to '{result.SlotId}'." : $"Save failed: {result.Error}";
            }
            if (GUILayout.Button("Autosave") && _saveService != null)
            {
                SaveResult result = _saveService.Autosave(_ctx.World);
                _statusMessage = result.Success ? $"Autosaved to '{result.SlotId}'." : $"Autosave failed: {result.Error}";
            }
            if (GUILayout.Button("Load (typed id)")) LoadSlot(_saveSlotId);
            GUILayout.EndHorizontal();

            GUILayout.Label("Load a slot:");
            IReadOnlyList<SaveSlotInfo> slots = _saveService?.ListSlots();
            if (slots == null || slots.Count == 0)
            {
                GUILayout.Label("(no saves found)");
            }
            else
            {
                foreach (SaveSlotInfo slot in slots)
                {
                    if (GUILayout.Button($"Load '{slot.SlotId}' — {slot.SavedAtUtc}"))
                        LoadSlot(slot.SlotId);
                }
            }

            if (!string.IsNullOrEmpty(_statusMessage))
                GUILayout.Label(_statusMessage);

            GUILayout.Space(6);
            GUILayout.Label("State dump");
            _stateScroll = GUILayout.BeginScrollView(_stateScroll, GUILayout.Height(240));
            GUILayout.TextArea(WorldStateSerializer.Serialize(_ctx.World));
            GUILayout.EndScrollView();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void ApplyConditionCheat(PlayerConditionState condition, string statName, float delta)
        {
            var cfg = _ctx.Definitions.Balance.Condition;
            switch (statName.Trim().ToLowerInvariant())
            {
                case "health": ConditionOps.ApplyHealth(condition, delta); break;
                case "stamina": ConditionOps.ApplyStamina(condition, delta); break;
                case "fatigue": ConditionOps.ApplyFatigue(condition, delta); break;
                case "hunger": ConditionOps.ApplyHunger(condition, delta); break;
                case "thirst": ConditionOps.ApplyThirst(condition, delta); break;
                case "bodytemp": condition.BodyTemperatureC += delta; break;
                case "exposure_black_water":
                    ConditionOps.AddExposure(condition, "black_water", delta);
                    ConditionOps.ApplyExposureStatusChain(condition, "black_water", _ctx.World.WorldTimeMinutes, cfg);
                    break;
                default:
                    _statusMessage = $"Unknown condition stat '{statName}' (health/stamina/fatigue/hunger/thirst/bodytemp/exposure_black_water).";
                    return;
            }
            ConditionOps.RecomputeIncapacitation(condition, cfg);
            _statusMessage = $"Applied {delta:+0.0;-0.0} to '{statName}'.";
        }

        private void ApplyShelterWaterCheat(ShelterState shelter, float delta)
        {
            var cfg = _ctx.Definitions.Balance.Shelter;
            shelter.WaterIntrusion.Units = WaterIntrusionRules.Clamp01To100(shelter.WaterIntrusion.Units + delta);
            var newLevel = WaterIntrusionRules.LevelFor(shelter.WaterIntrusion.Units, cfg);
            if (newLevel != shelter.WaterIntrusion.Level)
            {
                shelter.WaterIntrusion.Level = newLevel;
                _ctx.Events.Publish(new ShelterWaterChanged(shelter.Id, newLevel));
            }
            _statusMessage = $"Shelter water now {shelter.WaterIntrusion.Units:0} ({shelter.WaterIntrusion.Level}).";
        }

        private void ForceTriggerEventCheat(string eventId)
        {
            if (_ctx.World.ActiveEvents.Exists(e => e.EventId == eventId && e.State != EventLifecycleState.Resolved))
            {
                _statusMessage = $"'{eventId}' already active.";
                return;
            }

            string shelterId = _ctx.Definitions.Balance.NewGame.MainShelterId;
            if (!_ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
            {
                _statusMessage = "Shelter state not initialized.";
                return;
            }

            _ctx.Definitions.TryGetEvent(eventId, out var def);
            var instance = new ActiveEventState
            {
                EventInstanceId = System.Guid.NewGuid().ToString("N"),
                EventId = eventId,
                State = EventLifecycleState.Active,
                TriggeredAtMinute = _ctx.World.WorldTimeMinutes,
                DeadlineMinute = def.HardDeadlineMinutes > 0 ? _ctx.World.WorldTimeMinutes + def.HardDeadlineMinutes : (long?)null,
                SoftDeadlineMinute = def.SoftDeadlineMinutes > 0 ? _ctx.World.WorldTimeMinutes + def.SoftDeadlineMinutes : (long?)null,
            };
            _ctx.World.ActiveEvents.Add(instance);

            if (def.Tags.Contains("drain_backflow")) shelter.EventFlags.Add(ShelterEventFlags.DrainBackflowActive);
            else if (def.Tags.Contains("pump_jam")) shelter.EventFlags.Add(ShelterEventFlags.PumpJammed);

            _ctx.Events.Publish(new EventTriggered(instance.EventInstanceId, eventId));
            _statusMessage = $"Force-triggered '{eventId}'.";
        }

        private void LoadSlot(string slotId)
        {
            if (_saveService == null) return;

            LoadResult result = _saveService.Load(slotId);
            if (result.Success)
            {
                CopyWorldState(result.World, _ctx.World);
                _ctx.Events.Publish(new WorldStateReloaded());
                _statusMessage = $"Loaded '{slotId}'.";
            }
            else
            {
                _statusMessage = $"Load failed: {result.Error}";
            }
        }

        // Load() returns a brand-new WorldState; every other service already holds a reference
        // to GameContext.World, so fields are copied onto it in place rather than swapping the
        // reference (which would leave TickScheduler/RngService holding a stale World).
        private static void CopyWorldState(WorldState from, WorldState into)
        {
            into.WorldTimeMinutes = from.WorldTimeMinutes;
            into.CurrentDisasterPhase = from.CurrentDisasterPhase;
            into.RouteStates = from.RouteStates;
            into.LocationStates = from.LocationStates;
            into.ShelterStates = from.ShelterStates;
            into.NpcStates = from.NpcStates;
            into.Intel = from.Intel;
            into.DecisionLog = from.DecisionLog;
            into.ActiveEvents = from.ActiveEvents;
            into.ActiveTasks = from.ActiveTasks;
            into.TaskInventories = from.TaskInventories;
            into.PersistentFlags = from.PersistentFlags;
            into.RandomSeed = from.RandomSeed;
            into.RngStreams = from.RngStreams;
            into.Player = from.Player;
            into.PlaythroughId = from.PlaythroughId;
        }
    }
}
