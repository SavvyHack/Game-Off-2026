using UnityEngine;
using UnityEngine.UI;

namespace GameOff
{
    /// <summary>
    /// Minimal HUD: the interaction prompt under the crosshair and a timed message line.
    /// Shows a station's placeholder message when it is used, so unimplemented stations still respond.
    /// </summary>
    public class InteractionPromptUI : MonoBehaviour
    {
        [SerializeField] private Text promptText;
        [SerializeField] private Text messageText;
        [SerializeField] private float messageSeconds = 3.5f;

        private float messageHideTime;

        private void OnEnable()
        {
            Station.Interacted += HandleStationInteracted;
            SetPrompt(null);
            if (messageText != null)
            {
                messageText.enabled = false;
            }
        }

        private void OnDisable() => Station.Interacted -= HandleStationInteracted;

        private void Update()
        {
            if (messageText != null && messageText.enabled && Time.unscaledTime >= messageHideTime)
            {
                messageText.enabled = false;
            }
        }

        /// <summary>Pass null or empty to hide the prompt.</summary>
        public void SetPrompt(string text)
        {
            if (promptText == null)
            {
                return;
            }

            bool show = !string.IsNullOrEmpty(text);
            promptText.enabled = show;
            promptText.text = show ? text : string.Empty;
        }

        public void ShowMessage(string text)
        {
            if (messageText == null || string.IsNullOrEmpty(text))
            {
                return;
            }

            messageText.text = text;
            messageText.enabled = true;
            messageHideTime = Time.unscaledTime + messageSeconds;
        }

        public void Configure(Text prompt, Text message)
        {
            promptText = prompt;
            messageText = message;
        }

        private void HandleStationInteracted(Station station) => ShowMessage(station.PlaceholderMessage);
    }
}
