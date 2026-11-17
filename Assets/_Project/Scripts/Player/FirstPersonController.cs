using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff
{
    /// <summary>
    /// Walk-and-look controller for the pod interior. No jump or sprint: the pod is a few steps long.
    /// Input actions are created in code so the scene has no dependency on an input asset.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour
    {
        [SerializeField] private Transform cameraPivot;
        [SerializeField] private float moveSpeed = 2.6f;
        [SerializeField] private float mouseSensitivity = 0.09f;   // degrees per pixel of mouse travel
        [SerializeField] private float stickLookSpeed = 140f;      // degrees per second at full stick
        [SerializeField] private float maxPitch = 80f;
        [SerializeField] private float gravity = -18f;

        private CharacterController controller;
        private InputAction moveAction;
        private InputAction mouseLookAction;
        private InputAction stickLookAction;
        private InputAction releaseCursorAction;
        private InputAction captureCursorAction;
        private float pitch;
        private float verticalVelocity;

        /// <summary>Set false while a station screen is open so the player stays put.</summary>
        public bool ControlEnabled { get; set; } = true;

        public void Configure(Transform pivot) => cameraPivot = pivot;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();

            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.AddBinding("<Gamepad>/leftStick");

            mouseLookAction = new InputAction("MouseLook", InputActionType.Value, "<Mouse>/delta");
            stickLookAction = new InputAction("StickLook", InputActionType.Value, "<Gamepad>/rightStick");
            releaseCursorAction = new InputAction("ReleaseCursor", InputActionType.Button, "<Keyboard>/escape");
            captureCursorAction = new InputAction("CaptureCursor", InputActionType.Button, "<Mouse>/leftButton");
        }

        private void OnEnable()
        {
            moveAction.Enable();
            mouseLookAction.Enable();
            stickLookAction.Enable();
            releaseCursorAction.Enable();
            captureCursorAction.Enable();
            SetCursorCaptured(true);
        }

        private void OnDisable()
        {
            moveAction.Disable();
            mouseLookAction.Disable();
            stickLookAction.Disable();
            releaseCursorAction.Disable();
            captureCursorAction.Disable();
            SetCursorCaptured(false);
        }

        private void OnDestroy()
        {
            moveAction.Dispose();
            mouseLookAction.Dispose();
            stickLookAction.Dispose();
            releaseCursorAction.Dispose();
            captureCursorAction.Dispose();
        }

        private void Update()
        {
            if (releaseCursorAction.WasPressedThisFrame())
            {
                SetCursorCaptured(false);
            }
            else if (captureCursorAction.WasPressedThisFrame() && ControlEnabled)
            {
                SetCursorCaptured(true);
            }

            bool cursorCaptured = Cursor.lockState == CursorLockMode.Locked;

            if (ControlEnabled)
            {
                Look(cursorCaptured);
            }

            Move(ControlEnabled ? moveAction.ReadValue<Vector2>() : Vector2.zero);
        }

        private void Look(bool cursorCaptured)
        {
            Vector2 delta = stickLookAction.ReadValue<Vector2>() * (stickLookSpeed * Time.deltaTime);

            // Ignore the mouse while the cursor is free, otherwise moving it to another window spins the view.
            if (cursorCaptured)
            {
                delta += mouseLookAction.ReadValue<Vector2>() * mouseSensitivity;
            }

            transform.Rotate(0f, delta.x, 0f, Space.Self);

            pitch = Mathf.Clamp(pitch - delta.y, -maxPitch, maxPitch);
            if (cameraPivot != null)
            {
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
            }
        }

        private void Move(Vector2 input)
        {
            input = Vector2.ClampMagnitude(input, 1f);
            Vector3 velocity = (transform.right * input.x + transform.forward * input.y) * moveSpeed;

            if (controller.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // small downward push keeps isGrounded stable
            }

            verticalVelocity += gravity * Time.deltaTime;
            velocity.y = verticalVelocity;

            controller.Move(velocity * Time.deltaTime);
        }

        private static void SetCursorCaptured(bool captured)
        {
            Cursor.lockState = captured ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !captured;
        }
    }
}
