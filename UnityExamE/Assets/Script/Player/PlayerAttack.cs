using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject attackRangePrefab;   // 攻撃範囲を可視化するPrefab
    public Transform attackPoint;          // 攻撃の中心位置（Playerの前方）
    public float attackDuration = 0.4f;    // 攻撃範囲が残る時間
    public int attackDamage = 20;          // 与えるダメージ
    public float attackRange = 2f;         // 攻撃範囲半径
    private int attackPower;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Player Attack!");
            Attack();
        }
    }

    void Attack()
    {
        // 可視化用の範囲オブジェクトを生成
        GameObject currentRange = Instantiate(attackRangePrefab, attackPoint.position, attackPoint.rotation);
        Destroy(currentRange, attackDuration);

        // 範囲内の敵を検出してダメージを与える
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange);
        foreach (Collider enemy in hitEnemies)
        {
            EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
            if (enemyBase != null)
            {
                enemyBase.TakeDamage(attackPower);
            }

        }
    }
}
