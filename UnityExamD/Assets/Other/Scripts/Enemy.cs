using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EnemyAttackKind
{
    Explosion,
    DiffusionBullet,
    Laser,
}

public class Enemy : MonoBehaviour
{
    private const int EXIST_FRAME_EXPLOSION = 75;
    private const int EXIST_FRAME_BULLET = 50;
    private const int EXIST_FRAME_LASER = 34;
    private const int ATTACK_WAIT_EXPLOSION = 50;
    private const int ATTACK_WAIT_LASER = 9;
    
    // パラメーター
    [Header("ステータス")]
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _attackTurn = 3;
    [SerializeField] private int _power = 20;
    [SerializeField] private float _attackSize = 1.0f;
    [SerializeField] private int _score = 100;
    [SerializeField] private EnemyAttackKind _attackKind;
    // 攻撃プレハブ
    private GameObject _explosionPrefab;
    private GameObject _bulletPrefab;
    private GameObject _laserPrefab;
    // 現在ステータス
    private int _turn;
    private int _hp;
    private int _waitFrame;
    private bool _isAttack;
    // その他変数
    private GameObject _deathPrebaf;
    [Header("効果音")]
    [SerializeField] private AudioClip _damageSe;

    private AudioSource _seSource;
    private GameObject _player;
    private GameDirector _mgr;
    private ScoreManager _scoreMgr;
    private Transform _canvas;
    private Slider _hpBar;
    private Text _countText;

    public bool IsAttack { get { return _isAttack; } }

    private void Start()
    {
        var mgr = GameObject.Find("Manager");
        _mgr = mgr.GetComponent<GameDirector>();
        _scoreMgr = mgr.GetComponent<ScoreManager>();
        _canvas = transform.GetChild(0).transform;
        _hpBar = _canvas.GetChild(0).GetComponent<Slider>();
        _countText = _canvas.GetChild(2).GetComponent<Text>();

        _player = GameObject.Find("Player");
        _seSource = GameObject.Find("Se").GetComponent<AudioSource>();

        _explosionPrefab = (GameObject)Resources.Load("EnemyAttack/Explotion");
        _bulletPrefab    = (GameObject)Resources.Load("EnemyAttack/Bullet");
        _laserPrefab     = (GameObject)Resources.Load("EnemyAttack/Laser");
        _deathPrebaf     = (GameObject)Resources.Load("Death/EnemyDeath");

        _hp = _maxHp;
        _turn = _attackTurn;

        _countText.text = _turn.ToString();
    }

    private void FixedUpdate()
    {
        // 攻撃中のとき
        if (_isAttack)
        {
            --_waitFrame;
            if (_waitFrame <= 0)
            {
                _isAttack = false;
                _countText.text = _turn.ToString();
            }
        }

        // Hpの向きをプレイヤーに向くように
        _canvas.LookAt(_player.transform, Vector3.up);
        _canvas.localRotation = Quaternion.AngleAxis(180, Vector3.up) * _canvas.localRotation;
    }

    public void OnDamage(int damage)
    {
        if (_hp <= 0) return;

        // Hp減少
        _hp -= damage;
        _seSource.PlayOneShot(_damageSe);

        // Hpが0以下になったら
        if (_hp <= 0)
        {
            _hp = 0;
            // スコアを加算
            _scoreMgr.AddScore(_score);
            // 死亡エフェクトをたく
            Instantiate(_deathPrebaf, transform.position, Quaternion.identity);
            // マネージャーに死んだことを伝える
            _mgr.DestroyEnemy(this.gameObject);
        }

        // Hpバー更新
        _hpBar.value = (float)_hp / _maxHp;
    }

    public void ReduceTurn()
    {
        --_turn;
        _countText.text = _turn.ToString();
        if (_turn <= 0)
        {
            _turn = _attackTurn;
            _isAttack = true;
            // 種類に合わせて生成
            // 白爆発
            if (_attackKind == EnemyAttackKind.Explosion)
            {
                // 待機時間設定
                _waitFrame = EXIST_FRAME_EXPLOSION;

                // 生成・設定
                var instance = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
                instance.transform.localScale *= _attackSize;
                instance.GetComponentInChildren<EnemyAttack>().Init(_power, EXIST_FRAME_EXPLOSION, ATTACK_WAIT_EXPLOSION);
            }
            // 拡散弾
            else if (_attackKind == EnemyAttackKind.DiffusionBullet)
            {
                // 待機時間設定
                _waitFrame = EXIST_FRAME_BULLET;

                // 生成・設定
                var dir = Vector3.forward;
                for (int i = 0; i < 8; ++i)
                {
                    var instance = Instantiate(_bulletPrefab, transform.position, Quaternion.identity);
                    instance.transform.localScale *= _attackSize;
                    instance.GetComponentInChildren<EnemyAttack>().Init(_power, _waitFrame, 0, dir);
                    dir = Quaternion.AngleAxis(45, Vector3.up) * dir;
                }
            }
            // レーザー
            else if (_attackKind == EnemyAttackKind.Laser)
            {
                // 待機時間設定
                _waitFrame = EXIST_FRAME_LASER;

                // プレイヤーの方向を取得
                var dir = _player.transform.position - transform.position;
                dir.y = 0;
                dir.Normalize();
                // 正面方向からプレイヤーの方向までの角度・回転軸を取得
                var dot = Vector3.Dot(Vector3.back, dir);
                var angle = Mathf.Acos(dot) * Mathf.Rad2Deg;
                var axis = Vector3.Cross(Vector3.back, dir);
                // 回転クオータニオン生成
                var rot = Quaternion.AngleAxis(angle, axis);

                // 生成・設定
                var instance = Instantiate(_laserPrefab, transform.position, rot);
                instance.transform.localScale *= _attackSize;
                instance.GetComponentInChildren<EnemyAttack>().Init(_power, EXIST_FRAME_LASER, ATTACK_WAIT_LASER);
            }
        }
    }
}
