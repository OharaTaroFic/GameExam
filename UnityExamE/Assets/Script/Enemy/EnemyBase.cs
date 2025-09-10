using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    public int maxHP = 50;
    protected int currentHP;

    public float moveSpeed = 3f;
    public float detectionRange = 10f;
    public float attackRange = 2f;
    protected float attackTimer = 0f;
    public float attackInterval = 2f;

    protected Transform player;

    protected enum State { Idle, Chase, Attack }
    protected State currentState = State.Idle;

    protected virtual void Start()
    {
        currentHP = maxHP;
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        attackTimer += Time.deltaTime;

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
        // 具体的な攻撃処理は子クラスで
    }

    protected void ChangeState(State nextState)
    {
        currentState = nextState;
    }

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

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
