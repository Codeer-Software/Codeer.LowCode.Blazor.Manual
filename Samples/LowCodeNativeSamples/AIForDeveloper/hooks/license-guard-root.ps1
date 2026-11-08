# ホストソリューションのルートでエージェント (Claude Code / Codex / GitHub Copilot) を起動したときに、配下のデザインプロジェクト
# (DesignProjects/<名>/<デザインのフォルダ>/) へのファイル編集を、各ワークスペースの
# .claude/license-guard.ps1 (ライセンスで編集を止める PreToolUse フック) に委ねるスクリプト。
# 編集先の取り出しと判定はそちらが持つ。デザインプロジェクトのフォルダ名は、ワークスペース直下で app.clprj を持つフォルダから求める。
#
# hook から:  powershell -NoProfile -ExecutionPolicy Bypass -File "AIForDeveloper/hooks/license-guard-root.ps1" "<デザイナexeのパス>"
# 委ねた先が止めたら (exit 2・理由は標準エラー) そのまま exit 2。それ以外は何も出さず exit 0。ホストのテンプレートが持つ (フックの設定はデザイナの developer-workspace が settings.local.json.sample から生成する)。

param(
    [Parameter(Mandatory = $true)][string]$Exe,
    [string]$DesignProjectsDir = 'DesignProjects'
)

$ErrorActionPreference = 'SilentlyContinue'

# 入出力は UTF-8 (既定の OEM コードページ / ASCII だと日本語のパスが化けて判定が素通りする)
[Console]::InputEncoding = New-Object System.Text.UTF8Encoding $false
$OutputEncoding = New-Object System.Text.UTF8Encoding $false
$raw = [Console]::In.ReadToEnd()
if ([string]::IsNullOrWhiteSpace($raw)) { exit 0 }
try { $hook = $raw | ConvertFrom-Json } catch { exit 0 }

if (-not (Test-Path -LiteralPath $Exe)) { exit 0 }
if (-not (Test-Path -LiteralPath $DesignProjectsDir)) { exit 0 }

# 委ねた先は各ワークスペースで動くので、相対パスの基準 (cwd) が入力に無ければルートを入れて渡す
if (-not ($hook.cwd -is [string] -and $hook.cwd -ne '')) {
    $hook | Add-Member -NotePropertyName cwd -NotePropertyValue (Get-Location).Path -Force
    $raw = $hook | ConvertTo-Json -Compress -Depth 20
}

foreach ($ws in Get-ChildItem -LiteralPath $DesignProjectsDir -Directory) {
    $script = Join-Path $ws.FullName '.claude\license-guard.ps1'
    if (-not (Test-Path -LiteralPath $script)) { continue }

    $project = $null
    foreach ($d in Get-ChildItem -LiteralPath $ws.FullName -Directory) {
        if (Test-Path -LiteralPath (Join-Path $d.FullName 'app.clprj')) { $project = $d.Name; break }
    }
    if ($null -eq $project) { continue }

    Push-Location -LiteralPath $ws.FullName
    try {
        $raw | & powershell -NoProfile -ExecutionPolicy Bypass -File $script $Exe $project
        $code = $LASTEXITCODE
    } finally {
        Pop-Location
    }
    if ($code -eq 2) { exit 2 }
}
exit 0
