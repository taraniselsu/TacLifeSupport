@if "%KSP_TEST%"=="" (
	echo KSP_TEST has not been set!
	pause
	exit 1
)

xcopy /s /f /y GameData %KSP_TEST%\GameData\
pause
