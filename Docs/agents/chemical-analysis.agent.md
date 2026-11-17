# Feature: Chemical analysis (gameplay loop 2)

Branch: `feature/chemical-analysis`. Read [AGENTS.md](../../AGENTS.md) first.

Status: **not started. Design is incomplete; see Open questions before building.**

## Goal

After samples are recovered, the player works out what they are made of. The puzzle is deduction against a
rulebook: run tests, compare results with the rules, and commit to a conclusion.

## What has been decided

- Played at the `AnalysisBench` station (lab, right wall).
- Input is the samples from the dispatch loop, including their condition (intact or damaged).
- There are rules and tests; the player uses them to deduce which chemicals a sample contains.
- The feel reference is document-inspection games: a desk, a rulebook that grows over time, evidence to
  cross-check, a verdict to stamp, consequences for getting it wrong. Reference for feel only; all content is original.

## How it connects

- Entry: subscribe to `Station.Interacted` and react when `station.Type == StationType.AnalysisBench`
  (or wire the station's `onInteract`). Clear the station's placeholder message once implemented.
- Input: the shared `Sample` data type, agreed with `feature/sample-dispatch`.
- Output: a per-sample conclusion, and probably a per-planet verdict that narrative-progression consumes.

## Suggested data model (a starting sketch, not a decision)

- `ChemicalDefinition` (ScriptableObject): id, name, properties that tests can reveal.
- `TestDefinition` (ScriptableObject): the instrument, what property it reveals, any cost.
- `RuleDefinition` (ScriptableObject): a rulebook entry linking observations to conclusions.
- `Sample`: hidden true composition, condition, and the observations revealed so far.
- A pure C# evaluator for "is the player's conclusion correct" so it is unit-testable without a scene.

## Scope

In: bench UI, tests/instruments, rulebook, submitting a conclusion, feedback on right/wrong.

Out: sample recovery, travel, story beyond what the analysis verdict feeds.

## Open questions

- Real chemistry, simplified chemistry, or a fictional system?
- What tests exist and what does each reveal? Are tests limited (reagents, time, power)?
- What is the player's final answer: a composition, a "habitable / not habitable" verdict, both?
- How does the rulebook grow across planets (new rules, exceptions, contradictions)?
- What happens on a wrong answer: immediate feedback, delayed consequences, story impact?
- How does a damaged sample change the puzzle?
- Is there a daily quota or time pressure?
- Does analysis reveal story (evidence that other people survived)?

## Acceptance checklist (fill in once the design is settled)

- [ ] Player can open the analysis bench at the station and close it again.
- [ ] At least one sample can be tested and correctly identified using the rulebook.
- [ ] A wrong conclusion is detected and has a visible consequence.
- [ ] Evaluation logic covered by EditMode tests.
