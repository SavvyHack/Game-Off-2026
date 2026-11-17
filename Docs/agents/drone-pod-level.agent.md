# Feature: Drone pod level

Branch: `feature/drone-pod-level`. Read [AGENTS.md](../../AGENTS.md) first.

## Goal

The single location of the game: the interior of a one-person survey pod the player can walk around in first
person, with four stations that the other features plug into.

## Look target

[Docs/reference/pod-reference.png](../reference/pod-reference.png): a cramped, lived-in cylindrical pod shown
as a cutaway. Dark gunmetal hull, heavy ring bulkheads, cyan screen glow in the cockpit, a blue sofa, wall
monitors, a tall glass tube, a bunk with green bedding, thrusters at one end and a coupling ring at the other.
We take the layout and mood from it; the final art must be our own.

## Layout

A cylinder about 13 m long with a 3.8 m inner diameter and a flat deck. +Z is the front. Looking forward, +X is right.

| Zone | Z range | Contents | Station |
|---|---|---|---|
| Cockpit | 4.5 to 7 | pilot seat, dash, wraparound screens, round front viewport | `PilotSeat` |
| Lounge / ops | 0.5 to 4.5 | sofa and wall monitor (left), dispatch console and drone rack (right) | `DroneDispatch` |
| Lab | -3 to 0.5 | glass sample tube, locker, sample shelf (left), analysis bench (right) | `AnalysisBench` |
| Quarters | -6 to -3 | bunk with overhead cabinet (left), locker and small desk (right), sealed aft hatch | `Bunk` |
| Exterior | | coupling ring around the viewport, hull bands, thruster block aft | none |

Heavy bulkhead rings separate cockpit/lounge and lab/quarters; a lighter rib separates lounge/lab. The player
starts in the quarters, facing forward, so the first view is the whole length of the pod.

## How it is built

`Assets/_Project/Editor/PodLevelBuilder.cs` generates everything: procedural hull and ring meshes, primitive
furniture, URP materials, lights, post-processing, the player, the UI canvas, and the scene file. Rerun it
from **Tools > Game Off > Rebuild Drone Pod**. All dimensions are constants or literal positions in that file.

## Interaction model

- `FirstPersonController`: CharacterController movement, mouse/gamepad look, cursor lock.
- `PlayerInteractor`: raycast from screen centre (2.5 m); finds an `IInteractable` on the hit collider or its parents.
- `Station : IInteractable`: has a `StationType`, display name, prompt, placeholder message, highlight
  renderers, a `StandPoint` child (where a player can stand to use it) and a `FocusPoint` child (what they look at).
- `InteractionPromptUI`: crosshair, "[E] ..." prompt, and a timed message line for placeholders.

## Scope

In: pod geometry, lighting, player movement, interaction plumbing, placeholder stations.

Out: any puzzle logic or puzzle UI, travel, story, audio, final art.

## Open questions

- Does the player ever leave first-person walking (e.g. the camera locking onto a station screen while using it)?
- Is there gravity in the pod, or should movement eventually feel weightless?
- Should the viewport show the current planet, and should that change per planet?
- Final art direction: stylised low-poly, painterly like the reference, or something else?

## Acceptance checklist

- [x] Project compiles with no errors or warnings in Unity 6000.6.9f1.
- [x] All four stations exist, each with a free stand point from which it can be targeted (EditMode tests).
- [x] The centre aisle is clear from the quarters to the cockpit doorway (EditMode capsule sweep).
- [ ] Human Play-mode check: walk from the bunk to the pilot seat, see each prompt, press E at each station.
      (Not yet done; only batch-mode tests and still renders have been run.)
- [ ] Lighting pass: emissive props in the lab read too pale; colours need tuning.
- [ ] Greybox replaced with final art.
