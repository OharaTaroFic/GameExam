using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// UnityExamD（体験授業）の生徒の作品を、全員同じ設定で WebGL に書き出す。
/// build.ps1 が、作品の Assets/_OCBuild/Editor にこのファイルを入れてから、Unity を画面なしで起動して呼ぶ。
/// </summary>
public static class BatchBuild
{
    const string Template = "PROJECT:TapToStart";//「タップしてはじめる」の幕・全画面・縦向き固定
    const string TemplateFolder = "Assets/WebGLTemplates/TapToStart";

    public static void BuildWebGL()
    {
        try
        {
            string output = ResolveOutputPath();
            Debug.Log("[ExamDBuild] 出力先: " + output);

            ApplyCommonSettings();

            string[] scenes = ResolveScenes();
            if (scenes.Length == 0)
            {
                Debug.LogError("[ExamDBuild] シーンが1つも見つかりませんでした。");
                EditorApplication.Exit(1);
                return;
            }
            Debug.Log("[ExamDBuild] シーン " + scenes.Length + " 件: " + string.Join(", ", scenes));

            var opts = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(opts);
            BuildSummary s = report.summary;

            Debug.Log("[ExamDBuild] 結果=" + s.result
                    + " サイズ=" + (s.totalSize / (1024 * 1024)) + "MB"
                    + " 時間=" + s.totalTime
                    + " エラー=" + s.totalErrors);

            EditorApplication.Exit(s.result == BuildResult.Succeeded ? 0 : 1);
        }
        catch (Exception e)
        {
            Debug.LogError("[ExamDBuild] 例外: " + e);
            EditorApplication.Exit(1);
        }
    }

    // ------------------------------------------------------------------
    // 全員共通の設定（元の UnityExamD と同じ）。生徒の作品の設定はここで上書きされる。
    // ------------------------------------------------------------------
    static void ApplyCommonSettings()
    {
        // Gzip で小さくし、サーバーの設定なしでもブラウザの側で解凍できるようにする
        UnityEditor.PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        UnityEditor.PlayerSettings.WebGL.decompressionFallback = true;

        UnityEditor.PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
        UnityEditor.PlayerSettings.WebGL.dataCaching = true;

        // 「タップしてはじめる」の雛形。消されていたら Unity 標準で書き出す（音・全画面の対策は無くなる）
        if (Directory.Exists(TemplateFolder))
        {
            UnityEditor.PlayerSettings.WebGL.template = Template;
        }
        else
        {
            Debug.LogWarning("[ExamDBuild] " + TemplateFolder + " がありません。Unity 標準の雛形で書き出します。");
            UnityEditor.PlayerSettings.WebGL.template = "APPLICATION:Default";
        }

        UnityEditor.PlayerSettings.runInBackground = true;
        UnityEditor.PlayerSettings.SplashScreen.show = false;
        UnityEditor.PlayerSettings.stripEngineCode = true;
        UnityEditor.PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        UnityEditor.PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Release);

        EditorUserBuildSettings.development = false;
    }

    // ------------------------------------------------------------------
    static string[] ResolveScenes()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled && !string.IsNullOrEmpty(s.path) && File.Exists(s.path))
            .Select(s => s.path)
            .ToArray();
        if (scenes.Length > 0) return scenes;

        // 書き出すシーンの一覧が空のとき（生徒が消してしまったとき）は、元の UnityExamD の順番で拾う
        Debug.LogWarning("[ExamDBuild] 書き出すシーンの一覧が空です。決まった 4 つのシーンを使います。");
        return new[] { "Assets/TitleScene.unity", "Assets/GameScene.unity", "Assets/WinScene.unity", "Assets/LoseScene.unity" }
            .Where(File.Exists)
            .ToArray();
    }

    static string ResolveOutputPath()
    {
        string p = GetArg("-outputPath");
        if (string.IsNullOrEmpty(p)) p = Path.Combine(Directory.GetCurrentDirectory(), "Build/WebGL");
        Directory.CreateDirectory(p);
        return p;
    }

    static string GetArg(string name)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1];
        }
        return null;
    }
}
