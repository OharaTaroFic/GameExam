#Requires -Version 5.1
<#
  オープンキャンパス作品 まとめてビルド
  ダブルクリック用。引数なしで動きます。
#>

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$Base     = Split-Path $PSScriptRoot -Parent
$SrcRoot  = Join-Path $Base '作品を入れる'
$OutDir   = Join-Path $Base '完成したゲーム'
$LogDir   = Join-Path $OutDir '_ログ'
$CsScript = Join-Path $PSScriptRoot 'BatchBuild.cs'

function Line { Write-Host ('-' * 64) -ForegroundColor DarkGray }
function Title($t) { Write-Host ''; Line; Write-Host "  $t" -ForegroundColor Cyan; Line }
function Bad($t)  { Write-Host "  $t" -ForegroundColor Red }
function Good($t) { Write-Host "  $t" -ForegroundColor Green }
function Warn($t) { Write-Host "  $t" -ForegroundColor Yellow }

function Stop-Here($msg) {
    Write-Host ''; Bad '■ 中断しました'; Write-Host ''; Write-Host $msg; Write-Host ''
    Read-Host 'Enterキーで閉じます'; exit 1
}

Clear-Host
Title 'オープンキャンパス作品 まとめてビルド'

# ==================================================================
# 1. 準備チェック
# ==================================================================
if (-not (Test-Path $SrcRoot))  { Stop-Here "「作品を入れる」フォルダが見つかりません。`nファイル一式をコピーし直してください。" }
if (-not (Test-Path $CsScript)) { Stop-Here "「tools」フォルダの BatchBuild.cs がありません。`nファイル一式をコピーし直してください。" }

$hubRoots = @('C:\Program Files\Unity\Hub\Editor', 'C:\Program Files (x86)\Unity\Hub\Editor')
$secondary = Join-Path $env:APPDATA 'UnityHub\secondaryInstallPath.json'
if (Test-Path $secondary) {
    $p = (Get-Content $secondary -Raw).Trim().Trim('"')
    if ($p) { $hubRoots += (Join-Path $p 'Editor'); $hubRoots += $p }
}
foreach ($d in @('D:', 'E:', 'F:')) {
    $hubRoots += "$d\Program Files\Unity\Hub\Editor"; $hubRoots += "$d\Unity\Hub\Editor"
}
$hubRoots = $hubRoots | Where-Object { Test-Path $_ } | Select-Object -Unique
if ($hubRoots.Count -eq 0) { Stop-Here "Unity が見つかりませんでした。`nこのパソコンに Unity Hub と Unity 本体が入っているか確認してください。" }

$installed = @()
foreach ($r in $hubRoots) {
    Get-ChildItem $r -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $exe = Join-Path $_.FullName 'Editor\Unity.exe'
        if (Test-Path $exe) { $installed += [pscustomobject]@{ Version = $_.Name; Exe = $exe } }
    }
}
if ($installed.Count -eq 0) { Stop-Here 'Unity 本体（Unity.exe）が見つかりませんでした。' }

Write-Host ''
Write-Host '  このパソコンに入っている Unity:'
$installed | ForEach-Object { Write-Host "    ・$($_.Version)" }

New-Item -ItemType Directory -Force -Path $OutDir, $LogDir | Out-Null

# ==================================================================
# 2. 作品をさがす
# ==================================================================
Title '作品をさがしています'

$targets = @(); $skipped = @()
Get-ChildItem -Path $SrcRoot -Directory -ErrorAction SilentlyContinue | Sort-Object Name | ForEach-Object {
    $dir = $_; $name = $dir.Name
    if (-not (Test-Path (Join-Path $dir.FullName 'Assets'))) { $skipped += "$name … Unityの作品ではないようです"; return }
    if (Test-Path (Join-Path $OutDir "$name\index.html"))    { $skipped += "$name … ビルド済みなので飛ばします"; return }

    $verFile = Join-Path $dir.FullName 'ProjectSettings\ProjectVersion.txt'
    $ver = $null
    if (Test-Path $verFile) {
        $m = Select-String -Path $verFile -Pattern '^m_EditorVersion:\s*(\S+)' | Select-Object -First 1
        if ($m) { $ver = $m.Matches[0].Groups[1].Value }
    }
    if (-not $ver) { $skipped += "$name … Unityのバージョンが分かりませんでした"; return }

    $hit = $installed | Where-Object { $_.Version -eq $ver } | Select-Object -First 1
    if (-not $hit) { $skipped += "$name … Unity $ver がこのPCに入っていません"; return }

    $targets += [pscustomobject]@{ Name = $name; Path = $dir.FullName; Version = $ver; Exe = $hit.Exe }
}

