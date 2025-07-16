using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameData
{
    public static int score;
    public static int spCount;
    public static int playerHp;
}

public class GameDirector : MonoBehaviour
{
    struct EnemyData
    {
        public GameObject obj;
        public Enemy script;
    }

    struct BossData
    {
        public GameObject obj;
        public Boss script;
    }

    private const int CHNAGE_NEXT_FRAME = 25;

    private const int START_DELAY_FRAME = 75;
    private const int CHANGE_TURN_FRAME = 25;

    private Fade _fade;
    private CameraManager _cameraMgr;

    private Animator _animEffect;
    private Animator _turnAnim;
    private Player _player;
    private PlayerCamera _playerCamera;
    private BossData _boss;
    private List<EnemyData> _enemyList = new List<EnemyData>();
    private List<GameObject> _destoryList = new List<GameObject>();

    private int _frame = 0;

    private bool _isPlayerTurn = true;
    private bool _isReduceTurn = true;

    private bool _isSpAttack = false;
    private bool _isBossAttack = false;

    private bool _isStart = true;
    private bool _isBoss = false;
    private bool _isChangeTurn = false;
    private bool _isClear = false;
    private bool _isLose = false;
    private bool _isNext = false;

    [SerializeField] private AudioClip _clearBgm;
    [SerializeField] private AudioClip _loseBgm;
    [SerializeField] private AudioClip _warningSe;
    private AudioSource _bgmSource;
    private AudioSource _seSource;

    public bool IsPlayerTurn { get { return _isPlayerTurn; } }
    public bool IsSpAttack {  get { return _isSpAttack; } }
    public bool IsBossAttack { get { return _isBossAttack; } }
    public bool IsClear { get { return _isClear; } }
    public bool IsLose { get { return _isLose; } }
    public bool IsNext { get { return _isNext; } }

    void Start()
    {
        _fade = GameObject.Find("Fade").GetComponent<Fade>();
        _cameraMgr = GetComponent<CameraManager>();
        _player = GameObject.Find("Player").GetComponent<Player>();
        _playerCamera = GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>();
        _animEffect = GameObject.Find("Effect").GetComponent<Animator>();
        _turnAnim = GameObject.Find("TurnEffect").GetComponent<Animator>();
        _bgmSource = GameObject.Find("Bgm").GetComponent<AudioSource>();
        _seSource = GameObject.Find("Se").GetComponent<AudioSource>();

        var list = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
        foreach (var item in list)
        {
            EnemyData data;
            data.obj = item;
            data.script = item.GetComponent<Enemy>();
            _enemyList.Add(data);
        }

        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "BossStage")
        {
            _boss = new BossData();
            _boss.obj = GameObject.Find("Boss");
            _boss.script = _boss.obj.GetComponent<Boss>();
            _isBoss = true;
            _turnAnim.SetTrigger("OnBossEnter");
            _turnAnim.speed = 1.0f;
            _bgmSource.Stop();
            _seSource.PlayOneShot(_warningSe);
        }
        else
        {
            _turnAnim.speed = 0;
            // 何も設置されていないときは強制的にクリアシーンへ
            if (_enemyList.Count <= 0)
            {
                _isClear = true;
                return;
            }
        }
        
