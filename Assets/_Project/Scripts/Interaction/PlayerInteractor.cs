using UnityEngine;
using UnityEngine.InputSystem;

namespace GameOff
{
    /// <summary>
    /// Casts a ray from the centre of the view and lets the player use the <see cref="IInteractable"/> it hits.
    /// The interactable may sit on the hit collider or on any of its parents.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        public const float DefaultRange = 2.5f;

        [SerializeField] private Transform viewOrigin;
        [SerializeField] private float range = DefaultRange;
        [SerializeField] private LayerMask mask = Physics.DefaultRaycastLayers;
        [SerializeField] private InteractionPromptUI promptUI;

        private InputAction interactAction;
        private IInteractable current;

        public IInteractable Current => current;
        public float Range => range;

        public void Configure(Transform origin, InteractionPromptUI prompt)
        {
            viewOrigin = origin;
            promptUI = prompt;
        }

        private void Awake()
        {
            interactAction = new InputAction("Interact", InputActionType.Button);
            interactAction.AddBinding("<Keyboard>/e");
            interactAction.AddBinding("<Gamepad>/buttonWest");
        }

        private void OnEnable() => interactAction.Enable();

        private void OnDisable()
        {
            interactAction.Disable();
            SetCurrent(null);
        }

        private void OnDestroy() => interactAction.Dispose();

        private void Update()
        {
            SetCurrent(FindTarget());

            if (current != null && interactAction.WasPressedThisFrame())
            {
                current.Interact(gameObject);
            }
        }

        private IInteractable FindTarget()
        {
            if (viewOrigin == null)
            {
                return null;
            }

            var ray = new Ray(viewOrigin.position, viewOrigin.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, range, mask, QueryTriggerInteraction.Ignore))
            {
                return null;
            }

            return hit.collider.GetComponentInParent<IInteractable>();
        }

        private void SetCurrent(IInteractable target)
        {
            if (ReferenceEquals(target, current))
            {
                return;
            }

            // The previous target may have been destroyed; Unity's null check covers that.
            if (current is Object previous && previous != null)
            {
                current.SetFocused(false);
            }

            current = target;
            current?.SetFocused(true);

            if (promptUI != null)
            {
                promptUI.SetPrompt(current != null ? "[E] " + current.Prompt : null);
            }
        }
    }
}
