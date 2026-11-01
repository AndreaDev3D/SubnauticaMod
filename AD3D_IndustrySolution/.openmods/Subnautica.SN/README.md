# AD3D Industry Solution (Subnautica)

Automated industry for your outposts. The first machine is the **Mech Excavator**: a four-legged drilling rig you build on the sea floor. It plants its feet on the terrain, lowers its drill and keeps pulling resources out of the ground into its own storage while you're off exploring.

## Features

- **Mech Excavator**, built from the Habitat Builder (Exterior Modules) and placed outside on the sea floor.
- Drills one resource per cycle into a built-in storage locker, from common Titanium and Copper to rare Lithium and Diamond.
- Runs on **power** from any nearby source, like the vanilla outdoor machines. On weak power it keeps drilling, just slower.
- Needs **Lubricant** to keep running: open the lubricant cap and drop some in.
- Procedural animation: the legs adapt to uneven ground, the drill spins and telescopes down, and the bit kicks up sediment.
- Hover the machine to see its status, lubricant and power; click it to turn it on or off.
- Designed for Subnautica (the original game). For Below Zero, use [AD3D Industry Solution (Below Zero)](../Subnautica.BZ).

## Installation

1. Install **BepInEx** and **Nautilus** if you haven't already.
2. Download `AD3D_IndustrySolution.SN.zip` from the [releases](../../releases) page.
3. Unzip into your Subnautica `BepInEx/plugins` folder. The final path should look like:
   ```
   Subnautica/BepInEx/plugins/AD3D_IndustrySolution.SN/
   ```
4. Launch the game. The Mech Excavator appears in the Habitat Builder.

## Compatibility

Designed as an add-on for the AD3D base mod (modId 26 on OpenMods). See the manifest for the exact dependency pins used per release.

## Roadmap

- Per-biome resource tables: what the excavator finds depends on where you build it.
- A control panel on the machine.
- Sounds.
- More industrial machines.
