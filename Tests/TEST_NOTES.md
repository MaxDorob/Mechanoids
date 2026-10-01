# Test notes

Former comment blocks of the `.rwtest` files, moved here verbatim on 2026-10-01 when
comments were dropped from test scripts. One section per test title. The derivations of
bounds and tick budgets live here; the test itself carries only its title and its steps.

## Bladehopp lands on an enemy and hits it

APM_Bladehopp (Mantis) carries CompProperties_AbilityBash: job=APM_BashDamage,
jobFactor=30, moveSpeed=0.3. The Mantis uses it the way a player does: select,
gizmo "Bladehopp", click the Raider. The targeter runs ValidateTarget, the cast
job runs the 0.4 s warmup (24 ticks), and then CompAbilityEffect_Bash.Apply
starts the bash job itself (StartJob): TargetA = the target's cell, TargetB =
the target pawn, maxNumMeleeAttacks = 30.

JobDriver_Bash moves exactPos 0.3 cells per tick toward the target. Each time
it crosses into a new cell it sets Position to that cell - unless the target
cell is within 1.1 of the current Position, then Position = the target cell
and the bash toil runs. From 0,0 to 5,0 (centre 0.5): cell 4 is entered at
tick 12, the crossing into 5 at tick 15 finds cell 4 within 1.1 and puts the
Mantis ON 5,0 - the Raider's own cell. Budget 120 = the 24 warmup ticks plus
the 15 of the dash, three times over.

JobDriver_BashDamage then deals Blunt 30 (the jobFactor) to the things in the
3x3 around the Mantis; TargetB, the Raider, is always hit.

One enemy, nobody else near it: the Mechanitor stands at -4,0, far outside
the Raider's reach and the 3x3. "speed paused" from the Raider's spawn to the
click, so it cannot walk at the Mantis or the Mechanitor and start a fight
whose injuries the pain line would read as the Bladehopp's. The Raider is
spawned after the overseer walk for the same reason.

Bounds: pawns spawn without gear, so all 30 land. Injury pain is 0.0125 per
point: 30 on one part is 0.375; the smallest outer part a hit can pick
(~7-10 HP, destroyed) still leaves >= 0.08; a bone crack under the blunt hit
can add to it. min=0.05 max=0.8. The read follows the landing at once - the
drafted Mantis is adjacent to the Raider afterwards and its own melee would
raise the number.

Draft and overseer: Bladehopp has the default displayGizmoWhileUndrafted off,
so the gizmo shows only on a drafted mech, and a colony mech shows ability
gizmos and is draftable only under a mechanitor. The Mantis is the player's
so its own AI does not cast Bladehopp first and leave a cooldown.

DRAFT: a 30 blunt hit on a bare head can kill. A dead Raider still reads its
injuries, but if the first run is red on that, the report says it.
Probe (2026-09-30): the hit landed, but "Mantis" was not on 5,0 - "the cell
is empty", so the Raider was gone from it too. This reads how far apart the
two ended up, which "assert thing ... at" does not say.

## Blinding laser blinds eyes, not mechs

APM_BlindingLaser is an area cast, not a pawn target: canTargetLocations
true and Ability_EffectRadius 2.4. A click on a mech is taken as its cell, so
the targeter never refuses one - the earlier "target Lifter expect=refused"
could not hold. Ability.Activate picks the affected things with GenRadial
around the clicked cell, filtered by ValidAOEAffectedTarget: the verb's
targetParams (canTargetMechs false drops every mech) and each comp's Valid.
There is no faction filter, so a colonist in the radius is blinded too.

CompAbilityEffect_BlindingLaser.Apply then puts APM_LaserBlind on every
SightSource part - Sight setMax 0.05 for Ability_Duration 50 s - and its own
CanBlindPawn refuses mechanoids a second time (IsMechanoid, added by the mod
author on 2026-08-12, together with canTargetMechs false: mechs are immune
by design).

Geometry around the Target's cell 5,0 (the click on "Target" is taken as
its cell):
  Target 5,0  distance 0    inside 2.4  -> blinded
  Lifter 5,2  distance 2    inside 2.4  -> a mech, untouched
  Far    0,2  distance 5.4  outside     -> untouched
The Tinker (0,0) and the Mechanitor (0,-2) stand 5 and 5.4 away, outside.

Everyone stands in one sealed room without a roof (walls x -1..7, z -3..3,
door=none), and the clock is paused from the first spawn to the click: the
colonists cannot wander out of or into the radius. The Tinker is inside the
room too - the laser needs line of sight (requireLineOfSight defaults to
true) and a wall between caster and target would refuse the click.

The Tinker is used the way a player uses it: a colony Tinker with a mechanitor
overseer (without one vanilla shows a colony mech no ability gizmo), selected,
its gizmo "Blinding Laser" pressed and the Target clicked. Range 11 covers 5 cells.
All pawns are the player's, so nobody starts a fight.

Timing: the 0.6 s warmup is 36 ticks; the waitfor budget of 180 is that plus
slack, and it reports the tick the laser hit at.

## Celerus blinks to a free cell

APM_CelerusBlink carries CompProperties_BlinkTeleport (range 17.9, line of
sight) and CompProperties_BlinkWarmupVisuals (motes while casting, nothing a
script reads). CompAbilityEffect_BlinkTeleport.Apply re-checks
IsValidDestination and then sets caster.Position = target cell in the same
call - no flyer, no delay (stunTicks is not set, 0). The blink goes through
the gizmo "Blink" and a click on the cell, so the 0.15 s warmup (9 ticks) runs
first; the landing is waited for with a budget of 120.

IsValidDestination refuses, in order: out of bounds, beyond 17.9, no line of
sight, a door it cannot pass, an impassable or unwalkable cell, and a cell
another pawn stands on ("AbilityOccupiedCells"). The occupied cell is the
refusal only the comp makes: canTargetPawns=false sends the targeter to the
location, which is in reach (6.7 cells) and in sight. The targeter asks the
comp's Valid, so the refusal is a targeter refusal, not a greyed gizmo. The
refusal comes first - it costs no cooldown, the blink costs 600, and the last
line reads that cooldown as the same gizmo greyed out.

The gizmo shows undrafted (displayGizmoWhileUndrafted, not disabled), but a
colony mech shows no ability gizmo without an overseer, and an undrafted mech
wanders; so: overseer and draft. The Bystander is drafted to hold its cell.

## Command casket admits only a mechanitor

The command casket is entered only through the right-click menu of a colonist
on the casket (Building_MechCommandCasket.GetFloatMenuOptions): "Enter command
casket" when accepted, "Cannot enter command casket: <reason>" greyed when
not - "Occupied", "No power", "Waiting for X.", "X is suffering from
biostarvation.", "Not mechanitor". There is no gizmo for it. "ordermenu"
opens that menu the way a right-click does, and "menu" reads its options.

The plain colonist is the other direction and is asked first on the same,
powered casket, so the refusal cannot be the power talking.

The mechanitor is made by "overseer ... now=true" on a Lifter, which puts a
mechlink in directly. In Biotech the mechlink is no surgery: InstallMechlink
is a JobDef, run when a colonist uses the mechlink item, and no RecipeDef of
that name exists ("surgery ... recipe=InstallMechlink" was red on 2026-09-30
with "expected a known RecipeDef"). Using an item is not a verb yet; the
precondition here is the mechlink, not how it got there.

## Crypto swallow needs an overseer

A player Frostivus uses its commands only while a mechanitor controls it.
What the player meets first is vanilla's gate: Pawn_AbilityTracker.GetGizmos
shows a colony mech's abilities only while IsColonyMechPlayerControlled, and
that needs an overseer. A colony Frostivus without one has no "Crypto Swallow"
gizmo at all, so the first half asserts the gizmo absent. Behind it stands the
mod's own gate: FrostivusFoodPreservationUtility.HasFoodPreservationControl
asks IsColonyMechPlayerControlled, GetOverseer() != null and a control group -
the same gate that closes its two work givers.

The downed colonist is a target the faction rule accepts, so the only gate
left to refuse the first use is control.

The second half links a mechanitor to the Frostivus the player's way
("overseer" walks the Connect-to job), then presses the same gizmo and clicks
the colonist - which is also the control that the gizmo label is right.
Verb_CastAbilityTouch walks the Frostivus to the colonist first, then the 1 s
warmup (60 ticks) and the APM_CryptoSwallow job run; the waitfor budget of 600
is that walk plus the warmup, with slack.

## Crypto swallow takes a downed pawn

The promise: "Engulfs a humanlike pawn or corpse, sealing it within an
internal cryo-chamber. Preserves the target in suspended animation until
released." The refusal text says who may be taken alive: "Frostivus can
swallow any humanlike corpse or downed pawn, but only colonists, prisoners, or
slaves while alive."

