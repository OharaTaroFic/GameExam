using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameDirector : MonoBehaviour
{
    void Update()
    {
        //�Q�[����ʏ��Dot�̐���0�ɂȂ������̏���
        if (GameObject.FindGameObjectsWithTag("Dot").Length == 0)
        {
            //�Q�[���N���A�Ɉړ�
            SceneManager.LoadScene("WinScene");
        }
    }
}
