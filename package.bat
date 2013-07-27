@echo off

set MOD_NAME=TacLifeSupport

set DIR=%MOD_NAME%_%1

mkdir Release\%DIR%

xcopy /s /f /y GameData Release\%DIR%\GameData\
copy /y LICENSE.txt Release\%DIR%\GameData\%MOD_NAME%\
copy /y Readme.txt Release\%DIR%\GameData\%MOD_NAME%\

cd Release\%DIR%
7z a -tzip ..\%DIR%.zip GameData
cd ..\..
