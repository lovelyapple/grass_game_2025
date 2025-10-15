using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
public class ClientBuilder
{
    private const string AdminSymbolDefine = "ADMIN_BUILD";
    private const string PlayerSymbolDefine = "PLAYER_BUILD";
    [MenuItem("Build/BuildWindows_Admin")]
    public static void BuildWindowsAsAdmin()
    {
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
            locationPathName = "./Build/win_admin/AdminBuild.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            extraScriptingDefines = new string[] { AdminSymbolDefine },
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Build/BuildWindows_Player")]
    public static void BuildWindowsAsPlayer()
    {
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
            locationPathName = "./Build/win_player/PlayerBuild.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None,
            extraScriptingDefines = new string[] { PlayerSymbolDefine },
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Build/BuildiOS_Player")]
    public static void BuildiOSAsPlayer()
    {
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
            locationPathName = "./Build/ios_player",
            target = BuildTarget.iOS,
            options = BuildOptions.None,
            extraScriptingDefines = new string[] { PlayerSymbolDefine },
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
    [MenuItem("Build/BuildAndroid_Player")]
    public static void BuildAndroidAsPlayer()
    {
        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Select(x => x.path).ToArray(),
            locationPathName = "./Build/and_player",
            target = BuildTarget.Android,
            options = BuildOptions.None,
            extraScriptingDefines = new string[] { PlayerSymbolDefine },
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
