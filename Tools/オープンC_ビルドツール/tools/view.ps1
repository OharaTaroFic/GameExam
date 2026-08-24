#Requires -Version 5.1
<#
  完成したゲームをブラウザで確認するための簡易サーバ。
  Unity の WebGL は index.html を直接開いても動かないため、これを経由します。
  余計なソフトのインストールは不要です。
#>

[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$ErrorActionPreference = 'Stop'

$Base = Split-Path $PSScriptRoot -Parent
$Root = Join-Path $Base '完成したゲーム'

function Line { Write-Host ('-' * 62) -ForegroundColor DarkGray }

Clear-Host
Write-Host ''; Line
Write-Host '  できあがったゲームを見る' -ForegroundColor Cyan
Line

if (-not (Test-Path $Root)) {
    Write-Host ''
    Write-Host '  「完成したゲーム」フォルダがありません。' -ForegroundColor Red
    Write-Host '  先に「1_ビルド開始」を実行してください。'
    Write-Host ''
    Read-Host 'Enterキーで閉じます'
    exit 1
}

$games = @(Get-ChildItem $Root -Directory -ErrorAction SilentlyContinue |
           Where-Object { Test-Path (Join-Path $_.FullName 'index.html') } |
           Sort-Object Name)

if ($games.Count -eq 0) {
    Write-Host ''
    Write-Host '  見られるゲームがまだありません。' -ForegroundColor Yellow
    Write-Host '  先に「1_ビルド開始」を実行してください。'
    Write-Host ''
    Read-Host 'Enterキーで閉じます'
    exit 0
}

# ------------------------------------------------------------------
# 空いているポートを探す
# ------------------------------------------------------------------
$listener = $null
$port = 0
foreach ($p in 8080..8099) {
    try {
        $l = [System.Net.Sockets.TcpListener]::new([System.Net.IPAddress]::Loopback, $p)
        $l.Start()
        $listener = $l; $port = $p; break
    } catch { }
}
if (-not $listener) {
    Write-Host ''
    Write-Host '  ポートが空いていませんでした。パソコンを再起動して試してください。' -ForegroundColor Red
    Read-Host 'Enterキーで閉じます'; exit 1
}

$mime = @{
    '.html'='text/html; charset=utf-8'; '.htm'='text/html; charset=utf-8'
    '.js'='application/javascript';     '.css'='text/css'
    '.json'='application/json';         '.wasm'='application/wasm'
    '.data'='application/octet-stream'; '.mem'='application/octet-stream'
    '.symbols'='application/octet-stream'; '.unityweb'='application/octet-stream'
    '.png'='image/png'; '.jpg'='image/jpeg'; '.jpeg'='image/jpeg'
    '.gif'='image/gif'; '.svg'='image/svg+xml'; '.ico'='image/x-icon'
    '.mp3'='audio/mpeg'; '.ogg'='audio/ogg'; '.wav'='audio/wav'
    '.txt'='text/plain; charset=utf-8'
}

function Get-IndexHtml {
    $rows = ($games | ForEach-Object {
        $n = [System.Net.WebUtility]::HtmlEncode($_.Name)
        $u = [System.Uri]::EscapeDataString($_.Name)
        "<li><a href=""/$u/"">$n</a></li>"
    }) -join "`n"
    @"
<!DOCTYPE html><html lang="ja"><head><meta charset="utf-8">
<meta name="viewport" content="width=device-width,initial-scale=1"><title>作品いちらん</title>
<style>
body{font-family:"Yu Gothic UI","Meiryo",sans-serif;background:#2F4A40;color:#F2F0E6;
     margin:0;padding:48px 24px;line-height:1.8}
.w{max-width:760px;margin:0 auto}
h1{font-size:1.8rem;margin:0 0 8px}
p{color:rgba(242,240,230,.65);margin:0 0 32px}
ul{list-style:none;padding:0;margin:0;display:grid;gap:10px;
   grid-template-columns:repeat(auto-fill,minmax(220px,1fr))}
a{display:block;padding:16px 18px;color:#F2F0E6;text-decoration:none;
  background:rgba(242,240,230,.06);border:1px solid rgba(242,240,230,.18);border-radius:3px}
a:hover{border-color:#F3D35E;background:rgba(242,240,230,.12)}
</style></head><body><div class="w">
<h1>作品いちらん</h1><p>見たい作品をクリックしてください（$($games.Count) 件）</p>
<ul>
$rows
</ul></div></body></html>
"@
}

function Send-Response {
    param($Stream, [int]$Code, [string]$Status, [byte[]]$Body, [string]$Type, [string]$Encoding)
    $head = "HTTP/1.1 $Code $Status`r`n"
    $head += "Content-Type: $Type`r`n"
    $head += "Content-Length: $($Body.Length)`r`n"
    if ($Encoding) { $head += "Content-Encoding: $Encoding`r`n" }
    $head += "Cache-Control: no-cache`r`n"
    $head += "Connection: close`r`n`r`n"
    $hb = [System.Text.Encoding]::ASCII.GetBytes($head)
    $Stream.Write($hb, 0, $hb.Length)
    if ($Body.Length -gt 0) { $Stream.Write($Body, 0, $Body.Length) }
    $Stream.Flush()
}

Write-Host ''
Write-Host "  $($games.Count) 件の作品が見つかりました。" -ForegroundColor Green
Write-Host ''
Write-Host "  ブラウザで次のページを開きます:" 
Write-Host "    http://localhost:$port/" -ForegroundColor Cyan
Write-Host ''
Write-Host '  ※ 見終わったら、このウィンドウを閉じてください。' -ForegroundColor Yellow
Write-Host '  ※ このウィンドウを閉じるとゲームは見られなくなります。' -ForegroundColor Yellow
Write-Host ''
Line

Start-Process "http://localhost:$port/"

try {
    while ($true) {
        $client = $listener.AcceptTcpClient()
        try {
            $client.ReceiveTimeout = 5000
            $stream = $client.GetStream()

            # リクエスト行を読む
            $buf = New-Object byte[] 8192
            $n = $stream.Read($buf, 0, $buf.Length)
            if ($n -le 0) { continue }
            $req = [System.Text.Encoding]::ASCII.GetString($buf, 0, $n)
            $first = ($req -split "`r`n")[0]
            $parts = $first -split ' '
            if ($parts.Count -lt 2) { continue }

            $rawPath = ($parts[1] -split '\?')[0]
            $path = [System.Uri]::UnescapeDataString($rawPath)

            if ($path -eq '/' -or $path -eq '') {
                $body = [System.Text.Encoding]::UTF8.GetBytes((Get-IndexHtml))
                Send-Response $stream 200 'OK' $body 'text/html; charset=utf-8' $null
                continue
            }

            $rel = $path.TrimStart('/').Replace('/', '\')
            if ($rel -match '\.\.') { Send-Response $stream 403 'Forbidden' @() 'text/plain' $null; continue }

            $full = Join-Path $Root $rel
            if (Test-Path $full -PathType Container) { $full = Join-Path $full 'index.html' }

            # Unity は .gz や .br を付けたファイルを出力する場合がある
            $encName = $null
            if (-not (Test-Path $full -PathType Leaf)) {
                if (Test-Path "$full.gz" -PathType Leaf) { $full = "$full.gz"; $encName = 'gzip' }
                elseif (Test-Path "$full.br" -PathType Leaf) { $full = "$full.br"; $encName = 'br' }
            } else {
                if ($full -like '*.gz') { $encName = 'gzip' }
                elseif ($full -like '*.br') { $encName = 'br' }
            }

            if (-not (Test-Path $full -PathType Leaf)) {
                $body = [System.Text.Encoding]::UTF8.GetBytes('見つかりません')
                Send-Response $stream 404 'Not Found' $body 'text/plain; charset=utf-8' $null
                continue
            }

            $namePart = if ($encName) { [IO.Path]::GetFileNameWithoutExtension($full) } else { [IO.Path]::GetFileName($full) }
            $ext = [IO.Path]::GetExtension($namePart).ToLower()
            $type = if ($mime.ContainsKey($ext)) { $mime[$ext] } else { 'application/octet-stream' }

            $body = [IO.File]::ReadAllBytes($full)
            Send-Response $stream 200 'OK' $body $type $encName
        }
        catch { }
        finally { try { $client.Close() } catch { } }
    }
}
finally {
    try { $listener.Stop() } catch { }
}
