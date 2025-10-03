@echo off
setlocal enabledelayedexpansion
cd /d %~dp0/../
cls
set /p version=<ExtraProps/VERSION
set /p adofaipath=<ExtraProps/ADOFAIPath
set /p modname=<ExtraProps/MODNAME
set /p dllpath=<ExtraProps/DLL
echo Building %modname%[%version%]...
mkdir tmp
cd tmp
mkdir %dllpath%
copy ..\Info.json %dllpath%
//copy ..\Relief\%modname%\bin\Debug\*.* %dllpath%

set "sourceDir=..\Relief\%modname%\bin\Debug"
set "destDir=%dllpath%"

if not exist "%sourceDir%" (
    echo sourceDir is not defined
    goto :eof
)

for %%F in ("%sourceDir%\*") do (
    echo %%F | findstr /i "Unity" >nul
    if !errorlevel! equ 0 (
        del "%%F" >nul
        echo [Skipped] %%~nxF
    ) else (
        copy "%%F" "%destDir%" >nul 
        echo [Copied] !filename!
    )

    
)

cd %dllpath%
for /f "delims=" %%a in (Info.json) do (
    SET s=%%a
    SET s=!s:$VERSION=%version%!
    echo !s!
)>>"InfoChanged.json"
del Info.json
rename InfoChanged.json Info.json

cd ..

tar -a -c -f %dllpath%-%version%.zip %dllpath%

xcopy "%~dp0\tmp\%dllpath%\" "%adofaipath%\Mods\%dllpath%\" /S /E /Q /Y
move %dllpath%-%version%.zip ..
cd ..
rmdir /s /q tmp
pause