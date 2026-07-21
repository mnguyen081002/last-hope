using UnityEngine;

namespace LastHope.Gameplay
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public sealed class DirectionalSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] idleDown;
        [SerializeField] private Sprite[] idleUp;
        [SerializeField] private Sprite[] idleLeft;
        [SerializeField] private Sprite[] idleRight;
        [SerializeField] private Sprite[] walkDown;
        [SerializeField] private Sprite[] walkUp;
        [SerializeField] private Sprite[] walkLeft;
        [SerializeField] private Sprite[] walkRight;
        [SerializeField, Min(1f)] private float idleFramesPerSecond = 5f;
        [SerializeField, Min(1f)] private float walkFramesPerSecond = 10f;

        private SpriteRenderer spriteRenderer;
        private Rigidbody2D body;
        private Vector2 facing = Vector2.down;
        private float animationTime;

        public void Configure(
            Sprite[] downIdle, Sprite[] upIdle, Sprite[] leftIdle, Sprite[] rightIdle,
            Sprite[] downWalk, Sprite[] upWalk, Sprite[] leftWalk, Sprite[] rightWalk)
        {
            idleDown = downIdle;
            idleUp = upIdle;
            idleLeft = leftIdle;
            idleRight = rightIdle;
            walkDown = downWalk;
            walkUp = upWalk;
            walkLeft = leftWalk;
            walkRight = rightWalk;
        }

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            Vector2 velocity = body.linearVelocity;
            bool walking = velocity.sqrMagnitude > 0.01f;
            if (walking)
            {
                facing = velocity.normalized;
            }

            Sprite[] frames = SelectFrames(walking);
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            animationTime += Time.deltaTime * (walking ? walkFramesPerSecond : idleFramesPerSecond);
            spriteRenderer.sprite = frames[Mathf.FloorToInt(animationTime) % frames.Length];
        }

        private Sprite[] SelectFrames(bool walking)
        {
            if (Mathf.Abs(facing.x) > Mathf.Abs(facing.y))
            {
                return facing.x < 0f ? (walking ? walkLeft : idleLeft) : (walking ? walkRight : idleRight);
            }

            return facing.y > 0f ? (walking ? walkUp : idleUp) : (walking ? walkDown : idleDown);
        }
    }
}
