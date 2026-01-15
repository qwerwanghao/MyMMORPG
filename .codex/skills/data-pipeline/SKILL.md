# Skill: data-pipeline（表格/配表数据链路）

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

## 产物检查清单
- `Src/Data/Data` 有更新的 `*.txt`
- 客户端与服务器同步目录的文件时间一致
- 客户端/服务器启动后能读到新配置（必要时删除旧缓存或重启）





