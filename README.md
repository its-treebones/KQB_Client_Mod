# SteamClientMod.dll
Restores steam name and profile picture.

### Installing
1. Install BepInEx to load the mod [Download BepInEx](https://github.com/BepInEx/BepInEx/releases/download/v5.4.21/BepInEx_x64_5.4.21.0.zip)

2. extract BepInEx and move `doorstop_config`, `winhttp.dll` and `BepInEx` to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black`

3. Start KQB to initialize, then quit. There should now be a `Plugins` folder under `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\`

4. Clone this repo & copy `KQB_Client_Mod/Mods/SteamClientMod.dll` to `C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\Plugins`

Start the game and enjoy

### Uninstalling
Removing SteamClientMod.dll from C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx\plugins will uninstall the mod
Removing 
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\BepInEx
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\doorstop_config
 - C:\Program Files (x86)\Steam\steamapps\common\Killer Queen Black\winhttp.dll
 
 uninstalls the BepInEx and now your game is back to normal.

### Building yourself
So you don't trust any old .dll that's thrown your way? That's great! All of the source code is in the `KQB_Client_Mod/source/KQBMods` directory. You can open that Solution file in Visual Studio Code. It was made with VS2019, not sure if it's compatible with other versions I'm not a C# developer.


Get yourself a steam API key: [https://steamcommunity.com/dev/apikey]https://steamcommunity.com/dev/apikey
and insert it as the value for line 20 of `SteamClientMod.cs`:
```cs
static string apiKey = "ABCDEFGHIJKLMNO";
```

You'll be missing references, so you'll need to copy relevant libraries from the game, but since those are not mine to distribute they are not added here. You should place any `.dll`s you need inside the `KQB_Client_Mod/source/KQBMods/Libraries` directory.

After that, select "build solution" and grab your fresh off the press SteamClientMod.dll from inside `KQB_Client_Mod/Source/KQBMods/SteamClientMod/bin/Debug/SteamClientMod.dll`

# PanAudioMod.dll
I haven't finished testing, may cause performance issues. It *should* cause Mace attack and Queen Dash audio to pan like the other sounds do.

### Installing/Uninstalling:

Same as the Steam Client Mod. 
