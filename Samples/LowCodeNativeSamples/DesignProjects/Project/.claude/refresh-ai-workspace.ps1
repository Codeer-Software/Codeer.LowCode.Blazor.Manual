# デザイナ (exe) や拡張ライブラリが更新されたとき、フレームワーク所有分
# (ClaudeCodeForDesigner/ = Docs + 生成リファレンス一式、ルートの CLAUDE.md、.claude の共通設定) を
# claude-workspace で丸ごと最新化するフック用スクリプト。
#
# 仕組み:
#   デザイナ exe は独自フィールド/スクリプトオブジェクトのライブラリを参照してビルドされるため、追加/削除は
#   「再ビルド」= exe とその配置フォルダの DLL のタイムスタンプ更新として現れる。
#   その最終更新時刻の最大値 (バイナリ署名) を ClaudeCodeForDesigner/_ai_refresh.stamp と照合し、
#   同じなら何もしない。違えば claude-workspace を叩き、フレームワーク所有分を丸ごと入れ替える
#   (zip 展開 + ai-refresh。ユーザー所有の Project.md / LocalEnvironment.md / settings.local.json /
#   ddl/ / tools/ は不可侵)。スタンプは deploy 側が ai-refresh 成功時にだけ書く
#   (失敗時に古い exe を「最新」と誤記録しないため)。
#
# このワークスペースはデザイナ 1.3.15 以降が前提。古い exe に未知の verb を渡すと GUI が起動してしまうため、
# exe と同フォルダの Codeer.LowCode.Blazor.Designer.dll のバージョンで対応可否を判定し、未満なら何もしない
# (古い場合はデザイナを更新して Tools > Claude Code Workspace を再実行する)。
#
# hook から:  powershell -NoProfile -ExecutionPolicy Bypass -File ".claude/refresh-ai-workspace.ps1" "<デザイナexeのパス>" "<デザインプロジェクトのフォルダ>"
# 常に exit 0 (セッション/プロンプトをブロックしない)。

param(
    [Parameter(Mandatory = $true)][string]$Exe,
    [string]$Project = 'design'
)

$ErrorActionPreference = 'SilentlyContinue'

$stamp = 'ClaudeCodeForDesigner/_ai_refresh.stamp'

# exe が未配置なら何もしない (パス未確定でもセッションは続行させる)。
if (-not (Test-Path -LiteralPath $Exe)) { exit 0 }

# デザインプロジェクトがまだ無ければ何もしない (生成対象が無い。設置後のプロンプトで拾う)。
if (-not (Test-Path -LiteralPath "$Project/app.clprj")) { exit 0 }

# claude-workspace / ai-refresh 対応判定 (Designer 1.3.15 以降)。未満なら GUI が起動してしまうので叩かない。
$dir = [System.IO.Path]::GetDirectoryName($Exe)
$designerDll = [System.IO.Path]::Combine($dir, 'Codeer.LowCode.Blazor.Designer.dll')
if (-not (Test-Path -LiteralPath $designerDll)) { exit 0 }
try {
    $v = [version](Get-Item -LiteralPath $designerDll).VersionInfo.FileVersion
    if ($v -lt [version]'1.3.15.0') { exit 0 }
} catch { exit 0 }

# バイナリ署名: exe と同フォルダの *.dll の LastWriteTimeUtc の最大値 (deploy 側と同一定義)。
# 注: Split-Path は -LiteralPath と -Parent が別パラメータセットで両立しない (AmbiguousParameterSet) ため使わない。
$files = @(Get-Item -LiteralPath $Exe)
$files += Get-ChildItem -LiteralPath $dir -Filter *.dll -ErrorAction SilentlyContinue
# Ticks は Int64。Measure-Object -Maximum は double 化して精度を落とすので、Int64 のまま最大値を取る。
[long]$sig = 0
foreach ($f in $files) {
    [long]$t = $f.LastWriteTimeUtc.Ticks
    if ($t -gt $sig) { $sig = $t }
}

# 生成物が揃っていて署名が前回と同じなら最新 = 何もしない。
$prev = if (Test-Path -LiteralPath $stamp) { (Get-Content -LiteralPath $stamp -Raw).Trim() } else { '' }
if ((Test-Path -LiteralPath 'ClaudeCodeForDesigner/_field_catalog.md') -and ($prev -eq "$sig")) { exit 0 }

# フレームワーク所有分を丸ごと最新化 (スタンプは ai-refresh 成功時に deploy が書く)。
# デザイナ exe は GUI サブシステムのため & では待てない。Start-Process -Wait で完了まで待つ
# (待たないと、フック終了後もフォルダ入れ替え中の状態をセッションが読んでしまう)。
Start-Process -FilePath $Exe -ArgumentList @('claude-workspace', '.', '--project', $Project) -Wait -WindowStyle Hidden
exit 0
