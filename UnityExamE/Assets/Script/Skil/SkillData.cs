using UnityEngine;

public enum SkillTriggerType
{
    NormalAttack, ChargeAttack, RangedAttack,
    Dash, Charging, OnHit, Passive
}

public enum SkillCategory
{
    Attack, Buff, Debuff
}

[CreateAssetMenu(menuName = "Skill/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Šî–{î•ñ")]
    public string skillName;
    public SkillCategory category;
    public SkillTriggerType trigger;
    public Sprite icon;
    public float spCost;
    public float cooldown;

    [Header("UŒ‚Œnƒpƒ‰ƒ[ƒ^")]
    public float damage;
    public float range;
    public GameObject effectPrefab;

    [Header("‹­‰»/ã‘ÌŒnƒpƒ‰ƒ[ƒ^")]
    public float value;       // ‹­‰»”{—¦ or ã‘Ì”{—¦
    public float duration;    // Œø‰ÊŠÔ
}