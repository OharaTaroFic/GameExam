using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
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
    //入力
    InputAction _moveAction;//移動の入力（スティックとキーボード）
    //パラメータ
    public float MoveSpeed = 5f;//移動速度
    public float RotationSpeed = 360f;//回転速度 
    //見た目の設定（プレイヤー設定ファイル）
    public PlayerSettings Settings;
    //アニメーションコントローラー
    [Header("モーション（Animator Controller を入れる）")]
    [Label("待機")] public RuntimeAnimatorController AnimIdle;//待機アニメーション
    [Label("歩き")] public RuntimeAnimatorController AnimWalk;//歩行アニメーション
    [Label("走り")] public RuntimeAnimatorController AnimRun;//走行アニメーション
    [Label("勝ち")] public RuntimeAnimatorController AnimWin;//勝利時のアニメーション
    [Label("負け")] public RuntimeAnimatorController AnimLose;//敗北時のアニメーション
    //モーションが隠す持ち物（弓・矢・短剣など）も、常に表示するため
    Renderer[] _renderers;
    //モーションをなめらかにつなぐための土台（さわらない）
    //待機・歩き・走りは倒し具合（Speed）で混ぜ、勝ち・負けへは重ねながら切り替える
    //上の 5 つの欄に入れたモーションを、再生中にこの土台へ差し込む
    [HideInInspector] public RuntimeAnimatorController BaseController;
    AnimatorOverrideController _override;
    readonly RuntimeAnimatorController[] _applied = new RuntimeAnimatorController[5];
    string _animState = "";//土台の中の今の状態
    const float CrossFadeTime = 0.2f;//勝ち・負けへ切り替えるときに重ねる時間（秒）
    const float SpeedDamp = 0.1f;//待機・歩き・走りの混ぜ方を変えるときのなめらかさ（秒）
    //モーションごとの足の速さの表（足が地面を滑って見えないように、再生の速さを合わせる）
    [HideInInspector] public MotionSpeeds MotionSpeeds;

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
        _renderers = GetComponentsInChildren<Renderer>(true);
        //移動の入力を作る
        _moveAction = new InputAction("Move", InputActionType.Value);
        _moveAction.AddBinding("<Gamepad>/leftStick");//スティック（画面のスティックもこれとして入る）
        if (DevSettings.KeyboardEnabled)//キーボードは開発用（メニュー「授業 > キーボード操作を使う（開発用）」）
        {
            _moveAction.AddCompositeBinding("2DVector")//キーボード（WASD）
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            _moveAction.AddCompositeBinding("2DVector")//キーボード（矢印キー）
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/rightArrow");
        }
        _moveAction.Enable();
        //見た目を設定どおりにする
        if (Settings != null) { Settings.Apply(); }
        //モーションの土台を用意する
        if (BaseController != null)
        {
            _override = new AnimatorOverrideController(BaseController);
            _animator.runtimeAnimatorController = _override;
            ApplyMotions();
        }
    }

    //5 つの欄のモーションを土台に差し込む（再生中に欄を変えても、すぐ反映される）
    void ApplyMotions()
    {
        if (_override == null) { return; }
        SetMotion(0, "Idle", AnimIdle);
        SetMotion(1, "Walk", AnimWalk);
        SetMotion(2, "Run", AnimRun);
        SetMotion(3, "Win", AnimWin);
        SetMotion(4, "Lose", AnimLose);
    }

    void SetMotion(int index, string slot, RuntimeAnimatorController controller)
    {
        if (_applied[index] == controller) { return; }
        _applied[index] = controller;
        var clips = controller != null ? controller.animationClips : null;
        _override[slot] = (clips != null && clips.Length > 0) ? clips[0] : null;
    }

    //待機・歩き・走り（speed: 0 = 待機、0.5 = 歩き、1 = 走り。間は混ぜる）
    void PlayLocomotion(float speed)
    {
        _animator.SetFloat("Speed", speed, SpeedDamp, Time.deltaTime);
        PlayState("Locomotion");
    }

    //土台の中の状態へ、重ねながら切り替える
    void PlayState(string state)
    {
        if (_animState == state) { return; }
        _animState = state;
        _animator.CrossFadeInFixedTime(state, CrossFadeTime);
    }

    //状態に合わせてモーションを決める（土台が無いときは、前のやり方で設定ファイルごと差し替える）
    void Animate(PlayerState state, float speed)
    {
        if (_override == null)
        {
            RuntimeAnimatorController c = AnimIdle;
            if (state == PlayerState.Walk) { c = AnimWalk; }
            else if (state == PlayerState.Run) { c = AnimRun; }
            else if (state == PlayerState.Win) { c = AnimWin; }
            else if (state == PlayerState.Lose) { c = AnimLose; }
            _animator.runtimeAnimatorController = c;
            return;
        }
        ApplyMotions();
        _animator.speed = 1f;
        if (state == PlayerState.Win) { PlayState("Win"); }
        else if (state == PlayerState.Lose) { PlayState("Lose"); }
        else { PlayLocomotion(speed); }
    }

    //実際に進む速さ（秒速）に合わせて、歩きと走りの混ぜ方と再生の速さを決める（足が地面を滑って見えないように）
    void AnimateMove(float moveSpeed, float stick)
    {
        float walk = MotionSpeeds != null ? MotionSpeeds.Get(FirstClip(AnimWalk)) : 0f;
        float run = MotionSpeeds != null ? MotionSpeeds.Get(FirstClip(AnimRun)) : 0f;
        //歩き・走りの欄に、足が進まないモーション（待機など）が入っているときは、倒し具合のまま
        if (_override == null || walk < 0.3f || run < walk + 0.3f)
        {
            Animate(_playerState, stick);
            return;
        }
        float blend, rate = 1f;
        if (moveSpeed < 0.01f) { blend = 0f; }
        else if (moveSpeed < walk) { blend = 0.5f * moveSpeed / walk; }
        else if (moveSpeed < run) { blend = 0.5f + 0.5f * (moveSpeed - walk) / (run - walk); }
        else { blend = 1f; rate = Mathf.Clamp(moveSpeed / run, 0.5f, 2f); }
        Animate(_playerState, blend);
        _animator.speed = rate;
    }

    static AnimationClip FirstClip(RuntimeAnimatorController controller)
    {
        var clips = controller != null ? controller.animationClips : null;
        return (clips != null && clips.Length > 0) ? clips[0] : null;
    }

    void OnDestroy()
    {
        //移動の入力を片付ける
        _moveAction?.Dispose();
    }

    //モーションが部品を隠したあとに呼ばれる
    void LateUpdate()
    {
        //モーションが隠した持ち物を、表示し直す
        foreach (var r in _renderers) { r.enabled = true; }
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
        //現在のStateでアニメーションを決定（ボタンで選んだモーションを、そのまま見せる）
        float speed = 0f;
        if (_playerState == PlayerState.Walk) { speed = 0.5f; }
        else if (_playerState == PlayerState.Run) { speed = 1f; }
        Animate(_playerState, speed);
    }
    void TitleUpdate()
    {
        Animate(PlayerState.Idle, 0f);
    }
    void GameUpdate()
    {
        //入力値から移動ベクトルを作成
        Vector2 input = _moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(input.x, 0, input.y);

        //作成した移動ベクトルの大きさで現在のステートを決める。
        if (direction.sqrMagnitude < 0.01f) { _playerState = PlayerState.Idle; }
        if (direction.sqrMagnitude > 0.01f) { _playerState = PlayerState.Walk; }
        if (direction.sqrMagnitude > 0.98f) { _playerState = PlayerState.Run; }

        //アニメーションの切り替え（進む速さに合わせて、待機・歩き・走りをなめらかに混ぜる）
        AnimateMove(direction.magnitude * MoveSpeed, Mathf.Clamp01(direction.magnitude));
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
        Animate(PlayerState.Win, 0f);
    }
    void LoseUpdate()
    {
        Animate(PlayerState.Lose, 0f);
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
