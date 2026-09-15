# SOD323 display geometry

Source: https://www.diodes.com/assets/Package-Files/SOD323.pdf (2017-03-16).
The program renders a shared parametric mesh for top, bottom, longitudinal side,
3D and tape. This is not an imported manufacturer STEP model.

All values are millimetres; min / max / typical:

| Source dimension | Meaning | Min | Max | Typical |
|---|---|---:|---:|---:|
| A1 | Body standoff | — | 0.10 | 0.05 |
| A2 | Plastic body height | 1.00 | 1.10 | 1.05 |
| b | Lead width | 0.25 | 0.35 | 0.30 |
| c | Lead thickness | 0.10 | 0.15 | 0.11 |
| D | Body width | 1.20 | 1.40 | 1.30 |
| E | Body length | 1.60 | 1.80 | 1.70 |
| He | Overall lead span | 2.30 | 2.70 | 2.50 |
| L | Flat soldering foot length | 0.20 | 0.40 | 0.30 |

Nominal total height is A1+A2=1.10; the side dimension 1.05 measures A2 only.
Round corners, lead bends, draft and neutral material colors are illustrative,
not independently confirmed manufacturer geometry/material specifications.

Pads: X=0.590, Y=0.450; X1=2.700 is the outer span. Centre spacing is
X1-X=2.110, not 2.700. Reductions apply only to calculated stencil results.

Tape: documented W=8, P=4, P0=4, P2=2, hole diameter=1.5.
A0/B0/K0 are not specified by this sheet. The generic illustration cavity
is 2.90 long, 1.90 wide, 1.40 deep and marked Estimated in the database.
Its dimensions must not be represented as manufacturer-verified.
The original KiCad reference STEP and its metadata are retained separately.
