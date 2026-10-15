using UnityEngine;

//プレイヤーの見た目とモーションの設定
//見た目: プレイヤーの部品のうち、受け皿のマテリアルのままの部品に効く
//（生徒が Scene ビューで直接マテリアルを落とした部品には効かない）
[CreateAssetMenu(fileName = "プレイヤー設定", menuName = "授業/プレイヤー設定")]
public class PlayerSettings : ScriptableObject
{
    [Header("見た目（マテリアルを入れる）")]
    [Label("体の色")] public Material Body;
    [Label("目")] public Material Eyes;
    [Label("髪")] public Material Hair;
    [Label("剣と盾")] public Material SwordShield;
    [Label("弓矢")] public Material BowArrow;

    //モーションは Player の欄から読む。ここの値は「最初に戻す」で Player の欄に入れ直す
    [Header("モーション（「最初に戻す」で使う）")]
    [Label("待機")] public RuntimeAnimatorController Idle;
    [Label("歩き")] public RuntimeAnimatorController Walk;
    [Label("走り")] public RuntimeAnimatorController Run;
    [Label("勝ち")] public RuntimeAnimatorController Win;
    [Label("負け")] public RuntimeAnimatorController Lose;

    //プレイヤーの部品に貼ってある受け皿のマテリアル（さわらない）
    //上で選んだマテリアルの中身を、ここに写す
    [HideInInspector] public Material BodyTarget;
    [HideInInspector] public Material EyesTarget;
    [HideInInspector] public Material HairTarget;
    [HideInInspector] public Material SwordShieldTarget;
    [HideInInspector] public Material BowArrowTarget;

    //インスペクターで値が変わったとき
    void OnValidate()
    {
        Apply();
    }

    //選んだマテリアルを、受け皿のマテリアルに写す
    public void Apply()
    {
        Copy(Body, BodyTarget);
        Copy(Eyes, EyesTarget);
        Copy(Hair, HairTarget);
        Copy(SwordShield, SwordShieldTarget);
        Copy(BowArrow, BowArrowTarget);
    }

    static void Copy(Material from, Material to)
    {
        if (from == null || to == null || from == to) { return; }
        to.shader = from.shader;
        to.CopyPropertiesFromMaterial(from);
        to.renderQueue = from.renderQueue;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(to);
#endif
    }
}
