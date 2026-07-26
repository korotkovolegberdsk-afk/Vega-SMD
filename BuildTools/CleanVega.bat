@echo off
title SEP Clean

echo ==========================================
echo Cleaning project...
echo ==========================================

for /d /r %%d in (bin,obj) do (
    if exist "%%d" rd /s /q "%%d"
)

if exist Release rd /s /q Release

echo.
echo Finished.

pause