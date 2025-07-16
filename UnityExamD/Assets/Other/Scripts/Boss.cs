using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.iOS;
using UnityEngine;
using UnityEngine.UI;

public enum BossAttackKind
{
    BloodLance,
    Meteor,
    Thunder,
}

public class Boss : MonoBehaviour
{
    // 攻撃生存フレーム
    private const int EXIST_BLOOD_LANCE_FRAME = 137;
    private const int EXIST_METEOR_FRAME = 117;
    private const int EXIST_THUNDER_FRAME = 50;
    // 攻撃発生ディレイフレーム
    private const int DELAY_BLOOD_LANCE_FRAME = 50;
    private const int DELAY_METEOR_FRAME = 52;
    private const int DELAY_THUNDER_FRAME = 8;
    // 攻撃回数
    private const int DIV_BLOOD_LANCE_NUM = 4;
    private const int DIV_METEOR_NUM = 3;
    // 攻撃間隔
    private const int DIV_BLOOD_LANCE_FRAME = 5;
    private const int DIV_METEOR_FRAME = 12;

    // パラメータ
    [Header("攻撃種類")]
    [SerializeField] private BossAttackKind _attackKind;
    [Header("ステータス")]
    [SerializeField] private int _maxHp = 100;
    [SerializeField] private int _attackTurn = 3;
    [SerializeField] private int _power = 20;
    [SerializeField] private int _score = 100;
    // 攻撃プレハブ
    [Space, Space, Space, Header("攻撃プレハブ")]
    [SerializeField] private GameObject _bloodLancePrefab;
    [SerializeField] private GameObject _meteorPrefab;
    [SerializeField] private GameObject _thunderPrefab;
    // 現在ステータス
    private int _turn;
    private int _hp;
    // その他変数
    [Header("演出用プレハブ")]
    [SerializeField] private GameObject _deathPrebaf;
    [Header("効果音")]
    [SerializeField] private AudioClip _damageSe;

    private AudioSource _seSource;
    private GameDirector _mgr;
    private ScoreManager _scoreMgr;
    private Slider _hpBar;
    private Text _hpText;
    private Text _countText;
    private int _invaildFrame = 0;
    private bool _isAttackCreate = false;

    private void Start()
    {
        var mgr = GameObject.Find("Manager");
        _mgr = mgr.GetComponent<GameDirector>();
        _scoreMgr = mgr.GetComponent<ScoreManager>();

        // コンポーネント取得
        _seSource = GameObject.Find("Se").GetComponent<AudioSource>();
        _hpBar = GameObject.Find("BossHpBar").GetComponent<Slider>();
        _hpText = GameObject.Find("BossHpValue").GetComponent<Text>();
        _countText = GameObject.Find("BossCountText").GetComponent<Text>();

        _hp = _maxHp;
        _turn = _attackTurn;
        _countText.text = _turn.ToString();
        _hpText.text = _hp.ToString()  + " / " + _maxHp.ToString();
    }

    private void FixedUpdate()
    {
        if (_mgr.IsBossAttack)
        {
            if (_isAttackCreate) return;

            --_invaildFrame;
            if (_invaildFrame < 0)
            {
                if (_attackKind == BossAttackKind.BloodLance)
                {
                    var instance = Instantiate(_bloodLancePrefab);
                    var script = instance.GetComponent<BossAttack>();
                    script.Init(_power, EXIST_BLOOD_LANCE_FRAME, DELAY_BLOOD_LANCE_FRAME, DIV_BLOOD_LANCE_NUM, DIV_BLOOD_LANCE_FRAME);
                }
                else if (_attackKind == BossAttackKind.Meteor)
                {
                    var instance = Instantiate(_meteorPrefab);
                    var script = instance.GetComponent<BossAttack>();
                    script.Init(_power, EXIST_METEOR_FRAME, DELAY_METEOR_FRAME, DIV_METEOR_NUM, DIV_METEOR_FRAME);
                }
                else if (_attackKind == BossAttackKind.Thunder)
                {
                    var instance = Instantiate(_thunderPrefab);
                    var script = instance.GetComponent<BossAttack>();
                    script.Init(_power, EXIST_THUNDER_FRAME, DELAY_THUNDER_FRAME);

                }

                _isAttackCreate = true;
                _turn = _attackTurn;
                _countText.text = _turn.ToString();
            }
        }
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
            // スコア加算
            _scoreMgr.AddScore(_score);
            // 死亡エフェクトをたく
            Instantiate(_deathPrebaf, transform.position, Quaternion.AngleAxis(90, Vector3.left));
            // マネージャーに死亡を伝える
            _mgr.DestroyBoss();
        }

        // Hpバー更新
        _hpBar.value = (float)_hp / _maxHp;
        _hpText.text = _hp.ToString() + " / " + _maxHp.ToString();
    }

    public void ReduceTurn()
    {
        --_turn;
        _countText.text = _turn.ToString();
        if (_turn <= 0)
        {
            _invaildFrame = 15;
            _isAttackCreate = false;
            if (_attackKind == BossAttackKind.BloodLance)
            {
                _mgr.OnBossAttack(EXIST_BLOOD_LANCE_FRAME);
            }
            else if (_attackKind == BossAttackKind.Meteor)
            {
                _mgr.OnBossAttack(EXIST_METEOR_FRAME);
            }
            else if (_attackKind == BossAttackKind.Thunder)
            {
                _mgr.OnBossAttack(EXIST_THUNDER_FRAME);
            }
        }
    }
}
