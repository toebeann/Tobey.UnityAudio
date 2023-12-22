<p align="center">
  <img src="https://raw.githubusercontent.com/toebeann/Tobey.UnityAudio/main/icon.png"
</p>

# Tobey's Unity Audio Patcher for BepInEx

A universal BepInEx patcher to enable/disable Unity's built-in audio for any Unity game. Primarily useful for games where Unity's audio engine has been disabled by the game developers due to using a third-party audio engine, such as FMOD.

Includes a BepInEx plugin for in-game configuration with [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager).

## Usage

In most cases you won't need to do much beyond plopping the contents of the downloaded .zip from [the releases page](https://github.com/toebeann/Tobey.UnityAudio/releases) into your game folder (after installing [BepInEx](https://github.com/BepInEx/BepInEx), of course).

However, in some cases, manual configuration is required. The below configuration options can be edited in-game with [Configuration Manager](https://github.com/BepInEx/BepInEx.ConfigurationManager) (some options require enabling "Advanced settings" to view them), or by editing the `Tobey.UnityAudio.cfg` file generated in `BepInEx\config`:

```
## Settings file was created by plugin Unity Audio Patcher v2.0.0
## Plugin GUID: Tobey.UnityAudio

[Patching]

## Whether to apply the Unity Audio patch on game launch.
# Setting type: Boolean
# Default value: true
Enabled = true

## Whether to enable or disable Unity Audio. Automatic will enable Unity Audio if a reference to it is found in game's managed assemblies or BepInEx plugin assemblies, otherwise it will disable it.
# Setting type: PatchType
# Default value: Automatic
# Acceptable values: Automatic, Enable, Disable
Patch type = Automatic

## Comma-separated list of additional paths to search for assemblies which could be using Unity Audio. Ignored if `Patch type` is not set to `Automatic`.
# Setting type: String
# Default value: 
Additional assembly search paths = 

## Comma-separated list of assembly file names to exclude in the search for references to Unity Audio. Ignored if `Patch type` is not set to `Automatic`.
# Setting type: String
# Default value: Assembly-CSharp, Assembly-CSharp-firstpass, Assembly-UnityScript, Assembly-UnityScript-firstpass
Excluded assemblies = Assembly-CSharp, Assembly-CSharp-firstpass, Assembly-UnityScript, Assembly-UnityScript-firstpass

## Comma-separated list of additional fully qualified type names which signify the use of Unity Audio. Ignored if `Patch type` is not set to `Automatic`.
# Setting type: String
# Default value: 
Additional Unity Audio type names = 

[Runtime Testing]

## Whether to test the Unity Audio state at runtime. This will attempt to load a .wav file into memory as an AudioClip to determine the runtime state of Unity Audio.
# Setting type: Boolean
# Default value: true
Enabled = true

## The path to the .wav audio file to load for testing Unity Audio. Relative paths are relative to the location of the plugin. Ignored if `Enabled` is `false`. Must be a Waveform Audio file.
# Setting type: String
# Default value: chime.wav
.wav audio file path = chime.wav

## Whether the test audio file should be played on start up. Ignored if `Enabled` is `false`.
# Setting type: Boolean
# Default value: false
Play test audio file on plugin start = false

## Keyboard shortcut to play the test audio file on demand. Ignored it `Enabled` is `false`.
# Setting type: KeyboardShortcut
# Default value: 
Play test audio file shortcut = 
```

## License

Tobey's Unity Audio Patcher for BepInEx is licensed under the [LGPL-3.0](https://github.com/toebeann/Tobey.UnityAudio/blob/main/LICENSE) license.
