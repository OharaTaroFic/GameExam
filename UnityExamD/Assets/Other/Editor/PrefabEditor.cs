using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public class PrefabEditor
{
    static PrefabEditor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        // GUIの描画開始
        Handles.BeginGUI();

        // 位置指定
        float w = 160f;
        float h = 60f;
        float x = sceneView.position.width - w - 10;
        float y = sceneView.position.height - h - 40;
        Rect rect = new Rect(x, y, w, h);

        if (GUI.Button(rect, "Player Prefab Apply All"))
        {
            // プレイヤーのインスタンスを探す
            var player = GameObject.Find("Player");

            if (player == null)
            {
                Debug.Log("Playerが存在しません");
            }
            // 存在して、変更がある時
            else if (PrefabUtility.HasPrefabInstanceAnyOverrides(player, false))
            {
                PrefabUtility.ApplyPrefabInstance(player, InteractionMode.UserAction);
                Debug.Log("プレイヤーの設定を変更しました");
            }
        }

        Handles.EndGUI();
    }
}
