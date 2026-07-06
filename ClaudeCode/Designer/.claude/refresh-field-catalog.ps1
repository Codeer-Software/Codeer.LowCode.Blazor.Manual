# フィールド型カタログ (temporary/_field_catalog.md) を「必要なときだけ」再生成するフック用スクリプト。
#
# 仕組み:
#   デザイナ exe は独自フィールドライブラリを参照してビルドされるため、フィールドの追加/削除は
#   「再ビルド」= exe とその配置フォルダの DLL のタイムスタンプ更新として現れる。
#   その最終更新時刻の最大値をシグネチャにして temporary/_field_catalog.stamp に記録し、
#   前回と同じなら再生成をスキップ、違えば全取得する。
#
# 全取得は field-catalog の --out がファイル全体を上書きするため「全置換」= 削除されたフィールドも消える。
# 生成に成功したときだけスタンプを更新する (失敗時に古い exe を「最新」と誤記録しないため)。
#
# hook から:  powershell -NoProfile -ExecutionPolicy Bypass -File ".claude/refresh-field-catalog.ps1" "<デザイナexeのパス>"
# 常に exit 0 (セッション/プロンプトをブロックしない)。

param([Parameter(Mandatory = $true)][string]$Exe)

$ErrorActionPreference = 'SilentlyContinue'

$project = 'Design'
$out     = 'temporary/_field_catalog.md'
$stamp   = 'temporary/_field_catalog.stamp'

# exe が未配置なら何もしない (未対応・パス未確定でもセッションは続行させる)。
if (-not (Test-Path -LiteralPath $Exe)) { exit 0 }

# ビルド検知シグネチャ: exe と同フォルダの *.dll の LastWriteTimeUtc の最大値。
# 注: Split-Path は -LiteralPath と -Parent が別パラメータセットで両立しない (AmbiguousParameterSet) ため使わない。
$dir = [System.IO.Path]::GetDirectoryName($Exe)
$files = @(Get-Item -LiteralPath $Exe)
$files += Get-ChildItem -LiteralPath $dir -Filter *.dll -ErrorAction SilentlyContinue
# Ticks は Int64。Measure-Object -Maximum は double 化して精度を落とすので、Int64 のまま最大値を取る。
[long]$sig = 0
foreach ($f in $files) {
    [long]$t = $f.LastWriteTimeUtc.Ticks
    if ($t -gt $sig) { $sig = $t }
}

# 既存カタログがあり、シグネチャが前回と同じなら再生成不要。
$prev = if (Test-Path -LiteralPath $stamp) { (Get-Content -LiteralPath $stamp -Raw).Trim() } else { '' }
if ((Test-Path -LiteralPath $out) -and ($prev -eq "$sig")) { exit 0 }

# 出力先フォルダを保証してから全取得 (--out は全体上書き = 全置換)。
$outDir = [System.IO.Path]::GetDirectoryName($out)
if ($outDir) { New-Item -ItemType Directory -Force -Path $outDir | Out-Null }
& $Exe field-catalog $project --out $out

# 成功時のみスタンプ更新。
if ($LASTEXITCODE -eq 0) {
    Set-Content -LiteralPath $stamp -Value "$sig" -NoNewline
}
exit 0
