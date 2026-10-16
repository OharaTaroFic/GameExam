using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//プレイヤーの見た目（部品のマテリアル）とモーションの欄を変えたら、Player.prefab に自動で書き込む
//EditScene と GameScene のどちらで変えても、両方のシーンに引き継がれる
//再生中に変えたものは、再生を止めたときに書き込む（Unity の普通の動きでは消えてしまう）
[InitializeOnLoad]
public static class PlayerPrefabSync
{
    const string PrefabPath = "Assets/Player.prefab";
    const string SettingsPath = "Assets/PlayerSettings.asset";
    static readonly string[] MotionFields = { "AnimIdle", "AnimWalk", "AnimRun", "AnimWin", "AnimLose" };

    //再生中の変更（再生を止めたときに書き込む）
    static readonly Dictionary<string, Material[]> s_PendingMaterials = new Dictionary<string, Material[]>();
    static readonly Dictionary<string, RuntimeAnimatorController> s_PendingMotions = new Dictionary<string, RuntimeAnimatorController>();
    static bool s_Applying;

    static PlayerPrefabSync()
    {
        ObjectChangeEvents.changesPublished += OnChangesPublished;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    //エディタで何かが変わったとき
    static void OnChangesPublished(ref ObjectChangeEventStream stream)
    {
        if (s_Applying) { return; }
        for (int i = 0; i < stream.length; i++)
        {
            if (stream.GetEventType(i) != ObjectChangeKind.ChangeGameObjectOrComponentProperties) { continue; }
            stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var e);
            var obj = EditorUtility.EntityIdToObject(e.instanceId);
            if (obj is GameObject go)
            {
                foreach (var c in go.GetComponents<Component>()) { Handle(c); }
            }
            else if (obj is Component comp)
            {
                Handle(comp);
            }
        }
    }

