# Boss Character Pipeline

Use this for each new boss: Wraith, Sorcerer, Crystal, and any future bosses.
Based on lessons learned from the Troll boss build (2026-03-31 to 2026-04-02).

---

## STEP 1 — Meshy.ai Model Export
**Owner: Brian (manual)**

1. Generate or select the boss character model in Meshy.ai
2. Choose an idle/T-pose stance — Mixamo needs clear limb separation to auto-rig
3. Export as FBX with textures/materials included
4. Save the FBX and texture files to your local Downloads or staging folder

**Tips:**
- Avoid models with weapons baked into the mesh if possible — separate weapons are easier to position
- If the model has a weapon, note which hand holds it (needed for WeaponImpactPoint later)

---

## STEP 2 — Mixamo Animation
**Owner: Brian (manual)**

1. Go to mixamo.com, upload the Meshy FBX
2. Let Mixamo auto-rig (verify the skeleton preview looks correct)
3. Search for and download these 4 animations:
   - **Walking** — a forward walk/lumber that fits the character's personality
   - **Attack** — a melee swing, slam, or spell cast (NOTE: watch for big lateral movement that could send the boss off the bridge)
   - **Taunt** — a roar, chest beat, intimidation gesture
   - **Death** — a death fall or collapse
4. Download each as FBX with "Without Skin" for the 3 animation-only files, and "With Skin" for the Walking file (or whichever you want as the base mesh)
5. Rename the downloaded files before importing:
   - `[Boss]_Walking.fbx`
   - `[Boss]_Attack.fbx`
   - `[Boss]_Taunt.fbx`
   - `[Boss]_Death.fbx`

**Tips:**
- Pick animations with minimal root motion drift — the code handles movement
- For the Attack clip, mentally note when the "hit" lands (early, middle, late) — you'll measure this precisely in Step 6

---

## STEP 3 — Import FBX into Unity
**Owner: Brian + CoPlay**

Copy all 4 FBX files into `Assets/_Project/Characters/[BossName]/`

### CoPlay Prompt 1 — Set Rig Type
> In the Project window, select all FBX files in Assets/_Project/Characters/[BossName]/. In the Inspector, go to the Rig tab. Set Animation Type to "Humanoid". Click Apply.

### CoPlay Prompt 2 — Rename Animation Clips
> Select [Boss]_Walking.fbx in Assets/_Project/Characters/[BossName]/. In the Inspector Animation tab, rename the clip from "mixamo.com" to "[Boss]_Walking". Click Apply. Repeat for [Boss]_Attack, [Boss]_Taunt, and [Boss]_Death.

**OR do Prompt 2 manually** — it's 4 clicks per file, sometimes faster than waiting for CoPlay.

---

## STEP 4 — URP Materials
**Owner: Brian + CoPlay**

Meshy exports use Standard shader which looks pink in URP. Fix them.

### CoPlay Prompt 3 — Convert Materials to URP
> Select the [BossName] model in Assets/_Project/Characters/[BossName]/. In the Inspector Materials tab, click "Extract Materials" and save them to the same folder. Then select all extracted .mat files, and change their Shader to "Universal Render Pipeline/Lit". Reassign the base color texture if it got cleared.

**Tips:**
- Verify materials are standalone .mat files in the folder, NOT embedded in a scene file
- Check the model in Scene view — if it's still pink, the texture assignment got dropped during shader swap. Manually drag the albedo texture into Base Map.

---

## STEP 5 — Animator Controller
**Owner: Claude Code**

Tell Claude:
> "Create the Animator Controller for [BossName] based on the Troll pattern. States: Walk, Attack, Taunt, Die. Use the clips from Assets/_Project/Characters/[BossName]/."

Claude will:
- Create `Boss[Name]Controller.controller` in `Assets/_Project/Animations/`
- Set up states matching AnimatedBossBehavior expectations (Walk as default, Attack, Taunt, Die)
- Wire clip references

**Note:** If Claude can't wire clips programmatically (Unity asset references), Brian may need one CoPlay prompt:

### CoPlay Prompt 4 (if needed) — Wire Clips into Animator
> Open the Animator Controller "Boss[Name]Controller" in Assets/_Project/Animations/. Drag [Boss]_Walking clip into the Walk state, [Boss]_Attack clip into the Attack state, [Boss]_Taunt clip into the Taunt state, and [Boss]_Death clip into the Die state.

---

## STEP 6 — Measure Animation Timing
**Owner: Brian (manual, Play Mode)**

This is critical and can't be automated. Do this BEFORE asking Claude to write the behavior script.

### 6A — Clip Lengths
1. Click each FBX in Project window, look at Animation tab
2. Write down the clip length in seconds for each:
   - Walking: ___s
   - Attack: ___s (this becomes `intimidationAttackTime`)
   - Taunt: ___s (this becomes `tauntDuration`)
   - Death: ___s (this becomes `dyingDuration`)

