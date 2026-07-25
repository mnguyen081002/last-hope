using System.Collections.Generic;
using LastHope.Core.Commands;
using LastHope.Core.Events;
using LastHope.Core.Text;
using LastHope.Systems.Registry;
using TMPro;
using UnityEngine;

namespace LastHope.UI.Events
{
    /// <summary>
    /// Short-lived event banner, top-center (S14). Queues one line per event lifecycle
    /// notification and shows each for a few seconds — the persistent list/response UI is
    /// EventPanel (V); this is just the "something happened" ping visible mid-gameplay.
    /// Undiscovered events never reach this: EventSystem only publishes EventTriggered for
    /// auto-discovered instances and EventDiscovered at discovery.
    /// </summary>
    public sealed class EventToast : MonoBehaviour
    {
        private const float SecondsPerToast = 4f;

        private readonly Queue<string> _queue = new Queue<string>();
        private TextMeshProUGUI _label;
        private CanvasGroup _group;
        private float _remaining;
        private GameContext _ctx;

        private void Awake()
        {
            _group = gameObject.AddComponent<CanvasGroup>();
            _group.alpha = 0f;
            _group.interactable = false;
            _group.blocksRaycasts = false;

            var bg = gameObject.AddComponent<UnityEngine.UI.Image>();
            bg.color = new Color(0f, 0f, 0f, 0.7f);

            var textGo = new GameObject("Text", typeof(RectTransform));
            textGo.transform.SetParent(transform, false);
            _label = textGo.AddComponent<TextMeshProUGUI>();
            _label.fontSize = 20;
            _label.color = Color.white;
            _label.alignment = TextAlignmentOptions.Center;
            var rect = textGo.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        private void Start()
        {
            if (!GameServiceRegistry.TryGet(out _ctx)) return;

            _ctx.Events.Subscribe<EventTriggered>(e => Enqueue($"Event: {Name(e.EventId)}"));
            _ctx.Events.Subscribe<EventDiscovered>(e => Enqueue($"Discovered: {Name(e.EventId)}"));
            _ctx.Events.Subscribe<EventDeadlineApproaching>(e => Enqueue($"Deadline approaching: {Name(e.EventId)}"));
            _ctx.Events.Subscribe<EventExpired>(e => Enqueue($"Too late: {Name(e.EventId)}"));
            _ctx.Events.Subscribe<EventResolved>(e => Enqueue($"Resolved: {Name(e.EventId)}"));
        }

        private static string Name(string eventId) => DisplayName.PrettifyWithoutPrefix(eventId, "event_");

        private void Enqueue(string message) => _queue.Enqueue(message);

        private void Update()
        {
            if (_remaining > 0f)
            {
                _remaining -= Time.deltaTime;
                if (_remaining <= 0f) _group.alpha = 0f;
                return;
            }

            if (_queue.Count == 0) return;
            _label.text = _queue.Dequeue();
            _group.alpha = 1f;
            _remaining = SecondsPerToast;
        }
    }
}
