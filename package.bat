@echo off

set DIR=TacLifeSupport_v%1

mkdir Release\%DIR%

xcopy /s /f /y Parts Release\%DIR%\Parts\
xcopy /s /f /y Plugins Release\%DIR%\Plugins\
xcopy /s /f /y Resources Release\%DIR%\Resources\
copy /y LICENSE.txt Release\%DIR%\
copy /y Readme.txt Release\%DIR%\

cd Release
7z a -tzip %DIR%.zip %DIR%
cd ..