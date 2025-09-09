using UnityEngine;

public abstract class SkillBase : ScriptableObject
{
    public string skillName;
    public float cooldown = 1f;

    // ”­“®ˆ—
    public abstract void Activate(PlayerController player);
}
