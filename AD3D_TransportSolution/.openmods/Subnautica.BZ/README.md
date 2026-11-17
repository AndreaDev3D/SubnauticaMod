# AD3D Transport Solution (Below Zero)

The Below Zero edition of AD3D Transport Solution: a suspended monorail for your bases. Place **pylons** and the rail builds itself between them, over land or along the sea floor. Hang a **pod** on the rail and drive it, couple **cargo wagons** behind it, and take your **Prawn Suit** along on its own carrier.

> **First test release.** This is the first public build of the monorail. Expect rough edges, and please report what you find.

## Features

- **Monorail Pylon** (Titanium ×4). Build pylons 4 to 50 m apart: each new pylon lays a rail to the nearest free one. Rails climb up to 25° and turn up to 60° at a pylon, so the line can follow the terrain.
- **Telescoping pylons.** The Up and Down buttons on a pylon raise the rail in five steps, from about 4 m to 13 m. Rails and anything riding on them follow.
- **Monorail Pod** (Titanium ×6, Glass ×2, Wiring Kit, Lubricant). An enclosed, dry cabin for one. Aim at a rail to build it, click it to board, then use forward and back to drive at up to 10 m/s. You can breathe inside, also under water.
- **Call button.** Press the call box on any pylon and the nearest free pod drives itself there, wagons included.
- **Monorail Cargo Wagon** (Titanium ×5, Lubricant). A 6 × 6 cargo hold with doors on both sides. Back a pod into it to couple it; click the wagon to uncouple.
- **Monorail Prawn Carrier** (Titanium ×6, Wiring Kit, Lubricant). Walk a Prawn Suit onto the deck and get out to lock it on. It rides with the train and slowly recharges; get back in to unload.
- Works **above and below water**. Everything is in the Habitat Builder under Exterior Modules and unlocked from the start.
- Designed for Below Zero. For Subnautica, use [AD3D Transport Solution (Subnautica)](../Subnautica.SN).

## Installation

1. Install **BepInEx** and **Nautilus** for Below Zero if you haven't already.
2. Download `AD3D_TransportSolution.BZ.zip` from the [releases](../../releases) page.
3. Unzip into your Below Zero `BepInEx/plugins` folder. The final path should look like:
   ```
   SubnauticaZero/BepInEx/plugins/AD3D_TransportSolution.BZ/
   ```
4. Launch the game. The monorail parts appear in the Habitat Builder.

## Known limits

- A pylon carries at most two rails, so a network is a single line (a loop works); there are no junctions yet.
- There is no preview of the rail while you place a pylon.
- Pods and wagons ignore the builder's rotation: they face away from you.
- The pod needs no power yet.

## Compatibility

Designed as an add-on for the AD3D base mod (modId 26 on OpenMods). See the manifest for the exact dependency pins used per release.
