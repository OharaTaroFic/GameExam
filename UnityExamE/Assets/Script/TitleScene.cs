using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    // ゲームシーンに遷移
    public void LoadGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    // タイトルシーンに戻る（リトライ用など）
    public void LoadTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }

    // Escapeキーでタイトルに戻る例
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LoadTitleScene();
        }
    }
}
