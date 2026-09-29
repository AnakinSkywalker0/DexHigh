# Dragon Arena — Dexhigh Junior Unity Developer Assessment

A small 2.5D top-down battle: one player-controlled dragon against one AI-controlled dragon in an
enclosed arena, with three abilities each, health/cooldown UI, hit feedback and a Winner Screen with Restart.

- **Unity version:** 6000.3.24f1 (Unity 6), Universal Render Pipeline
- **Scene:** `Assets/_Project/Scenes/Battle.unity`
- **Platform:** Windows standalone

## Controls

Movement is **WASD** (not click-to-move). Movement keys are world-space from the top-down camera:
W = up the screen, S = down, A = left, D = right.

| Input | Action |
|---|---|
| **W A S D** | Move |
| **1** | Fire Breath — ranged cone in front of the dragon |
| **2** | Tail Swipe — close-range melee |
| **3** | Fly Attack — takes off, leaps forward and lands with an area hit |
| **Space / Left Shift** | Dash — short burst with a brief invulnerability window (about 1.1 s cooldown) |
| **Tab** | Toggle lock-on (on by default): your dragon always faces the enemy, a red ring marks it |
| Restart button | Shown on the Winner Screen |

With lock-on off, the dragon faces the direction it moves.

## Abilities

| Ability | Damage | Cooldown | Range | Notes |
|---|---|---|---|---|
| Fire Breath | 12 | 4 s | 8 | 60° cone, fire VFX + sound |
| Tail Swipe | 18 | 2.5 s | 2.5 | Melee, knockback |
| Fly Attack | 28 | 8 s | 6 (leap) | Take-off, arc, landing area damage, strong knockback |

Each ability has its own damage, cooldown (shown as a radial sweep on its icon), animation and visual effect.
Numbers live in `ScriptableObject` assets under `Assets/_Project/ScriptableObjects/Abilities/`, so tuning never touches code.

## Enemy AI

A readable three-state machine (`Assets/_Project/Scripts/AI/AIBrain.cs`):

- **Idle** — waits a few seconds at the start of a round, facing the player.
- **Chase** — paths to the player on a baked NavMesh; while waiting for its next attack it circle-strafes instead of standing still.
- **Attack** — chooses at random among abilities that are off cooldown and in range (never the same one twice in a row when there is a choice), then waits a randomised gap before the next.

It uses the same `AbilityController` and the same cooldowns as the player. A damage multiplier (0.6) on the enemy prefab is the only difficulty adjustment.

## Project structure

```
Assets/_Project/
  Scenes/            Battle.unity (+ baked NavMesh)
  Scripts/
    Core/            Health, IDamageable, DamageInfo
    Combat/          AbilityDefinition (SO), AbilityController, Knockback, HitFlash,
                     DamagePopup(+Spawner), DragonAnimator
    Player/          PlayerBrain  (movement, dash, lock-on, ability keys)
    AI/              AIBrain      (Idle / Chase / Attack)
    UI/              HealthBarUI, AbilitySlotUI, WinnerScreenUI
    Camera/          BattleCameraRig (frames both dragons, zooms with their distance)
    Systems/         MatchManager (win condition, restart)
  Prefabs/           Dragon_Base -> Dragon_Player / Dragon_AI variants, VFX, LockMarker, DamagePopup
  Animation/         DragonSoulEater.controller
  ScriptableObjects/ Abilities: Fire, Tail, Fly
```

Design notes: dragons are built by composition — both prefabs share `Health`, `AbilityController` and `HitFlash`;
only the "brain" differs (`PlayerBrain` reads input, `AIBrain` runs the state machine).
Damage is applied on the attack's impact frame (after a per-ability cast delay), not on the key press, so hits line up with the animation.

## Asset sources (all free, from the Unity Asset Store)

