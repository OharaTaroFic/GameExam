using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttack : MonoBehaviour
{
    private int _power;
    private int _existFrame;
    private int _attackDelayFrame;
    private int _divNum = 0;
    private int _divFrame;
    private bool _isAttacked = false;
    private Player _player;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
    }

    private void FixedUpdate()
    {
        --_existFrame;
        if (_existFrame < 0)
        {
            Destroy(this.gameObject);
            return;
        }

        if (!_isAttacked)
        {
            --_attackDelayFrame;
            if (_attackDelayFrame < 0)
            {
                _player.OnDamage(_power);
                --_divNum;
                _isAttacked = _divNum < 0;
                _attackDelayFrame = _divFrame;
            }
        }
    }

    public void Init(int power, int existFrame, int attackDelayFrame)
    {
        _power = power;
        _existFrame = existFrame;
        _attackDelayFrame = attackDelayFrame;
    }

    public void Init(int power, int existFrame, int attackDelayFrame, int divNum, int divFrame)
    {
        _power = power / divNum;
        _existFrame = existFrame;
        _attackDelayFrame = attackDelayFrame;
        _divNum = divNum;
        _divFrame = divFrame;
    }
}
