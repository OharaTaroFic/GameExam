using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    [SerializeField] private List<SkillData> skills; // プレイヤーが持つスキル
    private SkillExecutor executor;

    private void Awake()
    {
        executor = GetComponent<SkillExecutor>();
    }

    public void OnTrigger(SkillTriggerType triggerType)
    {
        foreach (var skill in skills)
        {
            if (skill.trigger == triggerType)
            {
                //executor.Execute(skill, transform);
            }
        }
    }
}