        _frame = START_DELAY_FRAME;
        _animEffect.speed = 0;
    }

    void FixedUpdate()
    {
        if (_isStart)
        {
            --_frame;
            if (_frame < 0)
            {
                _isStart = false;
                _frame = CHANGE_TURN_FRAME;
                _isChangeTurn = true;
                if (_isBoss)
                {
                    _turnAnim.SetTrigger("OnPlayer");
                    _bgmSource.Play();
                }
                _turnAnim.speed = 1;
            }
            return;
        }

        if (_fade.IsNowFade) return;
        if (_isClear || _isLose) return;

        if (_isNext)
        {
            --_frame;
            if (_frame < 0)
            {
                var name = SceneManager.GetActiveScene().name;
                if (name == "Stage1") _fade.OnFadeOut("Stage2");
                else if (name == "Stage2") _fade.OnFadeOut("BossStage");
                
            }
            return;
        }

        if (_isChangeTurn)
        {
            --_frame;
            if (_frame < 0)
            {
                if (_isReduceTurn)
                {
                    _isPlayerTurn = true;
                }
                _isChangeTurn = false;
            }
            return;
        }

        foreach (var item in _destoryList)
        {
            int size = _enemyList.Count;
            for (int i = 0; i < size; ++i)
            {
                var data = _enemyList[i];
                if (data.obj != item) continue;

                _enemyList.RemoveAt(i);
                break;
            }
        }

        if (_isSpAttack || _isBossAttack)
        {
            --_frame;
            if (_frame < 0)
            {
                _isSpAttack = false;
                _isBossAttack = false;
                _cameraMgr.EndMove();

                CheckClear();
                return;
            }

            return;
        }

        // プレイヤーのターンでないとき
        if (!_isPlayerTurn)
        {
            // ターンを減らしていないなら
            if (!_isReduceTurn)
            {
                // 敵のターンを減らす
                foreach (var enemy in _enemyList)
                {
                    enemy.script.ReduceTurn();
                }

                // ボスのターンを減らす
                if (_isBoss) _boss.script.ReduceTurn();

                _isReduceTurn = true;
            }
            // ターンを減らしているなら
            else
            {
                bool isAllAttack = true;
                // 敵全員が攻撃完了したらプレイヤーにターンを返す
                foreach (var enemy in _enemyList)
                {
                    if (enemy.script.IsAttack)
                    {
                        isAllAttack = false;
                        break;
                    }
                }

                if (isAllAttack)
                {
                    _isChangeTurn = true;
                    _frame = CHANGE_TURN_FRAME;
                    _turnAnim.SetTrigger("OnPlayer");
                    _player.AddSpCount();
                }
            }
        }
    }

    public void OnPlayerEnd()
    {
        _isReduceTurn = false;
        _isPlayerTurn = false;

        if (CheckClear()) return;

        _isChangeTurn = true;
        _frame = CHANGE_TURN_FRAME;
        _turnAnim.SetTrigger("OnEnemy");
    }

    private bool CheckClear()
    {
        if (_isBoss)
        {
            if (_boss.obj == null)
            {
                OnClear();
                return true;
            }
        }
        else
        {
            if (_enemyList.Count <= 0)
            {
                OnClear();
                return true;
            }
        }
        return false;
    }

    public void DestroyEnemy(GameObject obj)
    {
        Destroy(obj);
        _destoryList.Add(obj);
    }

    public void DestroyBoss()
    {
        Destroy(_boss.obj);
        _boss.obj = null;
    }

    public void OnSpAttack(int frame)
    {
        _frame = frame + PlayerCamera.MOVE_SP_BOSS_FRAME * 2;
        _isSpAttack = true;
        _isBossAttack = false;
        _cameraMgr.StartMove();
        _playerCamera.OnSpCameraView(frame);
    }

    public void OnBossAttack(int frame)
    {
        _frame = frame + PlayerCamera.MOVE_SP_BOSS_FRAME * 2;
        _isSpAttack = false;
        _isBossAttack = true;
        _cameraMgr.StartMove();
        _playerCamera.OnSpCameraView(frame);
    }

    public void OnAttackAllEnemy(int power)
    {
        foreach (var item in _enemyList)
        {
            item.script.OnDamage(power);
        }

        if (_isBoss) _boss.script.OnDamage(power);
    }

    public void OnClear()
    {
        var name = SceneManager.GetActiveScene().name;
        if (name == "BossStage")
        {
            _isClear = true;
            _animEffect.SetTrigger("OnClear");
            _animEffect.speed = 1;
            _player.OnClear();
            _bgmSource.Stop();
            _bgmSource.PlayOneShot(_clearBgm);
        }
        else
        {
            _cameraMgr.StartMove();
            GameObject.Find("PlayerCamera").GetComponent<PlayerCamera>().OnNextStage();
            _frame = CHNAGE_NEXT_FRAME;
            _isNext = true;
        }
    }

    public void OnLose()
    {
        _animEffect.SetTrigger("OnLose");
        _animEffect.speed = 1;
        _isLose = true;
        _bgmSource.Stop();
        _bgmSource.PlayOneShot(_loseBgm);
    }
}
