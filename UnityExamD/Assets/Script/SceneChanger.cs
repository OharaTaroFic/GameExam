using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

//画面のどこかをタップ（PC ではクリック）すると、次の画面へ進む
public class SceneChanger : MonoBehaviour
{
    public string nextSceneName;
    public float waitSeconds = 1.0f;//画面が出てからこの秒数は、タップを受け付けない（負けた瞬間の指でうっかり進まないように）

    float _startTime;

    // Start is called before the first frame update
    void Start()
    {
        _startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time - _startTime < waitSeconds) { return; }

        //タップ・クリック（Pointer はマウスとタッチの両方をまとめたもの）
        var pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }

        //キーボードは開発用（メニュー「授業 > キーボード操作を使う（開発用）」）
        if (!DevSettings.KeyboardEnabled) { return; }
        var keyboard = Keyboard.current;
        if (keyboard == null) { return; }

        if (keyboard.enterKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
        }

        if (keyboard.escapeKey.isPressed)
        {
            Application.Quit();
        }
    }
}
