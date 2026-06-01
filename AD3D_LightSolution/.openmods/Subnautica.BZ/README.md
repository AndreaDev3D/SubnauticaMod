# AD3D Light Solution (Below Zero)

The Below Zero edition of AD3D Light Solution. Brings the same buildable light kit and centralised **Light Switch** to Subnautica: Below Zero, with color and brightness controls so your snow-bound habitats and outposts don't all have to glow the same flat white.

## Features

- Indoor and outdoor lights that snap to BZ base modules and exterior surfaces.
- A `light_switch` buildable that groups every light it can reach.
- Live color picker and brightness slider from the switch panel.
- Tuned against the Below Zero APIs. For the original game, see [AD3D Light Solution (Subnautica)](../Subnautica.SN).

## Installation

1. Install **BepInEx** and **Nautilus** for Below Zero.
2. Download `AD3D_LightSolution.BZ.zip` from the [releases](../../releases) page.
3. Unzip into your Below Zero `BepInEx/plugins` folder:
   ```
   SubnauticaZero/BepInEx/plugins/AD3D_LightSolution.BZ/
   ```
4. Launch the game — the new lights and switch are available in the Habitat Builder.

## Usage

Build a `light_switch` inside a base or on an exterior wall. AD3D lights placed nearby auto-join its group. Open the switch to flip the group on/off, recolour, or rebalance brightness without opening every fixture individually.

## Compatibility

Acts as an add-on for the AD3D base mod (modId 26 on OpenMods). Built against current Nautilus/BZ. Check the manifest for the exact dependency pins used per release.

## Roadmap

- Switch to override the base's stock ceiling light.
- More fixture variants (spot, accent, emergency strip).

---
