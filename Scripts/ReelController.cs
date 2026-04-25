using UnityEngine;
using System.Collections;

/// <summary>
/// Controls the behavior of a single slot machine reel.
/// Handles continuous spinning, infinite symbol looping, deceleration, 
/// and exact snapping to a randomly selected symbol.
/// </summary>
public class ReelController : MonoBehaviour
{
    [Header("Spin Configuration")]
    [Tooltip("The speed at which the reel spins downwards.")]
    private float moveDistance = 15f; 
    
    [Tooltip("The duration (in seconds) it takes for the reel to decelerate before snapping.")]
    private float decelerationTime = 1.0f;
    
    // State tracking variables
    private bool isSpinning = false;
    private int stoppedSymbolIndex = -1;
    
    // Array to cache child symbol transforms for performance optimization
    private Transform[] symbolsArray;

    void Start()
    {
        // Enforce consistent speed and deceleration timing across all reel instances
        moveDistance = 15f;       
        decelerationTime = 1.0f;  

        // Cache the transform references of all child symbols at startup.
        // This avoids expensive GetComponent or GetChild calls during the Update loop.
        int childCount = transform.childCount;
        symbolsArray = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            symbolsArray[i] = transform.GetChild(i);
        }
    }

    void Update()
    {
        if (!isSpinning) return;

        // Continuously translate the symbols downwards while spinning
        MoveSymbolsDown(moveDistance * Time.deltaTime);
    }

    /// <summary>
    /// Translates all symbols downwards. If a symbol falls below the visible threshold,
    /// it is wrapped back to the top to create the illusion of an infinite reel.
    /// </summary>
    /// <param name="amount">The distance to move the symbols this frame.</param>
    void MoveSymbolsDown(float amount)
    {
        foreach (Transform symbol in symbolsArray)
        {
            // Translate symbol downward in local space
            symbol.localPosition += Vector3.down * amount;

            // Infinite Looping Logic:
            // Symbols are spaced exactly 1 unit apart. If a symbol falls below -2.5 (out of bounds),
            // wrap it exactly 5 units upwards to place it seamlessly at the top of the reel.
            if (symbol.localPosition.y < -2.5f)
            {
                symbol.localPosition += new Vector3(0, 5f, 0);
            }
        }
    }

    /// <summary>
    /// Initiates the spinning state of the reel.
    /// </summary>
    public void StartSpinning()
    {
        if (!isSpinning)
        {
            isSpinning = true;
        }
    }

    /// <summary>
    /// Triggers the deceleration and stopping sequence.
    /// </summary>
    public void StopSpinning()
    {
        if (isSpinning)
        {
            isSpinning = false;
            StartCoroutine(StopSequence());
        }
    }

    /// <summary>
    /// Coroutine that handles the multi-phase stopping mechanics:
    /// 1. Deceleration, 2. RNG Selection, 3. Smooth Snapping, 4. Final Alignment.
    /// </summary>
    IEnumerator StopSequence()
    {
        float elapsedTime = 0f;
        float currentSpeed = moveDistance;

        // Phase 1: Smooth Deceleration
        // Linearly interpolate the speed towards zero over the designated deceleration time.
        while (elapsedTime < decelerationTime && currentSpeed > 0.01f)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / decelerationTime;
            
            currentSpeed = Mathf.Lerp(moveDistance, 0.01f, t);
            MoveSymbolsDown(currentSpeed * Time.deltaTime);
            
            yield return null;
        }

        // Phase 2: RNG Target Selection
        // Randomly select one of the available symbols to be the winning outcome for this reel.
        stoppedSymbolIndex = Random.Range(0, symbolsArray.Length);
        Transform targetSymbol = symbolsArray[stoppedSymbolIndex];
        
        // Calculate the distance required to bring the target symbol exactly to the center (Y = 0).
        float currentY = targetSymbol.localPosition.y;
        float distanceToTravel = currentY; 
        
        // If the target symbol is already past the center point, force it to complete one full loop (+5 units).
        if (distanceToTravel < 0) 
        {
            distanceToTravel += 5f;
        }

        // Phase 3: Smooth Snap to Center
        // Use an ease-out cubic curve to simulate a mechanical click into place.
        float snapDuration = 0.5f;
        elapsedTime = 0f;
        float movedSoFar = 0f;

        while (elapsedTime < snapDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / snapDuration;
            t = 1f - Mathf.Pow(1f - t, 3f); // Ease-out curve formulation
            
            float targetMove = Mathf.Lerp(0, distanceToTravel, t);
            float stepMove = targetMove - movedSoFar;
            movedSoFar = targetMove;
            
            MoveSymbolsDown(stepMove);
            
            yield return null;
        }

        // Phase 4: Exact Alignment Locking
        // Round Y positions to exact whole numbers to prevent floating-point inaccuracies
        // from compounding and misaligning the reel over multiple spins.
        foreach (Transform symbol in symbolsArray)
        {
            float exactY = Mathf.Round(symbol.localPosition.y);
            symbol.localPosition = new Vector3(symbol.localPosition.x, exactY, symbol.localPosition.z);
        }
    }

    /// <summary>
    /// Returns the index of the currently stopped symbol for evaluation by the SlotManager.
    /// </summary>
    public int GetStoppedSymbolIndex()
    {
        return stoppedSymbolIndex;
    }

    /// <summary>
    /// Returns whether the reel is currently actively spinning.
    /// </summary>
    public bool IsSpinning()
    {
        return isSpinning;
    }
}