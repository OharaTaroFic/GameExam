using UnityEngine;
using UnityEngine.UI;

//文字（や画像）を、ゆっくり点滅させる（「タップしてスタート」などの案内を目に留まりやすくする）
[RequireComponent(typeof(Graphic))]
public class BlinkText : MonoBehaviour
{
    [Label("1 回の点滅にかかる秒数")] public float Period = 1.6f;
    [Label("いちばん薄いときの濃さ"), Range(0, 1)] public float MinAlpha = 0.25f;

    Graphic _graphic;
    float _baseAlpha;

    void Awake()
    {
        _graphic = GetComponent<Graphic>();
        _baseAlpha = _graphic.color.a;
    }

    void Update()
    {
        float t = (Mathf.Sin(Time.time * Mathf.PI * 2f / Period) + 1f) / 2f; //0〜1 を行き来する
        var c = _graphic.color;
        c.a = _baseAlpha * Mathf.Lerp(MinAlpha, 1f, t);
        _graphic.color = c;
    }
}
