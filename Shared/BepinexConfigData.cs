using BepInEx.Configuration;

namespace Tobey.UnityAudio;
internal static class BepinexConfigData
{
    public static readonly string FileName = "Tobey.UnityAudio.cfg";

    public static class Sections
    {
        public static class Patching
        {
            public static readonly string Name = "Patching";

            public static class Keys
            {
                public static class Enabled
                {
                    public static readonly string Name = "Enabled";
                    public static readonly string Description = "Whether to apply the Unity Audio patch on game launch.";
                    public static readonly bool DefaultValue = true;
                }

                public static class PatchType
                {
                    public static readonly string Name = "Patch type";
                    public static readonly string Description = "Whether to enable or disable Unity Audio. Automatic will enable Unity Audio if a reference to it is found in game's managed assemblies or BepInEx plugin assemblies, otherwise it will disable it.";
                    public static readonly UnityAudio.PatchType DefaultValue = UnityAudio.PatchType.Automatic;
                }

                public static class AdditionalAssemblySearchPaths
                {
                    public static readonly string Name = "Additional assembly search paths";
                    public static readonly string Description = $"Comma-separated list of additional paths to search for assemblies which could be using Unity Audio. Ignored if `{PatchType.Name}` is not set to `{UnityAudio.PatchType.Automatic}`.";
                    public static readonly string DefaultValue = string.Empty;
                    public static readonly object[] Tags = new[]
                    {
                        new ConfigurationManagerAttributes() { IsAdvanced = true },
                    };
                }

                public static class ExcludedAssemblies
                {
                    public static readonly string Name = "Excluded assemblies";
                    public static readonly string Description = $"Comma-separated list of assembly file names to exclude in the search for references to Unity Audio. Ignored if `{PatchType.Name}` is not set to `{UnityAudio.PatchType.Automatic}`.";
                    public static readonly string DefaultValue = "Assembly-CSharp, Assembly-CSharp-firstpass, Assembly-UnityScript, Assembly-UnityScript-firstpass";
                    public static readonly object[] Tags = new[]
                    {
                        new ConfigurationManagerAttributes() { IsAdvanced = true },
                    };
                }

                public static class AdditionalUnityAudioTypeNames
                {
                    public static readonly string Name = "Additional Unity Audio type names";
                    public static readonly string Description = $"Comma-separated list of additional fully qualified type names which signify the use of Unity Audio. Ignored if `{PatchType.Name}` is not set to `{UnityAudio.PatchType.Automatic}`.";
                    public static readonly string DefaultValue = string.Empty;
                    public static readonly object[] Tags = new[]
                    {
                        new ConfigurationManagerAttributes() { IsAdvanced = true },
                    };
                }
            }
        }

        public static class RuntimeTesting
        {
            public static readonly string Name = "Runtime Testing";

            public static class Keys
            {
                public static class Enabled
                {
                    public static readonly string Name = "Enabled";
                    public static readonly string Description = "Whether to test the Unity Audio state at runtime. This will attempt to load a .wav file into memory as an AudioClip to determine the runtime state of Unity Audio.";
                    public static readonly bool DefaultValue = true;
                }

                public static class AudioFilePath
                {
                    public static readonly string Name = ".wav audio file path";
                    public static readonly string Descrition = $"The path to the .wav audio file to load for testing Unity Audio. Relative paths are relative to the location of the plugin. Ignored if `{Enabled.Name}` is `false`. Must be a Waveform Audio file.";
                    public static readonly string DefaultValue = "chime.wav";
                }

                public static class PlayAudioFileOnStartUp
                {
                    public static readonly string Name = "Play test audio file on plugin start";
                    public static readonly string Description = $"Whether the test audio file should be played on start up. Ignored if `{Enabled.Name}` is `false`.";
                    public static readonly bool DefaultValue = false;
                }

                public static class PlayAudioFileKeyboardShortcut
                {
                    public static readonly string Name = "Play test audio file shortcut";
                    public static readonly string Description = $"Keyboard shortcut to play the test audio file on demand. Ignored if `{Enabled.Name}` is `false`.";
                    public static readonly KeyboardShortcut DefaultValue = KeyboardShortcut.Empty;
                }
            }
        }
    }
}
