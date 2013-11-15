set Path=%Path%;C:\Windows\Microsoft.NET\Framework64\v4.0.30319
MSBuild /p:Configuration=Release /target:Package Source\TacLifeSupport.csproj
pause
