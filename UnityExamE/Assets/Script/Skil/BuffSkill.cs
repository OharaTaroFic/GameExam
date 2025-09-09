using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/BuffSkill")]
public class BuffSkill : SkillBase
{
    public float duration = 5f;
    public float attackMultiplier = 1f;
    public float moveSpeedMultiplier = 1f;

    public override void Activate(PlayerController player)
    {
        player.StartCoroutine(BuffRoutine(player));
    }

    private IEnumerator BuffRoutine(PlayerController player)
    {
        player.SetAttackMultiplier(attackMultiplier);
        player.SetSpeedMultiplier(moveSpeedMultiplier);

        Debug.Log($"[{skillName}] 発動！ 攻撃力x{attackMultiplier}, 移動速度x{moveSpeedMultiplier}");

        yield return new WaitForSeconds(duration);

        player.SetAttackMultiplier(1f);
        player.SetSpeedMultiplier(1f);

        Debug.Log($"[{skillName}] バフ終了");
    }
}
