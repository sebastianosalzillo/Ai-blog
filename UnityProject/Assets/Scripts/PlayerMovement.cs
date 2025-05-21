using UnityEngine;

/// <summary>
/// Handles first-person player movement and mouse look.
/// This script should be attached to the main player GameObject.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Speed of the player's movement.")]
    public float speed = 5.0f;

    [Header("Mouse Look Settings")]
    [Tooltip("Sensitivity of the mouse look.")]
    public float mouseSensitivity = 100.0f;

    [Header("Object References")]
    [Tooltip("Transform of the player's body (this GameObject). Assign in Inspector or it will default to this object's transform.")]
    public Transform playerBody;

    [Tooltip("Player's first-person camera. Assign in Inspector or it will default to Camera.main.")]
    public Camera playerCamera;

    private float xRotation = 0f; // Stores the current X rotation for vertical mouse look.

    /// <summary>
    /// Called when the script instance is being loaded.
    /// Initializes player body and camera references if not set, and locks the cursor.
    /// </summary>
    void Start()
    {
        // Lock the cursor to the center of the screen and hide it.
        Cursor.lockState = CursorLockMode.Locked;

        // Assign playerCamera if not set in Inspector (defaults to main camera).
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                Debug.LogError("[PlayerMovement] Main Camera not found and playerCamera was not assigned. Please assign a camera in the Inspector or ensure a Camera tagged 'MainCamera' exists.");
                enabled = false; // Disable script if no camera can be found.
                return;
            }
        }

        // Assign playerBody if not set in Inspector (defaults to this GameObject's transform).
        if (playerBody == null)
        {
            playerBody = transform;
        }
        Debug.Log("[PlayerMovement] Initialized. Player Body: " + playerBody.name + ", Player Camera: " + playerCamera.name);
    }

    /// <summary>
    /// Called every frame. Handles player input for movement and mouse look.
    /// </summary>
    void Update()
    {
        // --- Player Movement ---
        // Get horizontal (A/D, Left/Right Arrow) and vertical (W/S, Up/Down Arrow) input.
        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = Input.GetAxis("Vertical");   

        // Calculate movement direction relative to the player's orientation.
        Vector3 moveDirection = playerBody.right * moveX + playerBody.forward * moveZ;

        // Normalize movement vector to prevent faster diagonal movement.
        if (moveDirection.magnitude > 1)
        {
            moveDirection.Normalize(); 
        }
        
        // Attempt to use CharacterController for movement if available.
        CharacterController controller = GetComponent<CharacterController>();
        if (controller != null && controller.enabled)
        {
            // Apply movement considering speed and deltaTime, using CharacterController for collision detection.
            controller.Move(moveDirection * speed * Time.deltaTime);
        }
        else
        {
            // Fallback to transform.Translate if no CharacterController is found or it's disabled.
            // This method does not handle collisions automatically.
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }

        // --- Mouse Look ---
        // Get mouse X and Y input, adjusted by sensitivity and deltaTime.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust vertical rotation (pitch) and clamp it to prevent flipping upside down.
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f); 

        // Apply vertical rotation to the camera.
        if (playerCamera != null)
        {
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }
        
        // Apply horizontal rotation (yaw) to the player body.
        playerBody.Rotate(Vector3.up * mouseX);
    }
}
