C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\releases\Latest\NET40\ "Monitored Undo Framework.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\SL4\ "SL4\MonitoredUndoSL4\MonitoredUndoSL4.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\WP7\ "WP7\MonitoredUndoWP7\MonitoredUndoWP7.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

C:\Windows\Microsoft.NET\Framework\v4.0.30319\msbuild.exe /target:Rebuild /property:Configuration=Release;OutDir=..\..\..\..\releases\Latest\NET35\ "Net35\MonitoredUndo35\MonitoredUndo35.sln"
IF %ERRORLEVEL% NEQ 0 GOTO ERROR

xcopy 


GOTO DONE

:ERROR
echo "BUILD FAILED"

:DONE
PAUSE
