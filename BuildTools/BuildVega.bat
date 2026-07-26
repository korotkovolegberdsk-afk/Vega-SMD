@echo off
title SEP Build

echo ==========================================
echo Building SEP...
echo ==========================================

dotnet restore

dotnet build SEP.sln -c Release

echo.
echo ==========================================
echo BUILD FINISHED
echo ==========================================

pause