    static void Handle(Component c)
    {
        if (!(c is Renderer) && !(c is Player)) { return; }
        var player = c.GetComponentInParent<Player>(true);
        if (player == null) { return; }

        if (EditorApplication.isPlaying)
        {
            //再生中は Player.prefab から作られたものかを判定できないので、プレイヤーなら対象にする
            Remember(c, player);
        }
        else
        {
            if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(player.gameObject) != PrefabPath) { return; }
            ApplyToPrefab(c);
        }
    }

    //編集中: シーンでの変更を Player.prefab に書き込む
    static void ApplyToPrefab(Component c)
    {
        var so = new SerializedObject(c);
        var props = new List<SerializedProperty>();
        if (c is Renderer)
        {
            var materials = so.FindProperty("m_Materials");
            props.Add(materials.FindPropertyRelative("Array.size"));
            for (int i = 0; i < materials.arraySize; i++) { props.Add(materials.GetArrayElementAtIndex(i)); }
        }
        else
        {
            foreach (var name in MotionFields) { props.Add(so.FindProperty(name)); }
        }

        s_Applying = true;
        try
        {
            foreach (var prop in props)
            {
                if (prop != null && prop.prefabOverride)
                {
                    PrefabUtility.ApplyPropertyOverride(prop, PrefabPath, InteractionMode.AutomatedAction);
                }
            }
        }
        finally
        {
            s_Applying = false;
        }
    }

    //再生中: 変更を覚えておく
    static void Remember(Component c, Player player)
    {
        if (c is Renderer r)
        {
            var path = AnimationUtility.CalculateTransformPath(r.transform, player.transform);
            s_PendingMaterials[path] = r.sharedMaterials;
        }
        else
        {
            var p = (Player)c;
            s_PendingMotions["AnimIdle"] = p.AnimIdle;
            s_PendingMotions["AnimWalk"] = p.AnimWalk;
            s_PendingMotions["AnimRun"] = p.AnimRun;
            s_PendingMotions["AnimWin"] = p.AnimWin;
            s_PendingMotions["AnimLose"] = p.AnimLose;
        }
    }

    //再生を止めたとき: 覚えておいた変更を Player.prefab に書き込む
    static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredEditMode) { return; }
        if (s_PendingMaterials.Count == 0 && s_PendingMotions.Count == 0) { return; }

        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            foreach (var kv in s_PendingMaterials)
            {
                var t = kv.Key == "" ? root.transform : root.transform.Find(kv.Key);
                var r = t != null ? t.GetComponent<Renderer>() : null;
                if (r != null) { r.sharedMaterials = kv.Value; }
            }
            if (s_PendingMotions.Count > 0)
            {
                var p = root.GetComponent<Player>();
                p.AnimIdle = s_PendingMotions["AnimIdle"];
                p.AnimWalk = s_PendingMotions["AnimWalk"];
                p.AnimRun = s_PendingMotions["AnimRun"];
                p.AnimWin = s_PendingMotions["AnimWin"];
                p.AnimLose = s_PendingMotions["AnimLose"];
            }
            s_Applying = true;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
            Debug.Log("再生中に変えたプレイヤーの見た目・モーションを、Player.prefab に保存しました");
        }
        finally
        {
            s_Applying = false;
            PrefabUtility.UnloadPrefabContents(root);
            s_PendingMaterials.Clear();
            s_PendingMotions.Clear();
        }
    }

    [MenuItem("授業/プレイヤーの見た目とモーションを最初に戻す")]
    static void ResetMenu()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog("最初に戻す", "再生を止めてから実行してください。", "OK");
            return;
        }
        if (!EditorUtility.DisplayDialog("最初に戻す", "プレイヤーの見た目とモーションを、最初の状態に戻します。よいですか？", "戻す", "やめる"))
        {
            return;
        }
        ResetToDefault();
    }

    //部品のマテリアルを受け皿に戻し、モーションの欄を「プレイヤー設定」の値に戻す
    public static void ResetToDefault()
    {
        var settings = AssetDatabase.LoadAssetAtPath<PlayerSettings>(SettingsPath);
        var root = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
            {
                var target = TargetFor(r.name, settings);
                if (target == null) { continue; }
                var arr = r.sharedMaterials;
                for (int i = 0; i < arr.Length; i++) { arr[i] = target; }
                r.sharedMaterials = arr;
            }
            var p = root.GetComponent<Player>();
            p.AnimIdle = settings.Idle;
            p.AnimWalk = settings.Walk;
            p.AnimRun = settings.Run;
            p.AnimWin = settings.Win;
            p.AnimLose = settings.Lose;
            s_Applying = true;
            PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        }
        finally
        {
            s_Applying = false;
            PrefabUtility.UnloadPrefabContents(root);
        }

        //開いているシーンのプレイヤーに残っている変更も取り消す
        s_Applying = true;
        try
        {
            foreach (var player in Object.FindObjectsByType<Player>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(player.gameObject) != PrefabPath) { continue; }
                foreach (var r in player.GetComponentsInChildren<Renderer>(true))
                {
                    var materials = new SerializedObject(r).FindProperty("m_Materials");
                    if (materials.prefabOverride) { PrefabUtility.RevertPropertyOverride(materials, InteractionMode.AutomatedAction); }
                }
                var so = new SerializedObject(player);
                foreach (var name in MotionFields)
                {
                    var prop = so.FindProperty(name);
                    if (prop.prefabOverride) { PrefabUtility.RevertPropertyOverride(prop, InteractionMode.AutomatedAction); }
                }
            }
        }
        finally
        {
            s_Applying = false;
        }
        settings.Apply();
        Debug.Log("プレイヤーの見た目とモーションを、最初の状態に戻しました");
    }

    //部品の名前から、その部品の受け皿のマテリアルを決める
    static Material TargetFor(string rendererName, PlayerSettings s)
    {
        switch (rendererName)
        {
            case "mesh_zelda":
            case "mesh_zeldaCape":
            case "mesh_zeldaPouch":
            case "mesh_zeldaWaist":
                return s.BodyTarget;
            case "mesh_zeldaEyes":
                return s.EyesTarget;
            case "mesh_zeldaHair":
            case "mesh_zeldaHairBackface":
                return s.HairTarget;
            case "mesh_dagger":
            case "mesh_iceRod":
            case "mesh_masterSword":
            case "mesh_shield":
                return s.SwordShieldTarget;
            case "mesh_arrow_01":
            case "mesh_arrow_02":
            case "mesh_arrow_03":
            case "mesh_quiver":
            case "mesh_bow":
                return s.BowArrowTarget;
            default:
                return null;
        }
    }
}
