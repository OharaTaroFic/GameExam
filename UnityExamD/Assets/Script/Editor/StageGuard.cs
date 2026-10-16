using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//ステージ作りの安全策（エディタ用）
//・迷路（シーンの Map）を、Scene ビューでクリックしても選べないようにする
//・画面の操作パネル（TouchCanvas。スティックなど）は、Scene ビューでは隠して選べないようにする
//  （画面用のキャンバスは Scene ビューに巨大な四角で出て、ステージ作りの邪魔になるため）
//  この設定は PC ごとの Library に保存されるので、かけ直し続ける（配ったばかりのプロジェクトでも効く）
//・敵と星を置いたり動かしたりしたら、高さをそろえる（敵は床 y = 0、星は床から少し浮かせた y = 0.5）
[InitializeOnLoad]
public static class StageGuard
{
    const string MapName = "Map";
    const string CanvasName = "TouchCanvas";
    const string EditableKey = "UnityExamD.MazeEditable";
    const string MenuPath = "授業/迷路と操作パネルを選べるようにする（先生用）";
    const float EnemyHeight = 0f;  //敵は足元が基準なので、床にそのまま立たせる
    const float DotHeight = 0.5f;  //星は球の中心が基準。床に埋まらず、上から見つけやすい高さ

    static StageGuard()
    {
        EditorSceneManager.sceneOpened += (scene, mode) => LockMaze(scene);
        ObjectChangeEvents.changesPublished += OnChangesPublished;
        //起動やスクリプトの再読み込みの直後は、Unity が Library の保存内容で上書きするので、
        //エディタの更新のたびに確かめて、外れていればかけ直す
        EditorApplication.update += KeepMazeLocked;
        EditorApplication.hierarchyChanged += () => s_Searched = false;
    }

    static GameObject s_Map;
    static GameObject s_Canvas;
    static bool s_Searched;

    static void KeepMazeLocked()
    {
        if (!s_Searched)
        {
            s_Map = GameObject.Find(MapName);
            s_Canvas = GameObject.Find(CanvasName);
            s_Searched = true;
        }
        //中の物（壁など）を 1 つずつ確かめる（あとから足した壁にもかかるように）
        if (s_Map != null && NeedsLock(s_Map, false)) { LockMaze(s_Map.scene); }
        if (s_Canvas != null && NeedsLock(s_Canvas, true)) { LockMaze(s_Canvas.scene); }
    }

    static bool NeedsLock(GameObject root, bool hide)
    {
        var svm = SceneVisibilityManager.instance;
        foreach (var t in root.GetComponentsInChildren<Transform>(true))
        {
            if (svm.IsPickingDisabled(t.gameObject) == MazeEditable) { return true; }
            if (hide && svm.IsHidden(t.gameObject) == MazeEditable) { return true; }
        }
        return false;
    }

    //先生用の切り替え（その PC だけに効く）
    static bool MazeEditable => EditorPrefs.GetBool(EditableKey, false);

    static void LockOpenScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) { LockMaze(SceneManager.GetSceneAt(i)); }
    }

    static void LockMaze(Scene scene)
    {
        if (!scene.isLoaded) { return; }
        var svm = SceneVisibilityManager.instance;
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name == MapName)
            {
                if (MazeEditable) { svm.EnablePicking(go, true); }
                else { svm.DisablePicking(go, true); }
            }
            else if (go.name == CanvasName)
            {
                if (MazeEditable) { svm.EnablePicking(go, true); svm.Show(go, true); }
                else { svm.DisablePicking(go, true); svm.Hide(go, true); }
            }
        }
    }

    [MenuItem(MenuPath)]
    static void ToggleMazeEditable()
    {
        EditorPrefs.SetBool(EditableKey, !MazeEditable);
        LockOpenScenes();
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleMazeEditableValidate()
    {
        Menu.SetChecked(MenuPath, MazeEditable);
        return true;
    }

    //エディタで何かが置かれた・変わったとき
    static void OnChangesPublished(ref ObjectChangeEventStream stream)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) { return; }
        for (int i = 0; i < stream.length; i++)
        {
            int id;
            switch (stream.GetEventType(i))
            {
                case ObjectChangeKind.CreateGameObjectHierarchy:
                    stream.GetCreateGameObjectHierarchyEvent(i, out var created);
                    id = created.instanceId;
                    break;
                case ObjectChangeKind.ChangeGameObjectOrComponentProperties:
                    stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var changed);
                    id = changed.instanceId;
                    break;
                default:
                    continue;
            }
            var obj = EditorUtility.EntityIdToObject(id);
            if (obj is GameObject go) { SnapToFloor(go); }
            else if (obj is Component c) { SnapToFloor(c.gameObject); }
        }
    }

    //敵と星の高さをそろえる（壁の上に置いても、宙に浮かせても決まった高さに戻る）
    static void SnapToFloor(GameObject go)
    {
        float height;
        if (go.CompareTag("Enemy")) { height = EnemyHeight; }
        else if (go.CompareTag("Dot")) { height = DotHeight; }
        else { return; }
        var t = go.transform;
        if (Mathf.Approximately(t.position.y, height)) { return; }
        Undo.RecordObject(t, "高さをそろえる");
        t.position = new Vector3(t.position.x, height, t.position.z);
    }
}
