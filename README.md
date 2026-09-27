# Trinket

A tiny charm hangs from the top edge of your desktop, on a cord, reacting to real physics when you grab, drag, flick, or let go of it.

Trinket is a lightweight Windows desktop utility — not a game, not a background hog. It sits quietly at the edge of your screen, occasionally stirs in a light breeze, and swings naturally when you interact with it. Built from scratch in C# and WPF, with an original rope-physics simulation (Verlet integration, distance constraints) driving every movement.

## Features

- **Real rope physics** — a multi-node rope simulated with Verlet integration, not a looping animation. Drag it, throw it, watch it settle.
- **24 built-in charms** — moon, star, sunflower, evil eye, cherries, cat, and many more, each with original hand-drawn vector artwork.
- **Custom charm import** — bring in your own PNG/WebP image and mark exactly where the cord should attach.
- **Charm Studio** — customize the cord style, color, and length; choose beads; pick where the charm hangs.
- **Click reactions** — sparkle, wiggle, spin, pulse, or bounce, depending on the charm.
- **Mouse Breeze & Ambient Breeze** — a fast swipe near the charm gives it a tiny nudge; an optional rare, subtle breeze keeps it from ever feeling too static.
- **Saved Trinkets** — save and reload your favorite cord + charm + bead combinations.
- **Multi-monitor aware** — pick which screen the charm lives on.
- **Genuinely idle-friendly** — the whole simulation sleeps (near-zero CPU) when nothing is moving, and wakes instantly on interaction.

## Privacy

No accounts. No telemetry. No analytics. No internet calls of any kind. Everything — settings, saved Trinkets, imported charm images — stays entirely on your own computer, under `%LOCALAPPDATA%\Trinket\`.

## Installing

Download the latest `TrinketSetup.exe` from the [Releases](../../releases) page and run it. Windows SmartScreen may warn that the app is unsigned — this is expected for independently distributed software; click "More info" → "Run anyway."

## Building from source

Requires the .NET SDK (10.0 or later) and Visual Studio 2022 (or the `dotnet` CLI).

git clone <this-repo-url>
cd Trinket
dotnet build
dotnet run --project Trinket


To build a distributable installer yourself, see `installer/Trinket.iss` (requires [Inno Setup](https://jrsoftware.org/isinfo.php)).

## Architecture

- `Physics/` — the rope simulation itself, with zero UI dependencies (fully unit-testable, see `Trinket.Tests/`)
- `Rendering/` — turns charm/cord/bead definitions into actual WPF visuals
- `Overlay/` — the transparent, click-through desktop overlay window
- `Studio/` — the settings/customization window
- `Services/` — settings persistence, tray icon, startup registration, monitor detection, asset storage

## License

MIT — see [LICENSE](LICENSE).
