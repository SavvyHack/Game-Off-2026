# AGENTS.md: read this first

This file is for AI agents and human contributors. Read it fully before creating or changing anything, then read
the feature doc for the branch you are on (see [Feature docs](#feature-docs)).

## The game

Far future. Humanity spread across many planets, and those planets went to war. A cataclysmic event (cause not
yet decided) left every inhabited planet unable to host life. The last surviving group from Earth is searching
for a new world. Nobody knows whether people from the other planets survived.

To find out, the survivors send **one person in a small drone pod** to investigate planets one at a time. The
player is that person. **The whole game takes place inside the pod.**

At each planet the player does two jobs:

1. **Sample recovery** (deduction, in the spirit of dispatch-management games). The player reads a situation on the planet and
   chooses which drones to send. Depending on how well the choice fits the situation, samples come back
   **recovered**, **recovered but damaged** (broken, burnt, contaminated), or **not recovered**.
2. **Chemical analysis** (deduction, in the spirit of document-inspection games). The player runs tests on the recovered
   samples and applies a rulebook to work out their chemical composition.

Tone: lonely, quiet, methodical. One person, one small room, a very large question.

All content (names, characters, art, rules, text) must be original. Other games are references for the *feel*
of a loop only; never copy their names, characters, art, UI or levels.

## Tech

| | |
|---|---|
| Engine | Unity 6.6 (`6000.6.9f1`); check `ProjectSettings/ProjectVersion.txt` |
| Render pipeline | URP |
| Input | Input System package (no legacy `UnityEngine.Input`) |
| Perspective | First-person 3D inside the pod |
| Language | C#, namespace `GameOff` |
| Tests | Unity Test Framework, EditMode tests in `Assets/_Project/Tests` |

## Repo layout

```
AGENTS.md                  this file
Docs/agents/*.agent.md     one spec per feature (read the one for your branch)
Docs/reference/            reference art (pod-reference.png is the look target for the pod)
Assets/_Project/
  Scenes/DronePod.unity    the pod level (generated; see below)
  Scripts/                 runtime code      (assembly GameOff.Runtime)
    Player/                first-person controller
    Interaction/           IInteractable, PlayerInteractor
    Stations/              Station, StationType
    UI/                    InteractionPromptUI
  Editor/                  editor-only code  (assembly GameOff.Editor)
    PodLevelBuilder.cs     generates the pod scene, materials and meshes
  Materials/ Meshes/ Settings/   generated assets
  Tests/                   EditMode tests    (assembly GameOff.Tests)
```

Everything we author lives under `Assets/_Project`. Third-party assets go in `Assets/ThirdParty/<name>`.

## The pod scene is generated

`Assets/_Project/Scenes/DronePod.unity` is produced by `PodLevelBuilder` (menu **Tools > Game Off > Rebuild Drone Pod**).
While the pod is still greybox:

- Change the layout by editing `PodLevelBuilder.cs` and rebuilding, **not** by hand-editing the scene.
  Hand edits are lost on the next rebuild.
- Puzzle features should add their own prefabs/UI and attach to the stations via the hooks below rather than
  rearranging the pod.
- When real art replaces the greybox, this rule will be retired and this section updated.

## Stations: where features plug in

The pod has four stations, each a `Station` component with a `StationType`:

| StationType | Where | Owner feature |
|---|---|---|
| `PilotSeat` | cockpit (front) | narrative-progression (travel between planets) |
| `DroneDispatch` | lounge / ops, right wall | sample-dispatch |
| `AnalysisBench` | lab, right wall | chemical-analysis |
| `Bunk` | quarters (aft) | narrative-progression (rest / end of day) |

When the player looks at a station and presses **E**, `Station.Interacted` (static event) and the station's
`onInteract` UnityEvent fire. Until a feature is implemented the station shows its `placeholderMessage`.
To implement a station: subscribe to the event (or wire `onInteract`), open your UI, and clear the placeholder message.

## Branch workflow

One branch per feature, branched from `main`, merged back by pull request:

| Branch | Feature doc |
|---|---|
| `feature/drone-pod-level` | [Docs/agents/drone-pod-level.agent.md](Docs/agents/drone-pod-level.agent.md) |
| `feature/sample-dispatch` | [Docs/agents/sample-dispatch.agent.md](Docs/agents/sample-dispatch.agent.md) |
| `feature/chemical-analysis` | [Docs/agents/chemical-analysis.agent.md](Docs/agents/chemical-analysis.agent.md) |
| `feature/narrative-progression` | [Docs/agents/narrative-progression.agent.md](Docs/agents/narrative-progression.agent.md) |
| `feature/ui-hud` | [Docs/agents/ui-hud.agent.md](Docs/agents/ui-hud.agent.md) |
| `feature/audio-atmosphere` | [Docs/agents/audio-atmosphere.agent.md](Docs/agents/audio-atmosphere.agent.md) |

Rules:

1. Work only on the branch for the feature you were asked to do. Need a new feature? Add a doc in
   `Docs/agents/` and a row to this table first.
2. Stay inside your feature's scope. If you must touch another feature's files, keep the change minimal and
   say so in the PR.
3. Scenes and prefabs merge badly. Prefer new prefabs and additive pieces over editing shared ones.
4. Commit `.meta` files with their assets. Never commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`.
5. Binary assets (textures, models, audio) go through Git LFS (`git lfs install` once per machine).

## Rules for AI agents

- **Read before you make.** This file, then your feature doc, then the code you are about to change.
- **Do not invent game design.** Each feature doc has an *Open questions* section. If your task depends on an
  unanswered question, ask the humans; do not quietly pick an answer and build on it. If you are told to
  proceed anyway, record the assumption in the feature doc.
- **Keep docs true.** If you change behaviour a doc describes, update the doc in the same commit.
- **Verify.** The project must compile with no errors and the EditMode tests must pass before you call work done.
  Say plainly what you could not verify (for example, anything that needs a human pressing Play).
- **Small, focused commits** with messages that say what changed and why.

## Code conventions

- Namespace `GameOff` (sub-namespaces per folder are optional). One public type per file, file named after the type.
- `[SerializeField] private` fields rather than public fields; expose read-only properties.
- No `FindObjectOfType` in per-frame code. Wire references in the inspector or through events.
- Game data (missions, drones, chemicals, rules) lives in ScriptableObjects, not hard-coded in MonoBehaviours.
- Runtime code must not reference `UnityEditor`. Editor code goes in `Assets/_Project/Editor`.
- Comments explain *why*, not *what*.

## Definition of done

- Compiles with zero errors and no new warnings.
- EditMode tests pass (Window > General > Test Runner, or batch mode `-runTests -testPlatform EditMode`).
- The feature doc's acceptance checklist is ticked or the gaps are stated.
- Docs updated.
