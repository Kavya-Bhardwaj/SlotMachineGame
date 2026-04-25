using UnityEngine;
using System.Collections;
using TMPro; // Required to manipulate TextMeshPro UI elements

/// <summary>
/// Core Game Manager for the Slot Machine.
/// Coordinates the three reels, evaluates win conditions based on stopped symbols, 
/// and handles the UI state and animations.
/// </summary>
public class SlotManager : MonoBehaviour
{
    [Header("Reel Dependencies")]
    [Tooltip("References to the individual reel controllers.")]
    [SerializeField] private ReelController reel1;
    [SerializeField] private ReelController reel2;
    [SerializeField] private ReelController reel3;
    
    [Header("Controls & UI")]
    [Tooltip("Reference to the lever/handle controller.")]
    [SerializeField] private HandleController handleController;

    [Tooltip("Reference to the TextMeshPro UI element for displaying game status to the player.")]
    [SerializeField] private TextMeshProUGUI statusText; 

    // Internal state tracking
    private int result1, result2, result3; // Stores the final symbol index for each reel
    private bool isSpinning = false;       // Prevents multiple spins from triggering simultaneously
    
    // Array mapping internal indices to symbol names for debugging/logic purposes
    private string[] symbolNames = { "Symbol1", "Symbol2", "Symbol3", "Symbol4", "Symbol1Copy" };

    void Start()
    {
        // Initialize the UI text to prompt the player to begin
        if (statusText != null)
        {
            statusText.text = "Click the Handle to Spin";
            statusText.color = Color.green;
        }
    }

    /// <summary>
    /// Initiates the entire spin sequence if the machine is currently idle.
    /// Called by the HandleController or external UI buttons.
    /// </summary>
    public void StartSpin()
    {
        if (!isSpinning)
        {
            // Update UI to reflect the active spinning state
            if (statusText != null)
            {
                statusText.text = "Spinning... Good Luck!";
                statusText.color = Color.white; 
            }
            
            StartCoroutine(SpinSequence());
        }
    }

    /// <summary>
    /// Coroutine managing the chronological flow of a slot machine spin.
    /// Starts all reels, then stops them sequentially with delays to build suspense.
    /// </summary>
    IEnumerator SpinSequence()
    {
        isSpinning = true;

        // Phase 1: Start all reels simultaneously
        reel1.StartSpinning();
        reel2.StartSpinning();
        reel3.StartSpinning();

        // Let them spin freely for a moment
        yield return new WaitForSeconds(2.5f);

        // Phase 2: Sequential Stopping
        // Stop Reel 1, wait for its deceleration, then record its final symbol
        reel1.StopSpinning();
        yield return new WaitForSeconds(1.5f);
        result1 = reel1.GetStoppedSymbolIndex();

        // Stop Reel 2, wait, and record
        reel2.StopSpinning();
        yield return new WaitForSeconds(1.5f);
        result2 = reel2.GetStoppedSymbolIndex();

        // Stop Reel 3, wait, and record
        reel3.StopSpinning();
        yield return new WaitForSeconds(1.5f);
        result3 = reel3.GetStoppedSymbolIndex();

        // Phase 3: Evaluate the final outcome
        CheckWinCondition();

        // Reset internal state to allow the next spin
        isSpinning = false;
        handleController.OnSpinComplete();
    }

    /// <summary>
    /// Evaluates if the stopped symbols across all three reels constitute a win.
    /// </summary>
    void CheckWinCondition()
    {
        // Check if Reel 1 matches Reel 2, and if Reel 2 matches Reel 3
        bool match1and2 = AreSymbolsEqual(result1, result2);
        bool match2and3 = AreSymbolsEqual(result2, result3);

        // If all three match, trigger the win sequence; otherwise, trigger the loss sequence.
        if (match1and2 && match2and3)
        {
            OnWin();
        }
        else
        {
            OnLose();
        }
    }

    /// <summary>
    /// Helper method to compare two symbol indices.
    /// Accounts for special cases, such as duplicated symbols in the asset pack.
    /// </summary>
    /// <param name="idx1">Index of the first symbol.</param>
    /// <param name="idx2">Index of the second symbol.</param>
    /// <returns>True if symbols are functionally identical.</returns>
    bool AreSymbolsEqual(int idx1, int idx2)
    {
        // Edge Case: Symbol1 (index 0) and Symbol1Copy (index 4) are visually and functionally the same.
        // If both indices are either 0 or 4, they are considered a match.
        if ((idx1 == 0 || idx1 == 4) && (idx2 == 0 || idx2 == 4))
            return true;
            
        // Standard comparison for all other symbols
        return idx1 == idx2;
    }

    /// <summary>
    /// Triggers the visual UI feedback for a winning outcome.
    /// </summary>
    void OnWin()
    {
        if (statusText != null)
        {
            StartCoroutine(JackpotSequence());
        }
    }

    /// <summary>
    /// Triggers the visual UI feedback for a losing outcome.
    /// </summary>
    void OnLose()
    {
        if (statusText != null)
        {
            StartCoroutine(LoseSequence());
        }
    }

    /// <summary>
    /// UI Coroutine for a Jackpot (Win). Flashes text to create excitement.
    /// </summary>
    IEnumerator JackpotSequence()
    {
        statusText.text = "JACKPOT!!!";
        statusText.color = Color.yellow;

        // Flash the text on and off 5 times rapidly
        for (int i = 0; i < 5; i++)
        {
            statusText.enabled = false;
            yield return new WaitForSeconds(0.2f);
            statusText.enabled = true;
            yield return new WaitForSeconds(0.2f);
        }

        // Brief pause after blinking before resetting
        yield return new WaitForSeconds(1f);
        
        // Reset UI for the next game
        statusText.text = "Click the Handle to Play Again";
        statusText.color = Color.green;
    }

    /// <summary>
    /// UI Coroutine for a Loss. Displays a brief consolation message.
    /// </summary>
    IEnumerator LoseSequence()
    {
        statusText.text = "Better luck next time...";
        statusText.color = Color.red;
        
        // Wait for 2 seconds so the player can read the message
        yield return new WaitForSeconds(2f);
        
        // Reset UI for the next game
        statusText.text = "Click the Handle to Play Again";
        statusText.color = Color.green;
    }
}