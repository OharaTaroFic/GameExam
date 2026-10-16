using UnityEngine;

//ゲーム全体を 1 秒 30 コマで動かす
//スマホの全画面では 1 秒 60 コマを描ききれず、コマの間隔がバラバラになって画面全体がカクついた（2026-10-17）
//30 コマにそろえると、なめらかに見える。移動は経過時間で計算しているので、動く速さは変わらない
public static class FrameRate
{
    public const int Target = 30;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = Target;
    }
}
