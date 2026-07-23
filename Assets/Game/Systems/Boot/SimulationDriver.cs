using LastHope.Core.Time;
using LastHope.Systems.Registry;
using UnityEngine;

namespace LastHope.Systems.Boot
{
    /// <summary>
    /// Sole Unity-Time-to-Core-time bridge (technical-specification.md mục 9/§8). Reads services
    /// in Start() (after every object's Awake in this scene load has run) rather than Awake(),
    /// so it does not depend on GameBootstrapper's component order. Debug pause/time-scale exist
    /// for tooling only — gameplay itself never pauses.
    /// </summary>
    public sealed class SimulationDriver : MonoBehaviour
    {
        [SerializeField] private int maxCatchUpMinutesPerFrame = 60;

        private TickScheduler _scheduler;
        private SimulationClock _clock;

        public bool DebugPaused { get; set; }
        public float DebugTimeScale { get; set; } = 1f;

        private void Start()
        {
            GameServiceRegistry.TryGet(out _scheduler);
            GameServiceRegistry.TryGet(out _clock);
        }

        private void Update()
        {
            if (_scheduler == null || _clock == null) return; // boot failed (definition load error)
            if (DebugPaused) return;

            float realDelta = Mathf.Min(Time.unscaledDeltaTime, 1f); // clamp OS-suspend / breakpoint spikes
            _clock.AccumulateRealSeconds(realDelta * DebugTimeScale);
            _scheduler.Advance(_clock, maxCatchUpMinutesPerFrame);
        }
    }
}
