@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul
set "PYTHONUTF8=1"
set "PYTHON_EXE="
set "PYTHON_ARGS="

where python >nul 2>&1
if not errorlevel 1 (
    python -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 set "PYTHON_EXE=python"
)
if defined PYTHON_EXE goto run

where py >nul 2>&1
if not errorlevel 1 (
    py -3 -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 (
        set "PYTHON_EXE=py"
        set "PYTHON_ARGS=-3"
    )
)
if defined PYTHON_EXE goto run

set "BUNDLED_PYTHON=%USERPROFILE%\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe"
if exist "%BUNDLED_PYTHON%" (
    "%BUNDLED_PYTHON%" -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 set "PYTHON_EXE=%BUNDLED_PYTHON%"
)
if not defined PYTHON_EXE (
    echo No Python with Pillow was found.
    echo Install Python and run: python -m pip install Pillow
    pause
    exit /b 1
)

:run
"%PYTHON_EXE%" %PYTHON_ARGS% "%~dp0split_png.py" %*
set "SPLIT_EXIT=%ERRORLEVEL%"
echo.
pause
exit /b %SPLIT_EXIT%
