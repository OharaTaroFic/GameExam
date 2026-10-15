using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*State.*/
enum SceneState//State.
{
    EditScene,
    TitleScene,
    GameScene,
    WinScene,
    LoseScene,
}
enum PlayerState//State.
{
    Idle,
    Walk,
    Run,
    Win,
    Lose
}


public class Player : MonoBehaviour
{
    //State.
    SceneState _sceneState;//シーンの状態
    PlayerState _playerState;//プレイヤーの状態
    //コンポーネント
    CharacterController _characterController;
    Animator _animator;
    //パラメータ
    public float MoveSpeed = 5f;//移動速度
    public float RotationSpeed = 360f;//回転速度 
    //アニメーションコントローラー
    public RuntimeAnimatorController AnimIdle;//待機アニメーション
    public RuntimeAnimatorController AnimWalk;//歩行アニメーション
    public RuntimeAnimatorController AnimRun;//走行アニメーション
    public RuntimeAnimatorController AnimWin;//勝利時のアニメーション
    public RuntimeAnimatorController AnimLose;//敗北時のアニメーション

    // Start is called before the first frame update
    void Start()
    {
        //現在のシーンを取得Debug.Log("EditScene!"); 
        var scene = SceneManager.GetActiveScene();
        //シーン名から現在の状態を決定
        if (scene.name == "EditScene") { _sceneState = SceneState.EditScene; Debug.Log("EditScene!"); }
        else if (scene.name == "GameScene") { _sceneState = SceneState.GameScene; Debug.Log("GameScene!"); }
        else if (scene.name == "TitleScene") { _sceneState = SceneState.TitleScene; Debug.Log("TitleScene!"); }
        else if (scene.name == "WinScene") { _sceneState = SceneState.WinScene; Debug.Log("WinScene!"); }
        else if (scene.name == "LoseScene") { _sceneState = SceneState.LoseScene; Debug.Log("LoseScene!"); }
        //コンポーネントの取得
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_sceneState)
        {
            case SceneState.EditScene:
                EditUpdate();
                break;
            case SceneState.TitleScene:
                TitleUpdate();
                break;
            case SceneState.GameScene:
                GameUpdate();
                break;
            case SceneState.WinScene:
                WinUpdate();
                break;
            case SceneState.LoseScene:
                LoseUpdate();
                break;
            default:
                break;
        }
    }

    /*各シーン用のアップデート*/
    void EditUpdate()
    {
        //現在のStateでアニメーションを決定
        switch (_playerState)
        {
            case PlayerState.Idle:
                GetComponent<Animator>().runtimeAnimatorController = AnimIdle;
                break;
            case PlayerState.Walk:
                GetComponent<Animator>().runtimeAnimatorController = AnimWalk;
                break;
            case PlayerState.Run:
                GetComponent<Animator>().runtimeAnimatorController = AnimRun;
                break;
            case PlayerState.Win:
                GetComponent<Animator>().runtimeAnimatorController = AnimWin;
                break;
            case PlayerState.Lose:
                GetComponent<Animator>().runtimeAnimatorController = AnimLose;
                break;
            default:
                break;
        }
    }
    void TitleUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimIdle;
    }
    void GameUpdate()
    {
        //入力値から移動ベクトルを作成
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //作成した移動ベクトルの大きさで現在のステートを決める。
        if (direction.sqrMagnitude < 0.01f) { _playerState = PlayerState.Idle; }
        if (direction.sqrMagnitude > 0.01f) { _playerState = PlayerState.Walk; }
        if (direction.sqrMagnitude > 0.98f) { _playerState = PlayerState.Run; }

        //Stateに応じた処理
        //アニメーションの切り替え
        if (_playerState == PlayerState.Idle)//待機
        {
            //アニメーションをIdleにする_
            _animator.runtimeAnimatorController = AnimIdle;
        }
        else if (_playerState == PlayerState.Walk)//歩行
        {
            //アニメーションをWalkにする
            _animator.runtimeAnimatorController = AnimWalk;

        }
        else if (_playerState == PlayerState.Run)//走る
        {
            //アニメーションをWalkにする
            _animator.runtimeAnimatorController = AnimRun;
        }
        //進行方向を向く
        if (_playerState == PlayerState.Walk || _playerState == PlayerState.Run)//歩行か走るなら
        {
            //進行方向のベクトルを作成
            Vector3 forward = Vector3.Slerp(transform.forward, direction, RotationSpeed * Time.deltaTime / Vector3.Angle(transform.forward, direction));
            //進行方向を向く
            transform.LookAt(transform.position + forward);
        }

        //移動方向に移動
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);
    }
    void WinUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimWin;
    }
    void LoseUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimLose;
    }

    /*ボタン操作用のメソッド*/
    public void OnClickIdleButton()
    {
        //アニメーション変更
        _playerState = PlayerState.Idle;
    }
    public void OnClickWalkButton()
    {
        //アニメーション変更
        _playerState = PlayerState.Walk;
    }
    public void OnClickRunButton()
    {
        //アニメーション変更
        _playerState = PlayerState.Run;
    }
    public void OnClickWinButton()
    {
        //アニメーション変更
        _playerState = PlayerState.Win;
    }
    public void OnClickLoseButton()
    {
        //アニメーション変更
        _playerState = PlayerState.Lose;
    }

    /*当たり判定*/
    private void OnTriggerEnter(Collider other)
    {
        // Dotにぶつかった時の処理
        if (other.tag == "Dot")
        {
            Debug.Log("Dotにぶつかった！");
            // 体験①：Dotを消そう(Destroy)
            Destroy(other.gameObject);
        }

        if (other.tag == "Enemy")
        {
            Debug.Log("Enemyにぶつかった！");
            SceneManager.LoadScene("LoseScene");
        }
    }
}
