REM *************************************************************************
REM DEFINE VARIABLES
REM *************************************************************************
REM 

SET OLDVERSION=1.4.0.0
SET NEWVERSION=1.4.0.1


SET MSFRAMEPATH="C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe"
SET MSFRAMEPATH_MP2="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
REM SET FILE_REPLACE=..\FileReplaceString.exe
SET FILE_REPLACE=FileReplaceString.exe
REM ******************************END VARIABLES******************************



REM *************************************************************************
REM Version Update
REM *************************************************************************

MOVE TvWishList.V.%OLDVERSION%.odt TvWishList.V.%NEWVERSION%.odt

DEL TvWishList.Source\MPEI2\*.mpe1
DEL TvWishList.Source\MPEI2\*.xml

REM **************************************************************************
REM Language Conversion From Transifex
REM "H:\User Daten\Visual C# Projects\TvWishList\TvWishListGitRepository\TvWishList\LanguageConverter.bat"
REM **************************************************************************


REM **************************************************************************
REM Plugin Code
REM **************************************************************************

%FILE_REPLACE% "TvWishList.Source\TvWishList2.0\TvWishList\TvWishList.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList2.0\TvWishList\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList2.0\TvWishList\TvWish.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\TvWishList1.2\TvWishList\TvWishList.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList1.2\TvWishList\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList1.2\TvWishList\TvWish.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\TvWishList1.1\TvWishList\TvWishList.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList1.1\TvWishList\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\TvWishList1.0.1\TvWishList\TvWishList.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishList1.0.1\TvWishList\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%


%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.2\TvWishListMP\MP1_Main_GUI.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\TvWish.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.2\TvWishListMP\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.6\TvWishListMP\MP1_Main_GUI.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\TvWish.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\TvWishListMP1.6\TvWishListMP\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\Install\Install\InstallSetup.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\Install\Install\InstallSetup.Designer.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\Install\Install\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\MPEI2\TvWishList.xmp2" %OLDVERSION% %NEWVERSION%

REM MP2
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\CommonCodeMP1MP2\TvWish.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\CommonCodeMP1MP2\Common_Main_GUI.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\plugin.xml" %OLDVERSION% %NEWVERSION%

%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMPExtendedProvider\TvWishListMPExtendedProvider\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%
%FILE_REPLACE% "TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2NativeTvProvider\TvWishListMP2NativeTvProvider\Properties\AssemblyInfo.cs" %OLDVERSION% %NEWVERSION%

REM %FILE_REPLACE% MP.release.r2d2.bat %OLDVERSION% %NEWVERSION%
REM %FILE_REPLACE% MP2.release.r2d2.bat %OLDVERSION% %NEWVERSION%

REM ******************** END VERSION UPDATE ********************



REM *************************************************************************
REM Update Different Versions
REM *************************************************************************

CD TvWishList.Source

COPY /Y "TvWishList2.0\TvWishList\TvWishList.cs" "TvWishList1.2\TvWishList\TvWishList.cs"
COPY /Y "TvWishList2.0\TvWishList\TvWishList.setup.cs" "TvWishList1.2\TvWishList\TvWishList.setup.cs"
COPY /Y "TvWishList2.0\TvWishList\TvWishList.setup.resx" "TvWishList1.2\TvWishList\TvWishList.setup.resx"
COPY /Y "TvWishList2.0\TvWishList\TvWishList.setup.Designer.cs" "TvWishList1.2\TvWishList\TvWishList.setup.Designer.cs"
COPY /Y "TvWishList2.0\TvWishList\EpgClass.cs" "TvWishList1.2\TvWishList\EpgClass.cs"
COPY /Y "TvWishList2.0\TvWishList\InstallPaths.cs" "TvWishList1.2\TvWishList\InstallPaths.cs"
COPY /Y "TvWishList2.0\TvWishList\Messages.cs" "TvWishList1.2\TvWishList\Messages.cs"
COPY /Y "TvWishList2.0\TvWishList\TvWish.cs" "TvWishList1.2\TvWishList\TvWish.cs"
REM Log.cs must not be copied from 2.0
COPY /Y "TvWishList2.0\TvWishList\LanguageTranslation.cs" "TvWishList1.2\TvWishList\LanguageTranslation.cs"

