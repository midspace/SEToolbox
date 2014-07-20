@echo off
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /fl %~dp0\Main\SEToolbox\Build\Build.proj %* /property:ReleaseRoot=%~dp0\bin;Configuration=Release
IF NOT [%NOPAUSE%] == [Y] PAUSE
