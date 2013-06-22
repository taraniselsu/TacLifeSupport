@echo off

set DIR=TacLifeSupport_%1

mkdir Release\%DIR%

xcopy /s /f /y GameData Release\%DIR%\GameData\
copy /y LICENSE.txt Release\%DIR%\GameData\TacLifeSupport\
copy /y Readme.txt Release\%DIR%\GameData\TacLifeSupport\

cd Release
7z a -tzip %DIR%.zip %DIR%
cd ..
