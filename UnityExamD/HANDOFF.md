# UnityExamD 引き継ぎ資料

最終更新: 2026-10-16 夜（PC: MahiroDesktop で作業。その前は teacherM）

## 0. 次にやること（ここから再開）

**今いるブランチは `builtin`**（描画の方式を URP からビルトインに切り替えた。「0.7」）。`main` は URP のまま。

**2026-10-16 夜にユーザーが決めたこと:**
- `builtin` は `main` に取り込んだ（描画の方式はビルトインで確定）
- **速い敵（8）は残す。**「クソゲーを作るほうが盛り上がる」
- **生徒が置く前の初期状態は「遅い敵 1 体・星 1 個」**。ただし、もう少しゲームを調整してからその状態にする（今は試しの配置のまま）
- **AI（Unity MCP）のパッケージは、最後の最後に外す**
- EditScene は今のまま。一括ビルドはその後で考える

1. ~~片付けの続き D~~ 済み（2026-10-16）: 未使用の色 20 個（古い版の `mat_body_*` など 14・`m_floor1〜4`・`m_body_blue 8`・`m_bowArrow _blue2`）を削除、打ち間違いの名前 5 個を修正（`m_eyes_gold2`・`m_eyes_green2`・`m_eyes_lightblue`・`m_hair_black`・`m_swordShield_blackwhite`）、敵用の `Enemy.controller` を `Assets/Enemy/` に移動。単色の `m_blue`・`m_green`・`m_red` は「間違った色」の候補として残す。**フォルダ分けはしない**（ユーザー: 生徒はフォルダを開くこと自体に頭を使う）。壊れた参照 0
2. ゲームの調整（ユーザーが続ける）→ 済んだら GameScene を初期状態（遅い敵 1・星 1）にし、プレイヤーの試し塗りを「最初に戻す」で灰色に戻す
3. 最後に AI（Unity MCP）のパッケージを外す
3. その後は段階C の残り → 配る前の準備（Unity MCP のパッケージを外す、URP のパッケージと設定ファイルの片付け、など） → 一括ビルドツール
4. ユーザーが確かめていないもの: タイトル・勝ち・負け画面のタップの動作（MCP からは入力をまねても確かめられない）。キーボードの切り替えはユーザーが確認済み

**モデルとエフォート:** Unity を操作して確かめながら進める作業なので **Opus・高**。

## 0.8 ビルトインにしたあとの調整（2026-10-16 夜、MahiroDesktop）

- **壁の色**: ビルトインの内蔵マテリアルだと真っ白で、上面と側面の差が飛んでいた。白・明るい灰色・中くらいの灰色・温かみのある灰色の比較画像から、ユーザーが **明るい灰色** を選んだ（プレイヤーに色を塗る前提なら、床との見分けやすさと立体感のバランスが一番よい）。`Assets/Material/m_wall.mat`（Standard、色 0.78、つや 0.2）を GameScene の壁 16 個と `Prefab/MiniLevel.prefab` の 25 個に貼った
- **試し塗り（実験用）**: Player.prefab の部品に直接マテリアルを貼った（生徒がドラッグで塗ったのと同じ形）。体・マント・ポーチ・腰 = m_body_red、目 = m_eyes_green1、髪 = m_hair_gold1、剣と盾など = m_swordShield_gold、弓矢 = m_bowArrow_brown。**配る前に「最初に戻す」で灰色に戻す**
- **試しの配置（実験用）**: GameScene に星を 7 個足して 8 個に、敵は 遅い（緑・元の (1.83, 0, 14.5)）と 普通（紫・(-8.01, 0, 3.03)）の 2 体。置き場所は、NavMesh の上・壁から 0.9 以上離れる・スタートから歩いて行ける、の条件で 0.5 間隔に候補を出し、なるべく散らばるように選んだ。StageChecker の問題 0 件
  - 最初は 速い（赤）も置いたが、プレイヤー（当時 5）より速く、ユーザーが「クリアできない」。外した
