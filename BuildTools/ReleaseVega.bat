@echo off
title SEP Release

echo ==========================================
echo Building Release...
echo ==========================================

call BuildSEP.bat

if not exist Release mkdir Release

xcopy SEP.Viewer\bin\Release\net8.0-windows Release\SEP_Release\ /E /I /Y

echo.
echo Release created.

pause