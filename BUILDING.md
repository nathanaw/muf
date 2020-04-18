# Build / Run / Release


## Build Library and Tests

Note: The solution includes projects that only run properly on windows machines.

1. Clone repo
2. Open bash shell in root directory of git repo
2. `dotnet build tests/MonitoredUndo.Tests/MonitoredUndo.Tests.csproj`
3. `dotnet test tests/MonitoredUndo.Tests/MonitoredUndo.Tests.csproj`

## Release

The solution and projects use the "Directory.Build.props" feature to keep solution-wide
values in the root of the solution.

To create a Pre-Release nuget package:

1. Build the library and run tests as above.
2. `dotnet pack src/MonitoredUndo/MonitoredUndo.csproj --version-suffix "alpha.1" --include-source --include-symbols`
3. Upload the package from `src/MonitoredUndo/Debug/` to nuget.


To create a Release nuget package:

1. Build the library and run tests as above.
2. `dotnet pack src/MonitoredUndo/MonitoredUndo.csproj --configuration Release --include-source --include-symbols`
3. Upload the package from `src/MonitoredUndo/Release/` to nuget.

