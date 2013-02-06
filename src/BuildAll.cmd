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
GOTO DONE

:ERROR
REM "==============="
REM "==============="
REM "    FAILED     "
REM "==============="
REM "==============="

:DONE
PAUSE
