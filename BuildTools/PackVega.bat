@echo off
setlocal EnableExtensions

title Pack SEP Source

REM -------------------------------------------------
REM Переходим в корень проекта
REM -------------------------------------------------
cd /d "%~dp0\.."

REM Удаляем старый архив
if exist "SEP_Source.zip" del /q "SEP_Source.zip"

REM -------------------------------------------------
REM Создаем временную папку
REM -------------------------------------------------
if exist "_PackTemp" rmdir /s /q "_PackTemp"

mkdir "_PackTemp"

REM -------------------------------------------------
REM Копируем только нужные файлы
REM -------------------------------------------------
robocopy . "_PackTemp" /E ^
 /XD _PackTemp bin obj .vs x64 x86 Debug Release ^
 /XF *.pdb *.user *.suo *.cache SEP_Source.zip ^
 /R:0 /W:0 /NFL /NDL /NJH /NJS /NC /NS

REM -------------------------------------------------
REM Создаем ZIP встроенным tar Windows 11
REM -------------------------------------------------
cd "_PackTemp"

tar -a -c -f "..\SEP_Source.zip" *

cd ..

REM -------------------------------------------------
REM Удаляем временную папку
REM -------------------------------------------------
rmdir /s /q "_PackTemp"

echo.
echo ==========================================
echo Archive created:
echo %CD%\SEP_Source.zip
echo ==========================================
echo.

pause