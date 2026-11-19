@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul
set "PYTHONUTF8=1"
where python >nul 2>&1
if not errorlevel 1 (
    python -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 goto python
)
where py >nul 2>&1
if not errorlevel 1 (
    py -3 -c "from PIL import Image" >nul 2>&1
    if not errorlevel 1 goto launcher
)
echo Python 3 with Pillow was not found. Install it with: python -m pip install Pillow
pause
exit /b 1
:python
python "%~dp0align_bottom.py" --directory "%~dp0." %*
goto done
:launcher
py -3 "%~dp0align_bottom.py" --directory "%~dp0." %*
:done
set "RESULT=%ERRORLEVEL%"
echo.
pause
exit /b %RESULT%
