# スマホのブラウザで動く Unity WebGL ゲーム：落とし穴と対策

体験授業で高校生に作らせ、LINE で URL を配る、スマホ縦画面の Unity WebGL ゲーム（ExamE など）を作るときに、**ちゃんと動かすため**の知見。
どういうゲームにするかは毎回変わるので、ここには書かない。

- 出どころ: UnityExamD（`UnityExamD/`、Unity 6000.3.24f1）を 2026-10-15〜17 に作って完成させたときに、実際に踏んで確かめたこと。経緯と細かい数字は `UnityExamD/HANDOFF.md`
- 実機: POCO M7 Pro 5G（Android 16、GPU は PowerVR B-Series BXM-8-256）、Brave。実サーバー: fic2022.boo.jp（ロリポップ、LiteSpeed）
- 確かめていないものは、そう書いてある

## 1. まず決めておくこと（ここを外すと後で作り直しになる）

| 項目 | 決めたこと | 理由 |
|---|---|---|
| 描画の方式 | **ビルトイン**（URP にしない） | URP だと、PowerVR の GPU のスマホでゲーム画面に入って約 2 秒で落ちた（下の 2） |
| 書き出しの圧縮 | **Gzip ＋ Decompression Fallback** | サーバーの設定なしで開ける。大きさは圧縮なしの約半分（68MB → 36MB） |
| 書き出しのページ | **自作の雛形**（`UnityExamD/Assets/WebGLTemplates/TapToStart`） | 音・全画面のため（下の 3） |
| 1 秒のコマ数 | **30fps**（`UnityExamD/Assets/Script/FrameRate.cs`） | 全画面で 60fps だとカクついた（下の 3） |
| Unity ロゴ（Splash Screen） | **切る** | 「タップしてはじめる」の幕がロゴにかぶる。Unity 6 は Personal でも切れる |
| 生徒の操作 | **画面のスティックだけ**。キーボードは開発用に隠す | PC のキーボードで作ったステージが、スマホでは難しすぎる、を防ぐ |
| 自作クラスの名前 | **`PlayerSettings` などの Unity の名前と同じ名前を付けない** | UnityExamD では `PlayerSettings` を作ってしまい、書き出しの設定を触るたびに取り違えた（下の 5） |

## 2. スマホで落ちる（URP）

- **Unity 6 の URP で書き出したものが、PowerVR の GPU のスマホで落ちた。ビルトインにしたら普通に動いた。** PC のブラウザでは安定。同じスマホで、Unity 2023.2・ビルトインの ExamC は動いていた。PowerVR は MediaTek の安い〜中くらいのスマホによく載っている（と思われる）ので、生徒のスマホ向けはビルトインが無難
- 症状: ゲーム画面が止まり、約 10 秒後に `webglcontextlost`。音楽は鳴り続ける。「お使いのブラウザは WebGL 非対応」と出ることもある
- URP のまま試して効かなかったもの: 画面の細かさを下げる（`devicePixelRatio = 1`）、描画の倍率、ミニマップ・影・敵・床・UI を切る。HDR をオフにすると少し持つようになっただけ
- 原因の見方: スマホで `brave://gpu`（`chrome://gpu`）を開き、一番下の「Log Messages」。今回は `GL_CONTEXT_LOST_KHR` → `Context reset detected after MakeCurrent` で、GPU のドライバー自身がリセットしていた
- **Chrome 系は、同じサイトで GPU が何度も落ちると、しばらくそのサイトの WebGL を止める。** 試すたびにブラウザを完全に終了する
- URP からビルトインへは、プロジェクトを作り直さずに切り替えられた（GraphicsSettings と品質設定の URP の割り当てを外す、URP/Lit のマテリアルを Standard に変換、URP のパッケージの中の `Lit.mat` を直接使っている物を探して差し替え、カメラの HDR・MSAA をオフ）。切り替え後は、`ProjectSettings/URPProjectSettings.asset`・`ShaderGraphSettings.asset`・`GraphicsSettings.asset` の `m_RenderPipelineGlobalSettingsMap` の URP の行が残るので、片付ける

## 3. ブラウザの決まりに合わせる（音・全画面・コマ数・サーバー）

- **開いてすぐ音は鳴らせない。** ブラウザは、ユーザーが一度触るまで音を出させない。最初のタップでタイトルが進んでしまうので、書き出しのページに「タップしてはじめる」の半透明の幕を足し、最初のタップは幕が受け取るようにした（音が鳴り始めるだけで、ゲームは進まない）
  - 雛形は、Unity 本体の `Editor\Data\PlaybackEngines\WebGLSupport\BuildTools\WebGLTemplates\Base\Default` を `Assets/WebGLTemplates/<名前>/` に写して直し、Player Settings の WebGL Template を `PROJECT:<名前>` にする
  - 幕は読み込みが終わってから出し、`click` で消す
