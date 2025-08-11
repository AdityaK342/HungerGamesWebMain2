# Hunger Games (web version)

Live: https://adityak342.github.io/HungerGamesWebMain2/

This is a browser port of the Hunger Games project from the Computational Science course at the Illinois Math and Science Academy, taught by Dr. Peter Dong. The original simulation, arena engine, visualizer and basically all of the design are his, credit goes to him. What I did was rewrite the core of it in C# on Blazor WebAssembly (plus a bit of JavaScript for the canvas) so it runs on GitHub Pages instead of needing Windows and Visual Studio.

## What it is

A predator/prey sim. 20 hares and 5 lynxes get dropped into a 50x50 arena with randomly placed trees, shrubs and water. Every animal has an "intelligence" that looks at what it can see and picks a direction. Lynxes chase hares, hares run from lynxes, everyone has stamina so nobody can sprint forever, and it ends when one side is wiped out.

In the class you write your own hare and lynx intelligence and compete against everyone else's. This version ships with simple rule-based ones so there's something to watch. There's also a perceptron-based intelligence in `SimulationCore/AI` but the demo doesn't use it yet.

Stats, if you're curious:

- Hares: top speed 12, 75 stamina, vision 40
- Lynxes: top speed 10, 100 stamina, vision 35

Hares are faster and see further but gas out quicker, which is most of what makes it interesting.

## Controls

Start / Pause / Reset, speed buttons from 0.5x to 4x, and toggles for vision cones, velocity vectors and animal IDs. You can also type in a seed and hit "Generate New Arena" if you want a repeatable layout.

## Running it locally

Needs the .NET 8 SDK.

```
dotnet run --project HungerGameWeb
```

Then open whatever URL it prints.

## Repo layout

The parts that actually run in the browser:

- `SimulationCore/`: the sim itself (arena, animals, AI, perceptron). No UI dependencies.
- `HungerGameWeb/`: the Blazor app. `SimulationService` drives the loop and hands each frame to `wwwroot/js/canvas.js`, which draws it.

Everything else (`Arena/`, `ArenaVisualizer/`, `HungerGames/`, `NeuralNet/`, `VisualizerControl/`, etc.) is the original WPF project this came from. It's here for reference but isn't part of the web build. The `.pcp` files under `HungerGames/Perceptrons/` are trained perceptron weights from the original assignment.

## Deploying

`.github/workflows/deploy.yml` publishes the Blazor project and pushes it to GitHub Pages on every push to `main`. The base href has to match the repo name (`/HungerGamesWebMain2/`) or nothing loads, so the workflow checks for it and fails instead of deploying a blank page. Took me a few tries to figure that one out.

## License

MIT. Original project by Dr. Peter Dong / IMSA.
