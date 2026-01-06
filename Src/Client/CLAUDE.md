# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Unity-based MMORPG client project with a C# server component. The project uses Unity 6 and follows a client-server architecture with Protobuf for network communication.

## Key Architecture Components

### Client Structure (Unity)
- **Assets/Game/**: Core game logic and scripts
  - **Scripts/Core/**: Core managers (DataManager, LoadingManager, etc.)
  - **Scripts/Services/**: Game services (UserService for authentication)
  - **Scripts/Network/**: Networking layer (NetClient for server communication)
  - **Scripts/UI/**: UI components and controllers
  - **Scripts/Models/**: Data models
  - **Scripts/Utilities/**: Utility classes and helpers (Singleton patterns)
  - **Scripts/Scene/**: Scene management
  - **Scripts/Log/**: Custom logging system

- **Assets/ThirdParty/**: External dependencies
  - **Common/**: Shared library with server
  - **Protocol/**: Protobuf definitions for network messages

### Server Structure
- Located in `../Server/GameServer/`
- C# .NET server implementation
- Entities, Managers, and Models directories

### Data Configuration
- **Data/**: Game configuration files (JSON format)
  - `GameConfig.txt`: Client-side configuration
  - `GameServerConfig.txt`: Server connection settings
  - Various `*Define.txt` files: Game data definitions (maps, characters, teleporters, spawn points)

## Network Architecture

The client uses a custom TCP-based networking layer:
- **NetClient**: Handles connection management, message queuing, and reliable transport
- **MessageDistributer**: Routes incoming messages to appropriate handlers
- **Protobuf**: Serializes network messages for efficient communication

Common development workflow:
1. Client connects to server via NetClient
2. Services (like UserService) handle high-level operations
3. Network messages are defined in Protocol assembly
4. Data is loaded from JSON files in Data directory

## Development Commands

### Unity Editor
- Open the project in Unity 6
- Main scene is likely in `Assets/Levels/`
- Build settings configured for Windows platform

### Testing
- Unity Test Runner is available (Edit Mode and Play Mode tests)
- Tests can be run via Unity Test Runner window

### Protocol Updates
When modifying network messages:
1. Update protocol definitions in Protocol assembly
2. Regenerate Protobuf classes
3. Update both client and server assemblies

## Important Patterns

### Singleton Pattern
The project extensively uses singleton patterns for managers:
- `DataManager.Instance`: Game data management
- `UserService.Instance`: User authentication service
- `NetClient.Instance`: Network client

### Event-Driven Architecture
- Services use Unity events for callbacks
- Message distributer handles incoming network messages
- UI components subscribe to service events

### Configuration Loading
- JSON-based configuration files
- DataManager handles loading and parsing
- Default values provided for missing configurations

## Common File Locations

- Server connection: `Assets/Game/Scripts/Services/UserService.cs`
- Network client: `Assets/Game/Scripts/Network/NetClient.cs`
- Data manager: `Assets/Game/Scripts/Core/DataManager.cs`
- UI Login: `Assets/Game/Scripts/UI/UILogin.cs`
- Character selection: `Assets/Game/Scripts/UI/UICharacterSelect.cs`
- Game config: `Data/GameConfig.txt`
- Server config: `Data/GameServerConfig.txt`

## Dependencies

- Unity 6
- Newtonsoft.Json for JSON serialization
- Custom Common and Protocol assemblies shared with server
- Unity Services (Analytics, Core)

## Development Notes

- The project uses Chinese comments and logging
- Default server connection is 127.0.0.1:8000
- Client includes reconnect logic with cooldown period
- Message queuing system handles network interruptions
- Loading manager handles async data loading operations