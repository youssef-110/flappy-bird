# Updraft

A one-button flying game for Windows, written in C# with Windows Forms. You steer a paper
glider through gaps in wind gates. Each press catches an updraft; gravity does the rest.
The gaps narrow and the gates speed up as your score climbs.

All drawing is done in code (no image files), and the game rules live in `Game.cs`,
separate from the window, so they are covered by unit tests.

## Controls

| Key | Action |
|---|---|
| Space, Up, W or mouse click | Catch an updraft |
| Space after a crash | Fly again |
| Esc | Quit |

## Run

Needs the [.NET 9 SDK](https://dotnet.microsoft.com/download) on Windows.

```bash
dotnet run --project Updraft
```

Or open `Updraft.sln` in Visual Studio 2022 and press F5.

## Test

```bash
dotnet test
```

The tests cover gravity and lift, crashing and restarting, gate collisions, and an
autopilot run that has to clear five gates.

## How it works

- **Physics:** a fixed glider X position with vertical velocity. Gravity is 1400 px/s²,
  an updraft sets the velocity to -430 px/s, and the fall speed is capped.
- **Frame timing:** the step is measured with a `Stopwatch` and clamped to 1/30 s, so a
  stalled window cannot push the glider through a wall.
- **Collisions:** circle against the two rectangles of each gate.
- **Difficulty:** the gap shrinks 3 px per point (to 130 px) and speed rises 4 px/s per
  point (to 290 px/s).
