# level-blockout reference: AI_Reactor_Project character

Character: `CavernPlayer` (third person, CharacterController, height 180 cm, radius 28 cm, crouch height 100 cm).
Values: measured by the user in the MetricsGym scene on 2026-11-26 (walk speed still to measure).
1 Unity unit = 100 cm.

## Measured table
| Metric | How it's tested | Number |
|---|---|---|
| Highest ledge you can jump onto | ledges, standing jump | 100 cm (measured) |
| Widest gap you can clear | gaps, full run-up | 400 cm (measured) |
| Steepest ramp you can walk up | ramps | 45° (measured) |
| Highest step you walk up without jumping | steps | 20 cm (measured) |
| Narrowest corridor you fit through | corridor | 80 cm (measured, passes all the way) |
| Walk speed | time 2000 cm | ____ cm/s (not measured yet; setting is 200 cm/s walk, 530 cm/s run) |

Status: 5 of 6 measured on 2026-11-26 in MetricsGym. Walk speed: still open.

## Worked-out numbers (use these when placing)
| Use | Rule | Value |
|---|---|---|
| Grid | rule 1 | 50 cm (steps 10 cm) |
| Easy gap (about 60%) | rule 2 | 240 cm |
| Safe gap (about 70%) | rule 2 | 280 cm |
| Medium gap (75%) | rule 2 | 300 cm |
| Hard gap (90%) | rule 2 | 360 cm |
| Never | rule 2 | 400 cm or more |
| Climbable ledge | rule 3 | at most 100 cm |
| Walk-up step | rule 3 | at most 20 cm |
| Walkable ramp | rule 3 | at most 45° (use 30° to be safe) |
| Blocking wall | rule 3 | at least 150 cm, usually 400 cm |
| Corridor width | rule 4 | at least 160 cm (2 x 80), usually 300 cm |
| Door | rule 4 | 200 cm wide x 250 cm high |
| Low cover | rule 5 | 90 cm (crouch 100 cm hides, jump 100 cm clears) |
| High cover | rule 5 | 250 cm |
| Lift | Echo Caverns | rises 400 cm in 3 s, waits 2 s |
| Target run time | Echo Caverns | under 90 s at about 530 cm/s |
