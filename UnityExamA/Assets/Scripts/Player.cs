using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // 【体験１】float型のjumpPowerを準備しよう！


    // Update is called once per frame
    void Update()
    {
        // 【体験２】Jumpボタン(Spaceキー)が押されたかどうか判定しよう！
        if (Input.GetButtonDown(""))
        {
            // 【体験３】Vector3のY軸方向に、jumpPowerだけ移動するように設定しよう！
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    // 壁にぶつかった時の処理
    void OnCollisionEnter(Collision collision)
    {
        // 【体験４】シーン「Main」を呼び出そう！
        SceneManager.LoadScene("");
    }
}
