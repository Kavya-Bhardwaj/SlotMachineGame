using UnityEngine;
using System.Collections;

/// <summary>
/// Manages player interaction with the slot machine lever/handle.
/// Listens for 2D raycast clicks, manages the visual handle state, 
/// physically moves the handle down when pulled, and signals the SlotManager.
/// </summary>
public class HandleController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Reference to the central SlotManager that controls game flow.")]
    [SerializeField] private SlotManager slotManager;

    [Header("Visual Assets")]
    [Tooltip("Sprite representing the handle in its resting 'up' state.")]
    [SerializeField] private Sprite handleUp;
    
    [Tooltip("Sprite representing the handle in its pulled 'down' state.")]
    [SerializeField] private Sprite handleDown;
    
    [Header("Animation Settings")]
    [Tooltip("How far down the handle should move on the Y axis when pulled.")]
    [SerializeField] private float pullOffset = 1.0f; // Adjust this in the Inspector!
    
    // Internal state tracking
    private SpriteRenderer spriteRenderer;
    private bool canPull = true;
    private Vector3 originalPosition; // Stores the starting position

    void Start()
    {
        // Cache the SpriteRenderer component and starting position
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position; // Save exactly where it started
        
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = handleUp;
        }
    }

    void Update()
    {
        // Listen for primary mouse button clicks
        if (Input.GetMouseButtonDown(0) && canPull)
        {
            // Convert screen click position to world space for 2D physics raycasting
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            // Check if the raycast intersected with the collider attached to this specific GameObject
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                Debug.Log("🔥 HANDLE PULLED!");
                StartCoroutine(PullHandle());
            }
        }
    }

    /// <summary>
    /// Coroutine that handles the visual animation of the lever pull,
    /// moves its position downwards, and triggers the game logic.
    /// </summary>
    IEnumerator PullHandle()
    {
        // Lock the handle to prevent multiple pulls
        canPull = false;

        // Phase 1: Visual Feedback - Change Sprite AND Move Down
        spriteRenderer.sprite = handleDown;
        transform.position = originalPosition + new Vector3(0, -pullOffset, 0); // Move it down
        
        // Brief pause to emphasize the mechanical feel of the pull
        yield return new WaitForSeconds(0.2f);

        // Phase 2: Core Game Trigger - Start spinning
        slotManager.StartSpin();

        // Phase 3: Wait for Sequence
        // Wait for the entire slot sequence (spinning, stopping, UI) to finish
        yield return new WaitForSeconds(9f);

        // Phase 4: Reset State
        // Return the handle visual and its position to the default state
        spriteRenderer.sprite = handleUp;
        transform.position = originalPosition; // Move it back up
        canPull = true;

        Debug.Log("✋ Handle ready for next spin!");
    }

    /// <summary>
    /// Callback intended to be triggered by the SlotManager.
    /// </summary>
    public void OnSpinComplete()
    {
        // Left empty intentionally for potential future event-driven expansions.
    }
}