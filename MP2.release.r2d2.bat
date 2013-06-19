REM *************************************************************************
REM release TvWishListMP2
REM *************************************************************************

Set Version=1.3.0.9

XCOPY "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Source\MP2 TvWishListMP2\Bin\TvWishListMP2\bin\Release\Plugins\TvWishListMP2" "C:\Program Files (x86)\Team MediaPortal\MP2-Client\Plugins\TvWishListMP2"  /S /C /I /Y
REM XCOPY "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Source\MP2 TvWishListMP2\TvWishListMP2 Resources\TvWishListMP2 Resources\Skin" "C:\Program Files (x86)\Team MediaPortal\MP2-Client\Plugins\TvWishListMP2\Skin" /S /C /I /Y
REM XCOPY "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Source\MP2 TvWishListMP2\TvWishListMP2 Resources\TvWishListMP2 Resources\Language" "C:\Program Files (x86)\Team MediaPortal\MP2-Client\Plugins\TvWishListMP2\Language" /S /C /I /Y


PAUSE
