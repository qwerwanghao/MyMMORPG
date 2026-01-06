# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

MyMMORPG is a dual-end MMORPG sample project: **Unity Client** + **.NET Server**, using Protobuf protocol, TCP long-connection, EF database access, and log4net logging. Suitable for studying network game architecture and secondary development.

## Architecture

### High-Level Structure

```
MyMMORPG/
├── Src/Client/           # Unity 6000.0.53f1 client
├── Src/Server/GameServer/ # .NET Framework 4.6.2 server
├── Src/Lib/              # Shared libraries (Common, Protocol)
├── Src/Data/             # Game data (Excel source + JSON output)
└── Tools/                # Protocol generation and utilities
```

### Key Architecture Points

1. **Dual-end shared libraries**: `Src/Lib/Common` (logging/network/singleton base) and `Src/Lib/Protocol` (Protobuf-generated code) are used by both client and server
2. **Protocol generation flow**: `Src/Lib/proto/message.proto` → `Tools/genproto.cmd` → `Src/Lib/Protocol/message.cs`. When modifying proto, you must commit both files and rebuild both ends
3. **Data pipeline**: `Src/Data/Tables/*.xlsx` → `python Src/Data/excel2json.py` → `Src/Data/Data/*.txt` → synced to `Src/Client/Data` and `Src/Server/GameServer/GameServer/bin/Debug/Data`
4. **Client startup**: `LoadingManager` → `ClientInitPipeline` → initializes logging, data (DataManager reads from `Data/*.txt`), services (UserService via NetClient connects to `127.0.0.1:8000`)
5. **Server startup**: `Program` → `GameServer` → `NetService` (TCP listener) → `UserService` (login/register handlers) → `DBService` (EF context)

### Client Core Components

- **`Assets/Game/Scripts/Core/ClientInitPipeline.cs`**: Main initialization entry point (new)
- **`Assets/Game/Scripts/Core/LoadingManager.cs`**: UI flow orchestration
- **`Assets/Game/Scripts/Core/DataManager.cs`**: Loads game data from JSON files in `Data/` directory
- **`Assets/Game/Scripts/Network/NetClient.cs`**: TCP client, message queue, reliable transport
- **`Assets/Game/Scripts/Services/UserService.cs`**: Authentication service
- **`Assets/Game/Scripts/Services/MapService.cs`**: Map management service
- **`Assets/Game/Scripts/Services/CharacterManager.cs`**: Character management
- **`Assets/Game/Scripts/GameObject/MainPlayerCamera.cs`**: Camera controller
- **`Assets/Game/Scripts/GameObject/PlayerInputController.cs`**: Player input handling

### Server Core Components

- **`Program.cs`**: Server entry point
- **`GameServer.cs`**: Main server loop, assembles Network/DB/Services
- **`Network/NetService.cs`**: TCP listener, creates NetConnection instances
- **`Services/UserSerevice.cs`**: Handles register/login/create character/delete character
- **`Services/DBService.cs`**: EF context holder (`Entities`)

### Network Protocol

- **Transport**: TCP with 4-byte length prefix + Protobuf `NetMessage`
- **Shared**: `PackageHandler` (in Common library) handles packing/unpacking
- **Routing**: `MessageDistributer` on both ends routes messages to subscribers

### Database

- **ORM**: Entity Framework (`.edmx` model)
- **Connection string**: `Src/Server/GameServer/GameServer/App.config`
- **Database name**: `ExtremeWorld`
- **Initialization script**: `Src/Server/GameServer/GameServer/Entities.edmx.sql`

## Common Commands

### Protocol Update

```bash
cd Tools
genproto.cmd
```

After running, commit both `Src/Lib/proto/message.proto` and `Src/Lib/Protocol/message.cs`.

### Data Update (Excel to JSON)

```bash
cd Src\Data
python excel2json.py
```

This converts Excel files in `Tables/` to JSON in `Data/` and syncs to client/server directories.

### Build Server

```bash
# Debug build
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Debug

# Release build
msbuild Src\Server\GameServer\GameServer.sln /p:Configuration=Release
```

### Run Server

```bash
cd Src\Server\GameServer\GameServer\bin\Debug
GameServer.exe
```

