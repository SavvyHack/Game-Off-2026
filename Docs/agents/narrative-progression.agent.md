# Feature: Narrative and progression

Branch: `feature/narrative-progression`. Read [AGENTS.md](../../AGENTS.md) first.

Status: **not started. Design is incomplete; see Open questions before building.**

## Goal

The structure that carries the player from planet to planet and tells the story around the two puzzle loops.

## What has been decided

- Far future; humanity is spacefaring; all the planets were at war.
- A cataclysmic event leaves every planet unable to host life.
- The last surviving group from Earth is looking for a new planet to live on.
- Nobody knows whether people from other planets are alive.
- One person is sent in a small drone pod to investigate planets. The player is that person, and the game
  never leaves the pod.
- Per planet, the loop is: arrive, recover samples (dispatch), analyse them (analysis).

## How it connects

- `PilotSeat` station: the natural place for choosing / travelling to the next planet.
- `Bunk` station: the natural place for ending a day or a planet, and for quiet story beats.
- Consumes results from sample-dispatch and chemical-analysis; supplies them with the current planet's content.

## Suggested structure (a starting sketch, not a decision)

- `PlanetDefinition` (ScriptableObject): name, description, its missions, its samples, its story beats.
- A `GameState` holding current planet, day, collected results; saved between sessions.
- A simple state flow: Travel -> Dispatch -> Analysis -> Report -> Rest -> Travel.

## Scope

In: planet sequence, game state and saving, story delivery, endings.

Out: the puzzle mechanics themselves, pod geometry, audio.

## Open questions

- What was the cataclysm? Is the cause something the player discovers?
- Who is the player character? Do they talk to the survivors back home, and how (text log, radio, nothing)?
- How many planets, and is the order fixed or chosen?
- Are other survivors found? Is that the central mystery, and how is evidence of them delivered?
- What are the endings, and what decides them (accuracy, choices, discoveries)?
- What does a wrong verdict cost (the survivors settle on a dead world)?
- Is there a resource to manage across planets (fuel, drones, supplies)?
- Session length target for the jam build?

## Acceptance checklist (fill in once the design is settled)

- [ ] Player can move from one planet to the next through the pilot seat.
- [ ] State persists across the dispatch and analysis loops for a planet.
- [ ] At least one ending is reachable.
