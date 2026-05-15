@echo off
REM OpenClaudeHere.bat
REM Open Windows Terminal and auto-close this batch window

set "WT_PATH=D:\Program Files\WindowsTerminal\WindowsTerminal.exe"

if not exist "%WT_PATH%" (
    echo ERROR: Windows Terminal not found
    echo Path: %WT_PATH%
    pause
    exit /b 1
)

start "" "%WT_PATH%" -d "%cd%" pwsh -NoExit -ExecutionPolicy Bypass -Command "opencode"
exit /b
