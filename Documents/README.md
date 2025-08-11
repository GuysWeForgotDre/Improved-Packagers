# Improved Packagers
Packagers can use unpackage mode at packing stations (baggies, jars or bricks) and load (and still unload) vehicles in loading docks.
- **Requires:** [MelonLoader](https://melonwiki.xyz/#/)
- **Recommended:** [Mod Manager Phone App](https://www.nexusmods.com/schedule1/mods/397)
---
## General Information
### Installation
- Drop the .dll into `%ScheduleOne Install%/Mods/`.
- Loading Bays config saved in `UserData/MelonPreferences.cfg`.
- Packing Station unpack marker saved in `UserData/ImprovedPackers.json`.
- Delete to uninstall.
### Unpackage
- Packagers will obey the pack / unpack setting of assigned packaging stations.
- They will unpack baggies, jars, or bricks; whichever is loaded.
- Still requires an empty or matching product in input slot (same as player unpacking).
- **`Important:`** You must open the UI of an any existing unpack station after you first install the mod. Due to how the stations work the mod can't read the un/pack arrow externally. Once viewed they are persisted automatically, and changes are tracked.
### Load Vehicles
- Packagers will use routes from storages and equipment to loading dock vehicles.
- The main branch (Il2Cpp) version allows each dock to be set to Load only, Unload only, or Dual (default).
- Alternate (Mono) version only has On / Off setting.
- Be careful when using Dual / On setting, that two Packagers don't get stuck in a loop together.
- Obeys item filters, stack size, and other normal mechanics.
- Configure each loading dock in *Mod Manager Phone App* or the config file above.
---
## Reference
### Source Code
- This program is open source under the `MIT license`. I encourage you to learn from it or use it in your own creations.
- [Github Repository](http://github.com/GuysWeForgotDre/PackagersLoadVehicles)
- Formerally called *Packagers Load Vehicles*
### Contact
Discord: `OnlyMurdersSometimes` | Github: `GuysWeForgotDre`