using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public float stayTime = 1.0f;  // Base円の中にいる必要時間
    public float baseThreshold = 0.3f; // Base円の中とみなす閾値
    public PuniconDebug puniCon;
    public PlayerController player;

    private float stayTimer = 0f;
    private bool isCharging = false;
    private int chosenIndex = -1;

    // スキル方向（左上, 右上, 下）
    private Vector2[] directions = new Vector2[]
    {
        new Vector2(-1, 1).normalized,  // スキル1（左上）
        new Vector2(1, 1).normalized,   // スキル2（右上）
        new Vector2(0, -1)              // スキル3（下）
    };

    private string[] skillNames = new string[]
    {
        "スキル1（左上）",
        "スキル2（右上）",
        "スキル3（下）"
    };

    void Update()
    {
        Vector2 input = puniCon.GetInput();

        if (Input.GetMouseButton(0)) // マウスを押してる間
        {
            if (!isCharging)
            {
                // Base円の範囲内にいるかチェック
                if (input.magnitude <= baseThreshold)
                {
                    stayTimer += Time.deltaTime;
                    if (stayTimer >= stayTime)
                    {
                        StartCharge();
                    }
                }
                else
                {
                    stayTimer = 0f; // 外に出たらリセット
                }
            }
            else
            {
                UpdateSkillSelection(input);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isCharging)
            {
                if (chosenIndex != -1)
                {
                    ActivateSkill(chosenIndex);
                }
                else
                {
                    Debug.Log("スキル選択キャンセル");
                }
            }
            CancelCharge();
        }
    }

    void StartCharge()
    {
        isCharging = true;
        chosenIndex = -1;
        player.SetSpeedMultiplier(0.4f); // 移動速度を40%に
        Debug.Log("=== スキル選択モード開始 ===");
    }

    void UpdateSkillSelection(Vector2 input)
    {
        if (input.magnitude < 0.3f)
        {
            chosenIndex = -1;
            return;
        }

        int tempChosen = -1;
        float maxDot = 0.7f;

        for (int i = 0; i < directions.Length; i++)
        {
            float dot = Vector2.Dot(input.normalized, directions[i]);
            if (dot > maxDot)
            {
                tempChosen = i;
                break;
            }
        }

        chosenIndex = tempChosen;
    }

    void ActivateSkill(int index)
    {
        Debug.Log(skillNames[index] + " 発動！");
    }

    void CancelCharge()
    {
        isCharging = false;
        stayTimer = 0f;
        chosenIndex = -1;
        player.SetSpeedMultiplier(1f);
    }
}
