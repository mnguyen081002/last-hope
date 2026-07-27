using UnityEngine;
using UnityEngine.InputSystem;

namespace LastHope.Presentation.Player
{
    /// <summary>
    /// Di chuyển nhân vật trên mặt phẳng 2D. Input Move map thẳng world X/Y — camera không
    /// xoay nên hướng world và hướng màn hình luôn trùng nhau.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] InputActionAsset controls;
        [Tooltip("Tốc độ đi bộ (m/s) khi không có modifier.")]
        [SerializeField] float baseSpeed = 3.5f;

        /// <summary>Khoảng đệm giữ player không dính sát vào collider sau khi chặn.</summary>
        const float SkinWidth = 0.02f;

        static readonly RaycastHit2D[] CastHitsBuffer = new RaycastHit2D[8];

        Rigidbody2D body;
        InputAction moveAction;
        Vector2 moveInput;
        ContactFilter2D obstacleFilter;

        /// <summary>
        /// Hệ số tốc độ do gameplay áp (Overload, Flood, Condition). 1 = bình thường.
        /// Các hệ thống P1+ ghi vào đây thay vì đổi <c>baseSpeed</c>.
        /// </summary>
        public float SpeedModifier { get; set; } = 1f;

        /// <summary>Hướng di chuyển hiện tại đã chuẩn hoá, dùng cho animation 8 hướng.</summary>
        public Vector2 Facing { get; private set; } = Vector2.down;

        public bool IsMoving => moveInput.sqrMagnitude > 0.0001f;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            // Kinematic body không tự chặn va chạm — Unity chỉ báo sự kiện, không cản
            // MovePosition. Phải tự cast rồi giới hạn quãng đường trước khi di chuyển.
            obstacleFilter = new ContactFilter2D();
            obstacleFilter.useTriggers = false;
            obstacleFilter.useLayerMask = false;

            if (controls != null)
            {
                moveAction = controls.FindActionMap("Gameplay", true).FindAction("Move", true);
            }
        }

        void OnEnable() => moveAction?.Enable();

        void OnDisable() => moveAction?.Disable();

        void Update()
        {
            moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

            // Chuẩn hoá để đi chéo không nhanh hơn đi thẳng.
            if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();

            if (IsMoving) Facing = moveInput.normalized;
        }

        void FixedUpdate()
        {
            Vector2 delta = moveInput * (baseSpeed * SpeedModifier * Time.fixedDeltaTime);
            if (delta.sqrMagnitude <= 0f) return;

            // Tách trục để player đi men theo tường (trượt) thay vì dính cứng khi đâm chéo góc.
            Vector2 position = body.position;
            position = MoveAxis(position, new Vector2(delta.x, 0f));
            position = MoveAxis(position, new Vector2(0f, delta.y));
            body.MovePosition(position);
        }

        /// <summary>Cast theo hướng delta, cắt quãng đường tại vật cản gần nhất nếu có.</summary>
        Vector2 MoveAxis(Vector2 from, Vector2 delta)
        {
            float distance = delta.magnitude;
            if (distance <= 0f) return from;

            Vector2 direction = delta / distance;
            int hitCount = body.Cast(direction, obstacleFilter, CastHitsBuffer, distance);
            if (hitCount == 0) return from + delta;

            float allowed = distance;
            for (int i = 0; i < hitCount; i++)
            {
                allowed = Mathf.Min(allowed, CastHitsBuffer[i].distance);
            }
            allowed = Mathf.Max(0f, allowed - SkinWidth);

            return from + direction * allowed;
        }
    }
}
