using HarmonyLib;
using HarmonyLib.Tools;
using System;

namespace Tobey.UnityAudio.Utilities;
internal static class TraverseHelper
{
    public static T SuppressHarmonyWarnings<T>(Func<T> fn)
    {
        var initialFilter = Logger.ChannelFilter;
        Logger.ChannelFilter &= ~Logger.LogChannel.Warn;
        try
        {
            return fn();
        }
        finally
        {
            Logger.ChannelFilter = initialFilter;
        }
    }

    public static Traverse CreateWithOptionalType(string name) =>
        SuppressHarmonyWarnings(() => Traverse.CreateWithType(name));
}
