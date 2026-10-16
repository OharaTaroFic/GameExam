using UnityEditor;
using UnityEngine;

//メニュー「授業 > キーボード操作を使う（開発用）」
//オンにした PC だけで、WASD・矢印キーで動いたり、Enter・スペースで画面を進めたりできる（その PC の EditorPrefs に保存）
//初期状態はオフ。生徒の PC では、画面のスティックだけで遊ぶ
[InitializeOnLoad]
public static class DevSettingsMenu
{
    const string Key = "UnityExamD.KeyboardEnabled";
    const string MenuPath = "授業/キーボード操作を使う（開発用）";

    static DevSettingsMenu()
    {
        DevSettings.KeyboardEnabled = EditorPrefs.GetBool(Key, false);
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        bool value = !EditorPrefs.GetBool(Key, false);
        EditorPrefs.SetBool(Key, value);
        DevSettings.KeyboardEnabled = value;
        Debug.Log(value ? "キーボード操作を使えるようにしました（この PC だけ。次に再生したときから効きます）" : "キーボード操作を切りました（次に再生したときから効きます）");
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, EditorPrefs.GetBool(Key, false));
        return true;
    }
}
