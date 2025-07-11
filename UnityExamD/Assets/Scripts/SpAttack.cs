using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class SpAttack : MonoBehaviour
{
    private int _power;
    private int _existFrame;
    private int _attackDelayFrame;
    private int _divNum = 0;
    private int _divFrame;
    private bool _isAttacked = false;
    private GameDirector _mgr;

    void Start()
    {
        _mgr = GameObject.Find("Manager").GetComponent<GameDirector>();
    }

    
    void FixedUpdate()
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
                _mgr.OnAttackAllEnemy(_power);
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
