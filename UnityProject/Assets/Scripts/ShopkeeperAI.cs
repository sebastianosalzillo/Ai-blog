using UnityEngine;
using System.Threading.Tasks; // Required for Task (asynchronous operations)

namespace AI.Shopkeeper
{
    /// <summary>
    /// Manages the AI logic for the shopkeeper, including its personality and interaction with the GoogleAIService.
    /// This component should be attached to the Shopkeeper GameObject.
    /// </summary>
    public class ShopkeeperAI : MonoBehaviour
    {
        [Header("Service Configuration")]
        [Tooltip("Reference to the GoogleAIService instance. This must be assigned in the Inspector.")]
        public GoogleAIService aiService;

        [Tooltip("Google AI API Key. This key will be passed to the GoogleAIService for API calls. " +
                 "Ensure this is set in the Inspector. For production, consider more secure ways to store API keys.")]
        public string apiKey;

        [Tooltip("The AI model to be used (e.g., 'gemini-1.5-flash', 'gemini-pro'). " +
                 "This will be passed to the GoogleAIService.")]
        public string modelName = "gemini-1.5-flash"; // Default model

        [Header("Shopkeeper Persona")]
        [Tooltip("The base prompt that defines the shopkeeper's personality, context, items they sell, and instructions for the AI. " +
                 "This is prepended to every player query.")]
        [TextArea(5, 15)] // Makes the string editable in a larger text area in the Inspector
        public string basePrompt = "You are Elara, a friendly and slightly eccentric shopkeeper in a magical medieval fantasy town named Silverbrook. " +
                                   "You sell common adventuring supplies like potions, ropes, torches, and basic weapons. " +
                                   "You are helpful, a little bit witty, and try to keep your responses relatively concise for a game. " +
                                   "When asked about items, mention if you have them and their imaginary price in gold pieces (e.g., Health Potion - 50 gold).";

        private bool isProperlyConfigured = false; // Flag to track if essential configurations are set.

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Validates the essential configurations for the ShopkeeperAI.
        /// </summary>
        void Start()
        {
            CheckConfiguration();
        }

        /// <summary>
        /// Checks if all necessary components and settings are assigned in the Inspector.
        /// Sets the 'isProperlyConfigured' flag based on the checks.
        /// </summary>
        private void CheckConfiguration()
        {
            if (aiService == null)
            {
                Debug.LogError("[ShopkeeperAI] Critical Error: GoogleAIService is not assigned in the Inspector. This component is essential for AI responses. Please assign it.");
                isProperlyConfigured = false;
                return; // Stop further checks if AI service is missing.
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogError("[ShopkeeperAI] Critical Error: Google AI API Key is not set in the Inspector. This is required to use the AI service. Please provide the API Key.");
                isProperlyConfigured = false;
                return; // Stop further checks if API key is missing.
            }
            
            // Optional checks with warnings for non-critical but recommended settings.
            if (string.IsNullOrEmpty(modelName))
            {
                Debug.LogWarning("[ShopkeeperAI] Warning: AI Model Name is not set. The GoogleAIService will use its default model. It's recommended to specify a model here for clarity.");
                // Not setting isProperlyConfigured to false, as GoogleAIService has a default.
            }

            if (string.IsNullOrEmpty(basePrompt))
            {
                Debug.LogWarning("[ShopkeeperAI] Warning: Base Prompt is empty. The shopkeeper AI might not have a defined personality or context, leading to generic or unexpected responses.");
            }
            
            isProperlyConfigured = true; // All critical checks passed.
            Debug.Log("[ShopkeeperAI] Configuration check completed. Ready for interaction (if critical checks passed).");
        }

