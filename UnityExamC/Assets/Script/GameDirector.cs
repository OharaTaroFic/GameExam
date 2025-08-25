using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    void Update()
    {
        //ゲーム画面上のDotの数が0個になった時の処理
        if (GameObject.FindGameObjectsWithTag("Dot").Length == 0)
        {
            //ゲームクリアに移動
            SceneManager.LoadScene("WinScene");
        }
    }
}
