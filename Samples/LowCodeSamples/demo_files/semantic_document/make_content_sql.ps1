# Writes a SQL file that puts the demo attachments of this folder into semantic_documents.file_content (bytea).
# Run after the CREATE TABLE / INSERT of ddl_semantic_document.txt. Rows are matched by file_name (= the file name in this folder).
#   powershell -ExecutionPolicy Bypass -File make_content_sql.ps1 -OutFile C:\Temp\semantic_document_content.sql
#   LowCodeSamples.Designer.exe sql "<Design folder>" --datasource Main --file C:\Temp\semantic_document_content.sql
param([Parameter(Mandatory = $true)][string]$OutFile)

$sb = New-Object System.Text.StringBuilder
Get-ChildItem -LiteralPath $PSScriptRoot -File | Where-Object { $_.Extension -ne '.ps1' } | Sort-Object Name | ForEach-Object {
    $bytes = [System.IO.File]::ReadAllBytes($_.FullName)
    $base64 = [System.Convert]::ToBase64String($bytes)
    $name = $_.Name.Replace("'", "''")
    [void]$sb.AppendLine("UPDATE semantic_documents SET file_content = decode('$base64', 'base64'), file_size = $($bytes.Length) WHERE file_name = '$name';")
}
[System.IO.File]::WriteAllText($OutFile, $sb.ToString(), (New-Object System.Text.UTF8Encoding($false)))
Write-Host "wrote $OutFile"
