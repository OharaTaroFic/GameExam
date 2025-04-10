using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    //パラメータ
    public float MoveSpeed = 5f;//移動速度
    public float RotationSpeed = 360f;//回転速度 
    enum State//State.
    {
        Idle,
        Walk,
        Run,
        Win,
        Lose
    }
    State _state;
    //アニメーションコントローラー
    public RuntimeAnimatorController IdleAnim;//待機アニメーション
    public RuntimeAnimatorController WalkAnim;//歩行アニメーション
    public RuntimeAnimatorController RunAnim;//走行アニメーション
    public RuntimeAnimatorController WinAnim;//勝利時のアニメーション
    public RuntimeAnimatorController LoseAnim;//敗北時のアニメーション

    string _sceneName = string.Empty; //シーン名を格納する変数

    // Start is called before the first frame update
    void Start()
    {
        //現在のシーン名を取得
        _sceneName = SceneManager.GetActiveScene().name;
        //シーン名に応じてスクリプトコンポーネントを外す
        if (_sceneName == "EditScene")//EditScene.
        {
            Destroy(GetComponent<test>());
        }

    }

}