foreach ($s in $skipped) { Write-Host "  とばす : $s" -ForegroundColor DarkGray }
foreach ($t in $targets) { Good  "  ビルド : $($t.Name)" }

if ($targets.Count -eq 0) {
    Write-Host ''; Warn '■ ビルドするものがありませんでした'; Write-Host ''
    Write-Host '  ・「作品を入れる」フォルダに作品フォルダが入っていますか？'
    Write-Host '  ・すでに全部ビルド済みかもしれません。'
    Write-Host ''; Read-Host 'Enterキーで閉じます'; exit 0
}

# ==================================================================
# 3. 同時実行数
# ==================================================================
$ramGB = [math]::Round((Get-CimInstance Win32_ComputerSystem).TotalPhysicalMemory / 1GB)
$cores = (Get-CimInstance Win32_ComputerSystem).NumberOfLogicalProcessors
$parallel = [math]::Max(1, [math]::Min([math]::Floor($ramGB / 8), [math]::Floor($cores / 4)))

$estMin  = [math]::Ceiling($targets.Count / $parallel * 20)
$estText = if ($estMin -lt 60) { "$estMin 分" } else { "$([math]::Round($estMin / 60, 1)) 時間" }

Title 'これから始めます'
Write-Host ''
Write-Host "    ビルドする作品   : $($targets.Count) 件"
Write-Host "    同時に処理する数 : $parallel 件（メモリ ${ramGB}GB から自動判断）"
Write-Host "    かかる時間の目安 : およそ $estText"
Write-Host ''
Warn '  ※ 進み具合はこのあと画面に出ます。'
Warn '  ※ このウィンドウは閉じないでください。'
Warn '  ※ パソコンがスリープしないよう電源設定をご確認ください。'
Write-Host ''
$ans = Read-Host '  始めてよければ y を入力して Enter（やめる場合はそのまま Enter）'
if ($ans -ne 'y') { Write-Host '  やめました。'; Read-Host '  Enterキーで閉じます'; exit 0 }

# ==================================================================
# 4. 進捗表示のしくみ
# ==================================================================

# Unity が開いたままのログを読むため、共有指定で開く
function Read-LogTail {
    param([string]$Path, [int]$Bytes = 6000)
    try {
        $fs = New-Object IO.FileStream($Path, [IO.FileMode]::Open, [IO.FileAccess]::Read, [IO.FileShare]::ReadWrite)
        try {
            $start = [Math]::Max(0, $fs.Length - $Bytes)
            [void]$fs.Seek($start, [IO.SeekOrigin]::Begin)
            $len = [int]($fs.Length - $start)
            if ($len -le 0) { return '' }
            $buf = New-Object byte[] $len
            [void]$fs.Read($buf, 0, $len)
            return [Text.Encoding]::UTF8.GetString($buf)
        } finally { $fs.Dispose() }
    } catch { return '' }
}

# ログの内容から、いま何をしているかを推定する
function Get-Phase {
    param([string]$Tail)
    if (-not $Tail) { return @{ Text = 'Unityを起動中';        Pct = 3  } }
    if ($Tail -match 'Exiting batchmode|Cleanup mono|Build completed') { return @{ Text = '仕上げ中';           Pct = 97 } }
    if ($Tail -match 'emcc|wasm-ld|Linking|WasmOpt|wasm-opt')          { return @{ Text = 'ブラウザ用に変換中'; Pct = 88 } }
    if ($Tail -match 'il2cpp|IL2CPP|native binary')                    { return @{ Text = 'プログラム変換中';   Pct = 62 } }
    if ($Tail -match 'Compiling shader|shader variants|ShaderCompiler'){ return @{ Text = '画面効果を処理中';   Pct = 50 } }
    if ($Tail -match 'BuildPlayer|Building Player|Bee\b|Tundra')        { return @{ Text = 'ゲームを組み立て中'; Pct = 42 } }
    if ($Tail -match 'Compiling.*Assembly|Script compilation')          { return @{ Text = 'スクリプト確認中';   Pct = 30 } }
    if ($Tail -match 'Import Asset|Importing|Refresh|AssetDatabase')    { return @{ Text = '素材を読み込み中';   Pct = 18 } }
    if ($Tail -match 'Licensing|Initialize engine|Loading GUID')        { return @{ Text = 'Unityを起動中';      Pct = 8  } }
    return @{ Text = '処理中'; Pct = 35 }
}

function Format-Span([double]$Minutes) {
    if ($Minutes -lt 60) { return ('{0:N0}分' -f $Minutes) }
    return ('{0:N0}時間{1:N0}分' -f [math]::Floor($Minutes / 60), ($Minutes % 60))
}

