# UnityExamD 引き継ぎ資料

最終更新: 2026-10-16（PC: DESKTOP-9I5RCOT / teacherM で作業）

## 0. 次にやること（ここから再開）

1. **段階B（スマホで遊べるようにする）**。詳しくは「6. これからの計画」。始める前に、作る内容をユーザーに確認する
2. その後は段階C の残り → 配る前の準備 → 一括ビルドツール

**モデルとエフォート:** Unity を操作して確かめながら進める作業なので **Opus・高**。

## 0.5 C: ステージ作りの安全策（2026-10-16 に完成。ユーザーが実機で確認済み）

- **`Assets/Script/Editor/StageGuard.cs`**
  - 迷路（シーンのルートにある `Map`。壁と床）を、**Scene ビューでクリックしても選べない**ようにする（`SceneVisibilityManager.DisablePicking`）。**Hierarchy からは選べる**（ユーザーがこれでよいと判断）
  - この設定は PC ごとの Library に保存され、起動やスクリプトの再読み込みの直後に Unity が保存内容で上書きする。シーンを開いたときにかけるだけでは効かなかったので、`EditorApplication.update` で毎回確かめて、外れていればかけ直す（`SceneView.duringSceneGui` では、Unity の窓が裏にあると動かなかった）
  - メニュー「授業 > 迷路を選べるようにする（先生用）」: その PC だけ（EditorPrefs）で、迷路を選べるようにする切り替え
  - 敵と星を置いたり動かしたりすると、高さをそろえる: **敵は y = 0（足元が基準）、星は y = 0.5**（球の中心が基準。0 だと半分埋まる。ユーザーが 0.25 / 0.5 / 1.0 から 0.5 を選んだ）。`ObjectChangeEvents` で拾うので、Undo に記録される操作（普通のドラッグ）で効く
- **`Assets/Script/Editor/StageChecker.cs`**: GameScene を再生するとき（ExitingEditMode）に点検する。中身は `StageChecker.Check()`（画面を出さずに試せる）
  - **遊べない問題 → 「OK」だけの画面を出して再生を止める**（ユーザーの判断）: プレイヤーがいない／星が 0 個／星や敵が迷路の外（Map の壁を囲む箱の外）／星にプレイヤーのスタート地点から歩いて行けない（NavMesh の経路で判定。壁に埋まった星や閉じた区画の星）／敵が NavMesh の上にいない（壁の中など）
  - **難しくなるだけの問題 → 「このまま遊ぶ／やめる」**: 敵がスタート地点から 4 未満
  - 止めたときは、問題のある物を選んだ状態にする。Console にも全部出す
- **`Enemy.cs`**: NavMesh の外にいるときと、プレイヤーがいないときは何もしない（エラーを出し続けない）
- 敵と星のプレハブは **`Assets/ステージ部品/Enemy.prefab`・`Dot.prefab`** に移した（GUID は同じ）
- GameScene の星は y = 0.5 に上げて保存済み
- `EditorUtility.InstanceIDToObject`（古い書き方の警告が出る）は `EntityIdToObject` に替えた（PlayerPrefabSync.cs も）

## 1. 何を作っているか

専門学校の**体験授業（約50分）**で遊ばせる、**スマホの縦画面で遊べるブラウザゲーム**（Unity の WebGL）。

授業でやること（2026-10-15 にユーザーが決定。この3つだけ）:
1. プレイヤーの見た目を決める（マテリアルをキャラクターの部品にドラッグする。CG体験）
2. プレイヤーのモーションを決める（Animator Controller を Player スクリプトの欄に入れる）
3. 敵や星を GameScene に置いて、ステージを作る（レベルデザイン。「みんなが作ったオリジナルゲーム」と呼ぶ）

- **コードの間違いを直す（プログラミング体験）は、授業から外した**（2026-10-15）。ゲームは最初から正しく動く状態で渡す
- 「隣の人と交換して遊ぶ」「後日 LINE で URL を送る」は授業でやるが、ゲームに機能として入れるものではない（2026-10-15 にユーザーが確認）。ただし URL を送るには、生徒ごとの WebGL ビルドと公開先の準備が要る
- **授業用メニュー（①見た目を決める、②ステージを作る… のようなシーンを開くメニュー）は入れない**（2026-10-15、ユーザーの判断）

## 2. 決まったこと

