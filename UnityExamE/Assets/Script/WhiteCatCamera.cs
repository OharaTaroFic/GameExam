using UnityEngine;

public class WhiteCatCamera : MonoBehaviour
{
    [Header("追従対象")]
    public Transform target;

    [Header("オフセット設定")]
    public float height = 12f;       // 高さ
    public float distance = 6f;      // 後方距離
    public float verticalAngle = 75f; // 俯瞰角度（真上90に近づける）
    public float horizontalAngle = 0f;

    [Header("キャラの表示位置調整")]
    public float lookUpOffset = 3f; // ← キャラの少し上を注視する

    [Header("追従スピード")]
    public float followSpeed = 8f;

    private Vector3 offset;

    void UpdateOffset()
    {
        // 角度をクォータニオンで作成
        Quaternion rot = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        offset = rot * new Vector3(0, 0, -distance) + Vector3.up * height;
    }

    void LateUpdate()
    {
        if (target == null) return;

        UpdateOffset();

        // ターゲット位置（キャラに対するカメラ位置）
        Vector3 targetPosition = target.position + offset;

        // スムーズに追従
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // ← キャラの「少し上」を見ることで、キャラが画面下寄りになる
        Vector3 lookTarget = target.position + new Vector3(0, lookUpOffset, 0);
        transform.LookAt(lookTarget);
    }
}