COPY /Y "TvWishList1.2\TvWishList\TvWishList.cs" "TvWishList1.0.1\TvWishList\TvWishList.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.cs" "TvWishList1.0.1\TvWishList\TvWishList.setup.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.resx" "TvWishList1.0.1\TvWishList\TvWishList.setup.resx"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.Designer.cs" "TvWishList1.0.1\TvWishList\TvWishList.setup.Designer.cs"
COPY /Y "TvWishList1.2\TvWishList\EpgClass.cs" "TvWishList1.0.1\TvWishList\EpgClass.cs"
COPY /Y "TvWishList1.2\TvWishList\InstallPaths.cs" "TvWishList1.0.1\TvWishList\InstallPaths.cs"
COPY /Y "TvWishList1.2\TvWishList\PipeCommands.cs" "TvWishList1.0.1\TvWishList\PipeCommands.cs"
COPY /Y "TvWishList1.2\TvWishList\Messages.cs" "TvWishList1.0.1\TvWishList\Messages.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWish.cs" "TvWishList1.0.1\TvWishList\TvWish.cs"
COPY /Y "TvWishList1.2\TvWishList\Log.cs" "TvWishList1.0.1\TvWishList\Log.cs"
COPY /Y "TvWishList1.2\TvWishList\LanguageTranslation.cs" "TvWishList1.0.1\TvWishList\LanguageTranslation.cs"

COPY /Y "TvWishList1.2\TvWishList\TvWishList.cs" "TvWishList1.1\TvWishList\TvWishList.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.cs" "TvWishList1.1\TvWishList\TvWishList.setup.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.resx" "TvWishList1.1\TvWishList\TvWishList.setup.resx"
COPY /Y "TvWishList1.2\TvWishList\TvWishList.setup.Designer.cs" "TvWishList1.1\TvWishList\TvWishList.setup.Designer.cs"
COPY /Y "TvWishList1.2\TvWishList\EpgClass.cs" "TvWishList1.1\TvWishList\EpgClass.cs"
COPY /Y "TvWishList1.2\TvWishList\InstallPaths.cs" "TvWishList1.1\TvWishList\InstallPaths.cs"
COPY /Y "TvWishList1.2\TvWishList\PipeCommands.cs" "TvWishList1.1\TvWishList\PipeCommands.cs"
COPY /Y "TvWishList1.2\TvWishList\Messages.cs" "TvWishList1.1\TvWishList\Messages.cs"
COPY /Y "TvWishList1.2\TvWishList\TvWish.cs" "TvWishList1.1\TvWishList\TvWish.cs"
COPY /Y "TvWishList1.2\TvWishList\Log.cs" "TvWishList1.1\TvWishList\Log.cs"
COPY /Y "TvWishList1.2\TvWishList\LanguageTranslation.cs" "TvWishList1.1\TvWishList\LanguageTranslation.cs"


COPY /Y "TvWishListMP1.6\TvWishListMP\MP1_Main_GUI.cs" "TvWishListMP1.2\TvWishListMP\MP1_Main_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\MP1_Edit_GUI.cs" "TvWishListMP1.2\TvWishListMP\MP1_Edit_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\MP1_Result_GUI.cs" "TvWishListMP1.2\TvWishListMP\MP1_Result_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Common_Edit_GUI.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Edit_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Common_Result_GUI.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Result_GUI.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\PipeClient.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\PipeClient.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\TvDatabaseConnect.cs" "TvWishListMP1.2\TvWishListMP\TvDatabaseConnect.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\TvWish.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\TvWish.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Log.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Log.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\InstallPaths.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\InstallPaths.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\LocalizeStrings.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\LocalizeStrings.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\CommonCodeMP1MP2\Messages.cs" "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Messages.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\Setup.cs" "TvWishListMP1.2\TvWishListMP\Setup.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\Setup.Designer.cs" "TvWishListMP1.2\TvWishListMP\Setup.Designer.cs"
COPY /Y "TvWishListMP1.6\TvWishListMP\Setup.resx" "TvWishListMP1.2\TvWishListMP\Setup.resx"



