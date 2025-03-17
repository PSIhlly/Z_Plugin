

set "current_dir=%~dp0"
set "root_dir=%~dp0
set "found=0"

:: 往上查找，直到找到 assets 文件夹
:search_up
if "%current_dir%"=="" goto not_found
if exist "%current_dir%Assets" (
    set "found=1"
    goto end
)
:: 更改当前路径，向上一级文件夹移动
set "current_dir=%current_dir:~0,-1%"
goto search_up


:not_found
cls
echo 没有找到Z_OtherProjects文件夹
pause

:end
cls
call "%current_dir%Assets\GameSample\Forms\run.bat"
call "%current_dir%Assets\Z_Level2\Z_UnitSystem\Core\Form\run.bat"
call "%current_dir%Assets\Z_Level2\Z_DataSystem\Core\Form\run.bat"
call "%current_dir%Assets\Z_Level3\Z_Fight\Core\Form\run.bat"
call "%current_dir%Assets\Z_Level3\Z_Map\Core\Form\run.bat"
call "%current_dir%Assets\Z_Level3\Z_Text\Core\Form\run.bat"

pause