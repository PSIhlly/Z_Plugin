@echo off
setlocal EnableExtensions DisableDelayedExpansion
chcp 65001 >nul
set "PYTHONUTF8=1"
set "PYTHONIOENCODING=utf-8"
where python >nul 2>&1
if errorlevel 1 goto try_py
python -c "from PIL import Image" >nul 2>&1
if not errorlevel 1 goto run_python
:try_py
where py >nul 2>&1
if errorlevel 1 goto missing_python
py -3 -c "from PIL import Image" >nul 2>&1
if not errorlevel 1 goto run_py
:missing_python
echo Python 3 with Pillow was not found.
echo Install Python 3, then run: python -m pip install Pillow
set "RESULT=1"
goto finish
:run_python
python "%~dp0resize_images.py" --directory "%~dp0." %*
set "RESULT=%ERRORLEVEL%"
goto finish
:run_py
py -3 "%~dp0resize_images.py" --directory "%~dp0." %*
set "RESULT=%ERRORLEVEL%"
:finish
echo.
if not "%RESULT%"=="0" echo Failed. See the error above.
pause
exit /b %RESULT%
