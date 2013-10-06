C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\releases\Latest\NET40\ "Monitored Undo Framework.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\NET35\ "Net35\MonitoredUndo35\MonitoredUndo35.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\PORTABLE\ "Portable\MonitoredUndoPortable\MonitoredUndoPortable.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" /testcontainer:"..\releases\Latest\NET40\MonitoredUndoTests.dll" /noresults
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" /testcontainer:"..\releases\Latest\NET35\MonitoredUndoTests35.dll" /noresults
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" /testcontainer:"..\releases\Latest\PORTABLE\MonitoredUndoTestsPortable.dll" /noresults
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

REM "==============="
REM "==============="
REM "    PASSED     "
REM "==============="
REM "==============="

if exist "..\nuget\content" rmdir /s "..\nuget\content"
if exist "..\nuget\lib" rmdir /s "..\nuget\lib"
if exist "..\nuget\MUF.1.2.nupkg" del "..\nuget\MUF.1.2.nupkg"

if not exist "..\nuget" mkdir "..\nuget"
if not exist "..\nuget\content" mkdir "..\nuget\content"
if not exist "..\nuget\lib" mkdir "..\nuget\lib"
if not exist "..\nuget\lib\net35" mkdir "..\nuget\lib\net35"
if not exist "..\nuget\lib\net40" mkdir "..\nuget\lib\net40"
if not exist "..\nuget\lib\portable-win+net45+sl40+wp" mkdir "..\nuget\lib\portable-win+net45+sl40+wp"

copy /y "..\docs\Monitored Undo Documentation.docx" "..\nuget\content\Monitored Undo Documentation.docx"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\NET40\MonitoredUndo.dll" "..\nuget\lib\net40\MonitoredUndo.dll"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\NET40\MonitoredUndo.pdb" "..\nuget\lib\net40\MonitoredUndo.pdb"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\NET35\MonitoredUndo35.dll" "..\nuget\lib\net35\MonitoredUndo35.dll"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\NET35\MonitoredUndo35.pdb" "..\nuget\lib\net35\MonitoredUndo35.pdb"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\PORTABLE\MonitoredUndoPortable.dll" "..\nuget\lib\portable-win+net45+sl40+wp\MonitoredUndoPortable.dll"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\PORTABLE\MonitoredUndoPortable.pdb" "..\nuget\lib\portable-win+net45+sl40+wp\MonitoredUndoPortable.pdb"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

..\tools\nuget.exe pack ..\nuget\MUF.1.2.nuspec -OutputDirectory ..\nuget\ -Version 1.2.1
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

GOTO DONE

:ERROR
REM "==============="
REM "==============="
REM "    FAILED     "
REM "==============="
REM "==============="

:DONE
PAUSE

