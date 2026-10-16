using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameDirector : MonoBehaviour
{
    [Label("取ったドットの数を出す文字")] public Text DotCounter;
    int _total;//最初にあったDotの数

    void Start()
    {
        _total = GameObject.FindGameObjectsWithTag("Dot").Length;
    }

    void Update()
    {
        int rest = GameObject.FindGameObjectsWithTag("Dot").Length;
        //取ったドットの数を出す（取った数/全部）
        if (DotCounter != null) { DotCounter.text = "<color=#FFD829>●</color> " + (_total - rest) + "/" + _total; }

        //ゲーム画面上のDotの数が0個になった時の処理
        if (rest == 0)
        {
            //ゲームクリアに移動
            SceneManager.LoadScene("WinScene");
        }
    }
}
