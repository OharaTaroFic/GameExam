using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneCamera : MonoBehaviour
{
    GameObject _player;
    Vector3 _distance;
    public float DelY = 5.0f;
    public float DelZ = -3.0f;

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        _distance = new Vector3(0,DelY,DelZ);
    }

    // Update is called once per frame
    void Update()
    {
       this.transform.position = _player.transform.position + _distance;
        transform.LookAt(_player.transform.position);
    }
}
