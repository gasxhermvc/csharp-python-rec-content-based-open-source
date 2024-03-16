@echo off
cd D:\Program\application\billy\rs2021
D:

REM Define var
set path=%CD%
echo %path%
set logFolder=%date:~-4%-%date:~4,2%
set logTempRoot=%path%\logs
set logTemp=%logTempRoot%\%logFolder%
set logCurrent=%date:~-4%%date:~4,2%%date:~7,2%
set findLogs=%logTemp%\log-%logCurrent%*.log


REM Check folder logs
IF EXIST %logTempRoot% (
echo %logTempRoot% is folder exist
) ELSE (
echo create folder %logTempRoot%
mkdir %logTempRoot%
)

REM Cal running code
set /a count=0
for /F "delims=" %%i in ('dir/s/b/a-d "%findLogs%"') do (set /a count=count+1)
set /a count=count+1
set paddedX=0

if %count% lss 10 (set "paddedX=0%count%") ELSE (set "paddedX=%count%")
echo %paddedX%	

REM create folder log a day
IF EXIST %logTemp% (
echo %logTemp% is folder exist
) ELSE (
echo create folder %logTemp%
mkdir %logTemp%
)

REM run program
call RecommendSystemContentBased.exe --EnvironmentVariable=Production --Debug=true >> "%logTemp%\log-%logCurrent%_%paddedX%.log" 2>&1