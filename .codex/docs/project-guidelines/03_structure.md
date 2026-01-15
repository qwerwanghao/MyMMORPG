## 3) 项目结构

```
MyMMORPG/
├── Src/
│   ├── Client/                    # Unity 客户端
│   │   ├── Assets/
│   │   │   ├── Game/Scripts/      # 玩法/网络脚本
│   │   │   ├── Levels/            # 场景文件
│   │   │   ├── Resources/         # 资源（含 log4net.xml）
│   │   │   └── ThirdParty/        # 第三方库（含 Common.dll）
│   │   ├── Data/                  # 策划数据 (*.txt)
│   │   └── ProjectSettings/       # Unity 项目配置
│   │
│   ├── Server/GameServer/         # .NET 服务器
│   │   └── GameServer/
│   │       ├── Network/           # 网络层
│   │       ├── Services/          # 业务服务
│   │       ├── Entities/          # 实体定义
│   │       └── App.config         # 服务器配置
│   │
│   ├── Lib/                       # 共享库
│   │   ├── proto/                 # Protobuf 定义
│   │   ├── Protocol/              # 生成的协议代码
│   │   └── Common/                # 日志/网络/单例
│   │
│   └── Data/                      # 策划数据源
│       ├── Data/*.txt             # JSON 格式数据
│       └── Excel2Json.cmd         # Excel 转换工具
│
├── Tools/
│   ├── genproto.cmd               # 协议生成脚本
│   └── protoc-3.2.0-win32/        # protoc 编译器
│
└── .codex/
    └── PROJECT_GUIDELINES.md      # 本文档
```

---






