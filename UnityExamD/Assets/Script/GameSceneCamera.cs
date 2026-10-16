using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSceneCamera : MonoBehaviour
{
    //カメラの距離（どれも斜め 45° から映す。プレイヤーは画面の下から 3 割くらいに映る）
    public enum CameraDistance
    {
        [InspectorName("遠め（距離13）")] Far,
        [InspectorName("ふつう（距離10）")] Normal,
        [InspectorName("近め（距離7）")] Near,
        [InspectorName("自由（下の3つの数字を使う）")] Custom,
    }

    GameObject _player;
    [Label("カメラの距離")] public CameraDistance Distance = CameraDistance.Normal;
    public float DelY = 5.0f;
    public float DelZ = -3.0f;
    public float AheadZ = 0.0f; //プレイヤーより先（画面の上側）をどれだけ映すか。大きいほどプレイヤーが画面の下に寄る

    // Start is called before the first frame update
    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        //再生中に「カメラの距離」を変えても、すぐに切り替わる
        float delY = DelY, delZ = DelZ, aheadZ = AheadZ;
        switch (Distance)
        {
            case CameraDistance.Far: delY = 9.2f; delZ = -9.2f; aheadZ = 3.9f; break;
            case CameraDistance.Normal: delY = 7.1f; delZ = -7.1f; aheadZ = 3.0f; break;
            case CameraDistance.Near: delY = 4.9f; delZ = -4.9f; aheadZ = 2.1f; break;
        }
        var target = _player.transform.position + new Vector3(0, 0, aheadZ);
        this.transform.position = target + new Vector3(0, delY, delZ);
        transform.LookAt(target);
    }
}
