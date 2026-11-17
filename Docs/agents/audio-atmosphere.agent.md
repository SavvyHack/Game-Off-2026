# Feature: Audio and atmosphere

Branch: `feature/audio-atmosphere`. Read [AGENTS.md](../../AGENTS.md) first.

Status: **not started.** The pod currently has greybox lighting and no audio.

## Goal

Make the pod feel like a small, humming, lonely place a long way from anyone.

## What exists

- Lighting from `PodLevelBuilder`: cyan screen glow in the cockpit, warm lamp light in the lounge, cool lab
  strip light with a green glow from the sample tube, dim warm light in the quarters, a bloom volume.
- A procedural star field outside the viewport.

## Suggested work

- Ambient bed: hull hum, air handling, occasional creaks and electronics.
- Per-station loops and one-shots: console beeps, drone launch/return, lab instruments.
- Footsteps on the deck; UI feedback sounds.
- Music, if any: sparse.
- Lighting states: e.g. alert lighting when a dispatch goes badly, dimmed lights at rest.
- An audio mixer with master / music / SFX groups, exposed to the settings menu from ui-hud.

## Scope

In: audio assets and playback, mixer, lighting mood and lighting states, post-processing.

Out: pod geometry, puzzle logic.

## Open questions

- Music or ambience only?
- Any voice (radio contact with the survivors), or text only?
- Where do audio assets come from (made by the team, licensed packs)? Record licences in `Docs/`.

## Acceptance checklist

- [ ] Ambient bed plays in the pod and loops cleanly.
- [ ] Interactions give audio feedback.
- [ ] Volume controlled through a mixer.
