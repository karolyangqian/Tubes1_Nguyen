@echo off

echo Cleaning all bin and obj folders...

rem Delete all "bin" folders
for /d /r %%i in (bin) do (
    echo Deleting folder %%i
    rd /s /q "%%i"
)

rem Delete all "obj" folders
for /d /r %%i in (obj) do (
    echo Deleting folder %%i
    rd /s /q "%%i"
)

echo Cleanup complete!