- **全画面にできるのは、ユーザーが触ったときだけ。** 上の幕のタップで、スマホのときだけ `document.documentElement.requestFullscreen({ navigationUI: "hide" })` → 成功したら `screen.orientation.lock("portrait")`（縦向き固定）。Android の Brave で動いた。**iPhone の Safari は、ゲームのページの全画面に対応していない。** ホーム画面に追加する形（PWA）は、https と生徒の手間が要るので見送った
- **全画面にしたら、キャラを動かすたびに画面全体がカクついた**（全画面をやめると直る）。全画面だと描く画面が大きく、60fps を描ききれずにコマの間隔がバラバラになるため、と見ている。**`Application.targetFrameRate = 30`（`vSyncCount = 0`）にしたら直った。** 移動を経過時間（`Time.deltaTime`）で計算していれば、速さは変わらない
  - 「書き換え 2 回に 1 回描く」（`vSyncCount = 2`）も同じくなめらかだったが、画面の書き換えの速さ（60Hz・120Hz）で結果が変わるので、単純な targetFrameRate にした
- **起動時の黄色い帯「Content-Type … Build/WebGL.wasm … should be "application/wasm"」** は、サーバーが `.wasm` の種類を伝えていない警告。5 秒で消え、遊べるが起動が少し遅くなる。fic2022.boo.jp は、OharaServer のルートの `.htaccess` に `AddType application/wasm .wasm` を入れて直した（新しいゲームでも、同じサーバーなら何もしなくてよい）
- 圧縮を Gzip にしても、Decompression Fallback をオンにすれば、サーバーに解凍の設定は要らない（ファイルが `.unityweb` になり、ブラウザの側で解凍する）。ExamC も Gzip で問題なかった

## 4. 確かめ方

- **スマホで見る**: 同じ Wi-Fi で、PC の書き出し先で `python -m http.server 8000 --bind 0.0.0.0` を動かし、スマホで `http://<PC の IP>:8000/` を開く（MahiroDesktop は 192.168.0.8）。`Build` フォルダごと配れば、設定違いの書き出しを並べて比べられる
- **スマホは前の版を覚えていることがある。** 書き出し直したら、ページを再読み込みしてもらう（「変化が分からない」の原因がこれだったことがある）
- **原因を探すときは、アドレスの `?` の後ろで動きを切り替える、一時的なスクリプトを入れる**と、1 回の書き出しで何通りも試せる。`[RuntimeInitializeOnLoadMethod]` で `Application.absoluteURL` を読む。カクつきの件では、`?fps` で「1 秒の書き換え回数・コマの間隔の最短と最長・画面の大きさ」を画面に出し、`&t30` などで設定を切り替えて比べた。終わったら消す
- **書き出したページに「記録とエラーを画面に出す欄」を足す**と、USB なしでスマホのエラーが読める（`console.log` などと `window.onerror` を横取りして、固定表示の欄に出す）。POCO（Xiaomi）は、USB デバッグで PC から覗く方法が、許可の画面が出ずに使えなかった
- **手触り（操作感・なめらかさ）は、数字で判断せず、スマホで本人に見てもらう**

## 5. Unity・エディタの落とし穴

- **自作のクラス名が Unity の名前とぶつかる。** UnityExamD には `PlayerSettings`（プレイヤーの見た目の設定）を作ってしまい、エディタのスクリプトや一括ビルドの `BatchBuild.cs` の中で `PlayerSettings.WebGL…` が通らず、コンパイルで止まった（一括ビルドは 1 回目これで全部失敗した）。`UnityEditor.PlayerSettings` と書けば通るが、最初から別の名前にする
- **生徒の C# が Shift-JIS で保存される**のを防ぐため、`.editorconfig`（`[*.cs] charset = utf-8-bom`、`end_of_line = crlf`）と `.vscode/settings.json` をプロジェクトに入れる
- **Game タブの大きさ・Scene ビューのカメラ・Scene ビューで選べない設定は、PC ごとの Library に保存される。** 配ったばかりの生徒の PC では効いていない。エディタのスクリプトで、起動直後やシーンを開いたときにかけ直す（`UnityExamD/Assets/Script/Editor/GameViewPortrait.cs`・`SceneViewStart.cs`・`StageGuard.cs`）。`EditorApplication.delayCall` は Unity の窓が裏にあると動かないことがあるので、`EditorApplication.update` を使う
- **生徒に選ばせるマテリアルが、いつの間にか別のマテリアルの「変種」（親付き）になって、色が変わる事故が 2 回起きた。** どの操作で起きたかは特定できていない（マテリアルを別のマテリアルの上に落とすと起きるが、その操作はしていなかった）。対策として、`AssetModificationProcessor.IsOpenForEdit` で編集できないようにし、親が付いたら Console に記録するようにした（`UnityExamD/Assets/Script/Editor/MaterialGuard.cs`）。部品へ貼ること自体は今までどおりできる。起きたら git で戻す
- **シーンで変えたものを、別のシーンにも引き継ぐ**（EditScene で塗った色を GameScene にも）には、プレハブへの書き込みが要る。`ObjectChangeEvents.changesPublished` で拾って `PrefabUtility.ApplyPropertyOverride`。**再生中の変更は、再生中はプレハブのインスタンスと判定されない**ので、止めたときに `LoadPrefabContents` → `SaveAsPrefabAsset`（`UnityExamD/Assets/Script/Editor/PlayerPrefabSync.cs`）
- **モーションの FBX に、部品の表示スイッチ（Renderer の `m_Enabled`）のカーブが入っていることがある。** 再生すると持ち物が消える。`LateUpdate` で `enabled = true` に戻すと効いた
- **モーションを Animator Controller ごと差し替えると、パッと切り替わってガタつく。** 土台の Animator Controller（ブレンドツリー）に `AnimatorOverrideController` でクリップを差し込み、`CrossFadeInFixedTime` で重ねる。足が地面を滑るのは、移動の速さとモーションの本来の速さの差（UnityExamD の `MotionSpeeds.cs`・`Editor/MotionSpeedsBaker.cs` で、足の骨の動きから測って合わせた）
- **引き伸ばした箱に模様を貼ると、模様も伸びる。** 場所で模様を貼るシェーダー（`UnityExamD/Assets/Shader/WorldTiled.shader`。ビルトインのサーフェスシェーダー）にすると、壁を伸ばしても模様の大きさが変わらない。スマホでも問題なく動いた
- **エディタで `Application.runInBackground = true` を実行すると、プロジェクトの設定（`PlayerSettings.runInBackground`）に残り、書き出した版にも効く**（裏に回ってもゲームが止まらなくなる）
- パッケージを外して Unity を開き直すと、開いていたシーンの控え `Assets/_Recovery/0.unity` ができることがある。中身が同じなら消してよい
- **使われていない物を洗い出すときは、フォルダを丸ごと「使われている起点」に入れない**（その中の使われていない画像を見落とした）。起点は `.mat`・`.controller` など種類で絞る