| Asset | Used for | Source |
|---|---|---|
| **Four Evil Dragons PBR** (SoulEater dragon, Blue/Red skins, animations) | Player and enemy dragons | Unity Asset Store — publisher/license: **[fill in from your Asset Store page]** |
| **Vefects — Free Fire VFX (URP)** | Fire/impact particle effects and fire sound loops | Unity Asset Store, by Vefects (vefects.com) |
| **Brawl Arena** (free version) | Arena floor, wall tiles, rocks, bones, skulls, chests | Unity Asset Store, by "Solo Player" (assetstore.unity.com/packages/slug/295013) |

No paid or ripped assets were used. Each pack remains under its own Asset Store license.
Everything under `Assets/_Project/` was written for this assessment.

## Running

- **Build:** unzip the Windows build and run the `.exe`.
- **From source:** open the project folder in Unity 6000.3.24f1, open `Assets/_Project/Scenes/Battle.unity`, press Play.
  Keep the Game view focused so it receives keyboard input.

---

# AI Usage Note

## Tools used
**Claude (Claude Code, Anthropic)** was the main tool for planning and building this project.
It ran inside the terminal/desktop app, edited the C# scripts, and drove the live Unity Editor through the Unity CLI
(creating prefabs, animator controllers, materials and scene changes, running Play mode and taking screenshots).
*[Add any other tools you used, e.g. ChatGPT or Copilot, here.]*

## What I used it for
- **Planning:** turning the brief into a build order, choosing an architecture (composition over inheritance, data-driven abilities, `NavMeshAgent` for the AI, `CharacterController` for the player).
- **Coding:** the combat, AI, player, UI and match-flow scripts.
- **Editor setup:** arena, prefabs, animator states and transitions, VFX prefabs, ability assets, URP material conversion, NavMesh baking.
- **Debugging:** reproducing problems in Play mode, reading values back from the running game, and checking screenshots.
- **Research/asset integration:** wiring the imported dragon, fire and arena packs into the existing systems.

## One example where the AI got something wrong
**The health bars looked frozen even though the game logic was correct.**
The AI set the bar images to "Filled" mode and wired them to the `Health` events, and its first tests
(which only read `fillAmount`) reported success. In real play the bars never visibly moved.
Reading the value back showed the truth: `fillAmount` was 0.60 while the bar on screen was still full.
The cause was that a UI `Image` in Filled mode needs a sprite to clip — with none assigned Unity draws the full rectangle,
so the fill value has no visual effect. The same mistake also broke the ability cooldown sweeps.
The fix was to add a plain white sprite and assign it to all five filled images, then verify with a screenshot of damaged bars.
The lesson: "the value is correct" is not the same as "the player can see it" — I now check the rendered result, not just the data.

Other AI mistakes that were found and fixed the same way (by testing and reading values back rather than trusting the code):
- **Double damage on the player:** each dragon has two colliders, and the hit check counted a target once per collider, so the player took double damage. Fixed by counting each target once per hit.
- **Knockback launched targets upward** because the hit direction wasn't flattened to the ground plane.
- **Stray quotation marks** ended up in ability names and animator trigger names from how values were escaped when authoring assets.
- **Dash tuning appeared to have no effect:** changing a field's default value in code does not update instances that Unity has already serialized, so the dash kept its old speed until the values were set explicitly on the prefab and scene.
- **Hit flash was invisible:** the flash set the base colour to white, which does nothing over a textured model, and the prefab's saved value overrode the new default.

## How the tools made the work faster or better
- Repetitive Editor work (prefab variants, animator transitions, material conversion, arena dressing) that would take hours by hand was scripted and re-run when the design changed, such as scaling the arena by 30%.
- Play-mode testing driven by script (triggering abilities, reading health and animator state, capturing screenshots) surfaced real bugs — double damage, frozen bars, a floating player capsule — that I would otherwise have found late.
- Limits: the automated tests could not press real keys, so keyboard feel (movement, lock-on, dash) still has to be judged by playing. *[Add your own note here on what you playtested and changed by hand — the assessment wants to see that you understand and control the code.]*
