# Feature: Sample dispatch (gameplay loop 1)

Branch: `feature/sample-dispatch`. Read [AGENTS.md](../../AGENTS.md) first.

Status: **not started. Design is incomplete; see Open questions before building.**

## Goal

At each planet the player recovers samples by sending drones from the pod. The player never goes down
themselves. The puzzle is deduction: read the situation, work out which drones suit it, send them.

## What has been decided

- Played at the `DroneDispatch` station (lounge / ops, right wall).
- Different situations call for different drones.
- Each mission ends in one of three outcomes, determined by how well the chosen drones fit the situation:
  - **Recovered**: sample is intact.
  - **Recovered, damaged**: sample comes back but is broken / burnt / otherwise compromised.
  - **Not recovered**: nothing comes back.
- The outcome should reflect the quality of the player's reasoning, not luck.
- The feel reference is dispatcher-style management games: a stream of situations, a limited roster, matching
  the right responder to the right job. Reference for feel only; all content is original.

## How it connects

- Entry: subscribe to `Station.Interacted` and react when `station.Type == StationType.DroneDispatch`
  (or wire the station's `onInteract`). Clear the station's placeholder message once implemented.
- Output: recovered samples (with their condition) are the input to chemical analysis. Agree the shared
  `Sample` data type with `feature/chemical-analysis` before either side builds on it.

## Suggested data model (a starting sketch, not a decision)

- `DroneDefinition` (ScriptableObject): id, name, description, traits/capabilities.
- `MissionDefinition` (ScriptableObject): briefing text, clues, hazards, the sample(s) available.
- `DispatchResult`: outcome enum (`Recovered`, `Damaged`, `NotRecovered`) plus the sample and a short report.
- A pure C# resolver `(mission, chosen drones) -> DispatchResult` so the rules are unit-testable without a scene.

## Scope

In: mission briefing UI, drone roster and selection, resolution, results report, handing samples on.

Out: chemical analysis, travel between planets, story text beyond mission briefings.

## Open questions

- What drones exist, and what distinguishes them (traits, strengths, weaknesses)?
- How many drones can be sent per mission, and is there a cost to sending more (fuel, wear, time)?
- Can drones be damaged or lost? Are they repaired or replaced?
- How is the situation presented: text briefing, sensor readouts, map, images? How explicit are the clues?
- Is the outcome fully deterministic from the choice, or is there a random element?
- What exactly turns "recovered" into "damaged"? One wrong trait? A missing capability?
- How many missions per planet? Is there time pressure or are they turn-based?
- Does a damaged sample change the analysis puzzle (harder, less certain, impossible)?

## Acceptance checklist (fill in once the design is settled)

- [ ] Player can open the dispatch console at the station and close it again.
- [ ] At least one complete mission can end in each of the three outcomes.
- [ ] Resolver logic covered by EditMode tests.
- [ ] Results are passed on in the agreed `Sample` format.
