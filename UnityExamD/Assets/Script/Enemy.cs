using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    GameObject target;
    NavMeshAgent agent;
    Animator animator;
    public float _speed = 10;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = _speed;
        animator = GetComponentInChildren<Animator>();
        target = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //壁の中など、歩けない場所に置かれたときや、プレイヤーがいないときは何もしない（エラーを出し続けないように）
        if (!agent.isOnNavMesh || target == null) { return; }
        // 体験③：敵agentの追跡先(destination)をtargetに設定しよう！
        agent.destination = target.transform.position;
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
