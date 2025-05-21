using UnityEngine;
using UnityEngine.UI; // Required for UI elements like InputField, Text, Button
using System.Threading.Tasks; // Required for asynchronous operations (Task)

namespace AI.Shopkeeper.UI
{
    /// <summary>
    /// Manages the user interface for interacting with the Shopkeeper AI.
    /// This includes handling player input, displaying AI responses, and managing UI visibility.
    /// This script should be attached to a Canvas GameObject or a UI manager GameObject in the scene.
    /// </summary>
    public class ShopkeeperUI : MonoBehaviour
    {
        [Header("Core Dependencies")]
        [Tooltip("Reference to the ShopkeeperAI instance that this UI will interact with. Must be assigned in the Inspector.")]
        public ShopkeeperAI shopkeeperAI;

        [Header("UI Element References")]
        [Tooltip("The main panel GameObject that contains all chat UI elements (input field, response text, button). Assign from the scene.")]
        public GameObject mainChatPanel;

        [Tooltip("InputField UI element for the player to type their messages. Assign from the scene.")]
        public InputField playerInputField;

        [Tooltip("Text UI element used to display the shopkeeper's AI-generated responses. Assign from the scene.")]
        public Text shopkeeperResponseText;

        [Tooltip("Button UI element that the player clicks to submit their input. Assign from the scene.")]
        public Button submitButton;

        [Tooltip("Optional: Text UI element for displaying status messages (e.g., 'Thinking...', 'Error'). Assign from the scene if used.")]
        public Text statusText; 

        private bool isFullyInitialized = false; // Tracks if all essential components are assigned and initialized.

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Validates essential component references and sets up initial UI state.
        /// </summary>
        void Start()
        {
            // --- Critical Component Null Checks ---
            // If any of these are missing, the UI cannot function correctly.
            if (mainChatPanel == null)
            {
                Debug.LogError("[ShopkeeperUI] CRITICAL ERROR: Main Chat Panel (mainChatPanel) is not assigned in the Inspector! This script will be disabled.");
                SetStatus("ERROR: UI Panel not linked.", true); // Show error even if statusText might be null.
                enabled = false; // Disable this script if the main panel is missing.
                return;
            }
            if (shopkeeperAI == null)
            {
                Debug.LogError("[ShopkeeperUI] CRITICAL ERROR: ShopkeeperAI reference is not set in the Inspector! This script will be disabled.");
                SetStatus("ERROR: Shopkeeper AI not linked.", true);
                enabled = false; 
                return;
            }
            if (playerInputField == null)
            {
                Debug.LogError("[ShopkeeperUI] CRITICAL ERROR: Player InputField (playerInputField) is not assigned in the Inspector! This script will be disabled.");
                SetStatus("ERROR: Player input field not linked.", true);
                enabled = false;
                return;
            }
            if (shopkeeperResponseText == null)
            {
                Debug.LogError("[ShopkeeperUI] CRITICAL ERROR: Shopkeeper Response Text (shopkeeperResponseText) is not assigned in the Inspector! This script will be disabled.");
                SetStatus("ERROR: Response text field not linked.", true);
                enabled = false;
                return;
            }
            if (submitButton == null)
            {
                Debug.LogError("[ShopkeeperUI] CRITICAL ERROR: Submit Button (submitButton) is not assigned in the Inspector! This script will be disabled.");
                SetStatus("ERROR: Submit button not linked.", true);
                enabled = false;
                return;
            }

            // --- Initialization ---
            // Add a listener to the submit button's onClick event to call OnSubmitInput when clicked.
            submitButton.onClick.AddListener(OnSubmitInput); 
            
            // Set the initial state: UI is hidden until activated by ShopInteractionManager.
            HideChatUI(); 
            
            isFullyInitialized = true; // All critical components are assigned.
            SetStatus("UI Initialized. Waiting for interaction trigger.", false); // Initial status message when UI is hidden.
            Debug.Log("[ShopkeeperUI] Initialized successfully. Chat UI is currently hidden.");
        }

        /// <summary>
        /// Sets a message in the statusText UI element and logs it.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="isError">If true, the message is logged as an error and displayed in red (if statusText is available).</param>
        private void SetStatus(string message, bool isError)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = isError ? Color.red : Color.black; // Use red for errors, black for normal status.
            }

