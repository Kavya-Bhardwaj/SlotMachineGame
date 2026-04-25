# 🎰 Unity 2D Slot Machine Game

## 📄 Game Overview
This is a fully functional, realistic 2D Slot Machine game built in Unity. It features three independently spinning reels, realistic mechanical deceleration, UI feedback for wins and losses, and a fully interactive slot handle. The game uses Unity's random number generator (RNG) to ensure completely fair and unpredictable outcomes on every spin.

## 🚀 Instructions to Run the WebGL Build
To play the game directly in your browser without opening Unity:
1. Navigate to the `/Build/WebGL/` folder in this repository.
2. Because WebGL requires a local web server to run due to browser security restrictions, you can run it using one of the following methods:
   - **VS Code:** Open the `WebGL` folder in VS Code and use the "Live Server" extension.
   - **Python:** Open a terminal in the `WebGL` folder and run `python -m http.server 8000`, then go to `http://localhost:8000` in your browser.
   - **Node.js:** Run `npx http-server` in the directory.
3. Click the **Handle** on the right side of the machine or the **UI Spin Button** (if enabled) to play!

## ✨ Bonus Features & Polish
I went beyond the basic requirements to add several layers of polish and "game feel":
- **Realistic Mechanical Snapping:** Instead of just stopping instantly, the reels use a mathematical cubic ease-out curve (`1f - Mathf.Pow(1f - t, 3f)`) to simulate the heavy, mechanical "click" of a real casino slot machine locking into place.
- **Dynamic Handle Animation:** The slot handle doesn't just change sprites; it physically translates its Y-position downward when pulled to give a satisfying, tactile feel to the player's input.
- **Juicy UI Animations:** The UI doesn't just statically change text. Winning triggers a flashing yellow `JACKPOT!!!` sequence, while losing triggers a delayed `Better luck next time...` prompt, keeping the player engaged.

## 🧠 Thought Process & Approach
My main goal was to create a highly optimized and scalable architecture. 
- **Infinite Reel Looping:** Instead of moving a massive parent object endlessly downward, I kept the parent `Reel` static. The script continuously translates the child symbols downwards and "wraps" them back to the top once they pass a specific Y-threshold (`-2.5f` wraps to `+5f`). This creates the illusion of an infinite reel without any physics glitches or floating-point precision errors over time.
- **Separation of Concerns (OOP):** 
  - `ReelController.cs`: Only handles spinning, math wrapping, and exact symbol snapping.
  - `HandleController.cs`: Only listens for player input (Raycasts) and visual handle state.
  - `SlotManager.cs`: Acts as the central brain, coordinating the timing of the reels, evaluating the RNG outcome, and triggering the UI states.
- **Precision Alignment:** To prevent the symbols from slowly drifting out of alignment after 50+ spins, the reels use `Mathf.Round()` to lock perfectly to whole numbers at the end of every spin sequence.

## 📁 Project Structure
The project follows clean Unity organization standards:
- `/Assets/Scripts/` - Contains all C# logic (SlotManager, ReelController, HandleController).
- `/Assets/Sprites/` - Contains all sliced 2D textures and UI graphics.
- `/Assets/Prefabs/` - Modular game objects for easy instantiation.
- `/Assets/Scenes/` - Contains the `MainScene`.
- `/Assets/UI/` - Contains Canvas elements and TextMeshPro assets.
- `/Build/WebGL/` - The compiled playable browser build.