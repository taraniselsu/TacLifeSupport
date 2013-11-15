@if "%KSP_PLAY%"=="" (
	echo KSP_PLAY has not been set!
	pause
	exit 1
)

xcopy /s /f /y GameData %KSP_PLAY%\GameData\
pause
