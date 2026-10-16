//開発用の切り替え（実行中に読む）
//値は、エディタ用スクリプト DevSettingsMenu が、その PC の設定（メニュー「授業 > キーボード操作を使う（開発用）」）から入れる
//書き出した版（WebGL など）では、キーボード操作は常に使えない（生徒に配る版に残らないように）
public static class DevSettings
{
#if UNITY_EDITOR
    public static bool KeyboardEnabled;
#else
    public const bool KeyboardEnabled = false;
#endif
}
