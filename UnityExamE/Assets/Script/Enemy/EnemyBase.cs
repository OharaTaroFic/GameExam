using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("共通設定")]
    public float moveSpeed = 2f;
    public float attackRange = 2f;
    public float detectionRange = 5f;

    [Header("攻撃関連")]
    public float attackInterval = 1.5f; // 攻撃間隔
    protected float attackTimer = 0f;

    protected Transform player;

    protected enum State { Idle, Chase, Attack }
    protected State currentState = State.Idle;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        // 攻撃タイマーを進める（範囲外でも進めてOK）
        attackTimer += Time.deltaTime;

        StateMachine();
    }

    void StateMachine()
    {
        switch (currentState)
        {
            case State.Idle: Idle(); break;
            case State.Chase: Chase(); break;
            case State.Attack: Attack(); break;
        }
    }

    protected virtual void Idle()
    {
        if (Vector3.Distance(transform.position, player.position) < detectionRange)
            ChangeState(State.Chase);
    }

    protected virtual void Chase()
    {
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            ChangeState(State.Attack);
            return;
        }

        Vector3 dir = (player.position - transform.position).normalized;
        transform.position += dir * moveSpeed * Time.deltaTime;
        transform.LookAt(player);
    }

    protected virtual void Attack()
    {
        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;

            Debug.Log($"{gameObject.name} が攻撃した！");

            // ★ Playerにダメージを与える
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(10); // 敵からのダメージは固定10
            }
        }
    }

    protected void ChangeState(State nextState)
    {
        currentState = nextState;
    }
}