| 項目 | 決まったこと |
|---|---|
| ルール | 上から見下ろす3D。星を全部集めてゴール、敵に当たると負け |
| ステージ作り | 敵・星などの部品をシーンにドラッグして置く。**壁は最初から置いておく**（当面は ExamC の GameScene の迷路をそのまま使う） |
| 画面 | スマホの縦画面 |
| キャラ・素材 | ExamC のものを丸ごと流用 |
| キャラの権利 | プレイヤーのモデル（部品名が mesh_zelda などのファンモデル）は、**使用が許されているとユーザーが明言済み**。公開の危険を再度指摘しないこと |
| 迷路と縦画面 | ExamC の迷路は横長（横が縦の約1.7倍）。**縦画面の対応をするときに、実際の画面を見て決める**。候補は「カメラの工夫で収める」か「迷路を90度回して縦長にする」。ただしゲーム中のカメラはプレイヤーを近くから追う視点なので、問題は小さいかもしれない |
| 操作 | 生徒はスマホで遊ぶ想定。PC で作ったステージがスマホでは難しすぎる、を防ぐため、**生徒が使う操作は画面のスティックだけ**（PC ではマウスでつかむ）。**キーボードは開発用のスイッチの裏に置き、生徒に配る版では切る。** Game ビューはスマホと同じ縦長の比率に固定する（段階B で作る） |
| 見た目の選び方 | **マテリアルを Scene ビューのキャラクターの部品へ直接ドラッグ**（「わちゃわちゃ」させたい、とユーザーの希望）。名前の一覧から選ぶ形は不採用。間違ったもの（床など）も入れられるままにする |
| モーションの選び方 | Player スクリプトの 5 欄（待機・歩き・走り・勝ち・負け）に Animator Controller を入れる（案B）。案C「EditScene に 5 人並べて、それぞれにドラッグ」は、B を授業の流れで試して物足りなければ上乗せする（B の欄に書き込む形で作れる） |
| 変更の引き継ぎ | EditScene でも GameScene でも、再生中でも、変えた見た目とモーションは自動で Player.prefab に保存され、もう片方のシーンに引き継がれる |
| 最初の見た目 | **何も付いていない灰色**（黒と灰色を画像で見比べてユーザーが選んだ） |
| 持ち物 | モーションが隠す持ち物（弓・矢・短剣など）も**常に全部表示**（試して違和感なし。切り替えのチェックは付けない） |
| ライト | EditScene・GameScene とも回転 (50, 30, 0)（カメラ側から当たる向き） |

## 3. 今の状態（2026-10-15）

**パソコンのキーボードで遊べる。** 再生して確かめたこと: 敵が迷路に沿って追ってくる、D キーで右へ動き走るモーションに切り替わる、敵に触れると LoseScene、Enter でタイトル、スペースで GameScene、星を取ると WinScene。再生中のエラーは無し。

### プレイヤー
- `Assets/Player.prefab`（idle1.fbx のプレハブ変種）。Player タグ、CharacterController（高さ 1.9、半径 0.4）、Animator（idle1.fbx には元々付いていないので追加。ルートモーション無し）、Player スクリプト。GameScene の左端（-13, 0, 3.2）に右向き、EditScene の中央に正面向きで配置
- 部品（Renderer）は 16 個。色の割り当て先は 5 系統: 体（mesh_zelda・Cape・Pouch・Waist）、目（mesh_zeldaEyes）、髪（mesh_zeldaHair・HairBackface）、剣と盾（dagger・iceRod・masterSword・shield）、弓矢（arrow_01〜03・quiver・bow）
- 最初は 16 個すべてに**受け皿のマテリアル** `Assets/Prefab/PlayerMaterial/Player_Body / Eyes / Hair / SwordShield / BowArrow.mat` が貼ってある。中身は「プレイヤー設定」で決まる（下）
- モーションの初期値: idle1・walk1・run1・win1・lose1

### 見た目とモーションの仕組み
- **`Assets/Script/Editor/PlayerPrefabSync.cs`**: `ObjectChangeEvents` で、プレイヤーの Renderer の `m_Materials` と Player の `AnimIdle`〜`AnimLose` の変更を拾う。編集中は `PrefabUtility.ApplyPropertyOverride` で Player.prefab に書き込む。再生中は覚えておき、再生を止めたとき（EnteredEditMode）に `LoadPrefabContents` → `SaveAsPrefabAsset` で書き込む
  - **再生中はプレイヤーが Player.prefab のインスタンスと判定されない**（`GetPrefabAssetPathOfNearestInstanceRoot` が空）。なので再生中は Player コンポーネントの有無だけで対象にしている（最初はこの判定で見逃していた）
  - スクリプトから直接代入した値（Undo に記録されない変更）は拾わない。なので Player.cs が実行中に Animator を差し替えても保存されない
- **`Assets/プレイヤー設定.asset`**（`PlayerSettings.cs`）: 先生用・やり直し用。見た目の 5 欄（体の色・目・髪・剣と盾・弓矢）に入れたマテリアルの中身（シェーダー・値・描画順）を `OnValidate` で受け皿に写す。**欄が空なら灰色 `Player_Plain.mat`（URP 標準 Lit の写し）を写す**。今は 5 欄とも空。モーションの 5 欄は「最初に戻す」で使う値（idle1・walk1・run1・win1・lose1）
  - 見た目の欄は、受け皿のままの部品にだけ効く。生徒がドラッグで直接塗った部品には効かない
