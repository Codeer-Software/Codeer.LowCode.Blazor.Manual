# 使い方: powershell -File _capture.ps1 -Category text -FieldName TextSingleLine
# 撮影→右端500pxをクロップして _properties_panel.png として保存

param(
    [Parameter(Mandatory=$true)][string]$Category,
    [Parameter(Mandatory=$true)][string]$FieldName,
    [int]$SleepSeconds = 3,
    [int]$PanelWidth = 500
)

$ErrorActionPreference = 'Stop'
$code = @"
using System;
using System.Runtime.InteropServices;
using System.Text;
public class Win32Helper {
    [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
    [DllImport("user32.dll")] public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
    public struct RECT { public int Left; public int Top; public int Right; public int Bottom; }
}
"@
Add-Type -TypeDefinition $code -Language CSharp -ErrorAction SilentlyContinue
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

Write-Output "$SleepSeconds 秒後に撮影"
Start-Sleep -Seconds $SleepSeconds

$hwnd = [Win32Helper]::GetForegroundWindow()
$rect = New-Object Win32Helper+RECT
[void][Win32Helper]::GetWindowRect($hwnd, [ref]$rect)
$title = New-Object System.Text.StringBuilder 256
[void][Win32Helper]::GetWindowText($hwnd, $title, 256)
$w = $rect.Right - $rect.Left
$h = $rect.Bottom - $rect.Top

$baseDir = "C:\GitHub\Codeer.LowCode.Blazor.Manual\Image\designer\fields\$Category"
if (-not (Test-Path $baseDir)) { New-Item -ItemType Directory -Path $baseDir -Force | Out-Null }

# フル画像を撮る
$fullPath = Join-Path $baseDir "${FieldName}_full.png"
$bitmap = New-Object System.Drawing.Bitmap $w, $h
$g = [System.Drawing.Graphics]::FromImage($bitmap)
$g.CopyFromScreen($rect.Left, $rect.Top, 0, 0, $bitmap.Size)
$bitmap.Save($fullPath)

# 右端 PanelWidth をクロップ
$panelPath = Join-Path $baseDir "${FieldName}_properties_panel.png"
$cropX = [Math]::Max(0, $w - $PanelWidth)
$cropW = [Math]::Min($PanelWidth, $w)
$cropRect = New-Object System.Drawing.Rectangle $cropX, 0, $cropW, $h
$cropped = New-Object System.Drawing.Bitmap $cropW, $h
$gc = [System.Drawing.Graphics]::FromImage($cropped)
$destRect = New-Object System.Drawing.Rectangle 0, 0, $cropW, $h
$gc.DrawImage($bitmap, $destRect, $cropRect, [System.Drawing.GraphicsUnit]::Pixel)
$cropped.Save($panelPath)

$gc.Dispose(); $cropped.Dispose()
$g.Dispose(); $bitmap.Dispose()

Write-Output "Title: $($title.ToString())"
Write-Output "Size : ${w}x${h}"
Write-Output "Full : $fullPath"
Write-Output "Panel: $panelPath"
