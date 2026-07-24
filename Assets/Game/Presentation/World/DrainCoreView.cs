using LastHope.Core.Commands;
using LastHope.Core.Logging;
using LastHope.Presentation.Interaction;
using UnityEngine;

namespace LastHope.Presentation.World
{
    /// <summary>Drain Core (main-shelter-design.md §5.3) is the one Fixed Core Component with
    /// real interaction in S10: reading current Water Intrusion. Clean/valve/seal/pump actions
    /// described in the design doc arrive with S11's Build System (Portable Pump) — nothing to
    /// act on yet, so Interact is read-only.</summary>
    public sealed class DrainCoreView : MonoBehaviour, IInteractable
    {
        [SerializeField] private string shelterId = "shelter_main";

        public string PromptText => "Check Drain Core";

        private void Awake() => WorldLabel.Create(transform, "Drain Core");

        public bool CanInteract(GameContext ctx) => true;

        public void Interact(GameContext ctx, CommandProcessor processor)
        {
            if (!ctx.World.ShelterStates.TryGetValue(shelterId, out var shelter))
            {
                GameLog.Info(LogCategory.World, "Drain Core: shelter state not found.");
                return;
            }

            GameLog.Info(LogCategory.World,
                $"Drain Core: water intrusion {shelter.WaterIntrusion.Level} ({shelter.WaterIntrusion.Units:0}/100).");
        }
    }
}