            if (isError)
            {
                Debug.LogError($"[ShopkeeperUI] Status Update (Error): {message}");
            }
            else
            {
                Debug.Log($"[ShopkeeperUI] Status Update: {message}");
            }
        }

        /// <summary>
        /// Sets the interactable state of the player input field and submit button.
        /// </summary>
        /// <param name="interactable">True to make UI elements interactable, false to disable them.</param>
        private void SetUIInteractable(bool interactable)
        {
            if (playerInputField != null)
            {
                playerInputField.interactable = interactable;
            }
            if (submitButton != null)
            {
                submitButton.interactable = interactable;
            }
        }

        /// <summary>
        /// Called when the submit button is pressed.
        /// Retrieves player input, sends it to the ShopkeeperAI, and displays the response.
        /// This method is asynchronous to allow waiting for the AI's response without freezing the game.
        /// </summary>
        public async void OnSubmitInput()
        {
            // Do not process input if UI is not initialized or not currently visible.
            if (!isFullyInitialized || (mainChatPanel != null && !mainChatPanel.activeSelf))
            {
                Debug.LogWarning("[ShopkeeperUI] OnSubmitInput called, but UI is not initialized or not visible. Input ignored.");
                return;
            }

            string playerInputText = playerInputField.text;

            // Check if the input text is empty or just whitespace.
            if (string.IsNullOrWhiteSpace(playerInputText))
            {
                SetStatus("Please type something to say to the shopkeeper.", false);
                return; // Do not proceed if input is empty.
            }

            // Disable UI elements while waiting for AI response to prevent multiple submissions.
            SetUIInteractable(false);
            SetStatus("Shopkeeper is thinking...", false); // Inform the player that the AI is processing.
            
            string aiResponse = "Sorry, I didn't quite catch that. Could you repeat?"; // Default response in case of an unexpected error.
            try
            {
                // Asynchronously get the response from the ShopkeeperAI.
                aiResponse = await shopkeeperAI.GetShopkeeperResponse(playerInputText);
            }
            catch (System.Exception ex) // Catch any exceptions during the AI call.
            {
                Debug.LogError($"[ShopkeeperUI] An exception occurred while getting shopkeeper response: {ex.Message}\nStack Trace: {ex.StackTrace}");
                SetStatus($"Error communicating with AI: {ex.Message}", true);
                aiResponse = "I seem to be having a bit of trouble understanding things right now. Please try again in a moment.";
            }

            // Display the AI's response (or error message).
            if (shopkeeperResponseText != null)
            {
                shopkeeperResponseText.text = aiResponse;
            }
            
            // Clear the input field for the next message and refocus it.
            if (playerInputField != null)
            {
                playerInputField.text = ""; 
                playerInputField.ActivateInputField(); // Helps player to type next message immediately.
            }

            // Re-enable UI elements.
            SetUIInteractable(true);

            // Update status based on the response (e.g., if AI indicated an error).
            if (aiResponse != null && (aiResponse.ToLower().Contains("error") || aiResponse.ToLower().Contains("not configured")))
            {
                SetStatus("There was an issue with the AI response. See message.", true);
            }
            else
            {
                SetStatus("Ready for your next message.", false);
            }
        }

        /// <summary>
        /// Activates and prepares the chat UI for interaction.
        /// Called by ShopInteractionManager when the player enters interaction range and presses the interaction key.
        /// </summary>
        public void ShowChatUI()
        {
            if (mainChatPanel == null || !isFullyInitialized) 
            {
                Debug.LogError("[ShopkeeperUI] Cannot show chat UI: mainChatPanel is null or UI not fully initialized.");
                return;
            }

            mainChatPanel.SetActive(true); // Make the main chat panel visible.
            SetUIInteractable(true);       // Enable input field and button.

            if (playerInputField != null)
            {
                playerInputField.text = ""; // Clear any previous input.
                playerInputField.ActivateInputField(); // Automatically focus on the input field.
            }
            if (shopkeeperResponseText != null)
            {
                // Set an initial greeting or clear previous conversation.
                shopkeeperResponseText.text = "Shopkeeper: Greetings, traveler! What can I get for you today?"; 
            }
            SetStatus("Chat active. Type your message and press Enter or click Submit.", false);
            Debug.Log("[ShopkeeperUI] Chat UI has been shown.");
        }

        /// <summary>
        /// Hides the chat UI.
        /// Called by ShopInteractionManager when the player moves out of range or presses the close UI key.
        /// </summary>
        public void HideChatUI()
        {
            if (mainChatPanel == null) 
            {
                Debug.LogWarning("[ShopkeeperUI] Attempted to hide chat UI, but mainChatPanel is null.");
                return;
            }

            mainChatPanel.SetActive(false); // Make the main chat panel invisible.
            SetStatus("Chat hidden. Approach the shopkeeper to talk.", false); 
            Debug.Log("[ShopkeeperUI] Chat UI has been hidden.");
        }

        /// <summary>
        /// Checks if the main chat UI panel is currently active and visible.
        /// </summary>
        /// <returns>True if the main chat panel is active, false otherwise.</returns>
        public bool IsChatUIVisible()
        {
            if (mainChatPanel == null) return false;
            return mainChatPanel.activeSelf;
        }

        /// <summary>
        /// Optional: Called when the player selects the input field.
        /// Can be used to clear non-error status messages.
        /// Hook this up to the InputField's OnSelect event in the Inspector if desired.
        /// </summary>
        public void OnInputFieldSelect()
        {
            // Only clear non-error status messages when the input field is selected and the UI is active.
            if (isFullyInitialized && IsChatUIVisible() && statusText != null && 
                !string.IsNullOrEmpty(statusText.text) && statusText.color != Color.red) 
            {
                // SetStatus("", false); // Example: Clear status. This can be too aggressive; use cautiously.
            }
        }

        /// <summary>
        /// Called when the GameObject this script is attached to is destroyed.
        /// Ensures that event listeners are cleaned up to prevent memory leaks or errors.
        /// </summary>
        void OnDestroy()
        {
            if (submitButton != null)
            {
                // Remove the listener for OnSubmitInput to prevent errors if the button is destroyed before this script.
                submitButton.onClick.RemoveListener(OnSubmitInput);
            }
        }
    }
}
