using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

//ステージ作りの安全策（エディタ用）
//・迷路（シーンの Map）を、Scene ビューでクリックしても選べないようにする
//  この設定は PC ごとの Library に保存されるので、シーンを開くたびにかけ直す（配ったばかりのプロジェクトでも効く）
//・敵と星を置いたり動かしたりしたら、高さをそろえる（敵は床 y = 0、星は床から少し浮かせた y = 0.5）
[InitializeOnLoad]
public static class StageGuard
{
    const string MapName = "Map";
    const string EditableKey = "UnityExamD.MazeEditable";
    const string MenuPath = "授業/迷路を選べるようにする（先生用）";
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
    static bool s_Searched;

    static void KeepMazeLocked()
    {
        if (s_Map == null && !s_Searched)
        {
            s_Map = GameObject.Find(MapName);
            s_Searched = true;
        }
        if (s_Map == null) { return; }
        if (SceneVisibilityManager.instance.IsPickingDisabled(s_Map) == MazeEditable) { LockMaze(s_Map.scene); }
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
        foreach (var go in scene.GetRootGameObjects())
        {
            if (go.name != MapName) { continue; }
            if (MazeEditable) { SceneVisibilityManager.instance.EnablePicking(go, true); }
            else { SceneVisibilityManager.instance.DisablePicking(go, true); }
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
