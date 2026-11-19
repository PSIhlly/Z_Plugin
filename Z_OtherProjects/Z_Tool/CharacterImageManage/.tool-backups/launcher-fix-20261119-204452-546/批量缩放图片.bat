@echo off
chcp 65001 >nul
setlocal
set "PYTHONUTF8=1"
where python >nul 2>nul
if not errorlevel 1 (
    python "%~dp0resize_images.py" --directory "%~dp0."
    goto finish
)
where py >nul 2>nul
if not errorlevel 1 (
    py -3 "%~dp0resize_images.py" --directory "%~dp0."
    goto finish
)
echo 未找到 Python 3，请先安装 Python 3 和 Pillow。
:finish
echo.
pause
endlocal
