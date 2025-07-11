using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    private const float MOVE_SPEED = 0.05f;
    private const float PC_DELTA_TIME = 0.0025f;

    private int _power;
    private int _existFrame;
    private int _attackDelayFrame;
    private Vector3 _dir;
    private Transform _trans;
    private Player _player;
    private bool _isAttack = false;
    private bool _isAttacked = false;
    
    private void Start()
    {
        _trans = transform.GetComponent<Transform>();
    }

    private void FixedUpdate()
    {
        --_existFrame;
        if (_existFrame < 0)
        {
            if (_isAttack && !_isAttacked) _player.OnDamage(_power);

            Destroy(transform.gameObject);
            return;
        }
        if (_isAttack && !_isAttacked)
        {
            --_attackDelayFrame;
            if (_attackDelayFrame < 0)
            {
                _isAttacked = true;
                _player.OnDamage(_power);
            }
        }
    }

    private void Update()
    {
        _trans.position += _dir * MOVE_SPEED * (Time.deltaTime / PC_DELTA_TIME);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isAttack) return;
        if (other.tag != "Player") return;
        _isAttack = true;
        _player = other.GetComponent<Player>();
    }

    public void Init(int power, int existFrame, int attackDelayFrame)
    {
        _power = power;
        _existFrame = existFrame;
        _attackDelayFrame = attackDelayFrame;
        _dir = Vector3.zero;
    }

    public void Init(int power, int existFrame, int attackDelayFrame, Vector3 dir)
    {
        _power = power;
        _existFrame = existFrame;
        _attackDelayFrame = attackDelayFrame;
        _dir = dir;
    }
}
