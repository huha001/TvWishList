using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
#if ( MP12 )
using MediaPortal.Common.Utils;
#endif

// Allgemeine Informationen über eine Assembly werden über die folgenden 
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die mit einer Assembly verknüpft sind.
[assembly: AssemblyTitle("TvWishListMP")]
[assembly: AssemblyDescription("MediaPortal plugin to input data for TvWishList")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("huha plugins")]
[assembly: AssemblyProduct("TvWishList")]
[assembly: AssemblyCopyright("Copyright © 2011")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf "false" werden die Typen in dieser Assembly unsichtbar 
// für COM-Komponenten. Wenn Sie auf einen Typ in dieser Assembly von 
// COM zugreifen müssen, legen Sie das ComVisible-Attribut für diesen Typ auf "true" fest.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("994b6a74-7d1c-4fc0-a312-0dade7671224")]

// Versionsinformationen für eine Assembly bestehen aus den folgenden vier Werten:
//
//      Hauptversion
//      Nebenversion 
//      Buildnummer
//      Revision
//
// Sie können alle Werte angeben oder die standardmäßigen Build- und Revisionsnummern 
// übernehmen, indem Sie "*" eingeben:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.2.0.11")]
[assembly: AssemblyFileVersion("1.2.0.11")]

#if ( MP12 )
[assembly: CompatibleVersion("1.1.6.27652")]
[assembly: UsesSubsystem("MP.SkinEngine")]
[assembly: UsesSubsystem("MP.Config")]
#endif