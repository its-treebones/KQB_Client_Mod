# Linux Server
Running a headless linux server

## Installing
1. Copy your game files to a temp location `/server/Killer Queen Black`
2. Install Unity 2019.4.17f1
3. Copy the LinuxPlayer, UnityPlayer, and _s.debug files &  from unity (TODO: verify this path...)`C:\Program Files\Unity\Hub\Editor\Editor\Data\PlaybackEngines\LinuxStandaloneSupport\Variations\linux64_headless_nondevelopment_mono`to `server/Killer Queen Black`
4. rename LinuxPlayer to `Killer Queen Black.x86_64`
5. replace `/server/Killer Queen Black/Killer Queen Black_Data/MonoBleedingEdge` with `C:\Program Files\Unity\Hub\Editor\Editor\Data\PlaybackEngines\LinuxStandaloneSupport\Variations\linux64_headless_nondevelopment_mono\Data\MonoBleedingEdge`
6. Install BepInEx for linux [Download BepInEx](https://docs.bepinex.dev/articles/user_guide/installation/index.html?tabs=tabid-nix)
7. Start KQB to initialize BepInEx, then quit. There should now be a `Plugins` folder under `/server/Killer Queen Black/BepInEx/`
8. Clone this repo 
9. From the cloned repo Add `libnetcode-io.so`, `libnext.so`, `libreliable-io.so`, `libsteam_api64.so` TO `/server/Killer Queen Black`
10. Add `BepInEx/LinuxServer.dll` to `/server/Killer Queen Black/BepInEx/Plugins`
11. In the script `run_bepinex.sh` set the executable to be` executable_name="Killer Queen Black.x86_64";`
12. Update the last line of the script to include the --start-server args: `"${executable_path}" --spectator --start-server 2444 -batchmode -nographics`
### Mac & Linux: 
TODO

Start the game and enjoy

---
## Uninstalling
Removing SteamClientMod.dll from C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\plugins will uninstall the mod
Removing 
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\doorstop_config
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\winhttp.dll
 
 uninstalls the BepInEx and now your game is back to normal.
 
---
## Building yourself
So you don't trust any old .dll that's thrown your way? That's great! All of the source code is in the `KQB_Client_Mod/source/KQBMods` directory. You can open that Solution file in Visual Studio Code. It was made with VS2019, not sure if it's compatible with other versions I'm not a C# developer.


Get yourself a steam API key: [https://steamcommunity.com/dev/apikey](https://steamcommunity.com/dev/apikey)
and insert it as the value for line 20 of `SteamClientMod.cs`:
```cs
static string apiKey = "ABCDEFGHIJKLMNO";
```

You'll be missing references, so you'll need to copy relevant libraries from the game, but since those are not mine to distribute they are not added here. You should place any `.dll`s you need inside the `KQB_Client_Mod/source/KQBMods/Libraries` directory.

After that, select "build solution" and grab your fresh off the press SteamClientMod.dll from inside `KQB_Client_Mod/Source/KQBMods/SteamClientMod/bin/Debug/SteamClientMod.dll`

# PanAudioMod.dll
I haven't finished testing, may cause performance issues. It *should* cause Mace attack and Queen Dash audio to pan like the other sounds do.

## Installing/Uninstalling:

Same as the Steam Client Mod. 