- **プレイヤーの速さ 5 → 6**（Player.prefab の MoveSpeed）。敵の種類ごとの速さ（遅い 2・普通 4・速い 8）は変えていない
- これで、ユーザーがスマホで「通しで良い感じにプレイできる」と確認
- **使われていない物の片付け（A）**: 書き出す 4 シーン・EditScene・生徒が置く部品・Player・PlayerSettings・`Material`/`Animators`（生徒が選ぶ材料）・スクリプト・`InputSystem_Actions.inputactions`（プロジェクトの入力の設定が参照）から依存をたどり、どこからも使われていない 62 ファイルを削除: ユニティちゃんの未使用のモーション 24・見本のプレハブ 13・見本のモーション設定 3・壊れたマテリアル 9、`Unity_Icon.png`、`AlphaMaskURP.shader`、`UI/title.png・win.png・lose.png`（切り抜き前の元画像）、`Prefab/MiniLevel.prefab`・`DotBase.prefab`、`Readme.asset`・`TutorialInfo`、`Script/EditPlayer.cs`、空の `Resources`。削除後、全シーン・全プレハブで壊れた参照 0、StageChecker 0 を確認
  - **URP の片付け（B）**: シーンのカメラ・ライトの URP 用の部品 3 個（GameScene 2、EditScene 1）を外し、`Assets/Settings`（URP の設定 7 個）を消し、`Packages/manifest.json` から `com.unity.render-pipelines.universal` を外した（core・shadergraph・universal-config も一緒に外れた）。壊れた参照 0・壊れたマテリアル 0・StageChecker 0、WebGL の書き出し成功（80MB → 64MB）を確認。UnityChan のフォルダ（敵と床の素材）は残す（ユーザーの判断）
  - **使っていないパッケージ（C）**: Visual Scripting・Timeline・Multiplayer Center・Collab Proxy・Rider 連携・Unity Pipeline（試作版）を `manifest.json` から外した。残りは ai.assistant・ai.navigation・ide.visualstudio・inputsystem・test-framework・ugui。壊れた参照 0
  - 残した: `Material` の重複や名前のおかしい色（`mat_*`、`m_floor*`、`m_body_blue 8`、`m_bowArrow _blue2`、`m_blue` など）と `Animators/Enemy`。どれも未決

## 0.7 スマホで落ちる件と、ビルトインへの切り替え（2026-10-16、MahiroDesktop）

**症状:** WebGL 版を POCO M7 Pro 5G（Android 16、GPU は **PowerVR B-Series BXM-8-256**、Brave）で開くと、タイトルのあとで「お使いのブラウザは WebGL 非対応」やクラッシュ。PC のブラウザでは安定。ExamC（Unity 2023.2・ビルトイン）は同じスマホで動いていた。

**分かったこと（順番に）:**
- サーバーのアクセス記録では、スマホにファイルは全部届いていた（通信の問題ではない）
- 画面の細かさを下げる（`config.devicePixelRatio = 1`）だけでは直らない
- **Mobile_RPAsset の HDR をオフ** → タブごと落ちることはなくなり、タイトルは動く。GameScene で止まる
- 書き出した index.html に「記録とエラーを画面に出す欄」を足して調べた: GameScene に入って約 2 秒で画面が止まり、約 10 秒後に `webglcontextlost`。ページと音楽は生きている。メモリは小さい（wasm 96〜115MB）
- `brave://gpu`（スマホで開く）の Log Messages: `GL_CONTEXT_LOST_KHR` → `Context reset detected after MakeCurrent` → `Restarting GPU process due to unrecoverable error`。**GPU のドライバー自身がリセットしていた**（見張りによる強制終了でもメモリ不足でもない）。Chrome はこの PowerVR のドライバーの不具合を 20 以上の回避策で抑えている
- URL で機能を切る一時的な仕組みで切り分け: ミニマップを切ると長く持つ、描画の解像度を上げると悪化（GPU の負荷が多いほど早く落ちる傾向）。持ち物の表示の入れ直し・影・敵・床・スティックの画面は、どれも単独の原因ではなかった
- Chrome 系は、同じサイトで GPU が何度も落ちると、しばらくそのサイトの WebGL を止める（「WebGL 非対応」が出ていたのはこれ）。試すたびにブラウザを完全に終了する必要があった
- **ユーザーの判断でビルトインに切り替えたら、スマホで普通に動いた**（「余裕でうごきますねぇ」）

