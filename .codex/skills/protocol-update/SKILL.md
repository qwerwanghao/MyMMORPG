# Skill: protocol-update（协议更新/生成）

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

## 常见坑
- 只改 proto 不生成 `message.cs`：客户端/服务器会出现解析失败/字段缺失
- 双端 DLL/代码未同步：先确认生成物与引用的版本





