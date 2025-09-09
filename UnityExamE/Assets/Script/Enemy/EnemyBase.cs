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
    public int attackDamage = 10; // プレイヤーに与えるダメージ

    [Header("HP関連")]
    public int maxHP = 50;
    protected int currentHP;

    protected Transform player;

    protected enum State { Idle, Chase, Attack }
    protected State currentState = State.Idle;

    protected virtual void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentHP = maxHP;
    }

    protected virtual void Update()
    {
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
        // プレイヤーが範囲外に出たら追跡に戻る
        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        // クールタイムが終わっていたら攻撃
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Debug.Log($"{gameObject.name} が攻撃した！");

            // Playerにダメージを与える
            PlayerController pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                pc.TakeDamage(attackDamage);
            }
        }
    }

    protected void ChangeState(State nextState)
    {
        currentState = nextState;
    }

    // ===== HP処理 =====
    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log($"{gameObject.name} が {damage} ダメージを受けた！ 残りHP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Debug.Log($"{gameObject.name} は倒れた！");
        Destroy(gameObject);
    }
}