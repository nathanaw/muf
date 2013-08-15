REM Bring dev tools into the PATH.
call "C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools\VsDevCmd.bat"

mkdir .\report

REM Restore packages
msbuild .\.nuget\NuGet.targets /target:RestorePackages

REM Ensure build is up to date
msbuild "..\src\Monitored Undo Framework.sln"^
 /target:Rebuild^
 /property:Configuration=Release;OutDir=..\..\releases\Latest\NET40\

REM Run unit tests through OpenCover
REM This allows OpenCover to gather code coverage results
.\packages\OpenCover.4.5.1604\OpenCover.Console.exe^
 -register:user^
 -target:MSTest.exe^
 -targetargs:"/noresults /noisolation /testcontainer:..\releases\Latest\NET40\MonitoredUndoTests.dll"^
 -filter:+[*]*^
 -output:.\report\output.xml

REM Generate the report
.\packages\ReportGenerator.1.9.0.0\ReportGenerator.exe^
 -reports:.\report\output.xml^
 -targetdir:.\report^
 -reporttypes:Html,HtmlSummary^
 -filters:-MonitoredUndoTests*
 
REM Open the report
start .\report\index.htm

pause
