using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 360f;

    CharacterController characterController;
    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        if (direction.sqrMagnitude > 0.01f)
        {
            Vector3 forward = Vector3.Slerp(
                transform.forward,
                direction,
                rotationSpeed * Time.deltaTime / Vector3.Angle(transform.forward, direction));
            transform.LookAt(transform.position + forward);
        }

        characterController.Move(direction * moveSpeed * Time.deltaTime);

        animator.SetFloat("Speed", characterController.velocity.magnitude);

        // ゲーム画面上のDotの数が0個になった時の処理
        if (GameObject.FindGameObjectsWithTag("Dot").Length == 0)
        {
            // 体験②：シーン「Win」をロードしよう！
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Dotにぶつかった時の処理
        if (other.tag == "Dot")
        {
            // 体験①：Dotを消そう(Destroy)

        }

        if (other.tag == "Enemy")
        {
            SceneManager.LoadScene("Lose");
        }
    }
}