        /// <summary>
        /// Asynchronously gets a response from the shopkeeper AI based on the player's input.
        /// This method constructs a full prompt using the basePrompt and playerInput, then queries the GoogleAIService.
        /// </summary>
        /// <param name="playerInput">The text input from the player (e.g., "Do you have any health potions?").</param>
        /// <returns>A Task representing the asynchronous operation, with a string result containing the shopkeeper's response. 
        /// Returns an error message or a generic placeholder if issues occur or the AI doesn't respond.</returns>
        public async Task<string> GetShopkeeperResponse(string playerInput)
        {
            // Ensure the component is configured before attempting to get a response.
            if (!isProperlyConfigured)
            {
                // Attempt to re-check configuration if Start might not have run or settings were changed dynamically (less common).
                CheckConfiguration(); 
                if (!isProperlyConfigured) // If still not configured after re-check
                {
                    Debug.LogError("[ShopkeeperAI] Attempted to get AI response, but the component is not configured properly (missing AI Service or API Key). Please check Inspector settings and console errors.");
                    return "Shopkeeper AI is not configured. Please check the console for error details.";
                }
            }
            
            // Handle empty player input gracefully.
            if (string.IsNullOrEmpty(playerInput))
            {
                Debug.LogWarning("[ShopkeeperAI] Player input is empty. Sending base prompt to elicit a generic or introductory response from the AI.");
                // Providing a neutral or very generic input if player says nothing, or just using base prompt.
                // For this setup, we'll let the prompt structure handle it.
            }

            // Construct the full prompt for the AI.
            // This structure helps the AI understand the context of a dialogue.
            // Example:
            // [Base Prompt defining shopkeeper persona]
            // Player: What do you sell?
            // Shopkeeper: [AI generates response here]
            string fullPrompt = $"{basePrompt}\n\nPlayer: {playerInput}\nShopkeeper:";
            
            Debug.Log($"[ShopkeeperAI] Sending prompt to AI Service. Model: {modelName}. Full prompt (first 100 chars): \"{fullPrompt.Substring(0, Mathf.Min(fullPrompt.Length, 100))}...\"");

            // Call the GoogleAIService to get the AI-generated text.
            // Pass the API key and model name from this component, allowing them to override GoogleAIService's defaults if set there.
            string aiResponseText = await aiService.GenerateTextAsync(fullPrompt, modelName, apiKey);

            // Handle cases where the AI service returns no valid response.
            if (string.IsNullOrEmpty(aiResponseText))
            {
                Debug.LogWarning("[ShopkeeperAI] AI service returned a null or empty response. The shopkeeper might be at a loss for words.");
                return "The shopkeeper seems lost in thought and doesn't respond. Perhaps try asking something else?";
            }

            // Return the successfully generated AI response.
            return aiResponseText;
        }

        // Example of how this ShopkeeperAI component might be interacted with by another script (e.g., a UI manager).
        // This is purely for illustrative purposes.
        /*
        public class PlayerInteractionExample : MonoBehaviour
        {
            public ShopkeeperAI shopkeeperAI; // Assign this in the Inspector.
            public TMPro.TMP_InputField playerInputField; // Assign a TextMeshPro InputField.
            public TMPro.TMP_Text shopkeeperResponseText; // Assign a TextMeshPro Text element for display.

            // This method could be called by a UI button's OnClick event.
            public async void OnPlayerSubmitInput()
            {
                if (shopkeeperAI == null || playerInputField == null || shopkeeperResponseText == null)
                {
                    Debug.LogError("[PlayerInteractionExample] Required components (ShopkeeperAI, InputField, ResponseText) not assigned.");
                    return;
                }

                string playerSays = playerInputField.text;
                if (string.IsNullOrWhiteSpace(playerSays)) 
                {
                    shopkeeperResponseText.text = "You need to say something to the shopkeeper!";
                    return;
                }

                shopkeeperResponseText.text = "Shopkeeper is thinking..."; // Provide immediate feedback.
                string aiResponse = await shopkeeperAI.GetShopkeeperResponse(playerSays);
                shopkeeperResponseText.text = "Shopkeeper: " + aiResponse;
                playerInputField.text = ""; // Clear the input field for the next interaction.
            }
        }
        */
    }
}
