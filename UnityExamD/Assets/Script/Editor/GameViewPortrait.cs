using UnityEditor;

//Game タブを、スマホの縦画面と同じ大きさ（1080 × 1920、9 : 16）にする
//Game タブの大きさは PC ごとに保存されるので、Unity の起動・スクリプトの読み直しの直後（数秒間）と、再生する直前にかけ直す
//（配ったばかりのプロジェクトでも、生徒が大きさを変えてしまっても、再生すれば縦長に戻る）
[InitializeOnLoad]
public static class GameViewPortrait
{
    const uint Width = 1080;
    const uint Height = 1920;
    const string SizeName = "スマホ縦（9:16）";

    const double KeepSeconds = 5; //読み込んだ直後は Unity が保存内容で上書きするので、しばらくかけ直し続ける
    static double s_KeepUntil;

    static GameViewPortrait()
    {
        s_KeepUntil = EditorApplication.timeSinceStartup + KeepSeconds;
        EditorApplication.update += KeepAfterLoad;
        EditorApplication.playModeStateChanged += state =>
        {
            if (state == PlayModeStateChange.ExitingEditMode) { Apply(); }
        };
    }

    static void KeepAfterLoad()
    {
        if (EditorApplication.timeSinceStartup > s_KeepUntil)
        {
            EditorApplication.update -= KeepAfterLoad;
            return;
        }
        Apply();
    }

    public static void Apply()
    {
        PlayModeWindow.GetRenderingResolution(out uint w, out uint h);
        if (w == Width && h == Height) { return; } //同じ大きさを何度も登録しないように
        PlayModeWindow.SetCustomRenderingResolution(Width, Height, SizeName);
    }
}
