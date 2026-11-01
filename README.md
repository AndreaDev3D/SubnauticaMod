# AD3D Subnautica Mods

Open-source mods for **Subnautica** and **Subnautica: Below Zero** by [AndreaDev3D](https://openmods.net/profile/andreadev3d).

Every mod is available for both games and is published on [OpenMods](https://openmods.net/profile/andreadev3d).

## Mods

| Mod | What it adds | Subnautica | Below Zero |
| :--- | :--- | :---: | :---: |
| **Light Solution** | Buildable indoor and outdoor lights, with a central switch to recolour and dim every fixture in range. | [OpenMods](https://openmods.net/mod/ad3d-light-solution-subnautica) | [OpenMods](https://openmods.net/mod/ad3d-lightsolution-mod) |
| **Energy Solution** | Solar panels, a wind turbine and a depth-scaling Deep Engine to power bases the sun never reaches. | [OpenMods](https://openmods.net/mod/ad3d-energysolution-mod) | [OpenMods](https://openmods.net/mod/ad3d-energysolution-mod-below-zero) |
| **Storage Solution** | Quantum crates, cabinets and wall storage, plus a storage monitor and a resource uploader. | [OpenMods](https://openmods.net/mod/ad3d-storagesolution-mod) | [OpenMods](https://openmods.net/mod/ad3d-storagesolution-mod-below-zero) |
| **Industry Solution** | A walking Mech Excavator that drills resources from the sea floor into its own storage. | [OpenMods](https://openmods.net/mod/ad3d-industry-solution-subnautica) | [OpenMods](https://openmods.net/mod/ad3d-industry-solution-below-zero) |

## Requirements

| | Subnautica | Below Zero |
| :--- | :---: | :---: |
| BepInEx | [Tobey's BepInEx Pack](https://openmods.net/mod/tobey-s-bepinex-pack-for-subnautica) | [Tobey's BepInEx Pack](https://openmods.net/mod/tobey-s-bepinex-pack-for-subnautica-below-zero) |
| Nautilus | [Nautilus](https://openmods.net/mod/nautilus) | [Nautilus](https://openmods.net/mod/nautilus) |

## Installation

1. Install BepInEx and Nautilus for your game.
2. Download the mod from its OpenMods page, or grab `<Mod>.SN.zip` (Subnautica) or `<Mod>.BZ.zip` (Below Zero) from the [latest release](https://github.com/AndreaDev3D/SubnauticaMod/releases/latest).
3. Unzip it into the game's `BepInEx/plugins` folder, e.g. `Subnautica/BepInEx/plugins/AD3D_LightSolution.SN/`.
4. Launch the game. The new items appear in the Habitat Builder.

## Building from source

Each `AD3D_*` folder is a standalone BepInEx plugin, built for either game through its build configuration:

```
dotnet build AD3D_LightSolution/AD3D_LightSolution.csproj -c SN   # Subnautica
dotnet build AD3D_LightSolution/AD3D_LightSolution.csproj -c BZ   # Below Zero
```

Every push to `main` builds all released mods for both games and publishes them as a single GitHub release, which OpenMods picks up automatically.

## License

[MIT](LICENSE)
