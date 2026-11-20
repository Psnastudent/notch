@echo off
echo ====================================================
echo  Launching Nectar (Dynamic Island ^& Dock)
echo ====================================================
taskkill /F /IM NotchLite.App.exe >nul 2>&1
taskkill /F /IM nectar.exe >nul 2>&1
start "" "%~dp0nectar\nectar.exe"
echo.
echo Nectar is now running!
echo To stop it: taskkill /F /IM nectar.exe
