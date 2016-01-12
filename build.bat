@echo off
%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /fl "%~dp0Main\SEToolbox\Build\Build.proj" %* "/property:ReleaseRoot=%~dp0bin;Configuration=Release;Platform=AnyCPU"
IF NOT [%NOPAUSE%] == [Y] PAUSE
