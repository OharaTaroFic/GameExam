using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour
{
    [SerializeField] private Text _text;

    private int _score;
    private int _now;
    private int _delta;

    private void Start()
    {

        var sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "Stage1") _score = 0;
        else _score = GameData.score;

        _now = _score;
        _delta = 0;

        _text.text = _now.ToString();
    }

    private void OnDestroy()
    {
        GameData.score = _score;
    }

    private void FixedUpdate()
    {
        if (_now < _score)
        {
            _now += _delta;
            if (_now > _score)
            {
                _now = _score;
            }
            _text.text = _now.ToString();
        }
    }

    public void AddScore(int add)
    {
        _score += add;

        var sub = _score - _now;
        _delta = Mathf.Max(sub / 25, 1);
    }
}
