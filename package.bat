set MOD_NAME=TacLifeSupport

@if "%KSP%"=="" (
	echo KSP has not been set!
	pause
	exit 1
)

set Path=%Path%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319
MSBuild /p:Configuration=Release /target:Package Source\%MOD_NAME%.csproj
pause
