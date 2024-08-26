@echo off
set "source=Z_AndroidOs-debug.aar"
set "temp_folder=Z_AndroidOs-debug"
set "java_source=classes.jar"
set "java_temp_folder=classes"
set "root=%~dp0"
rem 1. 将 .aar 后缀名改为 .zip
ren "%source%" "%source:.aar=.zip%"
set "source=Z_AndroidOs-debug.zip"


rem 2. 解压缩 .zip 文件
mkdir %temp_folder%
cmd /c "bz.exe x -o:%temp_folder% %source%"



rem 3. 删除解压后的 libs 文件夹
rmdir /s /q "%temp_folder%\libs"



rem 1. 将 .jar 后缀名改为 .zip
ren "%temp_folder%\%java_source%" "%java_source:.jar=.zip%"
set "java_source=classes.zip"

rem 2. 解压缩 .zip 文件
mkdir "%temp_folder%\%java_source%"
cmd /c "bz.exe x -o:%temp_folder%\%java_temp_folder% %temp_folder%\%java_source%"

rem 3. 删除解压后的 com.unity3d 文件夹
rmdir /s /q "%temp_folder%\%java_temp_folder%\com\unity3d"

rem 生成jar
del /Q /S "%temp_folder%\%java_source%"
cmd /c "bz.exe c -storeroot:no %temp_folder%\%java_source% %temp_folder%\%java_temp_folder%"

mdir /s /q "%temp_folder%\%java_temp_folder%"
ren "%temp_folder%\%java_source%" "%java_source:.zip=.jar%"





rem 生成aar
del /Q /S "%source%"
cmd /c "bz.exe c -storeroot:no %source% %temp_folder%
rmdir /s /q "%temp_folder%"
ren "%source%" "%source:.zip=.aar%"







echo '%source%'!
pause