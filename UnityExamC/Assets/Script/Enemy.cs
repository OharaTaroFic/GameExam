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
        // �̌��B�F�Gagent�̒ǐՐ�(destination)��target�ɐݒ肵�悤�I
        agent.destination = target.transform.position;
        animator.SetFloat("Speed", agent.velocity.magnitude);
    }
}
