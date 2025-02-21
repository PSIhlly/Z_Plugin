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

python %current_dir%Z_OtherProjects\Z_Tool\Excel2Cs\Excel2Cs.py %root_dir%  %root_dir% namespace:Z_Text
pause