COPY /Y "TvWishListMP1.2\TvWishListMP\MP1_Main_GUI.cs" "TvWishListMP1.1\TvWishListMP\MP1_Main_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\MP1_Edit_GUI.cs" "TvWishListMP1.1\TvWishListMP\MP1_Edit_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\MP1_Result_GUI.cs" "TvWishListMP1.1\TvWishListMP\MP1_Result_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\Common_Main_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Edit_GUI.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\Common_Edit_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Common_Result_GUI.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\Common_Result_GUI.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\PipeClient.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\PipeClient.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\TvDatabaseConnect.cs" "TvWishListMP1.1\TvWishListMP\TvDatabaseConnect.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\TvWish.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\TvWish.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Log.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\Log.cs"
REM installpaths was commented out - version number?
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\InstallPaths.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\InstallPaths.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\LocalizeStrings.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\LocalizeStrings.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\CommonCodeMP1MP2\Messages.cs" "TvWishListMP1.1\TvWishListMP\CommonCodeMP1MP2\Messages.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\Setup.cs" "TvWishListMP1.1\TvWishListMP\Setup.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\Setup.Designer.cs" "TvWishListMP1.1\TvWishListMP\Setup.Designer.cs"
COPY /Y "TvWishListMP1.2\TvWishListMP\Setup.resx" "TvWishListMP1.1\TvWishListMP\Setup.resx"

REM *************************************************************************
REM Compile ALL
REM *************************************************************************

%MSFRAMEPATH_MP2% TvWishList2.0\TvWishList.sln /p:Configuration=Release
%MSFRAMEPATH% TvWishList1.2\TvWishList.sln /p:Configuration=Release
%MSFRAMEPATH% TvWishList1.1\TvWishList.sln /p:Configuration=Release
%MSFRAMEPATH% TvWishList1.0.1\TvWishList.sln /p:Configuration=Release
%MSFRAMEPATH% TvWishListMP1.1\TvWishListMP.sln /p:Configuration=Release
%MSFRAMEPATH% TvWishListMP1.2\TvWishListMP.sln /p:Configuration=Release
%MSFRAMEPATH_MP2% TvWishListMP1.6\TvWishListMP.sln /p:Configuration=Release
%MSFRAMEPATH_MP2% Install\Install.sln /p:Configuration=Release

%MSFRAMEPATH_MP2% "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\TvWishListInterface.sln" /p:Configuration=Release
COPY /Y "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\bin\Release\TvWishListInterface.dll" "MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\bin\Release\TvWishListInterface.dll"
COPY /Y "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\bin\Release\TvWishListInterface.dll" "MP2 TvWishListMP2\TvWishListMPExtendedProvider\TvWishListMPExtendedProvider\bin\Release\TvWishListInterface.dll"

%MSFRAMEPATH_MP2% "MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2.sln" /p:Configuration=Release

%MSFRAMEPATH_MP2% "MP2 TvWishListMP2\TvWishListMPExtendedProvider\TvWishListMPExtendedProvider.sln" /p:Configuration=Release
%MSFRAMEPATH_MP2% "MP2 TvWishListMP2\TvWishListMP2NativeTvProvider\TvWishListMP2NativeTvProvider.sln" /p:Configuration=Release

REM ******************** END COMPILE  ********************



REM *************************************************************************
REM release All
REM *************************************************************************

CD TvWishList.Source

COPY /Y "Install\Install\bin\Release\Install.exe" "..\TvWishList.Release\Install.exe"