- **メニュー「授業 > プレイヤーの見た目とモーションを最初に戻す」**: 確認の画面のあと、全部品を受け皿に戻し、モーション欄を「プレイヤー設定」の値にし、開いているシーンのプレイヤーに残った差分も取り消す。中身は `PlayerPrefabSync.ResetToDefault()`（MCP からの試験はこちらを直接呼ぶ）
- **Player.cs**: モーションは自分の 5 欄から読む（欄名は `[Label]` で日本語表示）。`LateUpdate` で全 Renderer の enabled を毎フレーム true に戻す（モーションのデータが持ち物を隠すため。下の注意点）
- 項目名の日本語表示は `Assets/Script/LabelAttribute.cs` ＋ `Assets/Script/Editor/LabelDrawer.cs`
- 確認済み（2026-10-15）: 編集中の変更がプレハブへ反映・シーンに差分が残らない／再生中の変更が停止後に残る／GameScene に引き継がれる／最初に戻すで戻る。Scene ビューへの実際のドラッグもユーザーが試して問題なし

### シーンと設定
- ビルドのシーン一覧: TitleScene → GameScene → WinScene → LoseScene（勝ち・負けからタイトルへ戻る）。**EditScene は入れていない**（どこからも移動できない）
- タグ Dot・Enemy を登録済み（ExamC では TagManager が空だった。Unity は読み込み時に覚えていたが保存されていなかった）
- 入力は新しい仕組み（Input System）だけ。Player.cs の移動は `<Gamepad>/leftStick` とキーボード（WASD・矢印キー）を 1 つの InputAction で読む。段階B の画面のスティック（OnScreenStick）は Gamepad の左スティックとして入るので、そのまま効く見込み。SceneChanger.cs は Enter かスペースで次へ、Esc で終了（タップは段階B）。TitleScene と EditScene の EventSystem は InputSystemUIInputModule に差し替え済み
- ライティング: GameScene と EditScene は専用のライティングデータ（`Assets/GameScene/`、`Assets/EditScene/`）。以前は 1 つのデータを共有していて警告が出ていた。5 シーンとも開いたときの警告 0 件
- スクリプトはすべて UTF-8（BOM 付き）・CRLF。`.editorconfig` と `.vscode/settings.json` で固定（生徒が Visual Studio で編集しても Shift-JIS にならない）
- ビルド先は Windows のまま。WebGL への切り替えはまだ

## 4. 分かっている事実・注意点

- **モーションのデータ（全 24 個）に、部品の表示スイッチ（Renderer の m_Enabled）が入っている。** そのままだと再生中に弓・矢筒・矢 3 本・短剣・氷の杖が消え、win2・win4・win7 では剣・盾・マントも消える。編集中は全部見える（モーションが適用されないため）。今は Player.cs の `LateUpdate` で常に全部表示している。データには松明・本・ハープ・りんごなど、このモデルに無い部品の名前も入っている（無害）
- モーションは `Assets/Animators/` に idle1〜7、walk1、run1〜5、win1〜9（8 は無い）、lose1〜3 の Animator Controller がある。`idle2` に idle3 の動き、`win1` に lose1 の動きが余分な状態として入っている（どちらが再生されるかは未確認）
- `Assets/Material/` の色の素材: m_body_*、m_eyes_*、m_hair_*、m_swordShield_*、m_bowArrow_*、mat_* など
- **マテリアルが「変種」になる事故が起きた。** ユーザーが試している間に `m_eyes_darkPurple` が `m_eyes_dark` の変種（親付き）になり、紫の設定が消えた（操作のはずみと思われる。前のコミットの状態に戻した）。生徒も起こしうる
- URP のマテリアルは、使われると古い形式の欄（`_MainTex`、`_Color`）を新しい欄に合わせて自動で書き換える。git に変更として出るが無害
- **敵の速さ（NavMeshAgent・Enemy._speed とも 10）が、プレイヤー（MoveSpeed 5）の 2 倍。** ExamC のまま。難易度は段階B で実機を見て決める
- ゲーム中のカメラ（GameSceneCamera）はプレイヤーの上に 5、後ろに 3 から追いかける近い視点。迷路全体は映らない
- 床（AlphaMaskURP シェーダー）には、壁やキャラクターの影が落ちていないように見える（画像で見た印象。未確認）
- NavMesh（敵が歩ける範囲）は、GameScene と EditScene が `Assets/SceneSetting/NavMesh.asset` を共有している（警告は出ていない）
- EditScene の 5 つのボタン（待機・歩き・走り・勝ち・負け）は、再生中にモーションを見比べるためのもの
- EditPlayer.cs はどのシーンでも使われていない。古い `Input.GetAxis` のままで、中の "Lose" というシーン名も誤り
- 残っている壊れた素材は、ユニティちゃんの見本モデル（`Assets/UnityChan/Prefabs/*`）だけで、どのシーンも使っていない
- Console に「Error reason is 'NoSubscription' …（generators.ai.unity.com）」と出ることがある。Unity AI のパッケージの AI 生成機能が出す白いログで、害はない。2026-10-16 に MahiroCreative の組織の設定で Generators をオフにした（止まったかは確かめきれていない。詳しくは ClaudeCommon の `knowledge/unity-mcp.md`）

