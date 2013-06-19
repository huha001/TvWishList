REM *************************************************************************
REM DEFINE VARIABLES
REM *************************************************************************
REM 


SET PLUGIN_NAME=TvWishListMP2


SET RELEASE_DIR="C:\Program Files (x86)\Team MediaPortal\MP2-Client\Plugins"
REM remove quotes
SET RELEASE_DIR=###%RELEASE_DIR%###
SET RELEASE_DIR=%RELEASE_DIR:"###=%
SET RELEASE_DIR=%RELEASE_DIR:###"=%
SET RELEASE_DIR=%RELEASE_DIR:###=%

SET SOURCE_DIR="H:\User Daten\Visual C# Projects\MP2 TvWishListMP2\Bin\TvWishListMP2\bin\Release\Plugins"
REM remove quotes
SET SOURCE_DIR=###%SOURCE_DIR%###
SET SOURCE_DIR=%SOURCE_DIR:"###=%
SET SOURCE_DIR=%SOURCE_DIR:###"=%
SET SOURCE_DIR=%SOURCE_DIR:###=%

SET SOURCE_DIR2="H:\User Daten\Visual C# Projects\MP2 TvWishListMP2"
REM remove quotes
SET SOURCE_DIR2=###%SOURCE_DIR2%###
SET SOURCE_DIR2=%SOURCE_DIR2:"###=%
SET SOURCE_DIR2=%SOURCE_DIR2:###"=%
SET SOURCE_DIR2=%SOURCE_DIR2:###=%


MKDIR "%RELEASE_DIR%\%PLUGIN_NAME%"
CD "%SOURCE_DIR%"


XCOPY "%SOURCE_DIR%\%PLUGIN_NAME%" "%RELEASE_DIR%\%PLUGIN_NAME%" /S /C /I /Y
XCOPY "%SOURCE_DIR2%\%PLUGIN_NAME% Resources\%PLUGIN_NAME% Resources\Skin" "%RELEASE_DIR%\%PLUGIN_NAME%\Skin" /S /C /I /Y
XCOPY "%SOURCE_DIR2%\%PLUGIN_NAME% Resources\%PLUGIN_NAME% Resources\Language" "%RELEASE_DIR%\%PLUGIN_NAME%\Language" /S /C /I /Y

PAUSE


