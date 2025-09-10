using UnityEngine;

public class EnemyBoss : EnemyBase
{
    public GameObject attackHitbox; // 広い範囲攻撃用HitboxをUnityでアサイン

    protected override void Attack()
    {
        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f;
            Debug.Log($"{gameObject.name} が大技を開始！");

            StartCoroutine(BossAttackRoutine());
        }
    }

    private System.Collections.IEnumerator BossAttackRoutine()
    {
        Debug.Log($"{gameObject.name} が力をためている...！");
        yield return new WaitForSeconds(1.0f); // 長めのため時間

        attackHitbox.SetActive(true);
        Debug.Log($"{gameObject.name} の範囲攻撃が発動！");

        yield return new WaitForSeconds(0.5f);

        attackHitbox.SetActive(false);
        Debug.Log($"{gameObject.name} の範囲攻撃が終了！");
    }
}
