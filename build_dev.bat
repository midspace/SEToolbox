@echo off
setlocal
call "%VS140COMNTOOLS%vsvars32.bat"
"MSBuild.exe" /fl "%~dp0Dev\SEToolbox\BuildScripts\Build.proj" %* "/property:ReleaseRoot=%~dp0bin;Configuration=Release"
IF NOT [%NOPAUSE%] == [Y] PAUSE
