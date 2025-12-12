@echo off
setlocal

json-excel\json-excel json Tables\ Data\

set CLIENT_DATA=..\Client\Data
set SERVER_DATA=..\Server\GameServer\GameServer\bin\Debug\Data

if not exist "%CLIENT_DATA%" mkdir "%CLIENT_DATA%"

@copy Data\CharacterDefine.txt "%CLIENT_DATA%\"
@copy Data\MapDefine.txt "%CLIENT_DATA%\"
@copy Data\LevelUpDefine.txt "%CLIENT_DATA%\"
@copy Data\SpawnRuleDefine.txt "%CLIENT_DATA%\"

rem 同步所有生成的数据到服务器 Debug\Data（先清理旧数据）
if exist "%SERVER_DATA%" rmdir /S /Q "%SERVER_DATA%"
mkdir "%SERVER_DATA%"
xcopy /Y /E /I /Q Data\* "%SERVER_DATA%\"

pause
