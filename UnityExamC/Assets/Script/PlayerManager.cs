using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    //�p�����[�^
    public float MoveSpeed = 5f;//�ړ����x
    public float RotationSpeed = 360f;//��]���x 
    enum State//State.
    {
        Idle,
        Walk,
        Run,
        Win,
        Lose
    }
    State _state;
    //�A�j���[�V�����R���g���[���[
    public RuntimeAnimatorController IdleAnim;//�ҋ@�A�j���[�V����
    public RuntimeAnimatorController WalkAnim;//���s�A�j���[�V����
    public RuntimeAnimatorController RunAnim;//���s�A�j���[�V����
    public RuntimeAnimatorController WinAnim;//�������̃A�j���[�V����
    public RuntimeAnimatorController LoseAnim;//�s�k���̃A�j���[�V����

    string _sceneName = string.Empty; //�V�[�������i�[����ϐ�

    // Start is called before the first frame update
    void Start()
    {
        //���݂̃V�[�������擾
        _sceneName = SceneManager.GetActiveScene().name;
        //�V�[�����ɉ����ăX�N���v�g�R���|�[�l���g���O��
        if (_sceneName == "EditScene")//EditScene.
        {
            Destroy(GetComponent<test>());
        }

    }

}
