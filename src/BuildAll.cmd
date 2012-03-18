C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\releases\Latest\NET40\ "Monitored Undo Framework.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\SL4\ "SL4\MonitoredUndoSL4\MonitoredUndoSL4.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\WP7\ "WP7\MonitoredUndoWP7\MonitoredUndoWP7.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\NET35\ "Net35\MonitoredUndo35\MonitoredUndo35.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" /testcontainer:"..\releases\Latest\NET40\MonitoredUndoTests.dll" /noresults
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Common7\IDE\MSTest.exe" /testcontainer:"..\releases\Latest\NET35\MonitoredUndoTests35.dll" /noresults
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

REM "==============="
REM "==============="
REM "    PASSED     "
REM "==============="
REM "==============="

if not exist "..\nuget" mkdir "..\nuget"
if not exist "..\nuget\content" mkdir "..\nuget\content"
if not exist "..\nuget\lib" mkdir "..\nuget\lib"
if not exist "..\nuget\lib\net35" mkdir "..\nuget\lib\net35"
if not exist "..\nuget\lib\net40" mkdir "..\nuget\lib\net40"
if not exist "..\nuget\lib\sl3-wp" mkdir "..\nuget\lib\sl3-wp"
if not exist "..\nuget\lib\sl40" mkdir "..\nuget\lib\sl40"

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

copy /y "..\releases\Latest\SL4\MonitoredUndoSL4.dll" "..\nuget\lib\sl40\MonitoredUndoSL4.dll"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\SL4\MonitoredUndoSL4.pdb" "..\nuget\lib\sl40\MonitoredUndoSL4.pdb"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\WP7\MonitoredUndoWP7.dll" "..\nuget\lib\sl3-wp\MonitoredUndoWP7.dll"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

copy /y "..\releases\Latest\WP7\MonitoredUndoWP7.pdb" "..\nuget\lib\sl3-wp\MonitoredUndoWP7.pdb"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

..\tools\nuget.exe pack ..\nuget\MUF.1.0.nuspec -OutputDirectory ..\nuget\ -Version 1.2
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

