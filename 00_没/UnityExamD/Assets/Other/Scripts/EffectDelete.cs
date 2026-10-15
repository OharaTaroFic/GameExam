using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectDelete : MonoBehaviour
{
    [SerializeField] private int _time = 0;

    private void FixedUpdate()
    {
        --_time;
        if (_time < 0)
        {
            Destroy(this.gameObject);
        }
    }
}
