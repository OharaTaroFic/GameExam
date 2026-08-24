using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;            // NamedBuildTarget（2021.2以降）
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// オープンキャンパス体験授業のプロジェクトを、全員同じ設定で WebGL ビルドする。
/// Unity 2019 〜 Unity 6 で動くように書いてある。
/// </summary>
public static class BatchBuild
{
    public static void BuildWebGL()
    {
        try
        {
            string output = ResolveOutputPath();
            Debug.Log("[OCBuild] 出力先: " + output);

            ApplyCommonSettings();

            string[] scenes = ResolveScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[OCBuild] シーンが1つも見つかりませんでした。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[OCBuild] シーン " + scenes.Length + " 件: " + string.Join(", ", scenes));

            var opts = new BuildPlayerOptions
            {
                scenes           = scenes,
                locationPathName = output,
                target           = BuildTarget.WebGL,
                targetGroup      = BuildTargetGroup.WebGL,
                options          = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;

            Debug.Log("[OCBuild] 結果=" + s.result
                    + " サイズ=" + (s.totalSize / (1024 * 1024)) + "MB"
                    + " 時間=" + s.totalTime
                    + " エラー=" + s.totalErrors);

            EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception e)
        {
            Debug.LogError("[OCBuild] 例外: " + e);
            EditorApplication.Exit(1);
        }
    }

    // ------------------------------------------------------------------
    // 全プロジェクト共通の設定。学生側の設定はここで上書きされる。
    // ------------------------------------------------------------------
    static void ApplyCommonSettings()
    {
        // ブラウザ側で解凍させる。これが false だと画面が真っ白になる。
        PlayerSettings.WebGL.compressionFormat     = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        PlayerSettings.WebGL.dataCaching      = true;
        PlayerSettings.WebGL.template         = "APPLICATION:Default";

        PlayerSettings.runInBackground        = true;
        PlayerSettings.defaultWebScreenWidth  = 960;
        PlayerSettings.defaultWebScreenHeight = 540;

        // 容量とビルド時間を抑える
        PlayerSettings.stripEngineCode = true;

#if UNITY_2021_2_OR_NEWER
        PlayerSettings.SetManagedStrippingLevel(
            NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        PlayerSettings.SetIl2CppCompilerConfiguration(
            NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);
#else
        PlayerSettings.SetManagedStrippingLevel(
            BuildTargetGroup.WebGL, ManagedStrippingLevel.Low);
        PlayerSettings.SetIl2CppCompilerConfiguration(
            BuildTargetGroup.WebGL, Il2CppCompilerConfiguration.Release);
#endif

        EditorUserBuildSettings.development = false;

        // Personal ライセンスでは無効化できないので、失敗しても無視する
        try { PlayerSettings.SplashScreen.show = false; } catch { }
    }

    // ------------------------------------------------------------------
    static string[] ResolveScenes()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path))
            .Select(s => s.path)
            .ToArray();

        if (scenes.Length > 0) return scenes;

        // Build Settings が空の場合、Assets 以下のシーンを自動で拾う
        Debug.LogWarning("[OCBuild] Build Settings が空。Assets 以下のシーンを自動収集します。");
        return AssetDatabase.FindAssets("t:Scene", new[] { "Assets" })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.StartsWith("Assets/") && !p.Contains("/_OCBuild/"))
            .OrderBy(p => p)
            .ToArray();
    }

    static string ResolveOutputPath()
    {
        string p = GetArg("-outputPath")
                   ?? GetArg("-customBuildPath")
                   ?? Environment.GetEnvironmentVariable("CUSTOM_BUILD_PATH");

        if (string.IsNullOrEmpty(p))
            p = Path.Combine(Directory.GetCurrentDirectory(), "build/WebGL");

        Directory.CreateDirectory(p);
        return p;
    }

    static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                return args[i + 1];
        }
        return null;
    }
}
