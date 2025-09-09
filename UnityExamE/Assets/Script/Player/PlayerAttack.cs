using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("攻撃設定")]
    public float attackRange = 2f;       // 攻撃の範囲
    public int attackDamage = 10;        // 与えるダメージ
    public Transform attackPoint;        // 攻撃の中心（プレイヤー前方にEmptyを置くと良い）

    [Header("敵レイヤー指定")]
    public LayerMask enemyLayer;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 左クリック
        {
            Attack();
        }
    }

    void Attack()
    {
        // 攻撃範囲内の敵を検出
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider enemy in hitEnemies)
        {
            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(attackDamage);
            }
        }
    }

    // Sceneビューで攻撃範囲を見やすくする
    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
