using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Runtime.InteropServices;

[InitializeOnLoad]
public class PrefabEditor
{
    static PrefabEditor()
    {
        SceneView.duringSceneGui += OnChangePlayerSetting;
        EditorApplication.playModeStateChanged += OnSavePlayerAnimInfo;
    }

    static void OnChangePlayerSetting(SceneView sceneView)
    {
        // Editシーンなら終了
        var sceneName = EditorSceneManager.GetActiveScene().name;
        if (sceneName == "EditScene") return;

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

    static private RuntimeAnimatorController[] _saveInfo;

    static void OnSavePlayerAnimInfo(PlayModeStateChange state)
    {
        // Editシーンでないなら終了
        var sceneName = EditorSceneManager.GetActiveScene().name;
        if (sceneName != "EditScene") return;

        // 再生を抜ける時
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            // プレイヤーを探す
            var player = GameObject.Find("Player");

            // アニメーション情報を取得、保存
            var info = player.GetComponent<Player>();
            _saveInfo = info.SaveAnimInfo();
        }
        // Playモード終了後(Editに入った後)
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // プレイヤーを探す
            var player = GameObject.Find("Player");

            // 保存したアニメーションを適用
            var info = player.GetComponent<Player>();
            info.ApplySavedAnimInfo(_saveInfo);

            // 保存する
            PrefabUtility.ApplyPrefabInstance(player, InteractionMode.UserAction);
        }
    }
}
