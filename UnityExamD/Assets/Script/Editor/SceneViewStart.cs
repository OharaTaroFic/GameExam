using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//シーンを開いたときに、Scene ビューのカメラを決まった位置にする（そのあとは自由に動かせる）
//・GameScene: 迷路全体が収まる、斜め上からの見下ろし（敵や星を置きやすい）
//・EditScene: キャラの正面、全身が収まる距離（マテリアルを部品へドラッグしやすい）
//Scene ビューのカメラの位置は PC ごとの Library に保存されるので、シーンを開いたときと、Unity を起動した直後にかける
//（スクリプトの読み直しのたびには動かさない。作業中にカメラが飛ばないように）
[InitializeOnLoad]
public static class SceneViewStart
{
    const string StartedKey = "UnityExamD.SceneViewStarted"; //この Unity の起動の中で、もうかけたか
    const double StartupSeconds = 3;  //起動直後は Unity が保存内容で上書きするので、しばらくかけ直し続ける
    const double OpenedSeconds = 1;   //シーンを開いた直後も、少しのあいだかけ直す
    static double s_Until;

    static SceneViewStart()
    {
        EditorSceneManager.sceneOpened += (scene, mode) =>
        {
            if (mode == OpenSceneMode.Single) { Keep(OpenedSeconds); }
        };
        if (!SessionState.GetBool(StartedKey, false))
        {
            SessionState.SetBool(StartedKey, true);
            Keep(StartupSeconds);
        }
    }

    //しばらくのあいだ、エディタの更新のたびにかけ直す（Unity の窓が裏にあっても、更新は動く）
    static void Keep(double seconds)
    {
        s_Until = EditorApplication.timeSinceStartup + seconds;
        EditorApplication.update -= KeepUpdate;
        EditorApplication.update += KeepUpdate;
    }

    static void KeepUpdate()
    {
        if (EditorApplication.timeSinceStartup > s_Until)
        {
            EditorApplication.update -= KeepUpdate;
            return;
        }
        Apply(SceneManager.GetActiveScene());
    }

    public static void Apply(Scene scene)
    {
        var view = SceneView.lastActiveSceneView;
        if (view == null || !scene.isLoaded || EditorApplication.isPlayingOrWillChangePlaymode) { return; }

        if (scene.name == "GameScene")
        {
            var b = MazeBounds();
            if (!b.HasValue) { return; }
            float size = Mathf.Max(b.Value.size.x, b.Value.size.z) * 0.42f; //小さいほどアップになる
            //斜めから見ると手前（スタート地点のある下側）が切れやすいので、見る中心を少し手前にずらす
            view.LookAt(b.Value.center + new Vector3(0f, 0f, -2f), Quaternion.Euler(60f, 0f, 0f), size, false, true);
        }
        else if (scene.name == "EditScene")
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) { return; }
            var t = player.transform;
            //キャラの正面から（キャラが向いている向きの反対側から見る）
            view.LookAt(t.position + Vector3.up * 0.9f, Quaternion.LookRotation(-t.forward), 1.4f, false, true);
        }
    }

    //迷路（Map）の壁をすべて囲む箱
    static Bounds? MazeBounds()
    {
        var map = GameObject.Find("Map");
        if (map == null) { return null; }
        Bounds? b = null;
        foreach (var r in map.GetComponentsInChildren<Renderer>())
        {
            if (r.bounds.size.y < 0.01f) { continue; } //平らな床は除く
            if (b.HasValue) { var e = b.Value; e.Encapsulate(r.bounds); b = e; }
            else { b = r.bounds; }
        }
        return b;
    }
}
