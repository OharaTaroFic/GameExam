using UnityEditor;
using UnityEngine;

//生徒が選ぶ色（Assets/Material の .mat）を、Unity の中で編集できないようにする
//・部品へドラッグして貼ることは、今までどおりできる（書き換わるのは部品の側なので）
//・色そのものが書き換わったり、別の色が「親」に付いて変種になったりする事故を防ぐ
//・先生が色を直したいときは、メニュー「授業 > 色を編集できるようにする（先生用）」をオンにする（その PC だけ）
//・もし色に親が付いたら、Console に記録する（原因を突き止めるため）
[InitializeOnLoad]
public class MaterialGuard : AssetModificationProcessor
{
    const string Folder = "Assets/Material/";
    const string EditableKey = "UnityExamD.MaterialEditable";
    const string MenuPath = "授業/色を編集できるようにする（先生用）";

    static bool Editable => EditorPrefs.GetBool(EditableKey, false);

    static MaterialGuard()
    {
        ObjectChangeEvents.changesPublished += OnChangesPublished;
    }

    static bool IsGuarded(string assetPath)
    {
        return !Editable && assetPath != null && assetPath.StartsWith(Folder) && assetPath.EndsWith(".mat");
    }

    //Unity が「この素材を編集してよいか」を聞いてくるときに答える
    static bool IsOpenForEdit(string assetPath, out string message)
    {
        if (IsGuarded(assetPath))
        {
            message = "生徒が選ぶ色は、編集できないようにしています（授業 > 色を編集できるようにする）";
            return false;
        }
        message = "";
        return true;
    }

    [MenuItem(MenuPath)]
    static void Toggle()
    {
        EditorPrefs.SetBool(EditableKey, !Editable);
        Debug.Log(Editable ? "生徒が選ぶ色を、編集できるようにしました（この PC だけ）" : "生徒が選ぶ色を、編集できないようにしました");
    }

    [MenuItem(MenuPath, true)]
    static bool ToggleValidate()
    {
        Menu.SetChecked(MenuPath, Editable);
        return true;
    }

    //色に親が付いたら記録する
    static void OnChangesPublished(ref ObjectChangeEventStream stream)
    {
        for (int i = 0; i < stream.length; i++)
        {
            if (stream.GetEventType(i) != ObjectChangeKind.ChangeAssetObjectProperties) { continue; }
            stream.GetChangeAssetObjectPropertiesEvent(i, out var e);
            var path = AssetDatabase.GUIDToAssetPath(e.guid);
            if (!path.StartsWith(Folder) || !path.EndsWith(".mat")) { continue; }
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.parent != null)
            {
                Debug.LogWarning("色「" + mat.name + "」に、親「" + mat.parent.name + "」が付きました（変種になりました）。意図しない場合は、git で元に戻してください。", mat);
            }
        }
    }
}
