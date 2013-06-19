REM *************************************************************************
REM release TvWishListMP
REM *************************************************************************

Set Version=1.3.0.9

COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Source\TvWishListMP1.2\TvWishListMP\bin\Release\TvWishListMP.dll"   "C:\Program Files (x86)\Team MediaPortal\MediaPortal\plugins\Windows\TvWishListMP.dll"


COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Release\language\TvWishListMP\strings_en.xml"   "C:\ProgramData\Team MediaPortal\MediaPortal\language\TvWishListMP\strings_en.xml"
COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListV%Version%\TvWishListV%Version%.Release\language\TvWishListMP\strings_de.xml"   "C:\ProgramData\Team MediaPortal\MediaPortal\language\TvWishListMP\strings_de.xml"




PAUSE