**ビルトインへの切り替えでやったこと（`builtin` ブランチ）:**
- プロジェクトはそのまま（作り直していない）。URP のパッケージは入れたまま（カメラ・ライトに付いた URP 用の部品を壊さないため。使ってはいない）
- Graphics の既定と、品質設定 2 つ（Mobile・PC）の URP の割り当てを外した
- 品質設定 Mobile（WebGL が使う）をスマホ向けに: 光の数 1、くっきりした影だけ、影の分割なし、影の距離 30
- `Assets` の .mat のうち URP/Lit の 97 個を Standard に変換（`_BaseMap`→`_MainTex`、`_BaseColor`→`_Color`、`_Smoothness`→`_Glossiness`、`_Metallic`。全部不透明・発光や凹凸の画像なし。変換前後の値を全数で突き合わせて食い違い 0）。床 4 個は ExamC の元のシェーダー `Custom/AlphaMask` に戻した（項目名が同じ）
- GameScene の壁 16 個と `Prefab/MiniLevel.prefab` の 25 個が URP のパッケージの `Lit.mat` を直接使っていたので、Unity 内蔵の `Default-Material`（ExamC の元の壁と同じ）に差し替え
- 全シーンのカメラ 6 台の HDR・MSAA をオフ
- FBX の中に入っているマテリアル（URP のまま 156 個）は、どこでも使っていないのでそのまま
- 調査用の `Assets/Script/Diag/DiagSwitches.cs` は削除済み。`Assets/UnityChan/Stage/Shader/AlphaMaskURP.shader` と `Assets/Settings/` の URP の設定ファイルは使われずに残っている
- **注意:** 切り替えた直後の 1 回目の書き出しは、URP の書き出し前の点検（`URPBuildDataValidator`）が「RenderGraphSettings が無い」と止めた。設定の反映が遅れていたらしく、少し待ってもう一度書き出したら通った
- 書き出しの設定: WebGL、圧縮 Disabled、書き出し先 `Build/WebGL`（git の対象外）。スマホで見るときは、PC で `python -m http.server 8000 --bind 0.0.0.0`（`Build/WebGL` で）を動かし、同じ Wi-Fi から `http://<PC の IP>:8000`（MahiroDesktop は 192.168.0.8）
- 書き出した index.html は Unity の標準のまま（調査用の欄は、次の書き出しで消える）

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
- 敵と星のプレハブは、一度 `Assets/ステージ部品/` に移したが、**すぐ見られる場所に置きたいとのユーザーの希望で `Assets/Enemy.prefab`・`Assets/Dot.prefab`（直下）に戻した**（2026-10-16。GUID は同じ）。空の `ステージ部品` フォルダは、MCP からは消せなかった（削除は確認が要る）のでユーザーに消してもらう
- GameScene の星は y = 0.5 に上げて保存済み
- `EditorUtility.InstanceIDToObject`（古い書き方の警告が出る）は `EntityIdToObject` に替えた（PlayerPrefabSync.cs も）

## 0.6 段階B の進み具合（2026-10-16）

