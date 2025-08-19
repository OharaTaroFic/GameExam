using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public PuniconDebug puniCon;

    private float speedMultiplier = 1f;

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
}