CompAbilityEffect_CryptoSwallow.CanSwallowTarget, in order: a humanlike pawn
or corpse, not the caster, a caster that may use its map commands, spawned on
the same map, not already inside, then the faction rule above, then weight.
The Frostivus is used the way a player uses it: a colony Frostivus with a
mechanitor overseer (the control gate, and the only way a colony mech shows
its ability gizmo at all - that half is its own file, "Crypto swallow needs an
overseer"), selected, its gizmo "Crypto Swallow" pressed and the raider
clicked. The click runs the comp's Valid, which is CanSwallowTarget.

So the standing raider must be refused and the downed raider taken; the
second click is also the control that the targeter opened at all. The taken
pawn moves into the Frostivus inventory and carries APM_Hediff_Devoured, which
FrostivusCryoStasis_Patch reads as suspended. The inventory line is the effect
a player can see (the gear tab); the hediff is the marker the patches key on.
"reload" because both the inventory and the hediff are saved state.

Timing: Verb_CastAbilityTouch walks the Frostivus to the raider, then the 1 s
warmup (60 ticks) and the APM_CryptoSwallow job run; the waitfor budget of 600
is that walk plus the warmup, with slack.

## Duel binds both duelists

The promise: "A selected target is chosen for a duel and a shield is
generated around the duelists. For the duration of the duel, ranged attacks,
abilities and incoming damage ..." - both duelists are bound, and they take
more damage while it lasts.

A colonist reaches the duel through the duelist's psyspear: CompAbilityGranter
adds APM_Mech_Duel on equip. This file walks the player's route - the gizmo
"The duel trial" and a click on the raider - instead of "cast", so the
targeter's own validation and the 1 s warmup job run as in play.

APM_Mech_Duel runs its comps in order: GiveMentalState (APM_Duel on the
target, which forces it back on the caster and adds APM_InDuel to both), then
SpawnThingDuelAttached - APM_DuelSpot at the midpoint, a projectile
interceptor of radius 5.9 that destroys itself after 1200 ticks. APM_InDuel
carries IncomingDamageFactor x1.75 and MeleeCooldownFactor x0.7, so a colonist
that reads 1.0 on both reads 1.75 and 0.7.

The raider stands at 4 cells, inside the 8.9 reach. The waitfor budget is the
60 tick warmup plus slack; it reports the tick the duel began at.

## Duel carries on through a hook pull

Commit 58ade69: "A duel carries on while a duelist is pulled by the hook".
DuelPresenceRules.IsPresent is !gone && !downed && (spawned || in a spawned
flyer), compared by MapHeld. Before the fix a duelist inside
APM_PawnFlyer_Hooked was not spawned and the duel ended the frame the pull
began.

Both abilities are used the way a player uses them: select the caster, press
its gizmo, click the target. The duelist's psyspear grants "The duel trial";
the Terminus is a colony mech with a mechanitor overseer (without one vanilla
shows a colony mech no ability gizmo) and is drafted, so it holds its cell and
the only thing that moves the duelist is the hook. APM_HookPawn ("Grappling
Hook") pulls a victim smaller than the caster to the caster, so the colonist
flies.

Timing: the duel's 1 s warmup is 60 ticks, the waitfor budget of 240 is that
plus slack. The hook's 1 s warmup is 60 ticks and the hook projectile (speed
55) crosses the 10 cells in about 18; the flyer's budget of 240 covers both.

The flyer being there is the proof the pull happened - without that line the
final asserts would be green on a hook that never fired. The duel is read
after the flyer is gone, because a pawn inside a flyer is not on the map
and "assert pawn" does not find it there.

The duelist stands drafted and the raider is pacified. Undrafted, with an
armed hostile four cells away, the duelist walked off before the click in five
of eight runs up to 2026-10-01 02:47 ("Target is out of range" at 4 cells
against 8.9, measured from the duelist's own cell); the sister test "Duel binds
both duelists" drafts it and has never been red there. The cell assert in
front of the click says which way it went if it is red again. Pacify keeps
the raider from opening the fight before the duel does; the duel's mental
state forces the fight on both anyway.

DRAFT: between the duel's start and landing the two duelists fight for a few
hundred ticks. If the duelist downs the raider in that time, the duel ends for
a legitimate reason and this file goes red for it. The report of the first run
says which one it was.

## Duel effects keep their colours

A duel starts with the effecter APM_DuelStart (or APM_DuelStart_Boss when the
caster's kind ends in _Boss): the "VS" mote APM_DuelVS and the fire orbs
APM_DuelFireOrbs, and the spot APM_DuelSpot (or _Boss) with its projectile
shield of radius 5.9. The regular set is steel blue-grey (163, 180, 187), the
boss set orange (230, 130, 40).

Two duels side by side, far enough apart that the two shields do not touch:
a Dominus against a raider on the left, a boss Dominus against a raider on
the right. The colour follows the caster's kind, not its faction, so both
Dominus are colony mechs used the way a player uses them: each has its own
mechanitor overseer (a Dominus takes 5 of a mechlink's 6 bandwidth), is
selected, its gizmo "The duel trial" pressed and its raider clicked. The
duelists are raiders, so each duel is between hostiles and the end effects
would be the rewarding ones; this file looks only at the start.

Timing: both 1 s warmups (60 ticks) run side by side; the spots are waited for
with a budget of 240, so the asks begin the moment each duel has started.

"ask", because a colour is no game state. Headless, it reports skip.

## Duel winner takes the reward

MentalState_Duel.PostEnd, for a duel between hostiles: an opponent that is
dead or downed gives the survivor APM_DuelWinner +0.125 (one win is 1/8), and
APM_InDuel comes off both sides, and the spot is destroyed.

DuelPresenceRules ends the duel the moment a duelist stops counting as
present - downed is one of the ways out. The check runs in MentalStateTick,
so one short tick after the "down" is what lets the state notice.

Effect: APM_DuelWinner at 0.125 sits on its first stage (0 to 0.135) -
MoveSpeed +0.10. A trait-free human reads 4.6 bare, 4.7 after.

The duel is started the way a player starts it: select the duelist, press the
psyspear's gizmo "The duel trial", click the raider. The 1 s warmup (60 ticks)
runs, so the start is waited for with a budget of 240; waitfor stops the tick
the duel began, which keeps the fight before the "down" as short as the game
allows. "speed paused" keeps the clock still between the steps.

The other two endings - a timeout draw after 1200 ticks and a friendly duel
that rewards nothing - are not in this file: a draw needs two hostiles who
cannot reach each other while staying in each other's line of sight, which no
scene verb builds today.

## Dynamo turns into a power plant and back

"Power Plant Mode": CompAbilityEffect_ConvertToBuilding spawns
APM_DynamoPoweredGenerator at the Dynamo's cell with the caster's faction and
stores the Dynamo inside it. The building's "Mobile Mode" gizmo destroys it
and drops the Dynamo again, stunned for 60 ticks, with its fuel.

Both directions go through a gizmo, the way a player switches. The Dynamo is a
colony mech with a mechanitor overseer - without one vanilla shows a colony
mech no ability gizmo. The ability has targetRequired false, so pressing
"Power Plant Mode" opens no targeter: Command_Ability.ProcessInput queues the
casting job on the Dynamo itself. Its 5 s warmup is 300 ticks; the waitfor
budget of 480 is that plus slack.

While it is a building the pawn is off the map, so "assert pawn" cannot find
it; the building is the proof. After "Mobile Mode" the pawn is found again and
the building is gone - both directions of the same switch.

The reload sits between the two because the pawn lives in the building's
saved innerContainer: a lost ExposeData would drop the Dynamo for good, and
only a reload shows that.

Power output is not asserted: a freshly spawned Dynamo carries no uranium, and
the generator runs on the fuel it brings along.

## Every apex mech kind spawns armed with its own weapon

A mech gets its gun from the pawn generator, not from any C# of the mod:
PawnWeaponGenerator picks a ThingDef whose weaponTags share a tag with the
PawnKindDef's weaponTags and whose price fits weaponMoney. Every weapon
ThingDef here carries exactly one APM_* tag, and every armed kind writes
weaponTags Inherit="False" with exactly one tag, so each kind has exactly
one legal weapon. A renamed tag on either side leaves the mech unarmed with no
error in the log - that is what this test catches. No C# comp or
DefModExtension hands out equipment (no AddEquipment / PawnWeaponGenerator
call in Source), so the tags are the whole wiring.

kind (tag)                                       -> weapon
  Celerus  (APM_WeaponCelerusVoltana)            -> APM_Weapon_CelerusVoltana
  Celerus_Boss  (APM_WeaponCelerusApexVoltana)   -> APM_Weapon_CelerusApexVoltana
  Dominus  (APM_WeaponDominusMechlance)          -> APM_Weapon_DominusMechlance
  Dominus_Boss  (APM_WeaponDominusApexMechlance) -> APM_Weapon_DominusApexMechlance
  Terminus (APM_WeaponTerminusExoblade)          -> APM_Weapon_TerminusExoblade
  Terminus_Boss (APM_WeaponTerminusApexExoblade) -> APM_Weapon_TerminusApexExoblade
  Conqueror_AR/DMR/SMG (APM_WeaponConqueror*)    -> APM_Gun_Conqueror_AR/DMR/SMG
  Dynamo   (APM_WeaponEMPImpactor)               -> APM_Gun_EMP_Impactor
  Gazer    (APM_WeaponGazerLaser)                -> APM_Gazer_Laser
  Haze     (APM_HazeGraserTag)                   -> APM_Haze_BeamGun
  Javelin  (APM_WeaponJavelinRocketLauncher)     -> APM_Gun_JavelinRocketLauncher
  Lasher   (APM_MechanoidWhip)                   -> APM_Weapon_MechWhip
  Maul     (APM_WeaponMaul)                      -> APM_Gun_Maul
  Pulse    (APM_WeaponPulseShotgun)              -> APM_Gun_Pulse_Shotgun
  Ravager  (APM_WeaponRavagerArtillery)          -> APM_Ravager_Artillery
  Satellite (APM_WeaponSatellite)                -> APM_Gun_Satellite
  Siren    (APM_WeaponSirenChargeEmitter)        -> APM_Siren_ChargeEmitter
  Vassal   (APM_WeaponVassalSwordRifle)          -> APM_Gun_Vassal_SwordRifle
The boss kinds (races DominusB/TerminusB/CelerusB) get the Apex variant, and
asserting both kinds pins that the boss does not fall back to the regular one.

Left out: Aegis, Frostivus, Ingestor, Mantis, Tinker. Their kinds write no
weaponTags (Aegis an empty Inherit="False" list) and their races fight with
body tools only (shields, legs, fists, head), so they carry no equipment by
design. The language has no "holds nothing" form, and "equipment=<some def>
expect=absent" would be green for any unrelated def - a line that cannot go
red. There is no APM_Mech_Duel kind: Duel is an ability (APM_Mech_Duel).

rolled=true is the whole point: without it "spawn" strips the weapon the
generator rolled (GeneratedGear.StripFrom). No faction= means
kind.defaultFactionDef, i.e. hostile mechanoids; they would start shooting at
once, so the scene is paused before the first spawn and nothing ticks.

## Flash and beam effects look right

The rest of the ability pictures that no state can carry:
  APM_PulseWave     - the flash APM_PulseWaveBlindFlash, warm white
                      (255, 252, 240), and an expanding wave from the Pulse
  APM_BlindingLaser - the beam mote APM_Mote_BlindingLaser from Tinker to target
  APM_Shockwave     - the Gazer's charge mote, then APM_ShockwaveFlash
                      (255, 245, 220) and the travelling blast wave; pawns in
                      it are thrown by APM_PawnFlyer_ShockwaveThrown
  APM_Absorb        - owned by the Ingestor work test, not drawn here

The state half of the first two (blindness) is owned by "Pulse wave blinds
flesh in its radius" and "Blinding laser blinds eyes, not mechs"; this file
only asks for the picture.

Every ability is used the way a player uses it: select the mech, press its
gizmo. The three mechs are colony mechs with a mechanitor overseer - without
one vanilla shows a colony mech no ability gizmo. "Mechanitor" oversees Pulse
and Tinker (2 + 2 of a mechlink's 6 bandwidth), "Gazer mechanitor" the Gazer
(6). Both stand outside the pulse wave's 6.9 and the shockwave's 8.9 cells;
the shockwave throws every non-mech pawn in reach whoever owns the Gazer.
"pulse wave" and "Shockwave" have targetRequired false, so their gizmo opens
no targeter and casts on the mech itself; "Blinding Laser" opens one and the
colonist is clicked.

Timing: the gizmo route runs the warmup, so each ask plays the warmup ticks on
top of the ticks it played after the old instant cast - pulse wave 1.2 s (72)
+ 60 = 132, blinding laser 0.6 s (36) + 30 = 66, shockwave 3 s (180) + 180 =
360. The shockwave's charge mote is part of its warmup, so its claim now shows
on screen as written.

"ask". Headless, every line is a skip.

## Frostivus rescues loose food

The Frostivus preserves food: work type APM_FrostivusFoodPreservation, giver
APM_FrostivusRescueFood, job APM_FrostivusTakeFoodToInventory. A candidate is
haulable, nutritious, rottable and Fresh, not already in cold storage, and -
unforced - lying outside any storage. The job waits 120 ticks and moves the
stack into the inventory, where CompRottable_Patch stops it rotting.

FrostivusFoodPreservationUtility.HasFoodPreservationControl closes the giver
for a colony Frostivus without an overseer - the first half pins that. The
second half needs a mechanitor overseeing the Frostivus, built with the
"overseer" verb.

A simple meal lying loose on the ground is the plainest candidate: fresh,
rottable, not in storage. The mechanitor is fed full at the spawn: a fresh
colonist can spawn hungry and would eat the meal before the Frostivus gets it.

The meal and the Frostivus sit in a sealed room (door=none) so no pet or other
pawn on the map can walk in and eat it. Forbidding the meal is not an option:
the unforced giver skips forbidden food (FrostivusFoodPreservationUtility
:179) and the job fails on it (JobDriver_FrostivusFoodPreservation :25). The
room has no roof and no stockpile, so it is neither cold nor storage, and the
control check needs no range - the mechanitor can stay outside.

## Frostivus unload command needs food and an overseer

CompFrostivusFoodPreservation gives a colony Frostivus the command "Unload
preserved food" only while it carries food, and draws it greyed with
"Frostivus cannot use preservation commands right now." when
FrostivusFoodPreservationUtility.CanUseFrostivusMapCommand says no - which
for a colony Frostivus means it has no overseer.

So the three states a player can meet are three lines here:
  no food           -> not offered at all
  food, no overseer -> offered, greyed, with that reason word for word
  food, overseer    -> offered and usable
The positive half needs the overseer link the kit cannot build yet.

## Gazer emplacement names why it cannot fire

Building_GazerEmplacement.GetGizmos gives a colony emplacement one command,
labelled from its beam verb or, failing that, "Sun Ray"
(APM.GazerEmplacement.SunRay.Label). CanUseVerb refuses in this order:
"Not spawned.", "Sun Ray definition is missing.", "Broken down.",
"Turned off.", "No power.", "Needs an operator.".

Two of those are reachable by a scene: no power, then power and nobody at the
controls. Each is pinned word for word, in that order, so the second line also
proves the power check moved on. The inspect line "Status: Ready" says the
charge and cooldown machine is idle.

The usable direction needs a colonist manning it (CompMannable), which no
scene verb arranges; it stays an open row.

If the command's label turns out to come from the verb and not from the key,
the first line reports the gizmos that were offered - fix the label here.

## Gazer sun ray burns along its line

APM_SunRay's verb is Verb_ShootSunBeamAbility, and the damage is in the verb,
not in the ability: WarmupComplete lays a Bresenham path from the Gazer
through the target cell out to the full range 30.9 (beamTargetsGround, so a
pawn target is read as its cell), then every burst shot, 14 ticks apart,
HitCell()s the next cell of it outward from minRange 3.9 - each cell within
radius 2 of that point hands APM_SunRay 38 (AP 1.8, a Beam -> BeamWound) to
one random thing standing there, and sets it alight (beamChanceToAttachFire
1). The shots come from the verb, so the ray is used the way a player uses
it: select the Gazer, press the gizmo "Sun Ray", click the target. The cast
job then runs warmup and burst as in play.

Scene: Gazer at 0,0; "Target", a drafted colonist, at 12,0; "Behind", a
drafted colonist, at -8,0 - on the extension of the line but on the wrong
side of the Gazer, which the beam never sweeps.
Effect: one hit is 38 x BeamWound painPerSeverity 0.0125 = 0.475 pain, and
the fire only adds; min=0.2 leaves room for a hit that burns a part off.

Refused direction: a cell 2 away is inside minRange 3.9, so Verb.CanHitTarget
says no and Verb_CastAbility.ValidateTarget keeps the targeter open - a
targeter refusal, clicked as "target cell=2,0 expect=refused". The accepted
click on "Target" with the same gizmo is its control.

Budget: warmup 9.5 s = 570 ticks, then the beam reaches 12 cells after
(12 - 4) x 14 = 112 ticks: 682. The whole sweep is 570 + 27 x 14 = 948;
"tick 1250" covers it with a quarter to spare, so "Behind" and the cooldown
are read after the sweep has ended.

The hit on "Target" is read as a hit, not as pain. Each shot strikes every cell
within radius 2 of its point, so a pawn on the line sits under several shots
of 38 - enough to kill it. "waitfor pawn Target pain min=0.2" ended on
"Target is not on the map" in the runs of 2026-09-27 00:17 and 2026-10-01
02:47 (dead before the poll read it alive) and on 0 in 2026-09-30 01:20 (each
hit goes to one random thing on the cell, grass included). "assert hit" counts
the Gazer's hits from the battle log and holds on a corpse; until the kit has
it, "requires assert=hit" reports it as skip. What runs today: the ray was
fired (the gizmo is on cooldown) and "Behind" is spared.

The Gazer costs 6 bandwidth, the whole of a fresh mechlink. Its auto-laser
toggle defaults to on, so GazerLaserUtility does not block the ability.

## Ingestor recycles a corpse into chemfuel

The way a player gets this: the Ingestor's work type
APM_IngestorCorpseProcessing, giver APM_IngestorProcessCorpses. The giver
(WorkGivers/WorkGiver_IngestorCorpseProcessing.cs) needs a player Ingestor
that is spawned, awake, can move and manipulate - no overseer - and hands out
the APM_Absorb ability as a job on a fresh flesh corpse. Default mode is
"all valid corpses", so an unmarked corpse qualifies.

CompAbilityEffect_Absorb destroys the corpse and adds a batch to
APM_AbsorbedThing: floor(bodySize x 36) chemfuel, due after the
durationFromSize curve - 36 chemfuel after 30000 ticks for a human. The batch
is not waited out here: half a day of ticks for a spawn of chemfuel belongs to
its own test once the batch timer is readable; the hediff's own label names
it ("36 chemfuel in ...").

The ability cooldown is 20000 ticks, so right after one corpse the giver must
not hand out a second job - the second corpse is the other direction, and it
is only spawned once the first job has finished.

Job budget: walk plus a 5 s (300 tick) touch warmup; 2000 ticks is ample.

The job is run "until=ended", not "until=done": JobDriver_Absorb is a
JobDriver_CastAbilityMelee, which ends Incompletable after a cast that worked,
because the corpse it used up is gone. "done" wants Succeeded and was red in
every run up to 2026-09-30. What the job did is read on the next two lines.

## Ingestor settings steer its work

CompIngestorAbsorbSettings gives a colony Ingestor two toggles, each labelled
with what pressing it will do:
  strip:   "Enable stripping"         <-> "Disable stripping"
  process: "Use marked corpses only"  <-> "Recycle all valid corpses"
Both start off (strip nothing, recycle every valid corpse) and both are saved.

The process toggle has an effect this file can read: in marked-only mode the
giver APM_IngestorProcessCorpses offers nothing for an unmarked corpse. The
same corpse is offered first, in the default mode, so the "absent" line after
the toggle cannot be green because the giver never works.

The strip toggle's effect - apparel dropped before the corpse is destroyed -
needs the job to run; that is owned by "Ingestor recycles a corpse into
chemfuel" in its default mode, and a stripped run is an open row.

## Javelin fires from behind its own cover

Verb_ShootJavelin.CanHitTargetFrom (Verbs/Verb_ShootJavelin.cs:124) only
widens what vanilla allows. A line vanilla refuses is let through when all of:
  1. every cell blocking the line of sight lies within maxBlockerTiles = 6 of
     the launcher (LineBlockedOnlyByOwnCover -> JavelinIndirectFire),
  2. the simulated curve ends within hitRadius 0.5 of the target
     (MissileReaches), and
  3. no Fillage.Full thing sits on the curve at 5 tiles
     (solidObstacleArmTiles) or more from the launch (MissilePathClearOfSolids).
Vanilla Verb.TryStartCastOn checks the line a second time before the warmup,
through the non-virtual TryFindShootLineFromTo; Verb_ShootJavelin.TryStartCastOn
starts the warmup itself when that check is the only refusal (FINDINGS B35).
Before that fix the order was taken but no missile ever left the tube.
The missile leaves on the cardinal facing the target, flies 22 boost ticks
straight, then turns at 2 deg/tick; speed 15 = 0.15 tiles/tick.

Scene, east lane: Javelin at 0,0, five walls at x=3 (z -2..2), a drafted
colonist "Dummy A" at 20,0. The wall is 3 cells out - not adjacent, so vanilla
gets no lean-out cell and refuses the line; blocker distance 3 <= 6 passes
rule 1; the straight eastward curve passes rule 2; the wall is crossed at
2.5..3.5 tiles, inside the 5 tile arm distance, so rule 3 passes too and the
projectile ignores it on the same rule (JavelinObstacleRules.Blocks).
The dummy is drafted so it stands still: evasion (evasionMaxChance 0.5)
rolls only for a target moving into the missile.

What the hit leaves: ResolveAndAdvanceStack puts APM_Hediff_JavelinMissileLock
on the first pawn a basic rocket lands on (the marker), and the direct hit is
APM_JavelinWarhead 20 (Bomb child, fragmentation off) - the effect, read as
pain. Blast damage is 0, so nothing else in the scene hurts the dummy. The
player javelin has 0 uranium and fires the free basic rocket.
The injury is not named: Bomb leaves Shredded on soft tissue and its
hediffSolid, Crack, on a bone. The run of 2026-10-01 02:47 hit a bone and read
"APM_Hediff_JavelinMissileLock, Crack" against a "hediff=Shredded" line, so
that line read a real hit as a miss. Pain covers both: 20 x Shredded 0.0125 =
0.25, 20 x Crack 0.01 = 0.2; min=0.1 leaves room for a hit that takes off a
small part instead.

Budget: warmup 1.9 s = 114 ticks, flight 19.5 tiles / 0.15 = 130 ticks,
244 in all; 600 leaves room for the job start and the turn to face.

The refused direction, north lane: walls at z=10 (x -2..2), "Dummy B" at
0,20. The blocker is 10 cells out, > 6, so rule 1 refuses: terrain around the
target, not the launcher's cover. "Dummy B stays unhurt" alone would be green
even if the rule broke, because a missile allowed into that wall detonates on
it (rule 3 / the projectile) - so the claim there is "no missile left the
tube", read with "assert shots".

A player mech takes orders only with an overseer in range (24.9), so the
mechanitor stands behind it; now=true because the link is a precondition here.
The mechanitor is drafted so it stays there: undrafted it wanders off and the
Javelin drops out of command range before the orders arrive.
Probe (2026-09-30): with Vehicle Map Framework loaded, which transpiles
Projectile.Launch and patches Projectile.CanHit, the lock never arrived.
This line splits "nothing left the barrel" from "the missile flew and missed".

## Maul fires longer bursts when damaged

Verb_ShootMultiple (Verbs/Verb_ShootMultiple.cs) is the Maul blaster's
overclock: with k = (int)((1 - SummaryHealthPercent) * 10),
  ShotsPerBurst = burstShotCount 8 + k, read once when the warmup completes;
  every TryCastShot then launches up to k extra projectiles at random cells
  within 3 of the target, breaking out of that loop on a 50% roll before each.
So one burst launches between 8 + k and (8 + k) * (1 + k) projectiles.

Two Mauls in two lanes 10 cells apart, each forced onto its own wall 12 cells
out (the wall stands still and survives the burst: at most 12 aimed bullets
of 12 damage). Each Maul has its own mechanitor - bandwidth 4 each, a
mechlink gives 6.
  "Fresh": untouched, SummaryHealthPercent 1, k = 0 -> exactly 8.
  "Worn":  129 Blunt on its core part (MechanicalThorax 40 HP x healthScale
           3.82 = 152.8, so the part holds). Vanilla takes an injury's
           Severity / (75 x HealthScale) = 129 / 286.5 = 0.450 off summary
           health -> H = 0.550, k = 4 -> 12 shots, 12..60 projectiles.
           129 sits in the middle of the k = 4 band (114.6..143.2), so a
           rounding in the health sum cannot move k. The Crack line pins
           that the damage landed as an injury.
The damage uses the kit's full penetration, so the Maul's own armour
(Blunt 0.30) does not shave it.

Budget: warmup 1.2 s = 72 ticks, plus (shots - 1) x 4 ticks between shots:
28 for 8, 44 for 12 -> the burst ends by tick 116. RangedWeapon_Cooldown
5.8 s = 348 ticks, so the next burst cannot start before 464. tick 250 sees
exactly one burst in either lane.

The bursts are counted with "assert shots <pawn>": projectiles launched by
that pawn since its last "attack" step.

## Mech whip lashes every foe in its arc

Verb_Whip.TryCastShot (Verbs/Verb_Whip.cs:136) does no pull and no knockback.
What it does beyond one hit is a sweep: Cast() hits the aimed-at thing with
the full tool damage, then walks AffectedCells - a wedge WhipWidth 3.6 wide at
its end, from the Lasher out to (distance to target + 0.5), with line of
sight - and hits every thing in it whose faction is null or hostile to the
caster, at damageFactor 0.5 (0.1 on a corpse or downed pawn). Same-faction
and non-hostile pawns in the wedge are skipped.

Numbers: tool "edge" power 14, APM_Whip (CutBase) -> Cut. Full hit 14, swept
hit 7. Cut painPerSeverity 0.0125, so pain 0.175 on the target and 0.0875 on
a swept foe (unarmoured spawned colonists, no traits). The 30% EMP extra
damage does nothing to flesh (EMP harmsHealth false).

Geometry, Lasher at 0,0 facing the target at 4,0: reach 4.5, wedge half-angle
asin(1.8 / sqrt(16 + 1.8^2)) = 24.2 deg.
  Target     4,0   aimed at                       -> full hit
  Flank      4,1   14.0 deg, 4.12 cells, hostile  -> half hit
  Ally       4,-1  in the wedge, player faction   -> spared
  Far        6,0   inside verb range 6.9 but past the 4.5 reach -> spared
  Behind    -4,0   outside the wedge              -> spared
Pain bounds: Target min=0.05 (a 14 point cut on a small part can take the
part off instead); Flank min=0.04, below the lowest swept hit (0.07 at awful
quality) for the same reason; the spared three read 0.

Warmup 0, so the lash lands a tick or two after the order. The scene is
paused while it is built so nobody walks out of the wedge.
The Lasher needs an overseer to be drafted (bandwidth 2).
Target, Flank, Far and Behind have no faction: as enemies they fought the
drafted Ally and the Lasher from the first tick, and every pain line measured
that fight. Verb_Whip hits "t.Faction == null || t.HostileTo(caster)" (:180),
so factionless pawns are hit exactly like foes and start no fight.

One lash, on a paused clock. The file used "speed x1" and a waitfor after the
attack; with the clock running the Lasher lashed again every 2.6 s while the
asserts polled, and Flank read 0.1215 (2026-09-30 01:20) and 0.1581
(2026-10-01 02:47) against max 0.12. "tick 10" plays the first lash and
stops the clock again, long before the 156 tick cooldown runs out.

Flank has no upper bound any more. The damage goes through
AdjustedBaseMeleeDamageAmount, so it moves with the whip's quality
(MeleeWeapon_DamageMultiplier 0.8 awful to 1.65 legendary), and the kit makes
the whip with whatever quality it gets: a swept hit is 5.6 to 11.55 (pain 0.07
to 0.144), a full hit 11.2 to 23.1 (0.14 to 0.289). The ranges overlap, so no
bound tells "half" from "full" until the quality can be pinned. That is the
open "quality" verb in RimDevKit's WHAT_TO_TEST.md; the claim left here is
that the swept foe is hit and the spared three are not.

## Mechamancer gear raises bandwidth

The promise of the three pieces is bandwidth: the suit "dramatically
amplifies a mechanitor's bandwidth", the pack "increases a mechanitor's
maximum total bandwidth and control groups". equippedStatOffsets:
  suit   MechBandwidth +24, MechControlGroups +2, PainShockThreshold +0.05
  helmet MechBandwidth +14
  pack   MechBandwidth +18, MechControlGroups +1
(VacuumResistance only with Odyssey, left out.)

MechBandwidth is a MechanitorStatBase stat: its base is 0, and the 6 come
from the mechlink. A plain colonist reads 0 (red in every run up to
2026-09-30), so the colonist is made a mechanitor first: "overseer" on a
Lifter puts a mechlink in directly. Overseeing the Lifter spends bandwidth but
does not move the stat, which is the total.

With the mechlink the reads climb 6 -> 30 -> 44 -> 62 - one
bound per piece, each read after the piece went on, so a lost offset names
its piece. PainShockThreshold is the anchor WHAT_TO_TEST.md recommends: 0.8
for a trait-free human, 0.85 in the suit, with no parts and no factors to
muddy it.

MechControlGroups is not bounded: its base differs between a mechanitor and a
plain colonist, and a bound here would pin vanilla's number instead of the
offset. The +2 and +1 are an open row until a test starts from a real
mechanitor (see "RimDevKit needed", verb overseer).

"wear ... now=true" skips the walk: the claim is the offset, not the order,
and every walked wear shifts the tick every bound below is read at.

## Mechamancer gear renders

Commit e6006fe renamed every Mechamancer texture (MechJuggernaut* ->
Mechamancer*) and added masks. All three pieces draw with shaderType
CutoutComplex and useWornGraphicMask, so the "_m" texture decides which part
of the piece takes the apparel colour; the colorGenerator picks one of four
greys. A texture or mask that the rename missed shows as a pink square, an
invisible piece, or a piece tinted over its whole surface.

The suit ships Fat, Female, Hulk, Male and Thin bodies in north, south and
east (west mirrors east). The helmet and the pack have one set. The pack is
drawn as a utility pack with its own per-body offsets in east and west.

Four pawns, one per facing, all in the full set. "speed paused" before the
first spawn keeps them from walking or turning before the asks; paused after
the spawns, they had already moved. "wear ... now=true" puts the pieces on without
walking, so the facing given at the spawn survives.

"ask": nothing here is state. Headless, every line is a skip.

## Mechamancer pack deploys satellites

The promise: "When fueled with steel, it can construct small mechanoids that
operate independently without consuming bandwidth. Each mechanitor can deploy
up to two satellite mechanoids simultaneously." (The def actually loads
Plasteel, 30 per charge, 2 charges - the description says steel. That is a
text finding, not something this file pins.)

Verb_SpawnMech (Verbs/Verb_SpawnMech.cs): a self-cast, non-targetable verb on
a CompApparelReloadable with 2 charges. Each use spends a charge and spawns
APM_Mech_Satellite of the wearer's faction near the wearer. A new pack starts
full.

The gizmo label is the verb label "summon satellite" as written in the def -
a reloadable's command carries the verb's label, not a capitalised one.
If the first run reports the gizmo as not offered, the label is the first
suspect, and the report lists the gizmos that were there.

After two uses the charges are gone, so the third press must be refused - the
other direction, in the same test, after the positive half proved the label.
Warmup 1 s = 60 ticks; 120 ticks per use leaves room for the cast job.

## Mechamancer pack sits on a child

Children may wear the pack: the def sets developmentalStageFilter to
"Child, Adult". Without it vanilla falls back to adults only, and
Pawn_ApparelTracker.Wear refuses a child with the warning "<pawn> tried to
wear APM_Apparel_MechamancerPack but is not allowed to" - the pack is then
not worn, so the apparel asserts fail and "assert log clean" goes red.

Patch_MechamancerPackChildOffset shifts the pack's east and west draw offset
by 0.10 for the Child body type, because the adult offsets (-0.40 / +0.40 with
per-body corrections) put the pack beside a child's narrow back instead of on
it. North and south are not patched; they use the def's (0,0.05) at 0.7 scale
on every body.

A child in each facing proves the pack is worn and drawn from every side; an
adult beside the east and west child is the reference for the patched views.

The apparel asserts are state and run headless. "ask" is the picture;
headless, those lines are skips.

## Mechs wear their faction and boss finish

Every Apex mech body is drawn with shaderType CutoutWithOverlay: a base
texture plus a mask the game tints - for a colony mech with the colony's mech
colour, for a hostile one with the default. A mask that is missing, renamed
or saved in the wrong channel shows as a pink or magenta patch, or as a mech
that is tinted all over. The three lords also ship boss races
(APM_Mech_DominusB, _TerminusB, _CelerusB) with their own textures.

rolled=true keeps what the generator gave each mech - including the built-in
weapon, which "spawn" otherwise strips, and which is part of the silhouette.

Row 1 (z=4): the same kinds as colony mechs; row 2 (z=-4): hostile. The
three lords get a third row with their boss kinds. Every mech faces south, the
view the colony bar and most screenshots show.

"ask": the tint is no state. Headless, every line is a skip.

## Press the attack drives nearby apex mechs

The promise: "The Dominus emits a psychic pulse that influences nearby allies
to ignore pain, move faster and stay in high spirits."

APM_PressTheAttack reuses the SteelDiscipline comp: radius 14.9, hediff
APM_Hediff_PressTheAttack for 48000 ticks, and apexMechsOnly left at its
default true - only kinds whose defName starts with APM_Mech_ are touched.
The stage: MoveSpeed +0.40, Consciousness +0.10, painFactor 0.7, and x0.85 on
melee, aiming and ranged cooldown.

The Pulse writes MoveSpeed 4.3, so it reads 4.7 afterwards; Consciousness goes
from 1.0 to 1.1.

The Dominus is used the way a player uses it: a colony Dominus with a
mechanitor overseer (without one vanilla shows a colony mech no ability
gizmo), drafted because the def leaves displayGizmoWhileUndrafted off, then
its gizmo "Press The Attack" pressed. targetRequired is false, so the gizmo
opens no targeter and queues the cast on the Dominus itself. The comp buffs
the caster's own faction, so every mech here is a colony mech. The 1 s warmup
is 60 ticks; the waitfor budget of 180 is that plus slack.

Two pawns it must skip: "Far", an apex mech 20 cells out, and "Scyther", a
vanilla ally in range - the apexMechsOnly half. Whether colonists next to a
colony Dominus count as "nearby allies" is NOT asserted: the code says no, the
description reads like yes, and that is a decision for the maintainers, not
something a test should pick.

The caster's kind picks the variant: a kind whose defName ends in _Boss hands
out APM_Hediff_PressTheAttack_Boss, every other kind APM_Hediff_PressTheAttack
(CompAbilityEffect_SteelDiscipline.cs:43-44). Both stages share the parent
def, so the stat lines above own the effect of both; the boss pair here owns
only which variant lands. The caster buffs itself too. "Dominus boss" stands
20 cells south with its own mechanitor (5 of a mechlink's 6 bandwidth each),
and its Pulse is 20.6 cells from the regular Dominus, so neither cast reaches
the other pair. "Regular and boss effects keep their colours" uses the same
variants for its halo question and leaves this rule to this file.

## Pulse longjump lands short of the vanilla minimum

APM_PulseLongjump inherits Biotech's LongJumpMech (range 9.9, warmup 0.5 s =
30 ticks, line of sight) and changes two things: minRange 0 instead of 5.9,
and its own job APM_CastPulseJump (JobDriver_CastJump, but not interruptible
by the player, by damage or casually - the "protected" jump). The verb is
Verb_CastPulseLongjump : Verb_CastAbilityJump, so the landing is the vanilla
PawnFlyer. The jump is used the way a player uses it: select the Pulse,
press the gizmo "Mech longjump" (label from DefInjected), click the cell.

Claim 1: the Pulse jumps to a cell 4 away and stands there afterwards. 4 is
below vanilla's 5.9, so this also pins the minRange override - with the parent
value the click itself would be refused.
Claim 2 (refused): 14 cells is beyond 9.9. Verb_CastAbilityJump.ValidateTarget
asks CanHitTarget first, so the targeter turns the click down and stays open -
a targeter refusal, not a greyed gizmo - and the Pulse stays where it is.

Budget: 30 ticks of warmup plus a 4 cell flight (the vanilla flyer takes well
under a second for that), 300 ticks for both. waitfor stops at the landing.

Verb_CastPulseLongjump.OrderForceTarget, which the targeter's click calls,
snaps the order to the best free cell near the click
(RCellFinder.BestOrderedGotoDestNear), so the target cell is left empty and
walkable here and the snap keeps it.

## Pulse wave blinds flesh in its radius

The promise: "stunning and flash-blinding living pawns caught in the wave".
CompAbilityEffect_PulseWave takes every flesh pawn outside the Mechanoid
faction within 6.9 cells - friend or foe - and hangs APM_PulseWaveBlind on it
(Sight setMax 0.2, 1920 ticks) next to a stun.

The Pulse is used the way a player uses it: a colony Pulse with a mechanitor
overseer (without one vanilla shows a colony mech no ability gizmo), selected
and its gizmo "Pulse wave" pressed. targetRequired is false, so the gizmo
opens no targeter and queues the cast on the Pulse itself. The mechanitor
stands drafted 11.7 cells out, past the wave, since friend or foe counts.

The wave expands, so the hediff is waited for rather than read at once; the
300 tick budget holds the 1.2 s warmup (72 ticks) and is still generous for a
wave that reaches 6.9 cells, and waitfor reports how much of it was used.
"Far" at 10 cells and the player mech "Tin" at 3 cells are the two pawns it
must skip: one out of range, one not flesh. They are read after "Near" went
blind and after another 120 ticks, so the wave had every chance to reach them.

Sight is the effect behind the marker: the bare read pins full sight first.

Everybody stands drafted. In seven of nine runs up to 2026-10-01 02:47 "Near"
had no hediff at all after 300 ticks; in the two green runs the blind landed
at 64 and 84 ticks. An undrafted colonist covers about 5.5 cells in the
72 tick warmup (4.6 cells/s x 1.2 s), so a "Near" that starts 3 cells out can
be past 6.9 before the wave starts. The cell assert before the gizmo says
which way it went if it is red again.

## Purifier mode switches and survives a reload

CompToxicPurifier offers one gizmo, "Mode: <mode>", that opens a float menu of
the four modes: "Wastepacks over ground", "Ground over wastepacks" (the
default), "Wastepacks only", "Ground only". The inspect line reads the
wastepack store against its capacity of 15. Both "mode" and
"wastepacksCount" are saved in PostExposeData.

The gizmo label carries the mode, so it is also the read-back: after picking
"Wastepacks only" the old label must be gone and the new one pressable, and
both must hold after a reload. Pressing the new label opens the menu again;
picking the same mode closes it without changing anything.

The mode's effect on hauling (WorkGiver_HaulToToxicPurifier refuses ground-
first modes while cells are polluted) needs polluted ground and wastepacks on
the map, and is not in this file.

## Regular and boss effects keep their colours

The mod has one colour rule for its three lords, and it is written into the
defs rather than into code: every regular effect is tinted steel blue-grey
(163, 180, 187), and the boss effects of the same ability are tinted
  Dominus boss  orange (230, 130, 40)  - press the attack halo and wings
  Terminus boss gold   (229, 211, 127) - severing edge, overdrive wave
  Celerus boss  purple (0.40, 0.17, 0.58) smoke, grenade (160, 60, 230)
The regular Celerus smoke is (0.63, 0.71, 0.75), the same blue-grey.

Nothing in the game state carries a colour, so every line below is an "ask":
a person looks and answers. Headless, each one reports skip, never pass.
Each pair stands side by side, regular on the left, boss on the right, so the
question is always "do the two differ the way the rule says".

Every ability is used the way a player uses it: select the mech, press its
gizmo, click the cell where the ability wants one. Each Dominus, Terminus and
Celerus is a colony mech with its own mechanitor overseer (5, 5 and 4 of a
mechlink's 6 bandwidth) - without one vanilla shows a colony mech no ability
gizmo. The Dominus and Terminus are drafted because press the attack,
severing edge and overdrive leave displayGizmoWhileUndrafted off. "Severing
edge" and "Smokescreen" open the targeter and the cell is clicked (target
cell=, relative to the map centre like every coordinate here); "Press The
Attack" and "Overdrive" have targetRequired false and cast on the mech itself.

Press the attack: the Pulse mechs do not have the ability, only the Dominus
does. The Dominus buffs every APM_Mech_ kind of its own faction within 14.9
cells, itself included, and the caster's kind picks the variant: a _Boss
Dominus hands out APM_Hediff_PressTheAttack_Boss
(CompAbilityEffect_SteelDiscipline.cs:43-44). So each Dominus stands next to
its own Pulse, and the two pairs are 21 cells apart so that neither cast
reaches the other pair. The hediff lines are preconditions only: they make
sure each of the four wears the variant the ask looks at. Which variant a
caster hands out, and that it never hands out the other one, is owned by
"Press the attack drives nearby apex mechs".

The gizmo route runs the warmup, so the warmup motes (the severing edge intro,
the overdrive pillar) are on screen now as well, and each ask plays the
warmup ticks on top of what it played after the old instant cast: severing
edge 0.3 s (18) + 45 = 63, overdrive 1.1 s (66) + 90 = 156, smokescreen 1 s
(60) + 240 = 300. Press the attack has a 1 s (60) warmup; its hediffs are
waited for with a budget of 180.

Both Celerus stand drafted, like the two Terminus. In the run of 2026-10-01
02:47 the boss's smokescreen click on 8,10 was refused as out of range - 4
cells from where the boss was spawned, against a range of 20 - and the report
found "Celerus regular mechanitor" under the mouse there. Undrafted, either
pawn can have moved. The two cell asserts before the second click say which
one did if it is red again.

Drafting alone was not enough: in the runs of 2026-10-01 03:57 and 14:55 the
drafted boss still stood 1.4 and 3.2 cells off 8,6. The run plays at x1
between steps, and until its overseer and its draft land the Celerus is an
uncontrolled mech whose think tree ends in a wander (ThinkTrees_Celerus.xml);
at MoveSpeed 4.7 a few dozen ticks are enough. So the Celerus block is set up
under "speed paused" and the clock runs again only once both stand drafted.

## Repair station takes in and heals a mech

Building_RepairStation: the gizmo "Insert mech..." opens a float menu of every
colony mech the station accepts (CanAcceptPawn: a mechanoid, not a Satellite,
friendly, body size 0-2 for the small station, not occupied, powered, not at
full health), or the single dead entry "No mechs available.". Picking a mech
sends it in; the station repairs 1.5 HP per second (small station) and ejects
it when done. While it works, the inspect string reads "Repairing <mech>".

Power comes from a solar generator in an unroofed yard under "sky glow=1": at
full glow it gives 1700 W, far over the 500 W the station draws while
repairing. "waitfor power" on the station is the precondition: it waits
until CompPowerTrader.PowerOn, and a station that never gets power makes the
file red on that line, with the net's balance, not further down.

The damage: Blunt on a Lifter's core part lands as Crack. 10 points at 1.5 HP/s
is about 400 ticks of repair; the budget adds the walk in and leaves a wide
margin, and waitfor reports how much it used.

The other direction closes the file: the same mech, now at full health, is no
longer offered, and the menu says so with its dead entry.

## Shield charge stuns foes where it lands

APM_ShieldCharge (Aegis) carries CompProperties_AbilityBash: job=APM_BashStun,
jobFactor=10 ("Stun damage amount"), moveSpeed=0.3. Same driver as Bladehopp:
after the 0.4 s warmup (24 ticks) Apply starts the job, the Aegis covers 0.3
cells per tick and, once the target cell is within 1.1 of it, is put ON the
target cell (5 cells: 15 ticks, budget 120 for warmup and dash).

JobDriver_BashStun deals DamageDefOf.Stun 10 to every thing in the 3x3 around
the landing cell, skipping itself and non-hostile factioned things except the
target. Vanilla StunHandler turns Stun damage into amount x 30 ticks: 300.
Read right after the landing: min=280 max=300. The player colonist beside
the Raider must not be stunned.

The description says "stunning everything hit on the way"; the code stuns
only at the landing cell. The test pins the code; the wording is a finding.

Refused target: CompAbilityEffect_Bash.Valid refuses a cell the Aegis cannot
stand on. Sandbags are the one cell only that gate refuses: they do not block
sight, they are in reach (4.2 against 6.9), canTargetBuildings=false makes
the targeter aim at the location, and Sandbags are PassThroughOnly, so
Standable is false. A wall cell would also be turned down by the line of
sight. The targeter asks Valid, so this is a targeter refusal. Both uses go
through the gizmo "Shield Charge"; the accepted click on the Raider is the
control, and the last line reads the cooldown as the same gizmo greyed out.

The stun is read with "assert stun", ticks left at the landing.
The Raider has no faction: as an enemy it fought the Ally beside it from the
first tick, so the Ally's pain line measured that fight. JobDriver_BashStun
(JobDrivers_Bash.cs:137) skips only things with a faction that is not hostile,
so a factionless pawn is stunned like a foe. Paused from its spawn to the
click so it cannot wander off 5,0.
The bound is read first, on the frames right after the landing. The probe of
2026-09-30 settled it in the run of 2026-10-01 02:47: "assert stun" held at
the landing, and the bound behind five more steps read "the last stun came
from Aegis at tick 74783 for 300 ticks; it is tick 75445 now" - the 300 were
set, they had run below 280 by the time the line was reached. The kit has
"assert stun" now, so the "requires assert=stun" that held these lines back
is gone.

## Siren chats down a prisoner

The Siren is a warden mech: work type APM_SirenWarden, giver
APM_SirenChatWithPrisoner (WorkGiver_SirenWardenChat, JobDriver_SirenSong).
The gate SirenWardenUtility.CanSirenWork asks for a living, spawned, awake
APM_Mech_Siren and nothing about an overseer, so a colony Siren is asked here
directly, the way the work loop would ask it.

Gate order of the giver: a pawn, vanilla ShouldTakeCareOfPrisoner, the Siren
can sing (Talking), the prisoner is secure, awake and not downed, the
interaction mode is AttemptRecruit or ReduceResistance, the prisoner is
scheduled for an interaction, then reservation and reach. All exits but the
schedule one are silent, so "assert prisoner ... secure" and "assert reach"
stand above the work line to say which half closed if it turns red.

The song: two verses of 350 ticks at the prisoner, the outcome at the end:
resistance -= 1 x NegotiationAbility x moodCurve(0 -> 0.2, 0.5 -> 1, 1 -> 1.5).
A fresh prisoner sits near the middle of its mood, the Siren's fixed skill of
10 puts NegotiationAbility close to 1, so 10 drops by roughly 0.5 to 1.5.
The bounds 7.0 .. 9.8 hold that with room, and still fail both on "nothing
happened" and on a runaway factor.

Budget: two walks inside the prison block plus 700 ticks of singing; 3000 is
generous, and "job" reports the JobCondition it ended with.

## Siren lure refuses mechs, the downed and the recently lured

APM_Ability_SirenLure. CompAbilityEffect_SirenLureChannel.CanApplyOn asks
SirenLureUtility.CanStartLureOnTarget, which is SirenLureTargetRules.CanStartLure
over:
  CanAffectTarget - caster alive/spawned/not downed; target not the caster, not
    dead, not downed, spawned on the caster's map, flesh and not a mechanoid,
    PsychicSensitivity > 0, capable of Hearing
  alreadyBeingLured - target's CurJobDef is GotoMindControlled
  recentlyLured - target carries APM_Hediff_SirenLureCooldown
  then no other pawn's job is this ability or APM_SirenLureChannel on the target.
The comp's Valid asks the same rule, and the targeter runs it on the click:
a refused target keeps the targeter open. No reason text exists to pin, so
the refusals are "target ... expect=refused" after the gizmo "Aluring Song"
(label from Races_Mechanoids_Siren.xml).

On a valid target Apply runs in comp order: CompAbilityEffect_ForceJob starts
GotoMindControlled towards the caster (expiry Ability_Duration 10 s = 600
ticks), then the channel comp marks the target and puts the caster on
APM_SirenLureChannel. The mark lasts MarkTicks(600, targetQuietTicks 2500) =
3100 ticks. The channel fails as soon as the target is off GotoMindControlled,
so after 700 ticks both jobs are over and 2400 ticks of mark are left.

The mark is made by a real lure, not by give: HediffCompProperties_Disappears
without disappearsAfterTicks starts at 0 and would drop a given mark on the
next tick. The deaf-to-the-song refusal is asked of a second Siren, because the
first one is on its own 1000 tick cooldown and would be refused for that. The
second Siren's accepted cast on "Fresh" is what shows its refusal on "Lured"
was the mark and not the draft or the cooldown; the first Siren's accepted cast
does the same for the mech and downed refusals above it.

Timing: the lure's 0.5 s warmup is 30 ticks, so each accepted click is
followed by a waitfor on the mark with a budget of 150; the job lines behind
it read the same Apply.

Both Sirens are player mechs (a mech without faction= is hostile and casts on
its own), overseen and drafted: a colony mech shows ability gizmos only under
an overseer, and the lure leaves displayGizmoWhileUndrafted off. 2 x
BandwidthCost 3 = 6, a mechlink's bandwidth.

Not covered: that "Lured" actually ends up beside the caster. No assert reads
the distance between two pawns; the job lines stand in for the walk.

## Siren song lifts colonists in its radius

The promise: "a pleasant tune that lifts the spirits of nearby organic allies
and briefly helps them recover recreation".

APM_Ability_SirenSong has Ability_EffectRadius 12 and canUseAoeToGetTargets,
so vanilla Ability.GetAffectedTargets takes every thing within 12 cells of the
target cell (the Siren itself) that verb.targetParams.CanTarget accepts. The
targetParams are the defaults plus canTargetSelf - humans, animals AND mechs,
any faction. On each of them:
  - CompAbilityEffect_GiveHediff hangs APM_Hediff_SirenSong (ignoreSelf: never
    on the Siren), 480 s via Ability_Duration. Its only stage carries capMods
    Consciousness +0.05 and Moving +0.10, so a colonist at 1.00 reads 1.05 and
    1.10.
  - ApexMechanoids.CompAbilityEffect_NeedOffset adds 0.2 to Joy where the pawn
    has a Joy need: 0.30 -> 0.50.
  - APM_Thought_SirenSong (ThoughtWorker_Hediff) gives +6 mood while the hediff
    is on; a spawned pawn has no traits to scale it, so 5.5 .. 6.5.
Nothing in the code filters flesh or faction: a mech (or a hostile pawn) in the
radius gets the hediff too. The description promises "organic allies"; that is
a finding for the mod, not asserted here in either direction.

Setup: the song is used the way a player uses it - select the Siren, press
the gizmo "Siren's song" (label "siren's song" in Races_Mechanoids_Siren.xml).
targetRequired is false, so the gizmo opens no targeter and queues the cast
on the Siren itself. A colony mech shows ability gizmos only under an
overseer, and the song leaves displayGizmoWhileUndrafted off, so the Siren is
a player mech with an overseer and is drafted. faction=player also keeps it
from casting the song itself as a hostile mech right after spawning. The 1 s
warmup is 60 ticks; the first read after it is a waitfor with a budget of 180.

"Far" stands 15 cells out, past the radius of 12; its Joy stays at 0.30 (it
can only fall), so max 0.32. The thought is situational and recalculated on
the mood handler's interval, hence waitfor with 300 ticks.

## Siren warden work decides

Every Siren warden giver is guarded by SirenWardenUtility.CanSirenWork, which
accepts APM_Mech_Siren and nothing else, then by the interaction mode the
player set on the prisoner. This file asks the chat giver the questions a
player's choices put to it, in one scene:

  1. a colony Siren, prisoner on ReduceResistance  -> offered
  2. a colony Lasher, same prisoner                 -> not offered (not a Siren)
  3. the Siren, prisoner switched to MaintainOnly   -> not offered (mode)
  4. the Siren, prisoner on AttemptRecruit          -> offered again

Line 1 goes first so the two "absent" lines below it cannot be green because
the giver never works at all. The Lasher is a mech with its own work type, so
the refusal is the Siren check and not "a pawn that does no work".

"speed paused": nothing here needs the clock, and a pawn that walks between
the questions is the classic way a warden file goes red for the wrong reason.

## Solus orb arcs golden lightning

APM_Solus fires APM_SolusOrb (Burn 30), which arcs Burn 8 lightning out to 4.9
cells, up to three targets, drawn with the motes APM_Mote_SolusLightning_A..C
in gold (1.0, 0.85, 0.35). Range 19.9, minimum 3.9, warmup 5.2 s = 312 ticks.

Three raiders stand close together at 12 cells, so the arc has somewhere to
jump. The colonist is drafted and ordered to attack the middle one, the same
forced order a right-click gives.

"ask": a colour and a jump are no state. Headless, the line is a skip. The
damage half would be "the middle raider carries a Burn" - left out, because
a warmup of 312 ticks plus a miss chance is a roll, and this file is about
the picture.

## Starfall shreds its target and spares the bystander

The promise: "launches a single heavy shell that splits into three energy
blasts while still in flight."

The Ravager uses it the way a player does: select, gizmo "Starfall", click the
cell. Ability_Starfall.CanApplyOn freezes the target to a cell and asks
RavagerArtilleryUtility.CanFireAtCell (awake, unroofed caster, no thick roof
over the cell, Verb_CastStarfall.CanHitTarget - range 140, no line of sight
needed). After the 4.8 s warmup (288 ticks) the cell goes to
CompAbilityEffect_Starfall.Apply, which spawns APM_StarfallCarrier at the
caster and launches it.

Timing, from 20 cells (-10,0 to 10,0): carrier speed 32 = 0.32 tiles/tick,
about 63 ticks to the cell. Projectile_StarfallCarrier.Tick splits at
round(63 * 0.72) = 45 ticks, about 5.6 cells short, into three
APM_StarfallSplitProjectile (speed 64 = 0.64 tiles/tick, 9 to 12 ticks more),
each a Bomb explosion of radius 2.85 with no delay. Impact is near tick 57
after the warmup, so near 345 after the click; the budget of 600 covers the
288 warmup ticks plus five times the flight.

Where they land: the centre fragment aims at the target cell plus a forward
jitter of +-1.5 and a cell jitter of +-0.95, floored to a cell - at most 2 cells
along the flight and 1 across, 2.24 cells off, inside the 2.85 radius. So the
target is inside at least one blast every time. Bomb on flesh leaves Shredded.
The side fragments land 2.8 +- 1.25 across (plus 0.95 jitter), so no blast
reaches past about 8.4 cells from the target; "Far" stands 12 cells across
the flight line, off the shell's path.

Pawns are drafted so nobody walks in or out. The Ravager is a player mech (a
hostile one fires its own Starfall at spawn and the test's use finds a
cooldown) with a mechanitor overseer standing far behind it. The last line
reads the cooldown as the same gizmo greyed out.

DRAFT: three Bomb blasts of 50 can kill the target. The hediff line is chosen
because it reads the same on a corpse; if "assert pawn" does not find a dead
pawn by its handle, the report of the first run says so.
Probe (2026-10-01): in the runs of 2026-09-27 00:17 and 2026-10-01 02:47 the
drafted target had no hediff at all after 600 ticks; the green runs held at
345 and 347. This line splits "the shell never left" from "it flew and the
blasts missed".

## Stasis container takes a mech in

The player's stasis container (APM_MechanoidContainer_Player) starts empty.
While empty it shows "Choose mechanoid" (Comp_MechanoidContainerControlled);
the command opens a targeter for a colony mech that is ever controllable, not
downed, can move and can reach the container, and orders it in with
APM_EnterMechanoidContainer: walk there, wait 250 ticks, TryAcceptPawn.
Whether the mech has an overseer is deliberately not asked - "a mech nobody is
holding is the one the player most wants put away".

CanAcceptMech refuses first on power: the gizmo is drawn greyed with vanilla's
"No power". That line comes before the solar generator exists, so it pins the
refusal; the rest of the file is the same gizmo once power is there.

The inspect string is the read: "Contains nobody." while empty, "Contains
<mech>" afterwards. The mech lives in the saved innerContainer, so the read is
repeated after a reload.

Budget: a short walk inside the yard plus 250 ticks of waiting.

## Steel discipline drives allies in reach

The promise, from the ability description: "Drives every colonist and colony
mechanoid in a radius around the Vassal: they move faster, work faster, and
organics hold together better against mental breaks". The Vassal race
description adds the mood boost for a player-gestated Vassal.

CompAbilityEffect_SteelDiscipline (Comps/CompAbilityEffect) takes every
spawned, living pawn of the caster's faction whose integer squared distance is
at most 144 - twelve cells - and hangs APM_Hediff_SteelDiscipline on it for
15000 ticks. The Vassal leaves apexMechsOnly false, so colonists count. Flesh
pawns with a mood need also get APM_Thought_InspiredByDiscipline (+8).

The Vassal is used the way a player uses it: it has a mechanitor overseer
(without one vanilla shows a colony mech no ability gizmo), is selected and
its gizmo "Steel Discipline" pressed. targetRequired is false, so the gizmo
opens no targeter and queues the cast on the Vassal itself. The 1.5 s warmup
is 90 ticks; the first read after it is a waitfor with a budget of 240, and
the Apply that follows hands out every buff in one call. The mechanitor is a
colonist of the Vassal's faction and stands drafted 14 cells out, past the
twelve, so the buff leaves it alone.

The hediff is the marker; the numbers are the effect. The single stage
carries MoveSpeed +0.3 and WorkSpeedGlobal +0.15. A trait-free adult human
walks at 4.6, which the bare read below pins before the cast, so the
after-read is 4.9. MentalBreakThreshold -0.08 is left out: its base moves
with every trait and need.

WorkSpeedGlobal is not read as a total: the colony's ideoligion puts its own
offset on it (-16% in the run of 2026-09-30 01:20), so a bound on the total
pins the save, not the mod. The +0.15 is read as the part the hediff itself
contributes, with "assert statpart", which reports skip until the kit has it.

"Near" is asserted on its cell before the cast. In the rerun of 2026-09-30
12:42 that line was red - "the cell is empty" a few steps after the spawn:
an undrafted colonist walks off on its own, and a hostile "Raider" three cells
away gives it a reason to. That is also the likeliest reading of 2026-09-27,
where the Vassal held the buff and "Near" had none. So "Near" and "Far" stand
drafted - a drafted pawn holds its cell, and the radius is the thing under
test - and the Raider is pacified, so it neither fights nor scares anyone off.
Drafting alone was not enough: in the runs of 2026-09-30 23:28 and 2026-10-01
02:47 "Near" stood at 5,-1 and 2,-1, because it took its first steps in the
frames between its spawn and its draft. The scene is now built on a paused
clock and the clock starts only for the cast ("waitfor" does not advance a
paused clock).

The bare MoveSpeed 4.6 holds for an adult only: a rolled teenager walks at
4.37 (life stage x0.95, run of 2026-10-01 02:47, in the sister test), so
"Near" and "Far" are pinned to age 30.

"Far" stands 15 cells out and "Raider" 3 cells out but of another faction -
the two ways a pawn must be skipped. Both are asserted after "Near" went
green, so neither negation can pass because the cast did nothing.

Coordinates are the claim here: the radius is the thing under test.

## Steel discipline ends in burnout

HediffComp_ApplyHediffOnRemove (Comps/HediffComp) runs when the discipline
buff leaves a pawn that is alive and spawned: a mechanoid gets
APM_Hediff_DisciplineBurnout_Mech, anything else APM_Hediff_DisciplineBurnout,
and a flesh pawn with mood also APM_Thought_DisciplineBurnout (-6).

The buff is put on the way a player puts it on: the Vassal has a mechanitor
overseer (without one vanilla shows a colony mech no ability gizmo), is
selected and its gizmo "Steel Discipline" pressed. targetRequired is false,
so the gizmo opens no targeter and queues the cast on the Vassal itself. The
1.5 s warmup is 90 ticks; the first read after it is a waitfor with a budget
of 240, and the Apply hands out every buff in one call. The mechanitor may
take the buff too; nothing below reads it.

The buff lives 15000 ticks. Waiting that out costs over four minutes of a run
and tests vanilla's HediffComp_Disappears, not this mod. "give ... severity=0"
drops the severity to zero, Hediff.ShouldRemove turns true, and the health
tracker removes the hediff through the same PostRemoved path the timer takes.
The comp does not ask why it was removed - that is the code this file pins.

Effects, read against the bare values the first lines pin:
  colonist: MoveSpeed 4.6 - 0.15 = 4.45, and WorkSpeedGlobal -0.1 from the
            burnout itself
  mech:     MechEnergyUsageFactor 1.0 x 1.35 = 1.35
WorkSpeedGlobal is read as the part the burnout contributes, not as a total:
the colony's ideoligion puts its own offset on the total (-16% in the run of
2026-09-30 01:20, which read 0.74 = 1.0 - 0.1 - 0.16). "assert statpart"
reports skip until the kit has it.
The bare 4.6 holds for an adult only. In the run of 2026-10-01 02:47 the
rolled "Worker" was a teenager and read 4.37 (life stage x0.95) before
anything was cast, so it is pinned to age 30. It stands drafted so it cannot
walk out of the twelve-cell radius before the cast.
The mech row is the half nobody sees - no mood, no tab line that says why the
Vassal drains faster.

## Terminus overdrive hardens its strikes

The promise: "Terminus diverts power inward and briefly enters an overdriven
combat state. While active, it sheds its cape, strikes harder, recovers
between swings faster ..."

APM_TerminusOverdrive self-applies APM_Hediff_TerminusOverdrive for 2700
ticks: Moving +0.25, MeleeDamageFactor x1.6, MeleeCooldownFactor x0.65,
StaggerDurationFactor x0.35. "Strikes harder" is MeleeDamageFactor (1.0 to
1.6), "recovers faster" is MeleeCooldownFactor (1.0 to 0.65); both are read
bare first. The shed cape is a picture and lives in the colour tests.

The Terminus is used the way a player uses it: a colony Terminus with a
mechanitor overseer (without one vanilla shows a colony mech no ability
gizmo), drafted because the def leaves displayGizmoWhileUndrafted off, then
its gizmo "Overdrive" pressed. targetRequired is false, so the gizmo opens no
targeter and queues the cast on the Terminus itself. The 1.1 s warmup is 66
ticks; the first read after it is a waitfor with a budget of 240. The two
cooldown lines are the same gizmo greyed out, before and after a reload;
after the reload the Terminus is selected again, and it is still drafted
because the draft is saved.

EMPResistance x0.98 is not asserted: a factor on a base of zero moves nothing,
which is the likely bug the inventory named - it wants a decision (offset or
drop it), not a test that pins zero.

## Tinker projects a defence matrix only onto its own side

APM_DefenceMatrix has no effect comp that does anything on Activate - the
shield lives in its job. jobDef APM_ProjectDefenceMatrix runs
JobDriver_ProjectShield: the vanilla CastAbility toils (warmup 0.3 s = 18
ticks), then a toil that spawns a MechShield on the target's cell, ties it to
the target (SetTarget) and sets its interceptor to the Tinker's
MechRemoteShieldEnergy 200. The toil never completes on its own; the shield
goes when the job ends. So the matrix is used the way a player uses it:
select the Tinker, press the gizmo "Defence Matrix", click the pawn.

Who may be targeted is decided three times, and all three read the same
rule: same faction and not hostile (Verb_CastSameFactionAbility.CanHitTarget,
.ValidateTarget, .TryStartCastOn; CompAbilityEffect_SameFactionTarget
.CanApplyOn). Range is MechRemoteShieldDistance 9.9, line of sight required.
The click on the foe is turned down by ValidateTarget in the targeter; the
click on the ally with the same gizmo is its control.

Scene: Tinker at 0,0; "Ally", a drafted colonist, at 5,0; "Foe", a hostile
colonist, at 0,5. Both are 5 cells out, inside 9.9 and in sight, so the only
difference between them is the faction. The foe is asked first and then put
down: a standing foe within 2.9 cells would be a melee threat and refuse the
ally cast for a different reason (CasterHasCloseMeleeThreat), and a downed
pawn is no threat.

Budget: 18 ticks of warmup plus the toil's init; 120 ticks is generous and
waitfor reports the margin. The shield is read on the ally's cell, which it
holds because the ally is drafted.

## Tinker repairs a damaged mech

The Tinker is a repair mech: work type APM_TinkerRepair, emergency giver
APM_TinkerRepairMech, job APM_RepairMech (JobDriver_ApexRepairMech). One
RepairTick every round(120 / MechRepairSpeed 13.2) = 9 ticks until
MechRepairUtility.CanRepair turns false.

TinkerRepairUtility.HasRepairControl lets a colony Tinker work only while it
is IsColonyMechPlayerControlled - it needs an overseer, even when the giver is
asked directly. So the first half of this file pins exactly that: without an
overseer the giver hands out nothing. The second half links both mechs to a
mechanitor with "overseer", the player's way (right-click, walk, connect).

The damage: Blunt on the core part of a mech lands as Crack (the solid-part
hediff of Blunt). 10 points is far from lethal and well inside what one
repair job restores.

This file is the ordered repair: the player right-clicks the patient, which
asks the giver forced, and the JobDriver then runs for real ("job ... until=
done" is green only when JobDriver_ApexRepairMech ends Succeeded, i.e.
MechRepairUtility.CanRepair turned false). "work" and "job" ask unforced
unless the line says forced=true - the earlier comment here claimed the
opposite. Unforced, CanRepairMechNow wants CompMechRepairable.autoRepair on
the patient, which vanilla leaves false, so the two lines carry forced=true.

Whether the Tinker takes the job on its own - think tree, JobGiver_Work,
emergency giver, unforced - is not asked here. That is "Tinker repairs a mech
on its own" and "Tinker leaves a mech alone without auto repair".
Probe (2026-09-30): with both overseers in place the giver still answers
ShouldSkip, so CanDoTinkerRepair is false. Bandwidth is not it (2 + 1 of 6).
The left suspect is the awake gate: an idle colony mech lies down in
SelfShutdown, and Utils.IsAwakeOrInterruptibleSelfShutdown accepts that only
with an expiring, overridable job. A red line here names the job it has.

## Tinker repairs a mech on its own only with auto repair

The promise, both directions in one scene: a colony Tinker repairs a damaged
friendly mech without being told to - but only when the player switched the
patient's auto repair on. No "work" and no "job" line here - nothing hands the
Tinker its job.

This file merges "Tinker leaves a mech alone without auto repair" and
"Tinker repairs a mech on its own" (2026-10-01). The positive file was a
strict subset of the control half below, so the two always went red together.

The route a colony Tinker takes (ThinkTrees_Tinker.xml): work mode Work ->
JobGiver_Work with emergency=true -> APM_TinkerRepairMech (emergency,
priorityInType 240, above repair building) -> JobDriver_ApexRepairMech.
JobGiver_Tinker is NOT on this route: it sits under "Non-player fallback"
with invert=true and serves Tinkers of other factions only.

JobGiver_Work asks unforced, and unforced TinkerRepairUtility.CanRepairMechNow
(:175) returns "forced || repairable.autoRepair || pawn.Faction !=
Faction.OfPlayer", so a colony Tinker repairs unasked only what the player
switched on - the same rule vanilla's mechanitors follow. CompMechRepairable
.autoRepair is vanilla's "auto repair" toggle and defaults to false; the
"comp" lines flip it the way the player does.

Negative half: auto repair off, 2500 quiet ticks, no APM_RepairMech started
and the Crack still there. A negation alone would also be green on a Tinker
that does nothing at all (the ShouldSkip failure of "Tinker repairs a damaged
mech" was exactly that), so the positive half runs in the same scene.

Positive half: the toggle goes on, and the same Tinker has to act within the
same budget. Two reads, each for one half of the claim:
  jobhistory - the Tinker STARTED APM_RepairMech by itself (the decision)
  Crack gone - on exactly this patient (the effect). FindRepairableMech
               takes the closest candidate, so a damaged mech left standing
               by an earlier failed test could draw the Tinker away; then the
               first line is green and the second one red, and says so.
Both are waitfor lines: they count game ticks, stop the moment they hold and
report the tick they held at. 2500 is a budget, not a guess at the timing; if
a run reports "held at" near its end, raise the quiet stretch with it.

The Tinker stands undrafted (a drafted pawn gets no work) with its overseer
at once; Tinker 2 + Lifter 1 bandwidth of a fresh mechlink's 6. A colony mech
joins the default control group in work mode Work - if a run shows the Tinker
wandering instead, the work mode is the suspect.

Damage: Blunt 10 on the core part of a Lifter lands as Crack, far from lethal.

## Toxic mist gasses flesh around the haze

The promise: "the haze releases tox gas around itself, enveloping the
surrounding area in a toxic cloud."

The mist is used the way a player uses it: select the haze, press the gizmo
"Toxic Mist" (DefInjected). targetRequired is false, so the gizmo opens no
targeter and queues the cast on the haze itself; after the 1.5 s warmup (90
ticks) CompAbilityEffect_SpawnToxGas.Apply runs. Apply ignores the target
anyway: it queues a GradualGasEmission at the caster's own
cell in MapComponent_GradualGasEmitter - gasAmount 16 over spreadTicks 360 in
72 bursts of 16/72 every 5 ticks, the first burst on the first tick - and
pollutes up to 8 cells within 4. No gas is put down during Apply itself, so
everything below waits on the clock.

The emitter only ever adds gas at the centre cell; the vanilla gas grid
spreads it. "Near" stands on the cell beside the haze and is drafted so it
does not walk out. The flesh effect is vanilla's: tox gas in the cell hangs
ToxGasExposure (Biotech, initial severity 1, mild stage Breathing -0.15,
Sight -0.1). Budget 600 = the 360 tick emission plus four of vanilla's
60 tick gas checks, plus the 90 warmup ticks in front: 720. waitfor reports
how much was used. Breathing is read bare
first and must drop at least the mild stage's 0.15 afterwards.

The two that must stay clean: the haze itself stands in the thickest gas and
carries ToxicEnvironmentResistance 1 (and does not breathe), and "Far" stands
12 cells out, more than twice the 4.9 reach the mist's AI uses for its cloud.
Far is read after the emission has ended (another 120 ticks past the waitfor
at the latest point), so the cloud had every chance to reach it.

The caster is a player mech (a hostile haze casts on its own at spawn) with a
mechanitor overseer (without one a colony mech shows no ability gizmo) and
drafted. The overseer stands far outside the cloud. The cooldown is read as
the same gizmo greyed out.

The gas itself is not a Thing in 1.6 - it lives in map.gasGrid - and is read
with "assert gas", the density of one gas type on one cell.

## Ultimate sacrifice buffs conquerors and spends the caster

The promise, from the Conqueror description: "one unit in a group can choose
to overclock their shared processing unit, giving nearby conquerors a larger
boost, at the cost of entirely frying the overlocked unit's physical
components."

CompProperties_ConquerorSacrificeGiveHediff hands APM_Hediff_UltimateSacrifice
(9000 ticks) to the other awake Conquerors of the caster's faction within 12
cells, and CompAbilityEffect_KillCaster kills the caster. The hediff carries
Moving +0.25 and Consciousness +0.10, so a mech that reads 1.0 on both reads
1.25 and 1.10 afterwards.

The caster is used the way a player uses it: a colony Conqueror with a
mechanitor overseer (without one vanilla shows a colony mech no ability
gizmo), drafted because the def leaves displayGizmoWhileUndrafted off, then
its gizmo "Ultimate sacrifice" pressed. targetRequired is false, so the gizmo
opens no targeter and queues the cast on the caster itself. The comp buffs
the caster's own faction, so every mech here is a colony mech. The 5 s warmup
is 300 ticks; the waitfor budget of 480 is that plus slack.

"Far" stands 15 cells out, past the radius of 12, and "Pulse" is an ally in
range that is no Conqueror - the promise names conquerors. Both are asserted
after "Near" took the buff.

## Unity grows with conqueror allies

HediffComp_Unity (Comps/HediffComp_Unity.cs) sets APM_Hediff_Unity on every
Conqueror to 0.1 per other Conqueror of the same faction on the map, capped at
1.0 by the race comp, and falls back to initialSeverity 0.001 when alone. It
recounts every 250 ticks from a random offset, so a budget of 600 ticks holds
two full intervals whatever the offset.

Stages: from 0.1 MoveSpeed +0.05 (plus MeleeDPS x1.03), from 0.3 MoveSpeed +0.10.
The DMR is the one that is read. The Conqueror base writes MoveSpeed 4.3, so
alone it reads 4.3 and with two allies (severity 0.2, first stage) 4.35. The
gap is small but it is the whole effect of that stage; both bounds sit inside
half of it, so a lost statOffsets block turns the second read red.

The third Conqueror is a colony mech: another faction, so it must not count.
It is spawned before the second hostile one, and the severity after it is
asserted to still sit at the lone value.

## Weapons grant and take their abilities

CompAbilityGranter adds an ability on Notify_Equipped and removes it on
Notify_Unequipped. Three weapons carry one:
  APM_Weapon_SeveringBlade    -> APM_SeveringEdge   ("Severing edge", reach 4.9)
  APM_Weapon_Skipblade        -> APM_SkipbladeBlink ("Skip", reach 17.9)
  APM_Weapon_DuelistsPsySpear -> APM_Mech_Duel      (owned by the duel files)

The gizmo is what a player sees, and pressing it is what proves the label: a
"gizmo X expect=absent" line is only worth something next to a line that
presses X. So each ability is pressed once and aimed at a cell in reach, and
"assert ability ... range" reads the reach the def promises.

Swapping the blade for the skipblade drops the blade, which is the removal
half - a weapon that left its ability behind would keep "Severing edge" on the
bar. The known sharp edge: Notify_Unequipped also removes an ability the pawn
had before the weapon came. No pawn here has one, so that stays out.

The cells are the claim: 3 cells is inside 4.9, 8 cells inside 17.9.
