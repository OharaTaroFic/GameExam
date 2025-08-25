using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Manager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //任意のフォルダからPlayerプレハブを取得
        GameObject prefabFromAssets = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player.prefab");

        //プレハブをインスタンス化(正面むかせる
        if(prefabFromAssets != null)
        {
            Instantiate(prefabFromAssets, new Vector3(1, 0, 0), Quaternion.Euler(0, 180, 0));
        }
        else
        {
            Debug.LogError("PlayerPrefabがありません。");
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
