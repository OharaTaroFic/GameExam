using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public PuniconDebug puniCon;

    private float speedMultiplier = 1f;

    // HP管理
    public int maxHP = 100;
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
    }

    void Update()
    {
        Vector2 input = puniCon.GetInput();
        Vector3 move = new Vector3(input.x, 0, input.y);
        transform.position += move * moveSpeed * speedMultiplier * Time.deltaTime;
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // ===== ダメージ処理 =====
    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

        Debug.Log($"Playerが{damage}ダメージを受けた！ 残りHP: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Playerは倒れた...");
        // ゲームオーバー処理をここに書く
    }
}
