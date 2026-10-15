using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EditPlayer : MonoBehaviour
{
    //パラメータ
    public float MoveSpeed = 5f;//移動速度
    public float RotationSpeed = 360f;//回転速度 
    enum State//State.
    {
        Idle,
        Walk,
        Run,
        Win,
        Lose
    }
    State _playerState;
    //アニメーションコントローラー
    public RuntimeAnimatorController IdleAnim;//待機アニメーション
    public RuntimeAnimatorController WalkAnim;//歩行アニメーション
    public RuntimeAnimatorController RunAnim;//走行アニメーション
    public RuntimeAnimatorController WinAnim;//勝利時のアニメーション
    public RuntimeAnimatorController LoseAnim;//敗北時のアニメーション
    //コンポーネント
    CharacterController _characterController;
    Animator _animator;

    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponentInChildren<Animator>();
        _playerState = State.Idle;//初期状態をIdleにする
        _animator.runtimeAnimatorController = IdleAnim;
    }

    // Update is called once per frame
    void Update()
    {
        //入力値から移動ベクトルを作成
        Vector3 direction = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        //作成した移動ベクトルの大きさで現在のステートを決める。
        if (direction.sqrMagnitude < 0.01f) { _playerState = State.Idle; }
        if (direction.sqrMagnitude > 0.01f) { _playerState = State.Walk; }
        if (direction.sqrMagnitude > 0.98f) { _playerState = State.Run; }

        //Stateに応じた処理
        //アニメーションの切り替え
        if (_playerState == State.Idle)//待機
        {
            //アニメーションをIdleにする
            _animator.runtimeAnimatorController = IdleAnim;
        }
        else if(_playerState == State.Walk)//歩行
        {
            //アニメーションをWalkにする
            _animator.runtimeAnimatorController = WalkAnim;
           
        }
        else if (_playerState == State.Run)//走る
        {
            //アニメーションをWalkにする
            _animator.runtimeAnimatorController = RunAnim;
        }
        //進行方向を向く
        if(_playerState == State.Walk || _playerState == State.Run)//歩行か走るなら
        {
            //進行方向のベクトルを作成
            Vector3 forward = Vector3.Slerp(transform.forward, direction, RotationSpeed * Time.deltaTime / Vector3.Angle(transform.forward, direction));
            //進行方向を向く
            transform.LookAt(transform.position + forward);
        }

        //移動方向に移動
        _characterController.Move(direction * MoveSpeed * Time.deltaTime);

       
    }

    private void OnTriggerEnter(Collider other)
    {
        // Dotにぶつかった時の処理
        if (other.tag == "Dot")
        {
            Debug.Log("Dotにぶつかった！");
            // 体験①：Dotを消そう(Destroy)
            Destroy(other.gameObject);
        }

        if (other.tag == "Enemy")
        {
            SceneManager.LoadScene("Lose");
        }
    }
}
