using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stage : MonoBehaviour
{
    [SerializeField] private GameObject effectObject; // 消したいエフェクト
    private bool isAllEnemiesDefeated = false;        // 敵全滅フラグ

    void Update()
    {
        // まだ全滅チェックしていない場合だけ処理
        if (!isAllEnemiesDefeated)
        {
            // シーン内の "Enemy" タグ付きオブジェクトを取得
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

            //if (enemies.Length == 0)
            //{
            //    isAllEnemiesDefeated = true;

            //    // エフェクトを止める
            //    if (effectObject != null)
            //    {
            //        effectObject.SetActive(false);
            //    }
            //}
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isAllEnemiesDefeated = true;

                // エフェクトを止める
                if (effectObject != null)
                {
                    effectObject.SetActive(false);
                    Debug.Log("Effect Stopped");
                }
            }
        }
    }
}
