# Truck Mobile Game

A Unity mobile arcade-trucking prototype based on the project game-mechanics document.

## Current milestone

Prototype 1 proves the core driving loop:

- automatic forward motion;
- left/right touch steering with recentering;
- heavy arcade handling and chase camera;
- a short hand-built road with two-way traffic;
- a drive-through fuel station;
- destination success and out-of-fuel failure.

Police, trailers, multiple trucks, customization, the depot, hired drivers, economy, and procedural generation are intentionally deferred.

## Development target

- Unity 6.6 (`6000.6.6f1`)
- Universal Render Pipeline
- Landscape iOS, tested first on iPhone 16
- Target frame rate: 60 FPS

## Repository rules

- Commit `Assets`, `Packages`, and `ProjectSettings`.
- Do not commit Unity-generated `Library`, `Temp`, `Logs`, or build directories.
- Keep gameplay tuning in configuration assets rather than hard-coded constants.
- Use placeholder geometry until the driving prototype passes its validation gate.

## First playable

Open `Assets/Game/Scenes/Prototype.unity` and press Play.

- Keyboard: `A`/`D` or left/right arrows.
- Mouse: hold the left button on the left or right half of the Game view.
- iPhone: hold the left or right half of the screen.
- Recovery: press `R`; the truck also recovers automatically when upside down.

Use `Truck Game > Build iOS Prototype` to export the Xcode project to `Builds/iOS`.
