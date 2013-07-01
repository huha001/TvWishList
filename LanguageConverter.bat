
SET ORIGIN=%cd%
CD ..\..\Transifex\TvWishListTransiFexGetAllFinished
SET TRANSIFEX=%cd%
REM PAUSE

CALL TvWishListTransifexGetAllFinished.bat
REM PAUSE


XCOPY  "%TRANSIFEX%\Language MP1\*.xml"  "%ORIGIN%\TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\MP1 language" /S /C /I /Y
REM PAUSE
XCOPY  "%TRANSIFEX%\Language MP2\*.xml"  "%ORIGIN%\TvWishList.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\Language" /S /C /I /Y

REM PAUSE
EXIT

