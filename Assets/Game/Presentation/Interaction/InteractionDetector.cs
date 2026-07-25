using System;
using LastHope.Core.Commands;
using LastHope.Systems.Registry;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.Interaction
{
    /// <summary>
    /// Finds the nearest IInteractable within range around the player, preferring one the
    /// cursor points at. Polls on an interval rather than every frame (interactables don't
    /// move fast enough to need per-frame detection). 2026-07-25: Physics2D (3D->2D migration).
    /// </summary>
    public sealed class InteractionDetector : MonoBehaviour
    {
        [SerializeField] private float radius = 1.6f;
        [SerializeField] private float pollInterval = 0.15f;
        [SerializeField] private InputActionAsset inputActions;

        private GameContext _ctx;
        private CommandProcessor _processor;
        private InputAction _interactAction;
        private float _pollTimer;
        private readonly Collider2D[] _overlapBuffer = new Collider2D[16];

        public IInteractable Current { get; private set; }
        public event Action<IInteractable> TargetChanged;

        public void SetInputActions(InputActionAsset asset) => inputActions = asset;

        private void Awake()
        {
            if (inputActions != null)
            {
                var map = inputActions.FindActionMap("Gameplay", throwIfNotFound: false);
                _interactAction = map?.FindAction("Interact", throwIfNotFound: false);
            }
        }

        private void OnEnable() => _interactAction?.Enable();
        private void OnDisable() => _interactAction?.Disable();

        private void Start()
        {
            GameServiceRegistry.TryGet(out _ctx);
            GameServiceRegistry.TryGet(out _processor);
        }

        private void Update()
        {
            _pollTimer -= Time.deltaTime;
            if (_pollTimer <= 0f)
            {
                _pollTimer = pollInterval;
                Rescan();
            }

            if (_ctx != null && Current != null && Current.CanInteract(_ctx) &&
                _interactAction != null && _interactAction.WasPressedThisFrame())
            {
                Current.Interact(_ctx, _processor);
            }
        }

        private void Rescan()
        {
            int count = Physics2D.OverlapCircleNonAlloc(transform.position, radius, _overlapBuffer);

            IInteractable best = null;
            float bestDistSqr = float.MaxValue;

            Camera cam = Camera.main;
            bool hasCursorPoint = cam != null && Mouse.current != null;
            Vector2 cursorWorldPoint = default;
            if (hasCursorPoint)
                cursorWorldPoint = cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

            for (int i = 0; i < count; i++)
            {
                var candidate = _overlapBuffer[i].GetComponentInParent<IInteractable>();
                if (candidate == null) continue;

                if (hasCursorPoint && _overlapBuffer[i].OverlapPoint(cursorWorldPoint))
                {
                    best = candidate;
                    break; // cursor tiebreak wins outright over distance
                }

                float distSqr = ((Vector2)_overlapBuffer[i].transform.position - (Vector2)transform.position).sqrMagnitude;
                if (distSqr < bestDistSqr)
                {
                    bestDistSqr = distSqr;
                    best = candidate;
                }
            }

            if (!ReferenceEquals(best, Current))
            {
                Current = best;
                TargetChanged?.Invoke(Current);
            }
        }
    }
}