$script:PrevLines = 0
function Render {
    param([string[]]$Lines)
    $w = 78
    try { $w = [Math]::Max(40, [Console]::WindowWidth - 1) } catch { }
    $out = New-Object System.Text.StringBuilder
    foreach ($l in $Lines) {
        $s = $l
        if ($s.Length -gt $w) { $s = $s.Substring(0, $w) }
        [void]$out.AppendLine($s.PadRight($w))
    }
    # 前回より短くなった分を空行で消す
    for ($i = $Lines.Count; $i -lt $script:PrevLines; $i++) { [void]$out.AppendLine(' ' * $w) }
    $script:PrevLines = $Lines.Count
    try { [Console]::SetCursorPosition(0, 0) } catch { }
    [Console]::Write($out.ToString())
}

function Get-Bar {
    param([int]$Pct, [int]$Width = 34)
    $Pct = [Math]::Max(0, [Math]::Min(100, $Pct))
    $fill = [int][Math]::Round($Width * $Pct / 100)
    return '[' + ('=' * $fill) + ('.' * ($Width - $fill)) + ']'
}

# ==================================================================
# 5. ビルド
# ==================================================================
$swAll   = [Diagnostics.Stopwatch]::StartNew()
$running = New-Object System.Collections.ArrayList
$results = New-Object System.Collections.ArrayList
$queue   = [System.Collections.Queue]::new(@($targets))
$total   = $targets.Count

Clear-Host
try { [Console]::CursorVisible = $false } catch { }

function Draw {
    $doneCount = $results.Count
    $okCount   = @($results | Where-Object { $_.結果 -eq '成功' }).Count
    $ngCount   = $doneCount - $okCount

    # 全体の進み具合（実行中の分も部分的に足す）
    $partial = 0.0
    foreach ($j in $running) { $partial += ($j.Pct / 100.0) }
    $overall = [int](($doneCount + $partial) / $total * 100)

    # 残り時間の見積もり
    $avg = 20.0
    $fin = @($results | Where-Object { $_.所要分 -gt 0 })
    if ($fin.Count -gt 0) { $avg = ($fin | Measure-Object -Property 所要分 -Average).Average }
    $remainItems = $queue.Count + $running.Count
    $etaMin = [Math]::Max(0, ($remainItems * $avg / $parallel) - (($running | Measure-Object -Property Elapsed -Sum).Sum / [Math]::Max(1,$parallel)))

    $L = New-Object System.Collections.ArrayList
    [void]$L.Add('')
    [void]$L.Add('  ビルド中 － このウィンドウは閉じないでください')
    [void]$L.Add('  ' + ('-' * 64))
    [void]$L.Add('')
    [void]$L.Add(('   全体  {0} {1,3}%    完了 {2}/{3} 件' -f (Get-Bar $overall), $overall, $doneCount, $total))
    [void]$L.Add('')
    [void]$L.Add(('   経過 {0}   のこり およそ {1}   成功 {2} / 失敗 {3}' -f `
        (Format-Span $swAll.Elapsed.TotalMinutes), (Format-Span $etaMin), $okCount, $ngCount))
    [void]$L.Add('')
    [void]$L.Add('  ' + ('-' * 64))
    [void]$L.Add('   いま作業しているもの')
    [void]$L.Add('')

    if ($running.Count -eq 0) {
        [void]$L.Add('     （準備中）')
    } else {
        foreach ($j in ($running | Sort-Object Name)) {
            $nm = $j.Name
            if ($nm.Length -gt 18) { $nm = $nm.Substring(0, 17) + '…' }
            [void]$L.Add(('     {0,-19} {1} {2,3}%' -f $nm, (Get-Bar $j.Pct 20), $j.Pct))
            [void]$L.Add(('     {0,-19}   {1}   経過{2:N0}分   ログ更新 {3}秒前' -f `
                '', $j.Phase, $j.Elapsed, $j.Quiet))
            [void]$L.Add('')
        }
    }

    [void]$L.Add('  ' + ('-' * 64))
    [void]$L.Add('   終わったもの' + $(if ($doneCount -eq 0) { '  （まだありません）' } else { "  （$doneCount 件）" }))
    [void]$L.Add('')
    $recent = @($results | Select-Object -Last 8)
    foreach ($r in $recent) {
        $mark = if ($r.結果 -eq '成功') { '○' } else { '×' }
        $nm = $r.作品名
        if ($nm.Length -gt 22) { $nm = $nm.Substring(0, 21) + '…' }
        [void]$L.Add(('     {0} {1,-23} {2:N0}分' -f $mark, $nm, $r.所要分))
    }
    if ($doneCount -gt 8) { [void]$L.Add("     … ほか $($doneCount - 8) 件") }
    [void]$L.Add('')
    [void]$L.Add('  ' + ('-' * 64))
    [void]$L.Add('   「ログ更新」の数字が増え続けていても異常ではありません。')
    [void]$L.Add('   プログラム変換中は10分以上無言になることがあります。')
    [void]$L.Add('')

    Render $L.ToArray()
}

