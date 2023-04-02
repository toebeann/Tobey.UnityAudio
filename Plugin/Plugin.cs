using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tobey.UnityAudio;
[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal enum PlayClipType
    {
        OnStart,
        OnDemand,
    }

    AudioClip clip;

    private ConfigEntry<bool> testUnityAudioState;
    private ConfigEntry<string> audioFilePath;
    private ConfigEntry<bool> playClipOnStart;
    private ConfigEntry<KeyboardShortcut> playClipShortcut;

    private void Awake() => InitialiseConfig();

    private void InitialiseConfig()
    {
        #region Patcher Config Values, unused in plugin but exposed so that they can be managed with Config Manager
        Config.Bind(
            section: BepinexConfigData.Sections.Patching.Name,
            key: BepinexConfigData.Sections.Patching.Keys.Enabled.Name,
            description: BepinexConfigData.Sections.Patching.Keys.Enabled.Description,
            defaultValue: BepinexConfigData.Sections.Patching.Keys.Enabled.DefaultValue);

        Config.Bind(
            section: BepinexConfigData.Sections.Patching.Name,
            key: BepinexConfigData.Sections.Patching.Keys.PatchType.Name,
            description: BepinexConfigData.Sections.Patching.Keys.PatchType.Description,
            defaultValue: BepinexConfigData.Sections.Patching.Keys.PatchType.DefaultValue);

        Config.Bind(
            section: BepinexConfigData.Sections.Patching.Name,
            key: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Name,
            configDescription: new(
                description: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Description,
                tags: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.Tags),
            defaultValue: BepinexConfigData.Sections.Patching.Keys.AdditionalAssemblySearchPaths.DefaultValue);

        Config.Bind(
        section: BepinexConfigData.Sections.Patching.Name,
        key: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Name,
        configDescription: new(
            description: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Description,
            tags: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.Tags),
        defaultValue: BepinexConfigData.Sections.Patching.Keys.ExcludedAssemblies.DefaultValue);

        Config.Bind(
            section: BepinexConfigData.Sections.Patching.Name,
            key: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Name,
            configDescription: new(
                description: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Description,
                tags: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.Tags),
            defaultValue: BepinexConfigData.Sections.Patching.Keys.AdditionalUnityAudioTypeNames.DefaultValue);
        #endregion

        testUnityAudioState = Config.Bind(
            section: BepinexConfigData.Sections.RuntimeTesting.Name,
            key: BepinexConfigData.Sections.RuntimeTesting.Keys.Enabled.Name,
            description: BepinexConfigData.Sections.RuntimeTesting.Keys.Enabled.Description,
            defaultValue: BepinexConfigData.Sections.RuntimeTesting.Keys.Enabled.DefaultValue);

        audioFilePath = Config.Bind(
            section: BepinexConfigData.Sections.RuntimeTesting.Name,
            key: BepinexConfigData.Sections.RuntimeTesting.Keys.AudioFilePath.Name,
            description: BepinexConfigData.Sections.RuntimeTesting.Keys.AudioFilePath.Descrition,
            defaultValue: BepinexConfigData.Sections.RuntimeTesting.Keys.AudioFilePath.DefaultValue);

        playClipOnStart = Config.Bind(
            section: BepinexConfigData.Sections.RuntimeTesting.Name,
            key: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileOnStartUp.Name,
            description: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileOnStartUp.Description,
            defaultValue: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileOnStartUp.DefaultValue);

        playClipShortcut = Config.Bind(
            section: BepinexConfigData.Sections.RuntimeTesting.Name,
            key: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileKeyboardShortcut.Name,
            description: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileKeyboardShortcut.Description,
            defaultValue: BepinexConfigData.Sections.RuntimeTesting.Keys.PlayAudioFileKeyboardShortcut.DefaultValue);
    }

    private IEnumerator Start()
    {
        yield return TestUnityAudioState();
        yield return PlayClipAtMainCamera(PlayClipType.OnStart);
    }

    private IEnumerator TestUnityAudioState()
    {
        void logFailure(string missingMemberName, string missingMemberType) =>
            Logger.LogMessage($"Unable to determine the runtime state of Unity Audio due to missing {missingMemberType}: {missingMemberName}");

        if (!testUnityAudioState.Value)
        {
            Logger.LogInfo("Runtime testing disabled, skipping testing.");
            yield break;
        }

        Logger.LogMessage("Attempting to determine the runtime state of Unity Audio...");
        Logger.LogInfo("You may see warnings from AccessTools about being unable to find methods or types etc. during this operation.");

        var path = Path.GetFullPath(Path.Combine(Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName, audioFilePath.Value));
        if (!File.Exists(path))
        {
            logFailure(path, "audio file");
        }

        // in newer versions of Unity, UnityWebRequest.GetAudioClip is deprecated in favour of UnityWebRequestMultimedia.GetAudioClip,
        // so use the newer method where available
        var unityWebRequestMultimedia = new[] { "UnityWebRequestMultimedia", "UnityWebRequest" }.Select(Traverse.CreateWithType).FirstOrDefault(t => t.TypeExists());
        if (unityWebRequestMultimedia is null)
        {
            logFailure("UnityEngine.Networking.UnityWebRequest", "type");
            yield break;
        }

        // we need the type of UnityWebRequest on all versions, as this is the type that will be passed to DownloadHandlerAudioClip.GetContent
        var unityWebRequest = Traverse.CreateWithType("UnityWebRequest");
        if (!unityWebRequest.TypeExists())
        {
            logFailure("UnityEngine.Networking.UnityWebRequest", "type");
            yield break;
        }
        Type unityWebRequestType = unityWebRequest.GetValue<Type>();

        var audioType = Traverse.CreateWithType("AudioType");
        if (!audioType.TypeExists())
        {
            logFailure("UnityEngine.AudioType", "type");
            yield break;
        }
        Type audioTypeType = audioType.GetValue<Type>();

        var downloadHandlerAudioClip = Traverse.CreateWithType("DownloadHandlerAudioClip");
        if (!downloadHandlerAudioClip.TypeExists())
        {
            logFailure("UnityEngine.Networking.DownloadHandlerAudioClip", "type");
            yield break;
        }

        var getContent = downloadHandlerAudioClip.Method("GetContent", new[] { unityWebRequestType });
        if (!getContent.MethodExists())
        {
            logFailure($"{downloadHandlerAudioClip}:GetContent", "static method");
            yield break;
        }

        var getAudioClip = unityWebRequestMultimedia.Method("GetAudioClip", new[] { typeof(string), audioTypeType });
        if (!getAudioClip.MethodExists())
        {
            logFailure($"{unityWebRequestMultimedia}:GetAudioClip", "static method");
            yield break;
        }

        using var disposableRequest = getAudioClip.GetValue<IDisposable>($"file:///{path}", 20); // AudioType.WAV = 20
        var request = Traverse.Create(disposableRequest);
        var sendWebRequest = new[] { "SendWebRequest", "Send" }.Select(name => request.Method(name)).FirstOrDefault(m => m.MethodExists());
        if (sendWebRequest is null)
        {
            logFailure($"{unityWebRequestMultimedia}:SendWebRequest", "instance method");
            yield break;
        }

        // actually send the request to load the AudioClip, and wait for it to be loaded
        yield return sendWebRequest.GetValue();

        // retrieve the AudioClip
        clip = getContent.GetValue<AudioClip>(disposableRequest);
        var unityAudioEnabled = clip != null && clip.loadState != AudioDataLoadState.Unloaded; // When Unity Audio is disabled at runtime, AudioClip instances will have a load state of Unloaded.

        Logger.LogMessage($"Unity Audio is {(unityAudioEnabled ? "enabled" : "disabled")} at runtime.");
    }

    private IEnumerator PlayClipAtMainCamera(PlayClipType playClipType)
    {
        if (testUnityAudioState.Value &&
            clip != null &&
            clip.loadState != AudioDataLoadState.Unloaded &&
            playClipType switch
            {
                PlayClipType.OnStart => playClipOnStart.Value,
                PlayClipType.OnDemand => true,
                _ => false,
            })
        {
            yield return new WaitUntil(() => clip.loadState == AudioDataLoadState.Loaded && Camera.main != null && Camera.main.isActiveAndEnabled);
            Logger.LogDebug("Attempting to play clip at camera...");
            var audioSource = Camera.main.gameObject.AddComponent<AudioSource>();
            audioSource.enabled = audioSource.bypassEffects = audioSource.bypassListenerEffects =
                audioSource.ignoreListenerVolume = audioSource.bypassReverbZones = audioSource.ignoreListenerPause = true;

            audioSource.PlayOneShot(clip);

            yield return new WaitForSeconds(clip.length);
            yield return new WaitWhile(() => audioSource.isPlaying);
            Logger.LogDebug("Finished playing clip.");
            Destroy(audioSource);
        }
    }

    private void Update()
    {
        if (playClipShortcut.Value.IsDown())
        {
            StartCoroutine(PlayClipAtMainCamera(PlayClipType.OnDemand));
        }
    }
}
