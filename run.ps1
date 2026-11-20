Write-Host "====================================================" -ForegroundColor Cyan
Write-Host " Launching Nectar (Dynamic Island & Dock)" -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

Stop-Process -Name "NotchLite.App" -Force -ErrorAction SilentlyContinue
Stop-Process -Name "nectar" -Force -ErrorAction SilentlyContinue

$exe = Join-Path $PSScriptRoot "nectar\nectar.exe"
Start-Process -FilePath $exe -WorkingDirectory (Split-Path $exe)

Write-Host "`nNectar is now running!" -ForegroundColor Yellow
Write-Host "To stop it at any time: Stop-Process -Name nectar -Force" -ForegroundColor DarkGray
