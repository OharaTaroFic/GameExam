using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    public float attackRange = 2f;       // 攻撃の範囲
    public Transform attackPoint;        // 攻撃の中心
    [Header("敵レイヤー指定")]
    public LayerMask enemyLayer;

    private PlayerController player;

    void Start()
    {
        player = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            Attack();
        }
    }

    void Attack()
    {
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(player.GetAttackPower());
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
