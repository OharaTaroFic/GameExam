using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*State.*/
enum SceneState//State.
{
    EditScene,
    TitleScene,
    GameScene,
    WinScene,
    LoseScene,
}
enum PlayerState//State.
{
    Idle,
    Walk,
    Run,
    Win,
    Lose
}


public class Player : MonoBehaviour
{
    //State.
    SceneState _sceneState;//�V�[���̏��
    PlayerState _playerState;//�v���C���[�̏��
    //�R���|�[�l���g
    CharacterController _characterController;
    Animator _animator;
    //�p�����[�^
    public float MoveSpeed = 5f;//�ړ����x
    public float RotationSpeed = 360f;//��]���x 
    //�A�j���[�V�����R���g���[���[
    public RuntimeAnimatorController AnimIdle;//�ҋ@�A�j���[�V����
    public RuntimeAnimatorController AnimWalk;//���s�A�j���[�V����
    public RuntimeAnimatorController AnimRun;//���s�A�j���[�V����
    public RuntimeAnimatorController AnimWin;//�������̃A�j���[�V����
    public RuntimeAnimatorController AnimLose;//�s�k���̃A�j���[�V����

    // Start is called before the first frame update
    void Start()
    {
        //���݂̃V�[�����擾Debug.Log("EditScene!"); 
        var scene = SceneManager.GetActiveScene();
        //�V�[�������猻�݂̏�Ԃ�����
        if (scene.name == "EditScene") { _sceneState = SceneState.EditScene; Debug.Log("EditScene!"); }
        else if (scene.name == "GameScene") { _sceneState = SceneState.GameScene; Debug.Log("GameScene!"); }
        else if (scene.name == "TitleScene") { _sceneState = SceneState.TitleScene; Debug.Log("TitleScene!"); }
        else if (scene.name == "WinScene") { _sceneState = SceneState.WinScene; Debug.Log("WinScene!"); }
        else if (scene.name == "LoseScene") { _sceneState = SceneState.LoseScene; Debug.Log("LoseScene!"); }
        //�R���|�[�l���g�̎擾
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_sceneState)
        {
            case SceneState.EditScene:
                EditUpdate();
                break;
            case SceneState.TitleScene:
                TitleUpdate();
                break;
            case SceneState.GameScene:
                GameUpdate();
                break;
            case SceneState.WinScene:
                WinUpdate();
                break;
            case SceneState.LoseScene:
                LoseUpdate();
                break;
            default:
                break;
        }
    }

    /*�e�V�[���p�̃A�b�v�f�[�g*/
    void EditUpdate()
    {
        //���݂�State�ŃA�j���[�V����������
        switch (_playerState)
        {
            case PlayerState.Idle:
                GetComponent<Animator>().runtimeAnimatorController = AnimIdle;
                break;
            case PlayerState.Walk:
                GetComponent<Animator>().runtimeAnimatorController = AnimWalk;
                break;
            case PlayerState.Run:
                GetComponent<Animator>().runtimeAnimatorController = AnimRun;
                break;
            case PlayerState.Win:
                GetComponent<Animator>().runtimeAnimatorController = AnimWin;
                break;
            case PlayerState.Lose:
                GetComponent<Animator>().runtimeAnimatorController = AnimLose;
                break;
            default:
                break;
        }
    }
    void TitleUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimIdle;
    }
    void GameUpdate()
    {
        //���͒l����ړ��x�N�g�����쐬
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //�쐬�����ړ��x�N�g���̑傫���Ō��݂̃X�e�[�g�����߂�B
        if (direction.sqrMagnitude < 0.01f) { _playerState = PlayerState.Idle; }
        if (direction.sqrMagnitude > 0.01f) { _playerState = PlayerState.Walk; }
        if (direction.sqrMagnitude > 0.98f) { _playerState = PlayerState.Run; }

        //State�ɉ���������
        //�A�j���[�V�����̐؂�ւ�
        if (_playerState == PlayerState.Idle)//�ҋ@
        {
            //�A�j���[�V������Idle�ɂ���_
            _animator.runtimeAnimatorController = AnimIdle;
        }
        else if (_playerState == PlayerState.Walk)//���s
        {
            //�A�j���[�V������Walk�ɂ���
            _animator.runtimeAnimatorController = AnimWalk;

        }
        else if (_playerState == PlayerState.Run)//����
        {
            //�A�j���[�V������Walk�ɂ���
            _animator.runtimeAnimatorController = AnimRun;
        }
        //�i�s����������
        if (_playerState == PlayerState.Walk || _playerState == PlayerState.Run)//���s������Ȃ�
        {
            //�i�s�����̃x�N�g�����쐬
            Vector3 forward = Vector3.Slerp(transform.forward, direction, RotationSpeed * Time.deltaTime / Vector3.Angle(transform.forward, direction));
            //�i�s����������
            transform.LookAt(transform.position + forward);
        }

        //�ړ������Ɉړ�
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);
    }
    void WinUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimWin;
    }
    void LoseUpdate()
    {
        GetComponent<Animator>().runtimeAnimatorController = AnimLose;
    }

    /*�{�^������p�̃��\�b�h*/
    public void OnClickIdleButton()
    {
        //�A�j���[�V�����ύX
        _playerState = PlayerState.Idle;
    }
    public void OnClickWalkButton()
    {
        //�A�j���[�V�����ύX
        _playerState = PlayerState.Walk;
    }
    public void OnClickRunButton()
    {
        //�A�j���[�V�����ύX
        _playerState = PlayerState.Run;
    }
    public void OnClickWinButton()
    {
        //�A�j���[�V�����ύX
        _playerState = PlayerState.Win;
    }
    public void OnClickLoseButton()
    {
        //�A�j���[�V�����ύX
        _playerState = PlayerState.Lose;
    }

    /*�����蔻��*/
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
            Debug.Log("Enemy�ɂԂ������I");
            SceneManager.LoadScene("LoseScene");
        }
    }
}
