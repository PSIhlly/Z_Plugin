@echo off
setlocal EnableExtensions
set "PYTHON_EXE="
set "PYTHON_ARGS="

where python >nul 2>&1
if not errorlevel 1 (
    python -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 set "PYTHON_EXE=python"
)
if defined PYTHON_EXE goto :run

where py >nul 2>&1
if not errorlevel 1 (
    py -3 -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 (
        set "PYTHON_EXE=py"
        set "PYTHON_ARGS=-3"
    )
)
if defined PYTHON_EXE goto :run

set "BUNDLED_PYTHON=%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"
if exist "%BUNDLED_PYTHON%" (
    "%BUNDLED_PYTHON%" -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 set "PYTHON_EXE=%BUNDLED_PYTHON%"
)
if not defined PYTHON_EXE (
    echo No Python with Pillow was found.
    echo Install Pillow with: python -m pip install Pillow
    pause
    exit /b 1
)

:run
echo Processing 0.png...
"%PYTHON_EXE%" %PYTHON_ARGS% "%~dp0texture_tool.py" split
if errorlevel 1 (
    echo Split failed. See the error above.
    pause
    exit /b 1
)
echo Split complete: 1.png through 29.png and 50.png through 51.png.
pause
exit /b 0
