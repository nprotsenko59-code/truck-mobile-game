# Prototype 1

## Goal

Determine whether steering a continuously moving truck down a road is enjoyable on an iPhone.

## Included

1. One greybox truck with arcade physics.
2. Automatic speed and speed-sensitive left/right steering.
3. Smooth steering recentering and rollover assistance.
4. A high rear chase camera.
5. One 60–120 second hand-built route.
6. Two-way traffic, overtaking, barriers, and basic collisions.
7. Fuel consumption, warnings, one right-side drive-through station, and refuelling.
8. Destination success, out-of-fuel failure, pause, and restart.
9. A development-only tuning and performance overlay.

## Excluded

- Police and illegal cargo
- Trailers and multiple trucks
- Customization, garage, depot, hired drivers, and economy
- Random events and procedural generation
- Production art, monetization, analytics, and Android builds

## Validation gate

- The iPhone build holds approximately 60 FPS during normal driving.
- Steering, overtaking, station entry, and merging are predictable.
- Normal steering does not roll the truck and ordinary impacts are recoverable.
- Missing the fuel station causes failure; collecting fuel makes the destination reachable.
- Ten self-test runs can be completed without blocking defects.
- The owner rates steering predictability and enjoyment at least 4/5 before expanding scope.