function Update-Running {
    foreach ($j in $running) {
        $j.Elapsed = [math]::Round($j.SW.Elapsed.TotalMinutes, 1)
        $tail = Read-LogTail $j.Log
        $ph = Get-Phase $tail
        # 進捗は戻さない（見た目の安心のため）
        if ($ph.Pct -gt $j.Pct) { $j.Pct = $ph.Pct }
        $j.Phase = $ph.Text
        try {
            $j.Quiet = [int]((Get-Date) - (Get-Item $j.Log).LastWriteTime).TotalSeconds
        } catch { $j.Quiet = 0 }
    }
}

function Harvest {
    param([switch]$WaitAll)
    do {
        foreach ($job in @($running | Where-Object { $_.Proc.HasExited })) {
            $job.SW.Stop()
            $ok  = ($job.Proc.ExitCode -eq 0) -and (Test-Path (Join-Path $job.Out 'index.html'))
            [void]$results.Add([pscustomobject]@{
                作品名 = $job.Name
                結果   = $(if ($ok) { '成功' } else { '失敗' })
                所要分 = [math]::Round($job.SW.Elapsed.TotalMinutes, 1)
                ログ   = $job.Log
            })
            [void]$running.Remove($job)
        }
        $busy = if ($WaitAll) { $running.Count -gt 0 } else { $running.Count -ge $parallel }
        if ($busy) {
            Update-Running
            Draw
            Start-Sleep -Seconds 3
        }
    } while ($busy)
    Update-Running
    Draw
}

while ($queue.Count -gt 0) {
    Harvest

    $t   = $queue.Dequeue()
    $out = Join-Path $OutDir $t.Name
    $log = Join-Path $LogDir "$($t.Name).log"
    New-Item -ItemType Directory -Force -Path $out | Out-Null

    $inject = Join-Path $t.Path 'Assets\_OCBuild\Editor'
    New-Item -ItemType Directory -Force -Path $inject | Out-Null
    Copy-Item $CsScript (Join-Path $inject 'BatchBuild.cs') -Force

    $unityArgs = @(
        '-batchmode', '-nographics', '-silent-crashes', '-accept-apiupdate',
        '-logFile', $log, '-projectPath', $t.Path,
        '-buildTarget', 'WebGL',
        '-executeMethod', 'BatchBuild.BuildWebGL',
        '-outputPath', $out
    )
    $proc = Start-Process -FilePath $t.Exe -ArgumentList $unityArgs -PassThru -WindowStyle Hidden

    [void]$running.Add([pscustomobject]@{
        Name = $t.Name; Proc = $proc; SW = [Diagnostics.Stopwatch]::StartNew()
        Out = $out; Log = $log
        Pct = 1; Phase = 'Unityを起動中'; Elapsed = 0.0; Quiet = 0
    })
    Draw
}

Harvest -WaitAll
$swAll.Stop()
try { [Console]::CursorVisible = $true } catch { }

# ==================================================================
# 6. 結果
# ==================================================================
$okList   = @($results | Where-Object { $_.結果 -eq '成功' })
$failList = @($results | Where-Object { $_.結果 -eq '失敗' })

Clear-Host
$script:PrevLines = 0
Title '終わりました'
Write-Host ''
Write-Host "    成功 : $($okList.Count) 件"
Write-Host "    失敗 : $($failList.Count) 件"
Write-Host "    時間 : $(Format-Span $swAll.Elapsed.TotalMinutes)"
Write-Host ''

if ($failList.Count -gt 0) {
    Warn '失敗したもの:'
    $failList | ForEach-Object { Write-Host "    ・$($_.作品名)" }
    Write-Host ''
    Write-Host '  もう一度このファイルを実行すると、失敗した分だけやり直します。'
    Write-Host '  それでもダメなら、次のフォルダのログを担当者に渡してください:'
    Write-Host "    $LogDir" -ForegroundColor Cyan
    Write-Host ''
}

try {
    $results | Sort-Object 作品名 |
        Export-Csv -Path (Join-Path $OutDir '結果一覧.csv') -NoTypeInformation -Encoding UTF8
} catch { }

Write-Host "  できあがったもの: $OutDir" -ForegroundColor Cyan
Write-Host ''
Write-Host '  中身を見るには「2_ゲームを見る」をダブルクリックしてください。'
Write-Host ''
Read-Host 'Enterキーで閉じます'
