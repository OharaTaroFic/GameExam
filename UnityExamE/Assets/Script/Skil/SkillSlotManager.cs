using UnityEngine;

[CreateAssetMenu(menuName = "Skill/SkillSlotManager")]
public class SkillSlotManager : ScriptableObject
{
    public SkillBase[] equippedSkills; // 0=攻撃,1=バフ,2=デバフなど
}
