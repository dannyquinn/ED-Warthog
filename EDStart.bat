SET TARGET-GUI-PATH="%PROGRAMFILES(X86)%\Thrustmaster\TARGET\x64"
SET TARGET-SCRIPT-FILE="D:\Code\Danny\GitHub\ED-Warthog\WarthogScript\main.tmc"
SET ED-DISCOVERY-PATH="C:\Program Files\EDDiscovery"
SET ED-STATUS-CLIENT-PATH="D:\Code\Danny\GitHub\ED-Warthog\EDStatusFlagsClient\bin\Debug\netcoreapp3.1"

ECHO OFF

ECHO LAUNCHING: TargetGui
CD %TARGET-GUI-PATH%
IF NOT ERRORLEVEL 1 TASKLIST | FIND /I "TargetGui.exe" > nul || (START /d %TARGET-GUI-PATH% TargetGui.exe -r %TARGET-SCRIPT-FILE%)
PING -n 2 localhost > nul
TASKLIST | FIND /I "TargetGui.exe" > nul
IF NOT ERRORLEVEL 1 ECHO TargetGui Started
IF ERRORLEVEL 1 ECHO Failed to start TargetGui
ECHO.

ECHO LAUNCHING: ED Discovery
CD %ED-DISCOVERY-PATH%
IF NOT ERRORLEVEL 1 TASKLIST | FIND /I "EDDiscovery.exe" > nul || (START /d %ED-DISCOVERY-PATH% EDDiscovery.exe)
PING -n 2 localhost > nul
TASKLIST | FIND /I "EDDiscovery.exe" > nul
IF NOT ERRORLEVEL 1 ECHO EDDiscovery Started
IF ERRORLEVEL 1 ECHO Failed to start EDDiscovery
ECHO.

ECHO LAUNCHING: EDStatusFlagsClient.exe 
CD %ED-STATUS-CLIENT-PATH%
IF NOT ERRORLEVEL 1 TASKLIST | FIND /I "EDStatusFlagsClient.exe" > nul || (START /d %ED-STATUS-CLIENT-PATH% EDStatusFlagsClient.exe)
PING -n 2 localhost > nul
TASKLIST | FIND /I "EDStatusFlagsClient.exe" > nul
IF NOT ERRORLEVEL 1 ECHO EDStatusFlagsClient Started
IF ERRORLEVEL 1 ECHO Failed to start EDStatusFlagsClient
ECHO.




