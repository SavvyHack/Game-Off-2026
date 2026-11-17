using System;
using UnityEngine;
using UnityEngine.Events;

namespace GameOff
{
    /// <summary>
    /// A usable place in the pod (pilot seat, dispatch console, analysis bench, bunk).
    /// Features hook in through <see cref="Interacted"/> or <see cref="onInteract"/>; until they do,
    /// the station just shows <see cref="PlaceholderMessage"/>.
    /// </summary>
    public class Station : MonoBehaviour, IInteractable
    {
        private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

        [SerializeField] private StationType type;
        [SerializeField] private string displayName = "Station";
        [SerializeField] private string verb = "Use";
        [TextArea]
        [SerializeField] private string placeholderMessage;
        [Tooltip("Where a player can stand to use this station.")]
        [SerializeField] private Transform standPoint;
        [Tooltip("What the player looks at when using this station.")]
        [SerializeField] private Transform focusPoint;
        [Tooltip("Emissive renderers that brighten while the player looks at the station.")]
        [SerializeField] private Renderer[] highlightRenderers = Array.Empty<Renderer>();
        [SerializeField] private float highlightBoost = 2.5f;
        [SerializeField] private UnityEvent onInteract = new UnityEvent();

        private MaterialPropertyBlock propertyBlock;

        /// <summary>Raised for every station interaction. Filter on <see cref="Type"/>.</summary>
        public static event Action<Station> Interacted;

        public StationType Type => type;
        public string DisplayName => displayName;
        public string Prompt => verb + " " + displayName;
        public Transform StandPoint => standPoint;
        public Transform FocusPoint => focusPoint;
        public UnityEvent OnInteract => onInteract;

        /// <summary>Shown to the player while the station has no gameplay yet. Clear it once implemented.</summary>
        public string PlaceholderMessage
        {
            get => placeholderMessage;
            set => placeholderMessage = value;
        }

        public void Configure(StationType stationType, string name, string promptVerb, string placeholder,
            Transform stand, Transform focus, Renderer[] highlights)
        {
            type = stationType;
            displayName = name;
            verb = promptVerb;
            placeholderMessage = placeholder;
            standPoint = stand;
            focusPoint = focus;
            highlightRenderers = highlights ?? Array.Empty<Renderer>();
        }

        public void SetFocused(bool focused)
        {
            propertyBlock ??= new MaterialPropertyBlock();

            foreach (Renderer target in highlightRenderers)
            {
                if (target == null)
                {
                    continue;
                }

                if (!focused)
                {
                    target.SetPropertyBlock(null);
                    continue;
                }

                Material material = target.sharedMaterial;
                if (material == null || !material.HasProperty(EmissionColorId))
                {
                    continue;
                }

                target.GetPropertyBlock(propertyBlock);
                propertyBlock.SetColor(EmissionColorId, material.GetColor(EmissionColorId) * highlightBoost);
                target.SetPropertyBlock(propertyBlock);
            }
        }

        public void Interact(GameObject interactor)
        {
            onInteract.Invoke();
            Interacted?.Invoke(this);
        }
    }
}
