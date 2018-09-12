$path = $PSScriptRoot
Get-ChildItem -dir -Path $path -Recurse | Where-Object { $_.FullName.ToLower().EndsWith("\bin") -or $_.FullName.ToLower().EndsWith("\obj")} |
Foreach-object {
    Remove-item -Recurse -path $_.FullName -Force
    Write-Host "Folder <$($_.FullName)> deleted!" -ForegroundColor Green
}

Remove-item -Recurse -path "testresults" -Force
Remove-item -Recurse -path "deploy" -Force
Remove-item -Recurse -path "debug" -Force
