@echo off

rem 
set "ROOT_DIR=%cd%"

echo Running all .cmd files in subfolders...

rem 
for /R %%F in (*.cmd) do (
    echo Executing %%F...
    pushd "%%~dpF"
    call "%%~nxF"
    popd
)

echo All .cmd files have been executed!

rem 
cd /d "%ROOT_DIR%"
