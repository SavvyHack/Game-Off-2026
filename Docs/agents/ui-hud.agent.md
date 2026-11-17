# Feature: UI and HUD

Branch: `feature/ui-hud`. Read [AGENTS.md](../../AGENTS.md) first.

Status: **placeholder UI only** (crosshair, interaction prompt, timed message line).

## Goal

Shared UI: the in-world HUD, the frame that station screens open in, and menus. The puzzle features own the
contents of their own screens; this feature owns the common look and the shell around them.

## What exists

- `InteractionPromptUI` on a screen-space canvas built by `PodLevelBuilder`: a dot crosshair, a "[E] ..."
  prompt, and a message line that shows a station's placeholder text for a few seconds.
- It uses legacy uGUI `Text` with the built-in font so the project has no font import step. Expect to replace it.

## Suggested work

- Pick the UI technology (uGUI + TextMeshPro, or UI Toolkit) and a visual style that fits the pod's screens.
- A reusable "station screen" shell: open/close, cursor unlock, pause player movement while open.
- Main menu, pause menu, settings (mouse sensitivity, volume).
- An `EventSystem` with the Input System UI module (none exists in the scene yet, since nothing is clickable).

## Scope

In: HUD, shared screen shell, menus, settings, fonts and UI style.

Out: the puzzle rules and data shown inside dispatch and analysis screens.

## Open questions

- Are station screens diegetic (rendered on the in-world monitors) or overlay panels?
- uGUI or UI Toolkit?
- Does the HUD stay minimal (crosshair + prompt), or show persistent info (day, planet, objectives)?
- Controller support required for the jam build?

## Acceptance checklist

- [ ] Opening a station screen frees the cursor and stops player movement; closing restores both.
- [ ] Pause menu works from anywhere.
- [ ] Mouse sensitivity adjustable.
