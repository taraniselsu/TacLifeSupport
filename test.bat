@if "%KSP_TEST%"=="" (
	echo KSP_TEST has not been set!	
	exit 1
)

xcopy /s /f /y /e GameData "%KSP_TEST%\GameData\"