COPY /Y "TvWishList1.0.1\TvWishList\bin\Release\TvWishList.dll" "..\TvWishList.Release\TvWishList1.0.1\TvWishList.dll"
COPY /Y "TvWishList1.1\TvWishList\bin\Release\TvWishList.dll" "..\TvWishList.Release\TvWishList1.1\TvWishList.dll"
COPY /Y "TvWishList1.2\TvWishList\bin\Release\TvWishList.dll" "..\TvWishList.Release\TvWishList1.2\TvWishList.dll"
COPY /Y "TvWishList2.0\TvWishList\bin\Release\Mediaportal.TV.Server.Plugins.TvWishList.dll" "..\TvWishList.Release\TvWishList2.0\Mediaportal.TV.Server.Plugins.TvWishList.dll"
COPY /Y "TvWishListMP1.1\TvWishListMP\bin\Release\TvWishListMP.dll" "..\TvWishList.Release\TvWishListMP1.1\TvWishListMP.dll"
COPY /Y "TvWishListMP1.2\TvWishListMP\bin\Release\TvWishListMP.dll" "..\TvWishList.Release\TvWishListMP1.2\TvWishListMP.dll"
COPY /Y "TvWishListMP1.6\TvWishListMP\bin\Release\TvWishListMP.dll" "..\TvWishList.Release\TvWishListMP1.6\TvWishListMP.dll"


copy /Y "MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\bin\Release\SlimTv.Interfaces.dll" "MP2 TvWishListMP2\Bin\TvWishListMP2\bin\Release\Plugins\TvWishListMP2\SlimTv.Interfaces.dll"
XCOPY "MP2 TvWishListMP2\Bin\TvWishListMP2\bin\Release\Plugins\TvWishListMP2" "..\TvWishList.Release\TvWishListMP2"  /S /C /I /Y

copy /Y "MP2 TvWishListMP2\TvWishListMPExtendedProvider\TvWishListMPExtendedProvider\bin\Release\MPExtended.Services.Common.Interfaces.dll" "MP2 TvWishListMP2\Bin\TvWishListMPExtendedProvider\bin\Release\Plugins\TvWishListMPExtendedProvider\MPExtended.Services.Common.Interfaces.dll"
copy /Y "MP2 TvWishListMP2\TvWishListMPExtendedProvider\TvWishListMPExtendedProvider\bin\Release\MPExtended.Services.TVAccessService.Interfaces.dll" "MP2 TvWishListMP2\Bin\TvWishListMPExtendedProvider\bin\Release\Plugins\TvWishListMPExtendedProvider\MPExtended.Services.TVAccessService.Interfaces.dll"
XCOPY "MP2 TvWishListMP2\Bin\TvWishListMPExtendedProvider\bin\Release\Plugins\TvWishListMPExtendedProvider" "..\TvWishList.Release\TvWishListMPExtendedProvider"  /S /C /I /Y

XCOPY "MP2 TvWishListMP2\Bin\TvWishListMP2NativeTvProvider\bin\Release\Plugins\TvWishListMP2NativeTvProvider" "..\TvWishList.Release\TvWishListMP2NativeTvProvider"  /S /C /I /Y
XCOPY "MP2 TvWishListMP2\TvWishListMP2NativeTvProvider\TvWishListMP2NativeTvProvider\Language" "..\TvWishList.Release\TvWishListMP2NativeTvProvider\Language"  /S /C /I /Y

COPY /Y "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\bin\Release\TvWishListInterface.dll" "..\TvWishList.Release\TvWishListMP2"
COPY /Y "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\bin\Release\TvWishListInterface.dll" "..\TvWishList.Release\TvWishListMPExtendedProvider"
COPY /Y "MP2 TvWishListMP2\TvWishListInterface\TvWishListInterface\bin\Release\TvWishListInterface.dll" "..\TvWishList.Release\TvWishListMP2NativeTvProvider"

REM XCOPY "MP2 TvWishListMP2\TvWishListMP2 Resources\TvWishListMP2 Resources\Skin" "..\TvWishList.Release\TvWishListMP2\Skin" /S /C /I /Y
REM XCOPY "MP2 TvWishListMP2\TvWishListMP2 Resources\TvWishListMP2 Resources\Language" "..\TvWishList.Release\TvWishListMP2\Language" /S /C /I /Y

XCOPY "MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\MP1 language" "..\TvWishList.Release\language\TvWishListMP" /S /C /I /Y



REM ******************** END RELEASE ********************

CD "..\TvWishList.Release"
Install.exe -Install -KillTvServerOnMyOwnRisk

REM either one should work
"C:\Program Files (x86)\Team MediaPortal\MediaPortal TV Server\SetupTv.exe"
"C:\Program Files\Team MediaPortal\MediaPortal TV Server\SetupTv.exe"

