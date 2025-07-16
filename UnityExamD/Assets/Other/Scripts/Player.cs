using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*State.*/

public enum SceneState
{
    EditScene,
    TitleScene,
    GameScene,
}

enum PlayerState
{
    Idle,
    Move,
    Attack,
    Win,
    Lose,
}

public class Player : MonoBehaviour
{
    public enum SpAttackKind
    { 
        Flash,
        Sword_1,
        Sword_2
    }

    // 操作無効フレーム
    private const int TAP_INVAILD_TITLE = 12;
    private const int TAP_INVAILD_MAIN = 75;
    private const int START_INVAILD_NORMAL = 75;

    // 必殺生存フレーム
    private const int EXIST_FLASH_FRAME = 75;
    private const int EXIST_SWORD_1_FRAME = 87;
    private const int EXIST_SWORD_2_FRAME = 250;
    // 必殺発生ディレイフレーム
    private const int DELAY_FLASH_FRAME = 4;
    private const int DELAY_SWORD_1_FRAME = 15;
    private const int DELAY_SWORD_2_FRAME = 60;
    // 必殺攻撃回数
    private const int DIV_FLASH_NUM = 5;
    private const int DIV_SWORD_1_NUM = 3;
    private const int DIV_SWORD_2_NUM = 6;
    // 必殺攻撃間隔
    private const int DIV_FLASH_FRAME = 8;
    private const int DIV_SWORD_1_FRAME = 16;
    private const int DIV_SWORD_2_FRAME = 25;


    // 初めの演出時の移動速度
    private const float START_MOVE_SPEED = 20.0f / START_INVAILD_NORMAL;

    // 引っ張りキャンセルの長さ割合
    private float CANCEL_LEN_RATE = 0.02f;

    // 反射時速度軽減率
    private const float REFLECTION_LOSS_RATE = 0.8f;

    // State.
    private SceneState _sceneState;
    private PlayerState _playerState;

    // コンポーネント
    private Animator _animator;
    private Rigidbody _rigid;

    // パラメーター
    [Header("ステータス")]
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _power = 20;
    [SerializeField] private int _spPower = 100;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private int _spCost = 3;
    [SerializeField] private SpAttackKind _spKind;

    // アニメーションコントローラー
    [Header("アニメーション")]
    [SerializeField] private RuntimeAnimatorController _animIdle;   // 待機
    [SerializeField] private RuntimeAnimatorController _animMove;   // 移動
    [SerializeField] private RuntimeAnimatorController _animAttack; // 攻撃
    [SerializeField] private RuntimeAnimatorController _animWin;    // 勝利
    [SerializeField] private RuntimeAnimatorController _animLose;   // 敗北

    // フェード切り替え用
    private Fade _fade;
    private int _invaildFrame;

    // GameScene用変数
    private GameObject _flashPrefab;
    private GameObject _sword1Prefab;
    private GameObject _sword2Prefab;
    private GameObject _arrowPrefab;
    private GameObject _hitEffectPrefab;

    [Header("効果音")]
    [SerializeField] private AudioClip _shotSe;
    [SerializeField] private AudioClip _damageSe;
    [SerializeField] private AudioClip _spChargeSe;
    [SerializeField] private AudioClip _spMaxSe;

    private GameDirector _mgr;
    private CameraManager _cameraMgr;
    private GameObject _playerCamera;
    private Vector3 _touchPos;
    private Vector3 _startPos;
    private Vector3 _preVel;
    private int _hp = 0;
    private int _spCount = 0;
    private float _spAnimRate = 0;
    private bool _isStart = true;
    private bool _isShot = false;
    private bool _isMove = false;
    private bool _isChargeSp = false;
    private bool _isSpCreate = false;
    private bool _isUpdateSpAnim = false;
    private bool _isPush = false;
    private bool _isCreateArrow = false;
    private GameObject _arrowObj;
    private Slider _hpBar;
    private Text _hpText;
    private Animator _spAnimator;
    private AudioSource _seSource;

    // 画面調整用
    private float _maxLen;
    private float _cancelLen;

    public SceneState SceneState {  get { return _sceneState; } }
    public bool IsCreateArrow { get { return _isCreateArrow; } }

