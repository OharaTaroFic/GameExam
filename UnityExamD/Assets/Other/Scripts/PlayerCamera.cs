using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCamera : MonoBehaviour
{
    // 操作無効フレーム
    private const int START_INVAILD_NORMAL = 75;

    // スタート演出開始フレーム
    private const int ROT_FRAME = 39;
    // 初めに回転する大きさ
    private const float START_ROT_ANGLE = 32.0f;
    // 初めに移動する大きさ
    private readonly Vector3 START_ADD_POS = new Vector3(0, 6, -1.0f);
    private readonly Vector3 START_MOVE_SIZE = new Vector3(0, -2.5f, -0.5f);

    // 必殺・ボス攻撃時のフレーム関係
    public static int MOVE_SP_BOSS_FRAME = 10;
    private static float MOVE_SP_RATE = 25.0f / MOVE_SP_BOSS_FRAME;
    

    // 回転
    private const float ROT_RATE = 0.02f;
    private const float ROT_ANGLE_BASE = 7.2f;

    // 必殺時のカメラ情報
    private readonly Vector3 SP_POS = new Vector3(6, 16, 78);
    private readonly Quaternion SP_ROT = new Quaternion(0.376869559f, -0.160430059f, 0.066452302f, 0.909843683f);

    private GameDirector _mgr;

    private GameObject _player;
    private Player _playerScript;
    private bool _isPush = false;
    private bool _isRot = false;

    private int _invaildFrame = 0;
    private bool _isStart = true;

    private Vector3 _addPos;

    private Vector3 _deltaPos;
    private Vector3 _touchPos;

    private Action _spFixedFunc = null;
    private Vector3 _nowPos;
    private Quaternion _nowRot;
    private int _effectFrame;
    private int _waitFrame;
    private float _effectRate;
    private bool _isSpCameraView = false;
    private bool _isSpMove = false;
    private bool _isSpAddRate = true;

    private float _rotLen;

    private void Awake()
    {
        _mgr = GameObject.Find("Manager").GetComponent<GameDirector>();
        _player = GameObject.Find("Player");
        _playerScript = _player.GetComponent<Player>();
        _rotLen = Screen.width * ROT_RATE;
        _invaildFrame = START_INVAILD_NORMAL;
        _addPos = START_ADD_POS;

        // PlayerInputに関数を追加
        var input = _player.GetComponent<PlayerInput>();
        input.actions["Position"].performed += HoldTouch;
        input.actions["DoubleTouch"].started += OnDoubleTouch;
        input.actions["DoubleTouch"].canceled += OnDoubleTouch;
    }

    private void FixedUpdate()
    {
        if (_isStart)
        {
            --_invaildFrame;
            if (_invaildFrame < 0)
            {
                _isStart = false;
                _isRot = false;
            }
            else if (_invaildFrame < ROT_FRAME)
            {
                _isRot = true;
            }
            return;
        }

        if (_mgr.IsClear || _mgr.IsLose) return;

        if (_isRot)
        {
            --_invaildFrame;
            if (_invaildFrame < 0)
            {
                _isRot = false;
            }
            return;
        }

        if (_isSpCameraView) _spFixedFunc();
    }

    private void Update()
    {
        if (_playerScript.SceneState != SceneState.GameScene) return;

        if (_isStart)
        {
            if (_isRot)
            {
                transform.rotation = Quaternion.AngleAxis(START_ROT_ANGLE * Time.deltaTime, Vector3.left) * transform.rotation;
                _addPos += START_MOVE_SIZE * Time.deltaTime;
            }

            transform.position = _player.transform.position + _addPos;

            return;
        }

        if (_mgr.IsNext)
        {
            if (_isRot)
            {
                // 回転
                var rot = Quaternion.AngleAxis(START_ROT_ANGLE * Time.deltaTime, transform.right);
                transform.rotation = rot * transform.rotation;

                // 位置
                var pos = _player.transform.position;
                pos.y += START_ADD_POS.y;
                transform.position = Vector3.Slerp(transform.position, pos, 1.5f * Time.deltaTime);
            }

            return;
        }

        if (_mgr.IsClear || _mgr.IsLose)
        {
            var rot = _player.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 1.5f * Time.deltaTime);
            var pos = _player.transform.position + _player.transform.forward * 1.25f;
            pos.y += 1.1875f;
            transform.position = Vector3.Slerp(transform.position, pos, 1.5f * Time.deltaTime);
            return;
        }

        if (_isSpCameraView)
        {
            if (_isSpMove)
            {
                if (_isSpAddRate) _effectRate += Time.deltaTime * MOVE_SP_RATE;
                else              _effectRate -= Time.deltaTime * MOVE_SP_RATE;
                _effectRate = Mathf.Clamp(_effectRate, 0.0f, 1.0f);
                transform.position = Vector3.Lerp(_nowPos, SP_POS, _effectRate);
                transform.rotation = Quaternion.Lerp(_nowRot, SP_ROT, _effectRate);
            }
            return;
        }

        int count = 0;
        // 画面をタップしている指の数を取得
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.press.isPressed) count++;
            }
        }

        if (count < 2)
        {
            _isRot = false;
        }
        // 2本以上になったらカメラ操作開始
        else
        {
            // 初回だけ位置を記録
            if (!_isRot)
            {
                _deltaPos = _touchPos;
            }
            _isRot = true;
        }

        if (_isRot) RotCamera();

        transform.position = _player.transform.position + _addPos;
    }

    public void OnNextStage()
    {
        _isRot = true;
        _invaildFrame = ROT_FRAME;
    }

    /* 入力関数 */
    public void OnSpCameraView(int waitFrame)
    {
        // フレーム初期化
        _effectFrame = MOVE_SP_BOSS_FRAME;
        _waitFrame = waitFrame;
        _effectRate = 0;

        // 現在の情報を保存
        _nowPos = transform.position;
        _nowRot = transform.rotation;

        // フラグ変更
        _isSpCameraView = true;
        _isSpMove = true;
        _isSpAddRate = true;

        // 関数ポインタ変更
        _spFixedFunc = StartSpFunc;
    }

    public void OnDoubleTouch(InputAction.CallbackContext context)
    {
        if (_playerScript.SceneState != SceneState.GameScene) return;
        if (_isStart) return;
        if (_spFixedFunc != null) return;

        if (context.started)
        {
            _isPush = true;
            _deltaPos = _touchPos;
        }
        if (context.canceled)
        {
            _isPush = false;
        }
    }

    public void HoldTouch(InputAction.CallbackContext context)
    {
        if (_playerScript.SceneState != SceneState.GameScene) return;
        if (_isStart) return;

        _touchPos = context.ReadValue<Vector2>();

        if (_isPush)
        {
            RotCamera();
        }
    }

    /* その他の関数 */
    private void RotCamera()
    {
        if (_mgr.IsBossAttack || _mgr.IsSpAttack) return;
        if (_isSpCameraView) return;

        // 移動させた大きさを取得
        float len = _touchPos.x - _deltaPos.x;
        // 移動量から回転具合を計算
        float angle = len / _rotLen * ROT_ANGLE_BASE;
        // クオータニオン生成
        var rot = Quaternion.AngleAxis(angle, Vector3.up);
        // 回転
        _addPos = rot * _addPos;
        transform.localRotation = rot * transform.localRotation;
        // 保存位置更新
        _deltaPos = _touchPos;
    }

    private void StartSpFunc()
    {
        --_effectFrame;
        if (_effectFrame < 0)
        {
            // フレーム変更
            _effectFrame = _waitFrame;
            // 位置補正
            transform.position = SP_POS;
            transform.rotation = SP_ROT;
            // フラグ変更
            _isSpMove = false;
            // 関数ポインタ変更
            _spFixedFunc = WaitSpFunc;
        }
    }

    private void WaitSpFunc()
    {
        --_effectFrame;
        if (_effectFrame < 0)
        {
            // フレーム変更
            _effectFrame = MOVE_SP_BOSS_FRAME;
            _effectRate = 1;
            // フラグ変更
            _isSpMove = true;
            _isSpAddRate = false;
            // 関数ポインタ変更
            _spFixedFunc = EndSpFunc;
        }
    }

    private void EndSpFunc()
    {
        --_effectFrame;
        if (_effectFrame < 0)
        {
            // 位置補正
            transform.position = _nowPos;
            transform.rotation = _nowRot;
            // フラグ変更
            _isSpCameraView = false;
            // 関数ポインタをなくす
            _spFixedFunc = null;
        }
    }
}
