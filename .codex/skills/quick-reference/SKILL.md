# Skill: quick-reference（常用命令速查）

## 前置条件
- 你知道自己要操作的类型：协议 / 配表 / 构建 / 端口排查

## 常用命令
```bash
# 生成协议
cd Tools
genproto.cmd

# 转表
cd Src/Data
python excel2json.py

# 编译服务器（Debug）
msbuild Src\\Server\\GameServer\\GameServer.sln /p:Configuration=Debug

# 查端口占用（8000）
netstat -ano | findstr 8000
```

## 进一步阅读（长文档）
- `.codex/docs/project-guidelines/11_quick-reference.md`