    private void Awake()
    {
        // 現在のシーン名を取得
        var sceneName = SceneManager.GetActiveScene().name;

        // シーン名から現在の状況を決定
        if (sceneName == "EditScene")       _sceneState = SceneState.EditScene;
        else if (sceneName == "TitleScene") _sceneState = SceneState.TitleScene;
        else if (sceneName == "Stage1")  _sceneState = SceneState.GameScene;
        else if (sceneName == "Stage2")  _sceneState = SceneState.GameScene;
        else if (sceneName == "BossStage")  _sceneState = SceneState.GameScene;

        // コンポーネントの取得
        _animator = GetComponent<Animator>();
        _rigid = GetComponent<Rigidbody>();

        // シーン別初期化処理
        if (_sceneState == SceneState.EditScene)
        {
            Button bt;
            bt = GameObject.Find("ButtonIdle").GetComponent<Button>();
            bt.onClick.AddListener(OnClickIdleButton);
            bt = GameObject.Find("ButtonMove").GetComponent<Button>();
            bt.onClick.AddListener(OnClickMoveButton);
            bt = GameObject.Find("ButtonAttack").GetComponent<Button>();
            bt.onClick.AddListener(OnClickAttackButton);
            bt = GameObject.Find("ButtonWin").GetComponent<Button>();
            bt.onClick.AddListener(OnClickWinButton);
            bt = GameObject.Find("ButtonLose").GetComponent<Button>();
            bt.onClick.AddListener(OnClickLoseButton);
        }
        else if (_sceneState == SceneState.TitleScene)
        {
            // 初期化.
            _invaildFrame = TAP_INVAILD_TITLE;

            _fade = GameObject.Find("Fade").GetComponent<Fade>();

            Load();
        }
        else if (_sceneState == SceneState.GameScene)
        {
            // マネージャー取得.
            var mgr = GameObject.Find("Manager");
            _mgr = mgr.GetComponent<GameDirector>();
            _cameraMgr = mgr.GetComponent<CameraManager>();

            // コンポーネント取得.
            _hpBar = GameObject.Find("PlayerHpBar").GetComponent<Slider>();
            _hpText = GameObject.Find("PlayerHpValue").GetComponent<Text>();
            _spAnimator = GameObject.Find("SpGauge").GetComponent<Animator>();
            _seSource = GameObject.Find("Se").GetComponent<AudioSource>();
            _fade = GameObject.Find("Fade").GetComponent<Fade>();

            // オブジェクト取得.
            _playerCamera = GameObject.Find("PlayerCamera");
            Load();

            // 必殺技の関数をボタンに追加.
            var sp = GameObject.Find("SpGauge").GetComponent<Button>();
            sp.onClick.AddListener(OnSpAttack);

            // 画面サイズから最大引く距離、キャンセル距離を取得.
            var screen = new Vector2(Screen.width, Screen.height);
            _maxLen = screen.magnitude;
            _cancelLen = _maxLen * CANCEL_LEN_RATE;

            // はじめだけ判定を消す
            GetComponent<SphereCollider>().isTrigger = true;

            // 初期化.
            _animator.runtimeAnimatorController = _animMove;
            _isStart = true;
            _invaildFrame = START_INVAILD_NORMAL;
            _startPos = Vector3.zero;
            
            if (sceneName == "Stage1")
            {
                _hp = _maxHp;
                _spCount = 0;
            }
            else
            {
                _hp = GameData.playerHp;
                _spCount = GameData.spCount;

                if (_hp <= 0) _hp = _maxHp;

                _hpBar.value = (float)_hp / _maxHp;
            }
            _hpText.text = _hp.ToString() + " / " + _maxHp.ToString();

            if (_spCount < _spCost)
            {
                _spAnimRate = (float)_spCount / _spCost;
                _spAnimator.Play("SpChargeEffect", -1, _spAnimRate);
                _spAnimator.speed = 0;
            }
            else
            {
                _isChargeSp = true;
                _spAnimator.SetTrigger("OnMax");
                _spAnimator.speed = 1;
            }
        }
    }

    private void Load()
    {
        _flashPrefab     =  (GameObject)Resources.Load("PlayerSP/Flash");
        _sword1Prefab    =  (GameObject)Resources.Load("PlayerSP/Sword1");
        _sword2Prefab    =  (GameObject)Resources.Load("PlayerSP/Sword2");
        _arrowPrefab     =  (GameObject)Resources.Load("Arrow");
        _hitEffectPrefab =  (GameObject)Resources.Load("HitEffect");
    }

    private void OnDestroy()
    {
        GameData.playerHp = _hp;
        GameData.spCount = _spCount;
    }

    private void FixedUpdate()
    {
        --_invaildFrame;
        if (_sceneState == SceneState.EditScene)        EditUpdate();
        else if (_sceneState == SceneState.TitleScene)  TitleUpdate();
        else if (_sceneState == SceneState.GameScene)   GameUpdate();
    }

