using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Fade : MonoBehaviour
{
    const int FADE_FRAME = 13;

    private int _fadeFrame;
    private string _nextScene;
    private bool _isNowFade;
    private bool _isChange;
    private Animator _anim;

    public bool IsNowFade { get { return _isNowFade; } }

    private void Start()
    {
        _fadeFrame = FADE_FRAME;
        _isNowFade = true;
        _isChange = false;
        _anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!_isNowFade) return;

        --_fadeFrame;
        if (_fadeFrame < 0)
        {
            _isNowFade = false;
            if (_isChange)
            {
                SceneManager.LoadScene(_nextScene);
            }
        }
    }

    public void OnFadeOut(string nextScene)
    {
        if (_isNowFade) return;

        _fadeFrame = FADE_FRAME;
        _nextScene = nextScene;
        _isNowFade = true;
        _isChange = true;
        _anim.SetTrigger("OnFadeOut");
    }
}
