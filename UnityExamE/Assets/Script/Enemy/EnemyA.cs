using UnityEngine;

public class EnemyA : EnemyBase
{
    public GameObject attackHitbox; // Unityでアサイン（BoxやSphereで可視化可）

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
            Debug.Log($"{gameObject.name} が近接攻撃を開始！");

            StartCoroutine(AttackRoutine());
        }
    }

    private System.Collections.IEnumerator AttackRoutine()
    {
        // ため攻撃の時間
        Debug.Log($"{gameObject.name} が攻撃をためている...");
        yield return new WaitForSeconds(0.5f);

        // 攻撃判定ON
        attackHitbox.SetActive(true);
        Debug.Log($"{gameObject.name} の攻撃判定が有効化！");

        yield return new WaitForSeconds(0.3f);

        // 攻撃判定OFF
        attackHitbox.SetActive(false);
        Debug.Log($"{gameObject.name} の攻撃判定が終了！");
    }
}
