using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    // �y�̌��P�zfloat�^��jumpPower���������悤�I


    // Update is called once per frame
    void Update()
    {
        // �y�̌��Q�zJump�{�^��(Space�L�[)�������ꂽ���ǂ������肵�悤�I
        if (Input.GetButtonDown(""))
        {
            // �y�̌��R�zVector3��Y�������ɁAjumpPower�����ړ�����悤�ɐݒ肵�悤�I
            GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
        }
    }

    // �ǂɂԂ��������̏���
    void OnCollisionEnter(Collision collision)
    {
        // �y�̌��S�z�V�[���uMain�v���Ăяo�����I
        SceneManager.LoadScene("");
    }
}
