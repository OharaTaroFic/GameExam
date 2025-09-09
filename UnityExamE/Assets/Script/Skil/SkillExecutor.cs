using UnityEngine;

public class SkillExecutor : MonoBehaviour
{
    public PlayerController player;
    public SkillSlotManager skillSlots;

    private float[] cooldownTimers;

    void Start()
    {
        if (skillSlots == null || skillSlots.equippedSkills == null) return;
        cooldownTimers = new float[skillSlots.equippedSkills.Length];
    }

    void Update()
    {
        if (skillSlots == null || skillSlots.equippedSkills == null) return;

        for (int i = 0; i < cooldownTimers.Length; i++)
        {
            if (cooldownTimers[i] > 0) cooldownTimers[i] -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1)) TryUseSkill(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) TryUseSkill(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) TryUseSkill(2);
    }

    void TryUseSkill(int index)
    {
        if (index >= skillSlots.equippedSkills.Length) return;
        var skill = skillSlots.equippedSkills[index];
        if (skill == null) return;
        if (cooldownTimers[index] > 0) return;

        skill.Activate(player);
        cooldownTimers[index] = skill.cooldown;
    }
}
