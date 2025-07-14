using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    // 操作無効フレーム
    private const int START_INVAILD_NORMAL = 75;

    // 表示位置
    private readonly Rect MAIN_RECT = new Rect(0, 0, 1, 1);
    private readonly Rect VIEW_RECT = new Rect(0.75f, 0.75f, 0.25f, 0.25f);

    private const int FORWARD_DEPTH = 1;
    private const int MIDDLE_DEPTH  = 0;
    private const int BACK_DEPTH    = -1;

    private Camera _viewCamera;
    private Camera _playerCamera;
    private Player _player;

    private int _invaildFrame = 0;
    private bool _isStart = true;

    private bool _isMove = false;

    private bool _isPlayerView = true;

    public bool IsPlayerView { get { return _isPlayerView; } }

    private void Start()
    {
        _viewCamera = GameObject.Find("ViewCamera").GetComponent<Camera>();
        _playerCamera = GameObject.Find("PlayerCamera").GetComponent<Camera>();
        _player = GameObject.Find("Player").GetComponent<Player>();

        _isStart = true;
        _invaildFrame = START_INVAILD_NORMAL;
        _viewCamera.depth = BACK_DEPTH;
    }

    private void FixedUpdate()
    {
        if (_isStart)
        {
            --_invaildFrame;
            if (_invaildFrame < 0)
            {
                _isStart = false;
                _viewCamera.depth = BACK_DEPTH;
            }
            return;
        }

        if (_isMove) return;

        if (_player.IsCreateArrow)
        {
            if (_isPlayerView)
            {
                _viewCamera.depth = FORWARD_DEPTH;
            }
            else
            {
                _playerCamera.depth = BACK_DEPTH;
            }
        }
        else
        {
            if (_isPlayerView)
            {
                _viewCamera.depth = BACK_DEPTH;
            }
            else
            {
                _playerCamera.depth = FORWARD_DEPTH;
            }
        }
    }

    public void OnChange()
    {
        if (_isStart) return;
        if (_isMove) return;

        if (_isPlayerView)
        {
            // 表示位置の変更
            _playerCamera.rect = VIEW_RECT;
            _viewCamera.rect = MAIN_RECT;

            // 手前表示の変更
            _playerCamera.depth = FORWARD_DEPTH;
            _viewCamera.depth = MIDDLE_DEPTH;
        }
        else
        {
            // 表示位置の変更
            _playerCamera.rect = MAIN_RECT;
            _viewCamera.rect = VIEW_RECT;

            // 手前表示の変更
            _playerCamera.depth = MIDDLE_DEPTH;
            _viewCamera.depth = BACK_DEPTH;
        }

        _isPlayerView = !_isPlayerView;
    }

    public void StartMove()
    {
        _isMove = true;
        _viewCamera.depth = BACK_DEPTH;
        if (!_isPlayerView)
        {
            _playerCamera.rect = MAIN_RECT;
            _playerCamera.depth = FORWARD_DEPTH;
        }
    }

    public void EndMove()
    {
        _isMove = false;
        if (!_isPlayerView)
        {
            _playerCamera.rect = VIEW_RECT;
            _playerCamera.depth = BACK_DEPTH;
        }
    }
}
