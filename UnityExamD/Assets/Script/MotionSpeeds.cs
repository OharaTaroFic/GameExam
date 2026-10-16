using System.Collections.Generic;
using UnityEngine;

//モーションごとの「足が本来進む速さ」の表（さわらない）
//歩き・走りの足が地面を滑って見えないように、Player が再生の速さを合わせるときに使う
//モーションを増やしたら、メニュー「授業 > モーションの速さを測り直す（先生用）」で作り直す
public class MotionSpeeds : ScriptableObject
{
    [System.Serializable]
    public struct Entry
    {
        public AnimationClip Clip;
        public float Speed;//1 秒に進む距離（足が地面に着いている間の、足の後ろへの動きから測る）
    }

    public List<Entry> Entries = new List<Entry>();

    //見つからないときは 0
    public float Get(AnimationClip clip)
    {
        if (clip == null) { return 0f; }
        foreach (var e in Entries)
        {
            if (e.Clip == clip) { return e.Speed; }
        }
        return 0f;
    }
}
