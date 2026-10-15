using UnityEngine;

//インスペクターに出す項目名を日本語にする
public class LabelAttribute : PropertyAttribute
{
    public string Text;

    public LabelAttribute(string text)
    {
        Text = text;
    }
}