済んだもの:
- **Game タブを縦長に固定**: `Assets/Script/Editor/GameViewPortrait.cs`。**1080 × 1920（9 : 16）**（最初 9 : 19.5 で作ったが、ユーザーが 9 : 16 に変更）。公開 API の `PlayModeWindow.SetCustomRenderingResolution` を使う。起動・スクリプト読み直しの直後 5 秒間と、再生の直前にかけ直す（読み込み直後は Unity が保存内容で上書きするため、1 回だけでは効かなかった）
- **迷路を 90 度回して縦長に**（横 20.5 × 縦 31.5、中心 (0, 3.1)）。`Map`・プレイヤー・敵・星を、迷路の中心のまわりに `RotateAround(中心, Vector3.up, -90)` で回した（上から見て反時計回り。左端にあったスタートが下端に来る向き）。プレイヤーは下端 (-0.11, 0, -9.9) から上（+Z）向きにスタート。敵は (1.83, 0, 14.5)、星は (4.91, 0.5, 12.84)
- **NavMesh を作り直した**: GameScene 専用の `Assets/GameScene/NavMesh.asset`（旧来の `UnityEditor.AI.NavMeshBuilder.BuildNavMesh`。非推奨の警告は出るが動く）。EditScene と共有していた `Assets/SceneSetting/NavMesh.asset` は変えていない（ハッシュで確認）
- **ゲーム中のカメラ**（GameSceneCamera）: 縦画面では前の「上5・手前3」だと近くの壁で画面が埋まった。`AheadZ`（プレイヤーより先をどれだけ映すか。大きいほどプレイヤーが画面の下に寄る）を足し、9 通りの比較画像からユーザーが選んだ **DelY 7.1・DelZ -7.1・AheadZ 3.0（傾き約 45°・距離 10、プレイヤーは画面の下から約 3 割）** にした
  - Inspector の **「カメラの距離」**で 3 種類から選べる（どれも 45°）: 遠め（距離13: Y9.2 Z-9.2 A3.9）／**ふつう（距離10、初期値）**／近め（距離7: Y4.9 Z-4.9 A2.1）／自由（DelY・DelZ・AheadZ の数字を使う）。再生中に変えてもすぐ切り替わる（毎フレーム計算）
  - ユーザーの希望: プレイヤーは「一旦」画面の下寄り。Claude は、スティックを付けると親指で画面の下が隠れるので、付けたあとに見直すことを提案済み
- **壁を薄くした（1.5 → 1.0）**: スマホのスティックで動きやすく、画面も見やすくするため（ユーザーが 1.5／1.0／0.75 の比較画像から、Claude のおすすめの 1.0 を選んだ）。壁の中心の位置は変えず、厚み方向だけを減らした。四角いブロックが並んだ壁は、隣のブロックがくっついている向きを長さ方向とみなして、厚み方向だけ減らした（29 個中 25 個を変更）。十字・T 字に交わる所の 4 個は、つながりを保つため 1.5 のまま（細い壁の途中に太いかたまりが見える。気になれば作り直す）。壁はどれも Map の中で y 軸 180° 回っているが、箱なので厚みの向きは変わらない。NavMesh は作り直し済み、点検 0 件、再生で敵が追ってくることを確認
- **迷路を作り替えた（「今を整えた版」）**: 縦画面向けに、ユーザーが新しい 3 案（ループ型・3 つの部屋・ジグザグ）と見比べたうえで、「今の迷路の雰囲気のまま間隔を広げて整えた版」を選び、さらに「謎の柱（元の 1 × 1 の小さな四角）を 1.8 × 1.8 で復活」「上の縦長の壁を長く」と直した
  - Map は MiniLevel.prefab とのつながりを外した（GameScene だけが使っていた。プレハブのファイル自体はそのまま）。古い内側の壁 25 個を消し、`Wall_01`〜`Wall_12` の 12 個を新しく作った（外枠 4 枚と床はそのまま。壁の厚さ 1.0、Navigation Static）
  - 縦の列を x = -5.7, -1.9, 1.9, 5.7 にそろえ、内側の通路を約 2 → 2.8 に広げた。すき間は 2.3 以上（外枠の内側は x -9〜9、z -11.41〜17.6）
  - 壁の配置（中心 x, 中心 z, 横幅, 奥行き）: (0,-7.5,8,1) (-5.7,-1.5,6.6,1) (-1.9,-1.5,1,5) (1.9,3.5,1,8) (0,3.5,2.8,1) (5.7,-1.5,1,6) (5.7,7.25,1,5.5) (4.3,10.5,3.8,1) (-5.7,6,1,8) (-5.7,10.5,2,1) (-1.9,11.15,1,8.3) (1.9,-3.75,1.8,1.8)
  - NavMesh 作り直し済み、点検 0 件、再生で敵が追ってくることを確認
  - StageGuard の「迷路を選べない」は、Map の中の物を 1 つずつ確かめる形に直した（Map 自体だけを見ていたため、あとから足した壁にかかっていなかった）