    private void Update()
    {
        if (_sceneState != SceneState.GameScene) return;

        if (_isUpdateSpAnim)
        {
            var time = _spAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (time > _spAnimRate)
            {
                _isUpdateSpAnim = false;
                _spAnimator.speed = 0;
                _spAnimator.Play("SpChargeEffect", -1, _spAnimRate);
            }
        }

        // 画面をタップしているとき
        if (_isPush)
        {
            // 画面をタップしている指の数を取得
            int count = 0;
            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.press.isPressed) count++;
                }
            }

            // 2本以上になったらカメラ操作のため、終了
            if (count >= 2)
            {
                _isPush = false;
                if (_isCreateArrow) Destroy(_arrowObj);
                _isCreateArrow = false;
            }
        }
    }


    /* 各シーン用のアップデート */
    private void EditUpdate()
    {
        // 現在のステートに合わせてアニメーションを決定
        if (_playerState == PlayerState.Idle)        _animator.runtimeAnimatorController = _animIdle;
        else if (_playerState == PlayerState.Move)   _animator.runtimeAnimatorController = _animMove;
        else if (_playerState == PlayerState.Attack) _animator.runtimeAnimatorController = _animAttack;
        else if (_playerState == PlayerState.Win)    _animator.runtimeAnimatorController = _animWin;
        else if (_playerState == PlayerState.Lose)   _animator.runtimeAnimatorController = _animLose;
    }

    private void TitleUpdate()
    {
        // 待機状態で固定
        _animator.runtimeAnimatorController = _animIdle;
    }

    private void GameUpdate()
    {
        if (_isStart)
        {
            transform.position += Vector3.forward * START_MOVE_SPEED;
            if (_invaildFrame < 0)
            {
                _isStart = false;

                _animator.runtimeAnimatorController = _animIdle;
                // 消していた判定を戻す
                GetComponent<SphereCollider>().isTrigger = false;
            }
        }

        if (_mgr.IsSpAttack)
        {
            if (_isSpCreate) return;

            --_invaildFrame;
            if (_invaildFrame < 0)
            {
                if (_spKind == SpAttackKind.Flash)
                {
                    var instance = Instantiate(_flashPrefab);
                    var script = instance.GetComponent<SpAttack>();
                    script.Init(_spPower, EXIST_FLASH_FRAME, DELAY_FLASH_FRAME, DIV_FLASH_NUM, DIV_FLASH_FRAME);
                }
                else if (_spKind == SpAttackKind.Sword_1)
                {
                    var instance = Instantiate(_sword1Prefab);
                    var script = instance.GetComponent<SpAttack>();
                    script.Init(_spPower, EXIST_SWORD_1_FRAME, DELAY_SWORD_1_FRAME, DIV_SWORD_1_NUM, DIV_SWORD_1_FRAME);
                }
                else if (_spKind == SpAttackKind.Sword_2)
                {
                    var instance = Instantiate(_sword2Prefab);
                    var script = instance.GetComponent<SpAttack>();
                    script.Init(_spPower, EXIST_SWORD_2_FRAME, DELAY_SWORD_2_FRAME, DIV_SWORD_2_NUM, DIV_SWORD_2_FRAME);
                }
                
                _isSpCreate = true;
            }

            return;
        }

        if (_mgr.IsNext) return;
        if (_mgr.IsLose || _mgr.IsClear) return;
        if (!_mgr.IsPlayerTurn) return;
        
        // 現時点での速度を保存
        if (_rigid.velocity.sqrMagnitude > 0.01f)
        {
            _preVel = _rigid.velocity;
        }
        
        if (_isMove)
        {
            if (_isShot)
            {
                _isShot = false;
                return;
            }
            if (_rigid.velocity.sqrMagnitude < 1.0f)
            {
                _rigid.velocity = Vector3.zero;
                _isMove = false;
                _animator.runtimeAnimatorController = _animIdle;
                _mgr.OnPlayerEnd();
            }
        }
    }

    /* 入力用メソッド */
    public void OnClickIdleButton()
    {
        _playerState = PlayerState.Idle;
    }
    public void OnClickMoveButton()
    {
        _playerState = PlayerState.Move;
    }
    public void OnClickAttackButton()
    {
        _playerState = PlayerState.Attack;
    }
    public void OnClickWinButton()
    {
        _playerState = PlayerState.Win;
    }
    public void OnClickLoseButton()
    {
        _playerState = PlayerState.Lose;
    }

    public void OnSpAttack()
    {
        if (!_mgr.IsPlayerTurn) return;
        if (_isShot || _isMove) return;
        if (!_isChargeSp) return;

        _spCount = 0;
        _isSpCreate = false;
        _isChargeSp = false;
        _invaildFrame = 15;
        _spAnimator.Play("SpChargeEffect", -1, 0);
        _spAnimator.speed = 0;

        if (_spKind == SpAttackKind.Flash)
        {
            _mgr.OnSpAttack(EXIST_FLASH_FRAME);
        }
        else if (_spKind == SpAttackKind.Sword_1)
        {
            _mgr.OnSpAttack(EXIST_SWORD_1_FRAME);
        }
        else if (_spKind == SpAttackKind.Sword_2)
        {
            _mgr.OnSpAttack(EXIST_SWORD_2_FRAME);
        }
    }

    public void OnTouch(InputAction.CallbackContext context)
    {
        if (_sceneState == SceneState.EditScene) return;
        if (IsInvaildControl()) return;

        // 押したとき
        if (context.started)
        {
            _isPush = true;
            // 押した座標を保存
            _startPos = _touchPos;
        }
        // 離したとき
        if (context.canceled && _isPush)
        {
            _isPush = false;

            // 矢印を消す
            if (_isCreateArrow)
            {
                Destroy(_arrowObj);
                _isCreateArrow = false;
            }

            // 離した座標を保存
            var endPos = _touchPos;

            var vec = _startPos - endPos;
            // 一定の長さ以上なら力を加える
            if (vec.magnitude > _cancelLen)
            {
                vec.Normalize();
                vec.z = vec.y;
                vec.y = 0;

                if (_cameraMgr.IsPlayerView)
                {
                    var cameraRot = _playerCamera.transform.localRotation * Quaternion.AngleAxis(-30, Vector3.right);
                    vec = cameraRot * vec;
                }

                _seSource.PlayOneShot(_shotSe);
                _animator.runtimeAnimatorController = _animAttack;
                var force = vec * _moveSpeed;
                _rigid.AddForce(force, ForceMode.Impulse);
                _preVel = force / _rigid.mass;
                _isShot = true;
                _isMove = true;
            }
        }
    }

    public void HoldTouch(InputAction.CallbackContext context)
    {
        if (_sceneState == SceneState.EditScene) return;
        // 押している場所を保存
        _touchPos = context.ReadValue<Vector2>();

        if (IsInvaildControl()) return;

        // 押しているとき
        if (_isPush)
        {
            var vec = _startPos - _touchPos;
            var length = vec.magnitude;
            // キャンセル距離以上のとき
            if (length > _cancelLen)
            {
                // 生成しているなら
                if (_isCreateArrow)
                {
                    // サイズの変更
                    var scale = _arrowObj.transform.localScale;
                    scale.y = 1 + 19 * length / _maxLen;
                    _arrowObj.transform.localScale = scale;

                    // 向きの変更
                    vec.z = vec.y;
                    vec.y = 0;
                    vec.Normalize();
                    var rot = Quaternion.FromToRotation(_arrowObj.transform.up, vec);

                    if (_cameraMgr.IsPlayerView)
                    {
                        var cameraRot = _playerCamera.transform.localRotation;

                        cameraRot.x = 0;
                        cameraRot.z = 0;
                        cameraRot.Normalize();
                        rot = cameraRot * rot;
                    }

                    _arrowObj.transform.rotation = rot * _arrowObj.transform.rotation;
                    rot = Quaternion.FromToRotation(transform.forward, _arrowObj.transform.up);
                    transform.rotation = rot * transform.rotation;
                }
                // 生成していないなら
                else
                {
                    var pos = this.transform.position;
                    pos.y = 0.6f;
                    _arrowObj = Instantiate(_arrowPrefab, pos, Quaternion.AngleAxis(90, Vector3.right));
                    _isCreateArrow = true;
                }
            }
            // キャンセル距離のとき
            else
            {
                // 生成済みなら一度消す
                if (_isCreateArrow)
                {
                    Destroy(_arrowObj);
                    _isCreateArrow = false;
                }
            }
        }
    }

    public void OnNextScene(InputAction.CallbackContext context)
    {
        if (_sceneState == SceneState.EditScene) return;
        if (_fade.IsNowFade) return;
        if (!context.started) return;

        if (_invaildFrame > 0) return;

        // 現在のステートに合わせてシーン切り替え
        if (_sceneState == SceneState.TitleScene)
        {
            _fade.OnFadeOut("Stage1");
        }
        else if (_sceneState == SceneState.GameScene)
        {
            if (_mgr.IsLose || _mgr.IsClear)
            {
                _fade.OnFadeOut("TitleScene");
            }
        }
    }

    /* 判定用メソッド */
    private void OnCollisionEnter(Collision collision)
    {
        // ゲームシーン以外なら無視
        if (_sceneState != SceneState.GameScene) return;
        // 床なら無視
        if (collision.gameObject.tag == "Ground") return;

        // 反射
        Reflect(collision.contacts[0].normal);

        // 敵と当たったらダメージを与える。
        if (collision.gameObject.tag == "Enemy")
        {
            collision.transform.GetComponent<Enemy>().OnDamage(_power);
            var pos = transform.position;
            pos.y += 1;
            Instantiate(_hitEffectPrefab, pos, Quaternion.identity);
        }
        // ボスと当たったらダメージを与える。
        else if (collision.gameObject.tag == "Boss")
        {
            collision.transform.GetComponent<Boss>().OnDamage(_power);
            var pos = transform.position;
            pos.y += 1;
            Instantiate(_hitEffectPrefab, pos, Quaternion.identity);
        }
    }

    /* その他メソッド */
    private void Reflect(Vector3 norm)
    {
        norm.y = 0;
        norm.Normalize();

        var newVel = Vector3.Reflect(_preVel, norm);

        // 速度変更
        _rigid.velocity = newVel * REFLECTION_LOSS_RATE;

        // オブジェクトの向きを更新
        var rot = Quaternion.FromToRotation(transform.forward, newVel);
        transform.rotation = rot * transform.rotation;

        // ぶつかったままにならないように少しだけ離す
        transform.position += norm * 0.5f;
    }

    public void OnDamage(int damage)
    {
        if (_mgr.IsLose) return;

        // HP減少
        _hp -= damage;
        _seSource.PlayOneShot(_damageSe);

        // 死亡判定
        if (_hp <= 0)
        {
            _hp = 0;

            _animator.runtimeAnimatorController = _animLose;

            _invaildFrame = TAP_INVAILD_MAIN;

            // 死亡したことを伝える
            _mgr.OnLose();
        }

        // Hpバー・テキスト更新
        _hpBar.value = (float)_hp / _maxHp;
        _hpText.text = _hp.ToString() + " / " + _maxHp.ToString();
    }

    public void AddSpCount()
    {
        if (_isChargeSp) return;

        ++_spCount;
        // たまり切っていないなら
        if (_spCount < _spCost)
        {
            _spAnimRate = (float)_spCount / _spCost;
            _spAnimator.speed = 1;
            _isUpdateSpAnim = true;
            _seSource.PlayOneShot(_spChargeSe);
        }
        else
        {
            _isChargeSp = true;
            _spAnimator.SetTrigger("OnMax");
            _spAnimator.speed = 1;
            _seSource.PlayOneShot(_spMaxSe);
        }
    }
    
    public void OnClear()
    {
        _animator.runtimeAnimatorController = _animWin;
        _invaildFrame = TAP_INVAILD_MAIN;
    }

    /// <summary>
    /// ゲームシーンで操作を受け付けないかどうか
    /// </summary>
    /// <returns>true: 受け付けない/ false: 受け付ける</returns>
    private bool IsInvaildControl()
    {
        // フェード中なら受け付けない
        if (_fade.IsNowFade) return true;

        // ゲームシーン以外なら受け付けない
        if (_sceneState != SceneState.GameScene) return false;

        // 初めの演出中なら受け付けない
        if (_isStart) return true;

        // 必殺技使っていないなら受け付けない
        if (_mgr.IsSpAttack) return true;

        // ボス攻撃中なら受け付けない
        if (_mgr.IsBossAttack) return true;

        // クリアor死んでるなら受け付けない
        if (_mgr.IsClear || _mgr.IsLose) return true;

        // 動いてるなら受け付けない
        if (_isMove) return true;

        // プレイヤーのターンでないなら受け付けない
        if (!_mgr.IsPlayerTurn) return true;

        return false;
    }

    // Editor用関数
    public RuntimeAnimatorController[] SaveAnimInfo()
    {
        RuntimeAnimatorController[] info = new RuntimeAnimatorController[5];

        info[0] = _animIdle;
        info[1] = _animMove;
        info[2] = _animAttack;
        info[3] = _animWin;
        info[4] = _animLose;

        return info;
    }

    public void ApplySavedAnimInfo(RuntimeAnimatorController[] info)
    {
        _animIdle = info[0];
        _animMove = info[1];
        _animAttack = info[2];
        _animWin = info[3];
        _animLose = info[4];
    }
}
