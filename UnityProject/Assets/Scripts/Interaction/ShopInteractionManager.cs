using UnityEngine;
using UnityEngine.UI; // Required for UI Text elements

namespace AI.Shopkeeper.Interaction
{
    /// <summary>
    /// Manages player interaction with the shopkeeper based on proximity.
    /// Shows an interaction prompt when the player is in range and handles opening/closing the chat UI.
    /// This script should typically be attached to the Shopkeeper GameObject or a dedicated trigger volume around the shopkeeper.
    /// </summary>
    public class ShopInteractionManager : MonoBehaviour
    {
        [Header("Core References")]
        [Tooltip("Reference to the ShopkeeperUI script which controls the chat interface. Must be assigned in the Inspector.")]
        public AI.Shopkeeper.UI.ShopkeeperUI shopkeeperUIScript;

        [Tooltip("Reference to the Player's Transform component. Used to calculate distance. " +
                 "Assign in Inspector or the script will attempt to find a GameObject tagged 'Player' on Start.")]
        public Transform playerTransform;

        [Tooltip("UI Text element used to display the interaction prompt (e.g., 'Press E to Talk'). Must be assigned in the Inspector.")]
        public Text interactionPromptText;

        [Header("Interaction Configuration")]
        [Tooltip("The maximum distance (in world units) within which the player can interact with the shopkeeper.")]
        public float interactionRange = 3f;

        [Tooltip("The keyboard key the player must press to initiate interaction when in range.")]
        public KeyCode interactionKey = KeyCode.E;
        
        [Tooltip("The keyboard key the player can press to close the chat UI once it's open.")]
        public KeyCode closeUIKey = KeyCode.Escape; // Common key for closing UI panels.

        private bool isPlayerCurrentlyInRange = false; // Tracks if the player is within interactionRange.
        private bool isFullyInitialized = false; // Tracks if all essential references are set.

        /// <summary>
        /// Called when the script instance is being loaded.
        /// Validates references, finds the player if not assigned, and sets initial UI states.
        /// </summary>
        void Start()
        {
            // Attempt to find Player by tag if not assigned in Inspector.
            if (playerTransform == null)
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    playerTransform = playerObject.transform;
                    Debug.Log("[ShopInteractionManager] PlayerTransform automatically found using 'Player' tag.");
                }
                else
                {
                    Debug.LogError("[ShopInteractionManager] CRITICAL ERROR: PlayerTransform is not assigned and could not be found by tag 'Player'. This script requires a player reference. Please assign it in the Inspector or ensure your player GameObject is tagged 'Player'. Script will be disabled.");
                    enabled = false; // Disable script if player is essential and missing.
                    return;
                }
            }

            // Validate ShopkeeperUIScript reference.
            if (shopkeeperUIScript == null)
            {
                Debug.LogError("[ShopInteractionManager] CRITICAL ERROR: ShopkeeperUIScript is not assigned in the Inspector. This script cannot function without it. Please assign the reference. Script will be disabled.");
                enabled = false; 
                return;
            }

            // Validate InteractionPromptText reference.
            if (interactionPromptText == null)
            {
                Debug.LogError("[ShopInteractionManager] CRITICAL ERROR: InteractionPromptText (UI Text element) is not assigned in the Inspector. This script cannot display interaction prompts. Please assign it. Script will be disabled.");
                enabled = false; 
                return;
            }

            // Set initial state for the interaction prompt (hidden).
            interactionPromptText.gameObject.SetActive(false); 
            
            // Ensure the main chat UI is initially hidden.
            // ShopkeeperUI's Start method should also handle this, but this is a safeguard.
            if (shopkeeperUIScript.IsChatUIVisible())
            {
                shopkeeperUIScript.HideChatUI();
                Debug.LogWarning("[ShopInteractionManager] ShopkeeperUI was initially visible. It has now been hidden by ShopInteractionManager.");
            }
            
            isFullyInitialized = true;
            Debug.Log("[ShopInteractionManager] Initialized successfully. Waiting for player interaction.");
        }

        /// <summary>
        /// Called every frame.
        /// Handles checking player proximity, displaying interaction prompts, and processing interaction key presses.
        /// </summary>
        void Update()
        {
            // Do nothing if critical references are missing (script should have been disabled by Start).
            if (!isFullyInitialized) 
            {
                return;
            }

            // Calculate distance from this GameObject (e.g., the Shopkeeper) to the player.
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            isPlayerCurrentlyInRange = distanceToPlayer <= interactionRange;

            bool isChatUIVisible = shopkeeperUIScript.IsChatUIVisible();

            if (isPlayerCurrentlyInRange)
            {
                if (!isChatUIVisible)
                {
                    // Player is in range, and the chat UI is NOT currently active.
                    // Display the interaction prompt.
                    interactionPromptText.text = $"Press '{interactionKey.ToString()}' to Talk";
                    interactionPromptText.gameObject.SetActive(true);

                    // Check if the player presses the interaction key.
                    if (Input.GetKeyDown(interactionKey))
                    {
                        shopkeeperUIScript.ShowChatUI();          // Open the chat UI.
                        interactionPromptText.gameObject.SetActive(false); // Hide the prompt text once UI is open.
                    }
                }
                else
                {
                    // Player is in range, AND the chat UI IS currently active.
                    // Hide the interaction prompt (as UI is already open).
                    interactionPromptText.gameObject.SetActive(false); 

                    // Check if the player presses the key to close the UI.
                    if (Input.GetKeyDown(closeUIKey))
                    {
                        shopkeeperUIScript.HideChatUI();
                        // The interaction prompt will be re-shown in the next frame if still in range
                        // because isChatUIVisible will be false.
                    }
                }
            }
            else
            {
                // Player is NOT in range.
                // Ensure the interaction prompt is hidden.
                if (interactionPromptText.gameObject.activeSelf)
                {
                    interactionPromptText.gameObject.SetActive(false);
                }

                // Design Choice: The UI does NOT automatically close if the player moves out of range.
                // The player must explicitly close it with the 'closeUIKey'.
                // If auto-close was desired, you would add:
                // if (isChatUIVisible)
                // {
                //    shopkeeperUIScript.HideChatUI();
                // }
            }
        }

        /// <summary>
        /// Draws a yellow wire sphere in the Unity Editor to visualize the interactionRange.
        /// This is only visible when the GameObject this script is attached to is selected.
        /// </summary>
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}
