using AssetsTools.NET;
using AssetsTools.NET.Extra;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Tobey.UnityAudio;

public static class Patcher
{
    // Without the contents of this region, the patcher will not be loaded by BepInEx - do not remove!
    #region BepInEx Patcher Contract
    public static IEnumerable<string> TargetDLLs { get; } = Enumerable.Empty<string>();
    public static void Patch(AssemblyDefinition _) { }
    #endregion

    private static readonly Assembly assembly = Assembly.GetExecutingAssembly();
    private static readonly string globalGameManagersPath = Path.Combine(Directory.GetParent(Paths.ManagedPath).FullName, "globalgamemanagers");
    private static readonly ManualLogSource logger = Logger.CreateLogSource("UnityAudioPatcher");

    private static readonly ConfigFile config = new(Path.Combine(Paths.ConfigPath, BepinexConfigData.FileName), true);

    private static readonly ConfigEntry<bool> configPatchEnabled = config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.Enabled.Name,
        description: BepinexConfigData.Sections.Patching.Keys.Enabled.Description,
        defaultValue: BepinexConfigData.Sections.Patching.Keys.Enabled.DefaultValue);

    private static readonly ConfigEntry<PatchType> configPatchType = config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.PatchType.Name,
        description: BepinexConfigData.Sections.Patching.Keys.PatchType.Description,
        defaultValue: BepinexConfigData.Sections.Patching.Keys.PatchType.DefaultValue);

    private static readonly ConfigEntry<string> configAssemblySearchPaths = config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Name,
        configDescription: new(
            description: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Description,
            tags: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Tags),
        defaultValue: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.DefaultValue);

    private static readonly ConfigEntry<string> configExcludedAssemblies = config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Name,
        configDescription: new(
            description: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Description,
            tags: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Tags),
        defaultValue: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.DefaultValue);

    private static readonly ConfigEntry<string> configAdditionalTypeNames = config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Name,
        configDescription: new(
            description: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Description,
            tags: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Tags),
        defaultValue: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.DefaultValue);

    private static IEnumerable<string> AssemblySearchPaths => new List<string>(configAssemblySearchPaths.Value.Split(',')) { Paths.ManagedPath, Paths.PluginPath }
        .Select(path => path.Trim())
        .Distinct();

    private static IEnumerable<string> ExcludedAssemblies => new List<string>(configExcludedAssemblies.Value.Split(','))
        .Select(assembly => assembly.Trim())
        .Distinct();

    private static IEnumerable<string> AdditionalTypeNames => configAdditionalTypeNames.Value.Split(',')
        .Select(name => name.Trim())
        .Distinct();

    private static bool HasUnityAudioReferences()
    {
        var excludedAssemblyPrefixes = new[]
        {
            "Tobey.UnityAudio",
            "Unity"
        };

        return AssemblySearchPaths
            .Where(Directory.Exists)
            .SelectMany(path => Directory.GetFiles(path, "*.dll", SearchOption.AllDirectories))
            .Distinct()
            .Where(path => excludedAssemblyPrefixes.All(prefix => !Path.GetFileNameWithoutExtension(path).StartsWith(prefix)))
            .Where(path => ExcludedAssemblies.All(assembly => Path.GetFileNameWithoutExtension(assembly) != Path.GetFileNameWithoutExtension(path)))
            .SelectMany(path =>
            {
                try
                {
                    return AssemblyDefinition.ReadAssembly(path).Modules;
                }
                catch
                {   // if we couldn't read the assembly it's probably not a .NET assembly anyway, so just ignore it
                    return Enumerable.Empty<ModuleDefinition>();
                }
            })
            .SelectMany(module => module.GetTypeReferences())
            .Select(typeRef => typeRef.FullName)
            .Any(typeRefName => typeRefName.StartsWith("UnityEngine.Audio") || AdditionalTypeNames.Contains(typeRefName));
    }

    private static bool IntendedDisableAudioSetting() => configPatchType.Value switch
    {
        PatchType.Enable => false,
        PatchType.Disable => true,
        PatchType.Automatic => !HasUnityAudioReferences(),
        _ => !HasUnityAudioReferences()
    };

    // this is our entry method into the patch
    public static void Finish()
    {
        if (configPatchEnabled.Value)
        {
            PatchGame();
        }
        else
        {
            logger.LogInfo("Patcher disabled, skipping patching.");
        }
    }

    private static void PatchGame()
    {
        var intendedDisableAudioSetting = IntendedDisableAudioSetting();

        logger.LogInfo($"{(intendedDisableAudioSetting ? "Disabling" : "Enabling")} Unity Audio...");

        try
        {
            using var classPackageStream = assembly.GetManifestResourceStream(assembly.GetManifestResourceNames().Single(name => name.ToLowerInvariant().EndsWith("classdata.tpk")));
            var manager = new AssetsManager();
            manager.LoadClassPackage(classPackageStream); // load the class package
            AssetsFileInstance ggmInstance = manager.LoadAssetsFile(globalGameManagersPath, true); // load globalgamemanagers
            manager.LoadClassDatabaseFromPackage(ggmInstance.file.Metadata.UnityVersion); // load the relevant class database from the class package

            AssetFileInfo audioManager = ggmInstance.file.GetAssetInfo(4);
            var baseField = manager.GetBaseField(ggmInstance, audioManager);
            var disableAudio = baseField["m_DisableAudio"];

            if (disableAudio.AsBool == intendedDisableAudioSetting) // if setting in ggm already matches intended, just exit
            {
                logger.LogInfo($"Unity Audio is already {(intendedDisableAudioSetting ? "disabled" : "enabled")}, skipping patching.");
                return;
            }

            // now we should write the new setting to the ggm
            disableAudio.AsBool = intendedDisableAudioSetting;

            // write changes to temp file
            string tempPath = $"{globalGameManagersPath}.tmp";
            using (var writer = new AssetsFileWriter(tempPath))
            {
                ggmInstance.file.Write(writer, 0, new List<AssetsReplacer>
                {
                    new AssetsReplacerFromMemory(ggmInstance.file, audioManager, baseField)
                });
            }
            ggmInstance.file.Close(); // close the file so we can overwrite it
            manager.UnloadAll(); // unload the class package etc. as we're done with them

            // finally, overwrite the globalgamemanagers with the temp file
            File.Delete(globalGameManagersPath);
            File.Move(tempPath, globalGameManagersPath);

            logger.LogMessage($"Unity Audio successfully {(intendedDisableAudioSetting ? "disabled" : "enabled")}.");
        }
        catch (Exception exception)
        {
            logger.LogWarning("An exception was thrown during patching!");
            logger.LogError(exception);
        }
    }
}
