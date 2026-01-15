# Skill: data-pipeline（表格/配表数据链路）

## 前置条件
- 本地有 Python 3.10+（能运行 `python`）
- 需要转表的源表在：`Src/Data/Tables/*.xlsx`

## 何时使用
- 用户说“转表 / 配表 / excel2json / Tables / Data/Data / 同步客户端服务器”
- 新增/修改策划表后需要更新产物与同步目录

## 关键事实（本项目）
- 源表：`Src/Data/Tables/*.xlsx`
- 产物：`Src/Data/Data/*.txt`（JSON 文本）
- 默认脚本：`Src/Data/excel2json.py`
- 同步目录：
  - 客户端：`Src/Client/Data`
  - 服务器：`Src/Server/GameServer/GameServer/bin/Debug/Data`

## 标准流程
```bash
cd Src/Data
python excel2json.py
```

## 常见错误与处理
- `python` 找不到：安装 Python 并把 `python` 加入 PATH
- 产物更新但客户端不生效：确认 `Src/Client/Data` 同步目录文件时间已更新；必要时重启 Unity
- 服务器仍读旧数据：确认 `bin/Debug/Data` 同步目录已更新；必要时重启服务器