## 5. ここまでにやったこと（経緯）

- 2026-10-15 以前（teacherM の PC）: プロジェクト作成、`.gitignore`、公式 Unity MCP（`com.unity.ai.assistant` 2.20.0-pre.2）の追加、ExamC の `Assets` を丸ごとコピー（約490MB）、マテリアルを URP に変換（Standard 83 個 → URP/Lit を 1 個ずつ。床 4 種は URP 版シェーダー `Assets/UnityChan/Stage/Shader/AlphaMaskURP.shader` を作って差し替え。壁の内蔵マテリアルを URP の Lit.mat に差し替え）
- 2026-10-15（MahiroDesktop）:
  - 授業の内容を「見た目・モーション・レベルデザイン」の 3 つに絞った
  - 段階A（パソコンで遊べるようにする）: 文字コードの変換、タグの登録、プレイヤーのプレハブ、入力の書き直し、シーン一覧の登録、再生での確認
  - ライティングの警告の解消、ライトの向きの修正（盾にざらざらした体の影が落ちていた）
  - 見た目とモーションを生徒が変えて引き継げる仕組み（最初は「プレイヤー設定」で選ぶ形 → 「良すぎる」とのことで、ドラッグ＋自動反映の形に変更）
  - 持ち物の常時表示、最初の見た目を灰色に
- 2026-10-16（teacherM の PC）: C「ステージ作りの安全策」を作って、ユーザーが実機で確認。古い書き方の警告を解消。Unity AI の組織設定で Generators をオフ

## 6. これからの計画

- **C: ステージ作りの安全策** ← 完成（「0.5」）
- **段階B: スマホで遊べるようにする** ← 次: 画面のスティック、キーボードの切り替えスイッチ、Game ビューの縦長固定、タイトル・勝ち負け画面のタップ、縦画面と迷路の収め方、WebGL に切り替えて実機確認、必要なら敵の速さなどで難易度調整
- **段階C の残り**: EditScene を残すか（今はビルドに入っていない）、不要物（EditPlayer.cs、ユニティちゃんの見本）の片付け、案C（5 人並べる）を足すか
- **配る前の準備**: Unity MCP 用のパッケージ `com.unity.ai.assistant` を `Packages/manifest.json` から外す（2026-10-16 にユーザーが了承）。入れたままだと、AI 生成機能の契約確認で「Error reason is 'NoSubscription' … (https://generators.ai.unity.com)」が出たり、AI の画面が出たりして、生徒が混乱する。開発中は入れたままにする
- **一括ビルドツール**（後日 Claude が作る、とユーザーが予告）: 生徒全員の分をまとめて WebGL でビルドする。中身（入力の形、公開先など）は未定
  - 参考: ExamC 用 `C:\GitHub\GameExam\Tools\UntiyExamC_ビルドツール`（フォルダ名の Untiy は原文のまま）。作品フォルダを「作品を入れる」に入れて bat をダブルクリックすると、WebGL で順にビルドし、ローカルのサーバーで一覧から遊べる。中身は `tools/build.ps1`（全体の進行）、`tools/BatchBuild.cs`（Unity 側のビルド処理）、`tools/view.ps1`（見るためのサーバー）。非エンジニア向けの手順書つき
  - 同じフォルダの `引き継ぎ_ビルド失敗調査.md` に、**別の PC で途中まで進んでから失敗する問題が未解決**のまま残っている（有力な原因は、その Unity に WebGL のモジュールが入っていないこと）。ExamD 版を作るときは、この点を先に潰す
  - ExamC 版は手元で見るだけで、「LINE で URL を送る」ための公開先には対応していない

## 7. 別の PC で続けるとき

- Unity **6000.3.24f1** が必要
- Unity MCP を使うには、その PC の Claude Code に登録が要る（手順は ClaudeCommon の `knowledge/unity-mcp.md`）。プロジェクトを開けばパッケージは自動で入る
- Unity MCP で再生を試すときは、Unity の窓が裏にあると時間が進まない。再生中に `Application.runInBackground = true;` を実行する（ClaudeCommon の `knowledge/unity-mcp.md` に詳しく）
- Claude の記憶（このプロジェクト用のメモ）は PC ごとなので、この資料が正本