- **敵を 3 種類に**（ユーザーの希望。色と速さで差を出す。速さは 2・4・8 にユーザーが決定。最初の案は 3.5・5・8）: `Assets/Enemy_Slow.prefab`（緑・速さ 2）／`Assets/Enemy_Normal.prefab`（紫・4。元の Enemy.prefab の名前を変えたもの。GUID は同じ）／`Assets/Enemy_Fast.prefab`（赤・8）。おそい・はやいは「ふつう」の変種（Prefab Variant）なので、「ふつう」に加えた共通の変更は引き継がれる。色は 2 色ずつ（`Assets/Material/Enemy/m_enemy_<色>_dark/light.mat`。元の Left → dark、Right → light）。ミニマップの印も同じ色。青（プレイヤー）と黄（星）は避けた。プレイヤーの速さは 5
  - GameScene に置いてある敵には、シーン側で `_speed = 2` の上書きがあった（誰がいつ入れたかは不明。プレハブの値は 10 だったが、実際は 2 で動いていた）。ユーザーの判断で上書きを取り消した。さらにユーザーの希望で、**最初から置いてある敵は Enemy_Slow（緑・2）に差し替えた**（位置 (1.83, 0, 14.5) はそのまま。シーン側の上書きは 0 件）
  - ファイル名は最初 `Enemy_おそい／ふつう／はやい` だったが、ファイル名を英語にする決まりで `Enemy_Slow／Normal／Fast` に変えた（あわせて `プレイヤー設定.asset` → `PlayerSettings.asset`。PlayerPrefabSync.cs のパスと、PlayerSettings.cs の CreateAssetMenu の既定名も変更）
- **ミニマップ（左上・横幅 25%）**: ユーザーが位置の比較画像と、スマホゲームの調査（アクション RPG・MOBA は左上、撃ち合い系は右上が多い）を見て左上を選択
  - `Assets/Script/Minimap.cs`: `MinimapCamera`（正投影・真上）に付ける。再生開始時に Map の壁の範囲に合わせて位置と広さを自動で決め、256 px 幅の RenderTexture を作って `TouchCanvas/MinimapImage`（RawImage、左上 24 px、横幅 270、透明度 0.85、触っても反応しない）に貼る。RawImage はシーンでは無効にしておき、映像ができてから有効にする（再生前に白い四角が出ないように）
  - `Assets/Script/MinimapMarker.cs`: 再生開始時に、ミニマップにだけ映る色付きの丸（白い縁取り付き。画像はプログラムで作る）を子に作る。Player.prefab（青・1.8）、Enemy.prefab（赤・1.8）・Dot.prefab（黄・1.4）に付けた。生徒が置いた敵や星にも自動で付く。シーンやプレハブには印の物体は残らない
  - レイヤーを 2 つ追加: 6 = Minimap（印。ゲームのカメラでは映さない）、7 = Ground（床。ミニマップでは映さない）。GameScene の床を Ground に変更
