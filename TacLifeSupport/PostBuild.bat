set DIR=%1..\Plugins\
if not exist %DIR% mkdir %DIR%
copy Tac*.dll %DIR%

cd %1..
call test.bat