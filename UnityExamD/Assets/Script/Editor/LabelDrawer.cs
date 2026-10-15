using UnityEditor;
using UnityEngine;

//[Label("...")] を付けた項目を、日本語の名前で表示する
[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var text = ((LabelAttribute)attribute).Text;
        EditorGUI.PropertyField(position, property, new GUIContent(text), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }
}
