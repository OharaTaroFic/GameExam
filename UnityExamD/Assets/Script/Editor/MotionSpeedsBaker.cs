using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

//Assets/Animators のモーションを全部プレイヤーに当てて、足が本来進む速さを測り、表（MotionSpeeds）に書く
public static class MotionSpeedsBaker
{
    const string AnimatorsFolder = "Assets/Animators";
    const string PlayerPrefabPath = "Assets/Player.prefab";
    const string AssetPath = "Assets/Prefab/PlayerAnimator/MotionSpeeds.asset";

    [MenuItem("授業/モーションの速さを測り直す（先生用）")]
    public static void Bake()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        var go = Object.Instantiate(prefab);
        go.hideFlags = HideFlags.HideAndDontSave;
        var entries = new List<MotionSpeeds.Entry>();
        try
        {
            go.GetComponent<Animator>().enabled = false;
            var bones = go.GetComponentsInChildren<Transform>(true);
            var feet = new[] { bones.FirstOrDefault(t => t.name == "Bip001_Foot_L"), bones.FirstOrDefault(t => t.name == "Bip001_Foot_R") };
            if (feet[0] == null || feet[1] == null) { Debug.LogError("足の骨（Bip001_Foot_L / R）が見つかりません"); return; }

            foreach (var guid in AssetDatabase.FindAssets("t:RuntimeAnimatorController", new[] { AnimatorsFolder }))
            {
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(AssetDatabase.GUIDToAssetPath(guid));
                foreach (var clip in controller.animationClips.Distinct())
                {
                    if (entries.Any(e => e.Clip == clip)) { continue; }
                    float speed = Measure(go, feet, clip);
                    entries.Add(new MotionSpeeds.Entry { Clip = clip, Speed = speed });
                    Debug.Log(controller.name + "：" + speed.ToString("F2"));
                }
            }
        }
        finally
        {
            Object.DestroyImmediate(go);
        }

        var table = AssetDatabase.LoadAssetAtPath<MotionSpeeds>(AssetPath);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<MotionSpeeds>();
            AssetDatabase.CreateAsset(table, AssetPath);
        }
        table.Entries = entries;
        EditorUtility.SetDirty(table);
        AssetDatabase.SaveAssets();

        //プレイヤーに表をつなぐ
        var player = prefab.GetComponent<Player>();
        if (player.MotionSpeeds != table)
        {
            player.MotionSpeeds = table;
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
        }
        Debug.Log("モーションの速さを測りました（" + entries.Count + " 個）");
    }

    //足が地面に着いている間（いちばん低い所の近く）に、足が後ろへ動く速さの平均
    static float Measure(GameObject go, Transform[] feet, AnimationClip clip)
    {
        int steps = Mathf.Max(30, Mathf.RoundToInt(clip.length * 120));
        float dt = clip.length / steps;
        var speeds = new List<float>();
        foreach (var foot in feet)
        {
            var pos = new List<Vector3>();
            for (int i = 0; i <= steps; i++)
            {
                clip.SampleAnimation(go, i * dt);
                pos.Add(go.transform.InverseTransformPoint(foot.position));
            }
            float minY = pos.Min(p => p.y);
            for (int i = 1; i < pos.Count; i++)
            {
                if (pos[i].y > minY + 0.03f || pos[i - 1].y > minY + 0.03f) { continue; }
                speeds.Add(-(pos[i].z - pos[i - 1].z) / dt);
            }
        }
        return speeds.Count > 0 ? Mathf.Max(0f, speeds.Average()) : 0f;
    }
}
