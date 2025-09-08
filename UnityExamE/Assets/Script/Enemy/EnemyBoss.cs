using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class EnemyBoss : EnemyBase
{
    private int phase = 1;

    protected override void Update()
    {
        base.Update();
        UpdatePhase();
    }

    void UpdatePhase()
    {
        // HP管理はまだ入れてないのでテスト用に距離でフェーズ切替
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > 6f) phase = 1;
        else if (dist > 3f) phase = 2;
        else phase = 3;
    }

    protected override void Attack()
    {
        if (attackTimer < attackInterval) return;

        attackTimer = 0f;

        switch (phase)
        {
            case 1:
                Phase1Attack();
                break;
            case 2:
                Phase2Attack();
                break;
            case 3:
                Phase3Attack();
                break;
        }

        if (Vector3.Distance(transform.position, player.position) > attackRange)
            ChangeState(State.Chase);
    }

    void Phase1Attack()
    {
        Debug.Log("ボス フェーズ1：近接攻撃");
        DamagePlayer();
    }

    void Phase2Attack()
    {
        int pattern = Random.Range(0, 2);
        if (pattern == 0)
        {
            Debug.Log("ボス フェーズ2：近接攻撃");
        }
        else
        {
            Debug.Log("ボス フェーズ2：範囲攻撃");
        }
        DamagePlayer();
    }

    void Phase3Attack()
    {
        Debug.Log("ボス フェーズ3：必殺技！！");
        DamagePlayer();
    }

    void DamagePlayer()
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.TakeDamage(10);
        }
    }
}