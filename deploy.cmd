@ECHO OFF
REM SET Cake
SET CAKE_VERSION=0.23.0
SET CAKE_FOLDER=Cake.%CAKE_VERSION%
SET PATH=%~dp0\Tools;%PATH%

REM Cleanup any old Cake versions
FOR /f "delims=" %%c IN ('dir /AD /B "Tools\Cake*"') DO (
        IF NOT "%%c" == "%CAKE_FOLDER%" (RD /S /Q "Tools\%%c")
)


REM Install Dependencies
IF NOT EXIST "Tools" (md "Tools")
IF NOT EXIST "Tools\Addins" (md "Tools\Addins")
IF NOT EXIST "Tools\%CAKE_FOLDER%\Cake.exe" (
    echo Downloading Cake %CAKE_VERSION%
    nuget install Cake -Version %CAKE_VERSION% -OutputDirectory "Tools" -Source https://api.nuget.org/v3/index.json
    )

REM Execute deploy
Tools\%CAKE_FOLDER%\Cake.exe -version
Tools\%CAKE_FOLDER%\Cake.exe build.cake --Target="Publish"