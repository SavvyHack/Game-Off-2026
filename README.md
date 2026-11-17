# Game Off 2026

A first-person deduction game set entirely inside a one-person survey pod.

Humanity's planets destroyed each other. The last survivors from Earth send a single person out in a small
drone pod to find a world that can still hold life, and to learn whether anyone else is left. At each planet
you choose which drones to send for samples, then work out what those samples are made of.

## Getting started

1. Install **Unity 6.6 (6000.6.9f1)** through Unity Hub.
2. Install Git LFS once: `git lfs install`.
3. Clone the repo and add the folder in Unity Hub.
4. Open `Assets/_Project/Scenes/DronePod.unity` and press Play.

Controls: **WASD** move, **mouse** look, **E** interact, **Esc** release the cursor (click to capture it again).

## Rebuilding the pod

The pod is a generated greybox. To change it, edit `Assets/_Project/Editor/PodLevelBuilder.cs`, then run
**Tools > Game Off > Rebuild Drone Pod** in the editor.

## Contributing (humans and AI agents)

Read [AGENTS.md](AGENTS.md) first. Each feature has its own branch and its own spec in `Docs/agents/`.
