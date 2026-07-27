using LastHope.Core.Time;
using UnityEngine;

namespace LastHope.Systems.Boot
{
    /// <summary>
    /// MonoBehaviour **duy nhất** đọc <c>Time.deltaTime</c>. Mọi hệ thống khác nhận thời
    /// gian qua tick — nhờ vậy sim tái lập được và test chạy không cần engine loop.
    /// </summary>
    public class SimulationDriver : MonoBehaviour
    {
        /// <summary>Chặn delta lớn (alt-tab, breakpoint) để không dồn hàng nghìn phút cùng lúc.</summary>
        const float MaxRealDelta = 1f;

        [SerializeField] float autosaveIntervalSeconds = 300f;

        float autosaveTimer;

        void Update()
        {
            if (!GameBootstrapper.IsReady) return;

            var services = GameBootstrapper.Services;

            float delta = Mathf.Min(Time.deltaTime, MaxRealDelta);
            int minutes = services.Clock.AccumulateRealSeconds(delta);
            if (minutes > 0) services.Ticks.Advance(minutes);

            if (autosaveIntervalSeconds <= 0f) return;

            autosaveTimer += Time.unscaledDeltaTime;
            if (autosaveTimer >= autosaveIntervalSeconds)
            {
                autosaveTimer = 0f;
                services.SaveAutosave();
            }
        }
    }
}
