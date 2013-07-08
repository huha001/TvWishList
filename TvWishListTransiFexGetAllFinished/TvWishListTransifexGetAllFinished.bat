REM ************************************************************************************************
REM Get language Files for TvWishList from Transifex for MP1 Client, MP2 Client and TvServer
REM ************************************************************************************************


REM Cache Files from Transifex

"bin\tx.exe" pull -a --minimum-perc=100

REM PAUSE


REM Convert into MP1 language
"bin\MP2LanguageConverter.exe" "bin\MP1LanguageConversion.txt" PROCESS

REM PAUSE

REM Convert into MP2 language
"bin\MP2LanguageConverter.exe" "bin\MP2LanguageConversion.txt" PROCESS

REM PAUSE

XCOPY  "Language MP1\*.xml"  "..\TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\MP1 language" /S /C /I /Y
REM PAUSE
XCOPY  "Language MP2\*.xml"  "..\TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\Language" /S /C /I /Y

 PAUSE






