using UnityEngine;
using UnityEngine.InputSystem;

namespace Aula0.Runtime
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerController2D : MonoBehaviour
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string moveActionName = "Player/Move";
        [SerializeField] private float moveSpeed = 6.5f;
        [SerializeField] private float runAnimationFps = 10f;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private float edgePadding = 0.45f;
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] runFrames = new Sprite[5];

        private Rigidbody2D _body;
        private SpriteRenderer _spriteRenderer;
        private InputAction _moveAction;
        private Vector2 _moveInput;
        private float _animationTime;

        private void Awake()
        {
            _body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _moveAction = inputActions != null ? inputActions.FindAction(moveActionName, true) : null;
            if (worldCamera == null)
            {
                worldCamera = Camera.main;
            }
        }

        private void OnEnable()
        {
            _moveAction?.Enable();
        }

        private void OnDisable()
        {
            _moveAction?.Disable();
            _body.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            _moveInput = _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;
            if (_moveInput.sqrMagnitude > 1f)
            {
                _moveInput.Normalize();
            }

            UpdateSprite();
        }

        private void FixedUpdate()
        {
            var nextPosition = _body.position + (_moveInput * moveSpeed * Time.fixedDeltaTime);
            _body.MovePosition(ClampToCamera(nextPosition));
        }

        private Vector2 ClampToCamera(Vector2 position)
        {
            if (worldCamera == null || !worldCamera.orthographic)
            {
                return position;
            }

            var halfHeight = worldCamera.orthographicSize - edgePadding;
            var halfWidth = halfHeight * worldCamera.aspect;
            var cameraPosition = worldCamera.transform.position;

            position.x = Mathf.Clamp(position.x, cameraPosition.x - halfWidth, cameraPosition.x + halfWidth);
            position.y = Mathf.Clamp(position.y, cameraPosition.y - halfHeight, cameraPosition.y + halfHeight);
            return position;
        }

        private void UpdateSprite()
        {
            if (_moveInput.sqrMagnitude < 0.01f)
            {
                _animationTime = 0f;
                if (idleSprite != null)
                {
                    _spriteRenderer.sprite = idleSprite;
                }

                return;
            }

            if (_moveInput.x < -0.01f)
            {
                _spriteRenderer.flipX = true;
            }
            else if (_moveInput.x > 0.01f)
            {
                _spriteRenderer.flipX = false;
            }

            if (runFrames == null || runFrames.Length == 0)
            {
                return;
            }

            _animationTime += Time.deltaTime * runAnimationFps;
            var frameIndex = Mathf.FloorToInt(_animationTime) % runFrames.Length;
            var frame = runFrames[frameIndex];
            if (frame != null)
            {
                _spriteRenderer.sprite = frame;
            }
        }
    }
}
