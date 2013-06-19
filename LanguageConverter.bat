
SET ORIGIN=%cd%
SET VERSION=1.3.0.1
CD ..\..\Transifex\TvWishListTransiFexGetAllFinished
SET TRANSIFEX=%cd%
REM PAUSE

CALL TvWishListTransifexGetAllFinished.bat
REM PAUSE


XCOPY  "%TRANSIFEX%\Language MP1\*.xml"  "%ORIGIN%\TvWishListV%VERSION%.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\MP1 language" /S /C /I /Y
REM PAUSE
XCOPY  "%TRANSIFEX%\Language MP2\*.xml"  "%ORIGIN%\TvWishListV%VERSION%.Source\MP2 TvWishListMP2\TvWishListMP2\TvWishListMP2\Language" /S /C /I /Y

REM PAUSE
EXIT

