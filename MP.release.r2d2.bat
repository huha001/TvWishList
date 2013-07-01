REM *************************************************************************
REM release TvWishListMP
REM *************************************************************************


COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListGitRepository\TvWishList.Source\TvWishListMP1.2\TvWishListMP\bin\Release\TvWishListMP.dll"   "C:\Program Files (x86)\Team MediaPortal\MediaPortal\plugins\Windows\TvWishListMP.dll"


COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListGitRepository\TvWishList.Release\language\TvWishListMP\strings_en.xml"   "C:\ProgramData\Team MediaPortal\MediaPortal\language\TvWishListMP\strings_en.xml"
COPY /Y "H:\User Daten\Visual C# Projects\TvWishList\TvWishListGitRepository\TvWishList.Release\language\TvWishListMP\strings_de.xml"   "C:\ProgramData\Team MediaPortal\MediaPortal\language\TvWishListMP\strings_de.xml"




PAUSE