### 6B — Attack Impact Frame
1. Open the Attack clip in the Animation preview
2. Scrub through to find the frame where the weapon/hand makes contact
3. Note: what frame # out of total frames? (e.g., frame 43 of 74 = 58%)
4. This becomes the `impactNormalizedTime` for death strike positioning

### 6C — Impact Frame Offset (if the boss has a weapon or contact point)
1. Temporarily place the boss in the scene
2. Enter Play Mode, trigger the attack animation
3. Use Window > Show World Position to capture the world position of the weapon/hand at the impact frame
4. Subtract the boss root position from the weapon position = `impactFrameOffset` (Vector3)
5. Write down the offset: (___,  ___, ___)

**Give all these numbers to Claude for Step 7.**

---

## STEP 7 — Behavior Script
**Owner: Claude Code**

Tell Claude:
> "Create Boss[Name]Behavior.cs based on BossTrollBehavior as a template. Here are the animation timings: [paste numbers from Step 6]. The boss [does/does not] have a weapon. [Any unique mechanics for this boss]."

Claude will:
- Create the behavior script extending AnimatedBossBehavior
- Set duration fields to match measured clip lengths
- Configure death strike with correct impactFrameOffset and impactNormalizedTime
- Add any boss-specific mechanics (unique taunt effects, etc.)
- Use CrossFade for all transitions (never animator.Play)

---

## STEP 8 — EnemyData ScriptableObject
**Owner: Claude Code**

Tell Claude:
> "Create the EnemyData asset for Boss [Name]. HP: [X], Speed: [X], Damage: [X], spawnYOffset: 0."

Claude will:
- Create the ScriptableObject asset
- Set initial values (can be tuned later)

**Tip:** Start with spawnYOffset = 0. If the boss floats or sinks in testing, adjust then.

---

## STEP 9 — Prefab Assembly
**Owner: Brian + CoPlay**

### CoPlay Prompt 5 — Create Base Prefab
> Drag the [BossName] model from Assets/_Project/Characters/[BossName]/ into the scene. Unpack the prefab completely (right-click > Prefab > Unpack Completely). Rename it to "Boss_[BossName]".

### CoPlay Prompt 6 — Attach Components
> On the root GameObject "Boss_[BossName]" in the scene: Add component Boss[Name]Behavior. Add component CapsuleCollider. Assign the Animator Controller "Boss[Name]Controller" to the Animator component.

### CoPlay Prompt 7 — Wire EnemyData
> On Boss_[BossName], find the Boss[Name]Behavior component. Assign the EnemyData asset "Boss[Name]Data" to the enemyData field.

### CoPlay Prompt 8 (if weapon exists) — WeaponImpactPoint
> On Boss_[BossName], find the weapon mesh child (e.g., the hammer, staff, etc.). Create an empty child GameObject on it called "WeaponImpactPoint". Position it at the tip/strike point of the weapon. On Boss[Name]Behavior, assign this WeaponImpactPoint GameObject to the weaponImpactPoint field.

### CoPlay Prompt 9 — Save as Prefab
> Drag Boss_[BossName] from the Hierarchy into Assets/_Project/Prefabs/Bosses/ to create a new prefab. Then delete it from the scene.

---

## STEP 10 — Campaign Config
**Owner: Claude Code**

Tell Claude:
> "Add Boss [Name] to wave [N] in DefaultCampaign. Set it as Boss1 or Boss2 for that wave."

Claude will:
- Edit the campaign ScriptableObject or config
- Wire the new EnemyData + prefab references

---

## STEP 11 — Test
**Owner: Brian (Play Mode)**

1. Set `debugStartWave` on WaveSpawner to the wave with the new boss
2. Optionally set testing overrides (tensionCycles=0, bossUnlockThreshold=0, bossEntrancePressure=100) for instant boss spawn
3. Verify:
   - [ ] Boss spawns at correct height (not floating, not underground)
   - [ ] Walk animation plays smoothly during approach
   - [ ] Intimidation pause + attack sequence looks right
   - [ ] Taunt triggers at 50% HP, animation plays, health boost applies
   - [ ] Death strike: player freezes, boss slides to correct offset, hit lands on player
   - [ ] Death animation plays on kill
   - [ ] Boss stays on bridge during all phases
4. **REVERT testing overrides when done**
5. Report any issues to Claude for script fixes

---

## REMINDERS (Lessons Learned)
- **Never use animator.Play()** — always CrossFade for smooth transitions
- **SkinnedMeshRenderer Y offset does nothing** — adjust root transform or spawnYOffset
- **Unpack prefabs completely** — nested prefab cross-references cause "Type Mismatch"
- **Use GameObject fields, not Transform** — avoids prefab drag issues
- **Disable PlayerAnimationDriver** before death/revive animations on the player
- **CoPlay rate limit: 30k tokens/min** — keep prompts under 3 sentences, do simple Inspector work manually
- **Don't re-run rebuild scripts** — once the prefab is built, edit it directly
- **Materials must be standalone .mat assets** — not embedded in scene files