## 6. 配る前の片付け

- AI（Unity MCP）のパッケージ `com.unity.ai.assistant` を外す（生徒が AI の画面やログで混乱しないように）。`Packages/manifest.json` の行、`ProjectSettings/Packages/com.unity.ai.assistant/`、`UserSettings/AI.*` を消す。**外すと Claude は Unity を操作できなくなる**ので、最後にやる
- 試しの配置や試し塗りを、生徒が触る前の状態に戻す
- 使われていない素材・URP の残りを片付ける（上の 2・5）

## 7. 一括ビルド（生徒全員の分を書き出す）

`Tools/UnityExamD_ビルドツール` を写して直せば使える（使い方は中の `はじめに読む_手順書.txt`）。作るときに踏んだこと:

- **ExamC の道具は、書き出しの設定（雛形・圧縮）を上書きしていた。** そのままだと、音・全画面の対策が消える。`tools/BatchBuild.cs` の設定を、そのゲームの設定にそろえる
- **Unity はフォルダ名を中のファイル名に使う**（`<フォルダ名>.data.unityweb` など）。作品の名前（日本語）のフォルダに直接書き出さず、`WebGL` という名前で書き出してから移すと、ファイル名が `WebGL.*` にそろう
- 生徒の作品は Library が無いことが多い。1 件目が書き出せたら、その Library（約 2.6GB）を写してから次を書き出すと速い。写した分は終わったら消す（作品数ぶんディスクを食うため）
- 作品の名前は、Unity を起動しなくても、シーンのファイル（YAML）から読める。Text の文字は `m_Text: "制作…"` のように `\uXXXX` で入っている
- 回収した作品と書き出した結果は、`.gitignore` で git に入れない（大きく、生徒の名前も入る）
- 3 件で 7〜8 分（MahiroDesktop）。ゼロからの 1 件目も 3.3 分だったが、別の PC ではもっとかかるかもしれない

## 8. Claude が Unity を操作するとき（Unity MCP）の落とし穴

- **Unity の窓が裏にあると、試しの入力が届かない。** 仮のゲームパッドを足して値を入れても、スティックは 0 のままだった。操作の試験はスマホで本人に頼む
- **再生しても時間が進まないことがある**（窓が裏にあると、エディタが更新を止める）。再生中に `Application.runInBackground = true` で進むが、上の 5 のとおりプロジェクトの設定に残るので、あとで戻すか確かめる
- `Unity_RunCommand` では、`File.Delete`・`AssetDatabase.DeleteAsset`・確認の画面を出す処理が拒否される。消すのは bash で消して `AssetDatabase.Refresh()`
- **WebGL の書き出しは時間切れになりやすい。** `Unity_RunCommand` から `EditorApplication.update` に「再生中でも、コンパイル中でもなくなったら `BuildPipeline.BuildPlayer` を呼び、結果をファイルに書く」処理を登録し、bash の `until [ -f 結果 ]` ループ（バックグラウンド）で待つ
- `Camera.Render` で撮った画像には、画面の UI（Screen Space Overlay）が写らない。UI も入れたいときは、再生中に `ScreenCapture.CaptureScreenshot`
- エディタの画面（Inspector など）を撮るときは、Unity を手前に出して PC の画面ごと撮る。この PC は 150% 表示なので、撮る前に `SetProcessDPIAware()` を呼ばないと画面の一部しか撮れない（ClaudeCommon の `knowledge/shell-gotchas.md`）