- **GameScene の床を mat_Floor3（黒・紫で暗い）→ mat_Floor4（ベージュ・茶の細かい市松）に変更**（床 4 種類の比較画像からユーザーが選択。明るく、灰色の壁とキャラが見分けやすい）。WinScene も同じ mat_Floor4 を使っている。床の色を自由に染める調整欄は、必要になったら AlphaMaskURP シェーダーに足す
- StageChecker の点検の前に `Physics.SyncTransforms()` を呼ぶようにした（動かした直後は当たり判定の位置が古いまま）

- **画面のスティック**（ユーザーが「触った場所に出る」形を選択。さらに「常に出すのではなく、触ったら出る」に変更）
  - `Assets/Script/FloatingStick.cs`（Input System の `OnScreenControl` を継承した自作。controlPath は `<Gamepad>/leftStick` なので Player.cs は変更なし。PC ではマウスでドラッグして動く）。標準の `OnScreenStick` の「動的な原点」は、触れる範囲が初期位置のまわりの円で、土台が付いてこないので使わなかった
  - GameScene に `TouchCanvas`（Screen Space Overlay、CanvasScaler 1080 × 1920・match 0.5）→ `StickArea`（**画面全体**。透明な Image で触れる範囲。最初は下半分だったが、ユーザーの希望で全体に）→ `StickBase`（直径 300、CanvasGroup で濃さを変える）→ `StickKnob`（直径 140）、と `EventSystem`（InputSystemUIInputModule）を追加
  - 「触っていないときの濃さ」= 0（隠す）。0 より大きくすると、下の真ん中にうすく目印を出す。つまみが動ける半径 110
  - 画像は `Assets/UI/Stick/StickBase.png`・`StickKnob.png`（Pillow で作った白い円）
  - StageGuard で、Scene ビューでは `TouchCanvas` を隠して選べなくした（画面用キャンバスは Scene ビューで巨大な四角になり邪魔）。先生用メニューは「授業 > 迷路と操作パネルを選べるようにする（先生用）」に改名
  - 試験: 再生中に `OnPointerDown`/`OnDrag`/`OnPointerUp` を直接呼んで指の操作をまねた（左スティックの値が (0, 1) になり、プレイヤーが上へ走った）

- **キーボードの切り替えスイッチ（開発用）**: メニュー「授業 > キーボード操作を使う（開発用）」。オンにした PC だけ（EditorPrefs `UnityExamD.KeyboardEnabled`）で WASD・矢印キーの移動と、Enter・スペースで画面を進める（SceneChanger）が効く。初期状態はオフ。`Assets/Script/DevSettings.cs`（実行中に読む値。書き出した版では `const false` なので常に使えない）＋ `Assets/Script/Editor/DevSettingsMenu.cs`（メニューと、起動時に PC の設定を入れる）。切り替えは次の再生から効く
  - MCP からキー入力をまねる試験（`InputSystem.QueueStateEvent`）は、Unity の窓が裏にあると入力が受け付けられず、確かめにならなかった。キーボードの動作はユーザーに実機で確かめてもらう

