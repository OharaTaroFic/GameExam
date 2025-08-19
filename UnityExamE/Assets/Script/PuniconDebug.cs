using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PuniconDebug : MonoBehaviour
{
    public RectTransform baseRect;  // 台座
    public RectTransform knobRect;  // つまみ
    public float radius = 150f;     // 最大半径(px)

    private Vector2 inputVector;
    private bool isActive = false;

    void Update()
    {
        // 左クリック押した瞬間
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            baseRect.position = mousePos;
            knobRect.position = mousePos;

            baseRect.gameObject.SetActive(true);
            knobRect.gameObject.SetActive(true);

            isActive = true;
        }

        // ドラッグ中
        if (isActive && Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            Vector2 dir = mousePos - (Vector2)baseRect.position;
            dir = Vector2.ClampMagnitude(dir, radius);

            knobRect.position = (Vector2)baseRect.position + dir;
            inputVector = dir / radius;  // -1～1の範囲
        }

        // 離したとき
        if (Input.GetMouseButtonUp(0))
        {
            baseRect.gameObject.SetActive(false);
            knobRect.gameObject.SetActive(false);

            inputVector = Vector2.zero;
            isActive = false;
        }
    }

    // 入力ベクトル取得用（将来キャラ制御で使う）
    public Vector2 GetInput()
    {
        return inputVector;
    }
}
