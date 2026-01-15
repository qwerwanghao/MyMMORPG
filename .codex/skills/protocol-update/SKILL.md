# Skill: protocol-update（协议更新/生成）

## 前置条件
- Windows 环境可运行 `Tools/genproto.cmd`
- 你接受“proto 改动必须提交生成物”：`Src/Lib/Protocol/message.cs`

## 何时使用
- 用户说“协议 / proto / message.proto / genproto / message.cs”
- 需要新增字段/消息，或修复客户端与服务器协议不一致

## 标准流程
1. 修改 `Src/Lib/proto/message.proto`
2. 生成代码：
   ```bash
   cd Tools
   genproto.cmd
   ```
3. 提交：
   - `Src/Lib/proto/message.proto`
   - `Src/Lib/Protocol/message.cs`

## 常见错误与处理
- `genproto.cmd` 找不到 `protoc` / 执行失败：确认 `Tools/` 目录结构未变、脚本里引用路径正确
- 运行成功但仍解析失败：确认两端都重新编译并同步引用同一版 `message.cs`/`Protocol.dll`
- 只改 proto 未生成：会导致字段缺失/解析失败（双端版本漂移）