- **タイトル・勝ち・負けの画面を縦画面向けに作り直し、タップで進むようにした**
  - 縦画面にしたらキャラ（の立つ黄色い床 `Stage`）が画面の右外に出ていた。ユーザーによると「本当は黄色い床の上にキャラが出て、勝ち・負けモーション、タイトルは待機モーション」。ExamC のシーンファイルにはキャラが置かれていなかったが、Player.cs にはシーン名で待機・勝ち・負けのモーションに切り替える処理があったので、3 画面とも **Player.prefab を Stage の上（カメラの方を向けて）に置いた**。生徒が決めた見た目・モーションがそのまま出る
  - カメラを Stage の正面（x = Stage の x、高さ 1.3、手前 3.4）に移し、キャラが画面の真ん中より下に来るようにした。カメラの Culling Mask から Minimap レイヤーを外した（ミニマップの印が浮かんで見えないように）
  - Canvas の CanvasScaler を 1080 × 1920・match 0.5 に。ロゴは上の真ん中に、比率を保って表示。勝ち・負けの画像には「PRESS ENTER KEY」が焼き込まれていたので、切り取った `Assets/UI/win_logo.png`・`lose_logo.png`（あわせて余白を切った `title_logo.png`）を作った（元の画像はそのまま）
  - 案内 `TapGuide`（「タップしてスタート」「タップしてタイトルへ」。NotoSansJP・太字・68・白・黒の縁取り、`BlinkText.cs` でゆっくり点滅）と、その後ろに半透明の黒い帯 `TapGuideBack`（黒・透明度 0.65、780 × 120）。位置はユーザーの希望で**キャラの足元の高さ**（画面の下から 220。黄色い床に重なる）
  - `SceneChanger.cs`: `Pointer.current.press.wasPressedThisFrame`（マウスとタッチの両方）で次の画面へ。画面が出てから 1 秒はタップを受け付けない（負けた瞬間の指でうっかり進まないように）。キーボード（Enter・スペース）は開発用の切り替えがオンのときだけ
  - タップの動作は、MCP からは入力をまねても確かめられない（キーボードと同じ理由）ので、ユーザーに確かめてもらう

- **シーンを開いたときの Scene ビューのカメラ**: `Assets/Script/Editor/SceneViewStart.cs`。GameScene は迷路全体が収まる斜め上（60°）からの見下ろし（迷路の壁を囲む箱の中心から手前へ 2 ずらした点、size = 長い辺 × 0.42。ユーザーの希望で「もう少しアップ」にした）、EditScene はキャラの正面・全身（腰の高さを中心、size 1.4）。Scene ビューの位置は PC ごとの Library に保存されるので、シーンを開いたとき（1 秒間）と、Unity を起動した直後（3 秒間、SessionState で起動ごとに 1 回だけ）にかけ直す。スクリプトの読み直しでは動かさない（作業中にカメラが飛ばないように）。`EditorApplication.delayCall` は Unity の窓が裏にあると動かなかったので、`EditorApplication.update` を使っている

残り（計画）: WebGL に切り替えて実機確認・難易度調整

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
| ファイル名 | **英語にする**（2026-10-16 にユーザーが決定。日本語のファイル名・フォルダ名は使わない）。メニューの表示名や Inspector の項目名（`[Label]`）は日本語でよい |
| 描画の方式 | **ビルトイン**（2026-10-16 にユーザーが決定。URP だと PowerVR の GPU のスマホで GameScene の描画中に GPU がリセットされて落ちた。「0.7」）。`builtin` ブランチで切り替え済み、`main` への取り込みは未決 |

## 3. 今の状態（2026-10-15 時点の記述。プレイヤーの位置・カメラ・敵の速さ・URP などは古い。新しい内容は「0.5」〜「0.7」）

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
- **`Assets/PlayerSettings.asset`**（`PlayerSettings.cs`）: 先生用・やり直し用。見た目の 5 欄（体の色・目・髪・剣と盾・弓矢）に入れたマテリアルの中身（シェーダー・値・描画順）を `OnValidate` で受け皿に写す。**欄が空なら灰色 `Player_Plain.mat`（もとは URP 標準 Lit の写し。ビルトインへの切り替えで Standard・色 0.5 の灰色になった）を写す**。今は 5 欄とも空。モーションの 5 欄は「最初に戻す」で使う値（idle1・walk1・run1・win1・lose1）
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
- NavMesh（敵が歩ける範囲）は GameScene だけが `Assets/GameScene/NavMesh.asset` を使う。EditScene（敵がいない）からは 2026-10-16 に外し、古い共有の `Assets/SceneSetting/NavMesh.asset` は削除した
- EditScene のボタン（2026-10-16）: CanvasScaler を 1080×1920・match 0.5 に、5 つのボタンを画面の下端（下から 80）に 190×110 で横一列、文字は NotoSansJP 太字 44 で「待機・歩き・走り・勝ち・負け」（Player の欄と同じ言葉）
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
