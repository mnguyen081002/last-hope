using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.Interaction
{
    /// <summary>
    /// Tìm <see cref="IInteractable"/> gần nhất quanh player, xử lý giữ phím (nếu
    /// <see cref="IInteractable.HoldDurationSeconds"/> &gt; 0) hoặc kích hoạt tức thì. Thả
    /// phím sớm, đổi target, hoặc target ra khỏi tầm đều hủy tiến trình giữ, không roll loot.
    /// </summary>
    public class InteractionDetector : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;
        [SerializeField] float detectionRadius = 1.6f;

        static readonly Collider2D[] OverlapBuffer = new Collider2D[16];

        InputAction interactAction;
        ContactFilter2D filter;

        bool isHolding;
        float holdElapsed;

        public IInteractable CurrentTarget { get; private set; }
        public bool IsHolding => isHolding;

        public float HoldProgress01 =>
            CurrentTarget != null && CurrentTarget.HoldDurationSeconds > 0f
                ? Mathf.Clamp01(holdElapsed / CurrentTarget.HoldDurationSeconds)
                : 0f;

        void Awake()
        {
            filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.useLayerMask = false;

            if (controls != null)
            {
                interactAction = controls.FindActionMap("Gameplay", true).FindAction("Interact", true);
            }
        }

        void OnEnable() => interactAction?.Enable();

        void OnDisable() => interactAction?.Disable();

        void Update()
        {
            var nearest = FindNearestInteractable();
            if (!ReferenceEquals(nearest, CurrentTarget))
            {
                CancelHold();
                CurrentTarget = nearest;
            }

            if (CurrentTarget == null || interactAction == null) return;

            if (!interactAction.IsPressed())
            {
                CancelHold();
                return;
            }

            if (CurrentTarget.HoldDurationSeconds <= 0f)
            {
                // Instant: chỉ kích hoạt đúng frame nhấn xuống, giữ phím không lặp lại liên tục.
                if (interactAction.WasPressedThisFrame()) CurrentTarget.Interact();
                return;
            }

            isHolding = true;
            holdElapsed += Time.deltaTime;

            if (holdElapsed >= CurrentTarget.HoldDurationSeconds)
            {
                CurrentTarget.Interact();
                CancelHold();
            }
        }

        void CancelHold()
        {
            isHolding = false;
            holdElapsed = 0f;
        }

        IInteractable FindNearestInteractable()
        {
            int count = Physics2D.OverlapCircle(transform.position, detectionRadius, filter, OverlapBuffer);

            IInteractable nearest = null;
            float nearestSqrDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                var interactable = OverlapBuffer[i].GetComponent<IInteractable>();
                if (interactable == null) continue;

                float sqrDistance =
                    ((Vector2)OverlapBuffer[i].transform.position - (Vector2)transform.position).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = interactable;
                }
            }

            return nearest;
        }
    }
}
