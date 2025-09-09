using UnityEngine;

[CreateAssetMenu(menuName = "Skills/AttackSkill")]
public class AttackSkill : SkillBase
{
    public int damage = 20;
    public float radius = 2f;

    public override void Activate(PlayerController player)
    {
        // プレイヤー周囲の敵にダメージ（Prefab不要）
        Collider[] hitEnemies = Physics.OverlapSphere(player.transform.position, radius, LayerMask.GetMask("Enemy"));

        foreach (Collider enemy in hitEnemies)
        {
            EnemyBase enemyScript = enemy.GetComponent<EnemyBase>();
            if (enemyScript != null)
            {
                enemyScript.TakeDamage(damage);
            }
        }

        Debug.Log($"[{skillName}] 発動！ {hitEnemies.Length}体の敵に{damage}ダメージ");
    }

    // Sceneビューで範囲確認用
    public void OnDrawGizmos(PlayerController player)
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.transform.position, radius);
    }
}
