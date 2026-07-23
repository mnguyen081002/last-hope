using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
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
    /// always-on DebugOverlay). "Add Item" is the one sanctioned Command Layer bypass — an
    /// explicit, clearly-labeled cheat, not a pattern for gameplay code to follow.
    /// </summary>
    public sealed class DebugPanel : MonoBehaviour
    {
        private bool _visible;
        private Vector2 _stateScroll;
        private string _addItemId = "item_test";
        private string _addItemQty = "1";
        private string _fastForwardMinutes = "60";
        private string _saveSlotId = "manual_0";
        private string _statusMessage = "";

        private GameContext _ctx;
        private TickScheduler _scheduler;
        private SimulationDriver _driver;
        private SaveService _saveService;

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _scheduler);
            GameServiceRegistry.TryGet(out _saveService);
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

            GUILayout.BeginArea(new Rect(280, 10, 380, 640), GUI.skin.box);
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
            GUILayout.Label("Add Item (bypasses Command Layer — debug only)");
            GUILayout.BeginHorizontal();
            _addItemId = GUILayout.TextField(_addItemId, GUILayout.Width(180));
            _addItemQty = GUILayout.TextField(_addItemQty, GUILayout.Width(40));
            if (GUILayout.Button("Add") && int.TryParse(_addItemQty, out int qty) && qty > 0)
            {
                if (_ctx.Definitions.TryGetItem(_addItemId, out _))
                {
                    InventoryOps.AddItem(_ctx.World.Player.Inventory, _ctx.Definitions, _addItemId, qty,
                        () => System.Guid.NewGuid().ToString("N"));
                    _statusMessage = $"Added {qty}x {_addItemId}.";
                }
                else
                {
                    _statusMessage = $"Unknown item id '{_addItemId}'.";
                }
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

            GUILayout.EndArea();
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
            into.ActiveEvents = from.ActiveEvents;
            into.ActiveTasks = from.ActiveTasks;
            into.PersistentFlags = from.PersistentFlags;
            into.RandomSeed = from.RandomSeed;
            into.RngStreams = from.RngStreams;
            into.Player = from.Player;
        }
    }
}
