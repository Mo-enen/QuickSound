
dotnet publish --property WarningLevel=0

del /S Publish\*.dll
del /S Publish\*.pdb
@rd /S /Q "Build"

pause