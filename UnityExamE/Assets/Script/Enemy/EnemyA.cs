using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyA : EnemyBase
{
    protected override void Attack()
    {
        // 射程外に出たら追跡に戻る
        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
            ChangeState(State.Chase);
            return;
        }

        // クールタイムが終わっていたら攻撃
        if (attackTimer >= attackInterval)
        {
            attackTimer = 0f; // ← ここ大事！
            Debug.Log($"{gameObject.name} (雑魚) が攻撃した！");
        }
    }
}