Default listen: `127.0.0.1:8000`

### Update Shared Library DLLs

The Common and Protocol projects have post-build events that copy DLLs to `Assets/ThirdParty/`. To manually rebuild:

```bash
msbuild Src/Lib/Common/Common.csproj /p:Configuration=Debug
msbuild Src/Lib/Protocol/Protocol.csproj /p:Configuration=Debug
```

### Unity Client

- Open project: `Src/Client/` in Unity 6000.0.53f1+
- Main scenes: `Assets/Levels/MainCity.unity`, `Assets/Levels/Test.unity`
- Play in Editor for testing

## Initial Setup

### Database (First Time Only)

1. Create SQL Server database named `ExtremeWorld`
2. Run `Src/Server/GameServer/GameServer/Entities.edmx.sql` to initialize tables
3. Modify `Src/Server/GameServer/GameServer/App.config` connection string to point to your SQL Server instance

### Recommended Debug Order

1. Start server first (confirm port/database available)
2. Open Unity project and play scene (e.g., `Assets/Levels/Test.unity`)
3. Verify login round-trip (register → login → create character → delete character)

## Important Patterns

### Singleton Pattern

Extensively used for managers:
- `DataManager.Instance`: Game data
- `UserService.Instance`: User authentication
- `NetClient.Instance`: Network client
- `MapService.Instance`: Map management
- `CharacterManager.Instance`: Character management

### Service Initialization

New initialization pipeline via `ClientInitPipeline.Run()`:
1. Initialize logging (log4net)
2. Ensure Unity singletons (NetClient, SceneManager)
3. Run service initialization (MapService, UserService, CharacterManager)

### Event-Driven Architecture

- Services use Unity events/callbacks
- MessageDistributer routes incoming network messages
- UI components subscribe to service events (not direct network packet handling)

## Key File Locations

| Purpose | Path |
|---------|------|
| Client entry | `Assets/Game/Scripts/Core/ClientInitPipeline.cs` |
| Data manager | `Assets/Game/Scripts/Core/DataManager.cs` |
| Network client | `Assets/Game/Scripts/Network/NetClient.cs` |
| User service | `Assets/Game/Scripts/Services/UserService.cs` |
| Map service | `Assets/Game/Scripts/Services/MapService.cs` |
| Character manager | `Assets/Game/Scripts/Core/CharacterManager.cs` |
| Server entry | `Src/Server/GameServer/GameServer/Program.cs` |
| Server main | `Src/Server/GameServer/GameServer/GameServer.cs` |
| Server network | `Src/Server/GameServer/GameServer/Network/NetService.cs` |
| Protocol source | `Src/Lib/proto/message.proto` |
| Database config | `Src/Server/GameServer/GameServer/App.config` |
| Client data | `Src/Client/Data/*.txt` |
| Server data | `Src/Server/GameServer/GameServer/bin/Debug/Data/*.txt` |

## Logging

| Side | Location |
|------|----------|
| Client | `Src/Client/Log/client.log` |
| Server | `Src/Server/GameServer/GameServer/Log/` |
| Config | `Assets/Resources/log4net.xml` (client), `Src/Server/GameServer/GameServer/log4net.xml` (server) |

**Important**: Use `Common.Log` / `Common.UnityLog` for business logging. In `UnityConsoleAppender.cs`, use `Debug.Log*` internally to prevent log4net recursion.

## Language and Communication

- Use Simplified Chinese for responses and comments (matching existing codebase)
- Complex tasks: Create todo list first, wait for confirmation before executing
- When uncertain: Ask questions rather than assume

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Can't connect to server | Verify server running on port 8000; check firewall |
| Database error | Verify `ExtremeWorld` database exists; check connection string in `App.config` |
| Protocol parse error | Run `genproto.cmd` and rebuild both ends |
| DLL version error | Rebuild Common/Protocol projects |

## Further Reading

Detailed project guidelines, workflows, and onboarding info in `.codex/`:
- `.codex/PROJECT_GUIDELINES.md`: Full project conventions
- `.codex/ONBOARDING.md`: New team member guide
- `.codex/AGENTS.md`: AI agent workflow rules
