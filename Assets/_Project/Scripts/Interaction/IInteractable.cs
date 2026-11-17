using UnityEngine;

namespace GameOff
{
    /// <summary>Anything the player can look at and use with the interact key.</summary>
    public interface IInteractable
    {
        /// <summary>Text shown after the key hint, e.g. "Use Drone Dispatch Console".</summary>
        string Prompt { get; }

        /// <summary>Called when the player starts or stops looking at this object.</summary>
        void SetFocused(bool focused);

        void Interact(GameObject interactor);
    }
}
