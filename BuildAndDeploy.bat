@echo on
REM Set variables based on arguments passed to the script
set projectName=%1
set buildPath=%2
set projectDir=%3
set targetDir=%4
set Configuration=%~5
set subnauticaFolder=D:\Steam\steamapps\common\Subnautica\BepInEx\plugins\%projectName%.%Configuration%\
set subnauticaZeroFolder=D:\Steam\steamapps\common\SubnauticaZero\BepInEx\plugins\%projectName%.%Configuration%\
set zipDestination=%3..\Download\%Configuration%\%projectName%.%Configuration%.zip

REM Debugging: Echo paths to verify
echo projectName=%projectName%
echo buildPath=%buildPath%
echo projectDir=%projectDir%
echo targetDir=%targetDir%
echo Configuration=%Configuration%

REM Determine the correct folder based on Configuration
if "%Configuration%"=="SN" (
    set targetFolder=%subnauticaFolder%
) else if "%Configuration%"=="BZ" (
    set targetFolder=%subnauticaZeroFolder%
) else (
    echo "Unknown Configuration: %Configuration%"
    pause
    exit /b 1
)

echo targetFolder=%targetFolder%

REM Create directories if they do not exist
if not exist "%targetFolder%\Assets" (
    mkdir "%targetFolder%\Assets"
)

REM Copy build files
xcopy "%buildPath%" "%targetFolder%" /y
xcopy "%projectDir%*.json" "%targetFolder%" /y
xcopy "%projectDir%Assets\*.asset" "%targetFolder%\Assets\" /y
xcopy "%projectDir%Assets\*.manifest" "%targetFolder%\Assets\" /y
xcopy "%projectDir%Assets\*.png" "%targetFolder%\Assets\" /y

REM Copy AD3D_Common.dll file
xcopy "%targetDir%\AD3D_Common.dll" "%targetFolder%" /y
xcopy "%targetDir%\Assets\*.asset" "%targetFolder%\Assets\" /y
xcopy "%targetDir%\Assets\*.manifest" "%targetFolder%\Assets\" /y
xcopy "%targetDir%\Assets\*.png" "%targetFolder%\Assets\" /y

REM Zip the mod folder
set "SEVENZIP=C:\Program Files\7-Zip\7z.exe"
if exist "%SEVENZIP%" (
    "%SEVENZIP%" a -tzip "%zipDestination%" "%targetFolder%" -mx9
) else (
    echo WARNING: 7z.exe not found at %SEVENZIP%. Skipping zip creation.
)

REM Notify completion
echo Mod build and deployment complete.
pause