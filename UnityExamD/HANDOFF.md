# UnityExamD 引き継ぎ資料

最終更新: 2026-10-15（PC: DESKTOP-9I5RCOT / teacherM で作業）

## 1. 何を作っているか

専門学校の**体験授業（約50分）**で遊ばせる、**スマホの縦画面で遊べるブラウザゲーム**（Unity の WebGL）。

授業の流れ（想定）:
1. コードの間違いを直して、正常に動くようにする（プログラミング体験）
2. 動くようになったゲームのシーンに、敵や星を置いてステージを作る（レベルデザイン。「みんなが作ったオリジナルゲーム」と呼ぶ）
3. 隣の人と交換して遊び、問題を直す
4. できたゲームは、後日 LINE で URL を送る

CG体験も兼ねたいときは、プレイヤーキャラのマテリアル（色）やモーションを、用意されたものから選んで変えさせる。

## 2. 決まったこと（段階1。最後の1点だけ未決）

| 項目 | 決まったこと |
|---|---|
| ルール | 上から見下ろす3D。星を全部集めてゴール、敵に当たると負け |
| ステージ作り | 敵・星などの部品をシーンにドラッグして置く。**壁は最初から置いておく**（当面は ExamC の GameScene の迷路をそのまま使う） |
| 画面 | スマホの縦画面 |
| キャラ・素材 | ExamC のものを丸ごと流用 |
| キャラの権利 | プレイヤーのモデル（部品名が mesh_zelda などのファンモデル）は、**使用が許されているとユーザーが明言済み**。公開の危険を再度指摘しないこと |

**未決（ユーザーの返事待ち）:** ExamC の迷路は横長（横が縦の約1.7倍）で、縦画面とどう合わせるか。
- ア: 迷路はそのまま、カメラの工夫で収める（迷路は小さく見える）
- イ: 迷路を90度回して縦長にする
- ウ: 今は決めず、縦画面の対応のときに実際の画面を見て決める

これが決まれば段階1は終わり。次は段階2（バグ修正の題材の設計）の予定。ClaudeCommon の決まりどおり、一度に一つの段階だけ進める。

## 3. ここまでにやったこと

- `.gitignore` を作成（ExamA〜C と同じ Unity 標準）
- 公式 Unity MCP（`com.unity.ai.assistant` 2.20.0-pre.2）を `Packages/manifest.json` に追加し、Claude から Unity を操作できるようにした
- ExamC（Unity 2023.2、ビルトイン描画）の `Assets` を丸ごとコピー（603ファイル、約490MB。大半はモーションの FBX）
- ExamD は URP（新しい描画方式）なので、マテリアルを変換した:
  - Standard の 83 個 → URP/Lit（Unity の一括変換機能は URP 17.3 で内部エラーになったため、1 個ずつ変換）
  - マテリアルの変種 2 個（mat_body_dark、mat_swordShield_blue）は、親との引き継ぎを外してから変換
  - 床 4 種（mat_Floor1〜4）: ビルトイン専用のシェーダーだったので、同じ働きの URP 版 `Assets/UnityChan/Stage/Shader/AlphaMaskURP.shader` を作って差し替え
  - 敵の Left / Right → URP/Lit
  - GameScene の壁 29 個と `Assets/Prefab/MiniLevel.prefab` の 25 個: Unity 内蔵の灰色の素材（ビルトイン用）を URP の Lit.mat に差し替え
  - 全 5 シーンと全プレハブを機械的に点検済み。残っている壊れた素材は、ユニティちゃんの見本モデル（`Assets/UnityChan/Prefabs/*`、9 マテリアル）だけで、どのシーンも使っていない（ExamC でも元から壊れていた）
- GameScene と EditScene をカメラの画像で確認。迷路・床・敵・ドットは正常に表示

## 4. 分かっている事実・注意点

- **プレイヤーは初期状態で真っ黒。** `Assets/Prefab/model/*.fbx` の中の素材には画像が無く、`Assets/Material/` の体・髪・目・武器のマテリアル（m_body_*、mat_hair_*、mat_eyes_*、mat_swordShield_*、mat_bowArrow_*）を当てると色が付く。これが CG 体験の素材。URP でも当てれば正しく表示されることを確認済み
- モーションは `Assets/Animators/` に idle1〜7、walk1、run1〜5、win1〜9（8 は無い）、lose1〜3 の Animator Controller がある
- **GameScene にはプレイヤーがいない**（カメラ・ライト・GameDirector・BGM・Map・Enemy・Dot だけ）。ExamC では授業中に置かせていたのかもしれない（未確認）
- ExamC のスクリプトは古い入力の仕組み（`Input.GetAxis`）を使っているが、ExamD の設定は新しい入力の仕組みだけ（`activeInputHandler: 1`）。このままでは実行時にエラーになるはず（未確認。スマホ操作を作るときに合わせて対応する）
- タグ（Dot、Enemy）が ExamC の TagManager に登録されていない（`tags: []`）。授業の課題の一部だった可能性がある（未確認）
- シーンを開くと「LightingData が古い」という警告が出る（ライティングの焼き直しで消える。害はない）
- スクリプトのファイルは Shift-JIS（ExamC のまま）。Claude の Edit ツールでは書き換えられないので、直すときは UTF-8 への変換をユーザーに確認する
- 今のビルド先は Windows のまま。WebGL への切り替えはまだしていない

## 5. 別の PC で続けるとき

- Unity **6000.3.24f1** が必要
- Unity MCP を使うには、その PC の Claude Code に登録が要る（手順は ClaudeCommon の `knowledge/unity-mcp.md`）。プロジェクトを開けばパッケージは自動で入る
- Claude の記憶（このプロジェクト用のメモ）は PC ごとなので、この資料が正本
