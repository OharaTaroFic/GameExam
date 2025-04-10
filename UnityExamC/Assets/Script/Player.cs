using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
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
    //�R���|�[�l���g
    CharacterController _characterController;
    Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _state = State.Idle;//������Ԃ�Idle�ɂ���
        _animator.runtimeAnimatorController = IdleAnim;
    }

    // Update is called once per frame
    void Update()
    {
        //���͒l����ړ��x�N�g�����쐬
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //�쐬�����ړ��x�N�g���̑傫���Ō��݂̃X�e�[�g�����߂�B
        if (direction.sqrMagnitude < 0.01f) { _state = State.Idle; }
        if (direction.sqrMagnitude > 0.01f) { _state = State.Walk; }
        if (direction.sqrMagnitude > 0.98f) { _state = State.Run; }

        //State�ɉ���������
        //�A�j���[�V�����̐؂�ւ�
        if (_state == State.Idle)//�ҋ@
        {
            //�A�j���[�V������Idle�ɂ���
            _animator.runtimeAnimatorController = IdleAnim;
        }
        else if(_state == State.Walk)//���s
        {
            //�A�j���[�V������Walk�ɂ���
            _animator.runtimeAnimatorController = WalkAnim;
           
        }
        else if (_state == State.Run)//����
        {
            //�A�j���[�V������Walk�ɂ���
            _animator.runtimeAnimatorController = RunAnim;
        }
        //�i�s����������
        if(_state == State.Walk || _state == State.Run)//���s������Ȃ�
        {
            //�i�s�����̃x�N�g�����쐬
            Vector3 forward = Vector3.Slerp(transform.forward, direction, RotationSpeed * Time.deltaTime / Vector3.Angle(transform.forward, direction));
            //�i�s����������
            transform.LookAt(transform.position + forward);
        }

        //�ړ������Ɉړ�
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);

       
    }

    private void OnTriggerEnter(Collider other)
    {
        // Dot�ɂԂ��������̏���
        if (other.tag == "Dot")
        {
            Debug.Log("Dot�ɂԂ������I");
            // �̌��@�FDot��������(Destroy)
            Destroy(other.gameObject);
        }

        if (other.tag == "Enemy")
        {
            SceneManager.LoadScene("Lose");
        }
    }
}
