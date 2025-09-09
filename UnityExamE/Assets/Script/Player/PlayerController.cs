using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public PuniconDebug puniCon;

    private float speedMultiplier = 1f;

    // 攻撃ステータス
    [Header("攻撃ステータス")]
    public int baseAttack = 10;
    private float attackMultiplier = 1f;

    // HP管理
    public int maxHP = 100;
    private int currentHP;

    private Transform cam;

    // === スキル ===
    [Header("スキル設定")]
    public SkillBase skill1; // バフ
    public SkillBase skill2; // 攻撃
    private float skill1CooldownTimer = 0f;
    private float skill2CooldownTimer = 0f;

    void Start()
    {
        currentHP = maxHP;
        cam = Camera.main.transform;
    }

    void Update()
    {
        HandleMovement();
        HandleSkillInput();
        UpdateCooldowns();
    }

    private void HandleMovement()
    {
        Vector2 input = puniCon.GetInput();

        Vector3 forward = cam.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 right = cam.right;
        right.y = 0f;
        right.Normalize();

        Vector3 move = forward * input.y + right * input.x;
        transform.position += move * moveSpeed * speedMultiplier * Time.deltaTime;

        if (move.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                Time.deltaTime * 10f
            );
        }
    }

    private void HandleSkillInput()
    {
        // 長押し検出は既に実装済みとのことなので、ここは呼び出し用だけ
        if (Input.GetKeyDown(KeyCode.Alpha1) && skill1CooldownTimer <= 0f)
        {
            skill1?.Activate(this);
            skill1CooldownTimer = skill1.cooldown;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2) && skill2CooldownTimer <= 0f)
        {
            skill2?.Activate(this);
            skill2CooldownTimer = skill2.cooldown;
        }
    }

    private void UpdateCooldowns()
    {
        if (skill1CooldownTimer > 0f) skill1CooldownTimer -= Time.deltaTime;
        if (skill2CooldownTimer > 0f) skill2CooldownTimer -= Time.deltaTime;
    }

    // ===== 攻撃力関連 =====
    public int GetAttackPower()
    {
        return Mathf.RoundToInt(baseAttack * attackMultiplier);
    }
    public void SetAttackMultiplier(float multiplier)
    {
        attackMultiplier = multiplier;
    }

    // ===== 移動速度バフ用 =====
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    // ===== HP処理 =====
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
    }
}
