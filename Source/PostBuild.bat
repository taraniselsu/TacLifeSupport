set DIR=%1..\GameData\TacLifeSupport\
if not exist %DIR% mkdir %DIR%
copy Tac*.dll %DIR%

cd %1..
call test.bat