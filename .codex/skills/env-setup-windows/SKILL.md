---
name: env-setup-windows
description: "Windows 环境准备：依赖安装、SDK/Targeting Pack、常见缺失修复"
metadata:
  short-description: "Windows 环境准备"
---

# Skill: env-setup-windows（Windows 环境准备）

## 前置条件
- Windows 开发环境
- 有权限安装 Unity / VS / SQL Server / Python

## 何时使用
- 新人机器环境搭建 / 首次打开 Unity / VS 构建失败 / 缺少 Targeting Pack

## 快速检查清单
- Unity：`6000.0.53f1+`
- VS：安装 “.NET 桌面开发” + `.NET Framework 4.6.2 Targeting Pack`
- SQL Server：Express/Developer/LocalDB
- Python：`3.10+`（用于 `Src/Data/excel2json.py`）

## 常见错误与处理
- Unity 版本不匹配：用 Unity Hub 安装指定版本再打开项目
- 编译提示缺少 `.NETFramework,Version=v4.6.2`：补装 Targeting Pack

## 进一步阅读（长文档）
- `.codex/docs/onboarding/02_env-setup.md`
- `.codex/docs/project-guidelines/02_dependencies.md`



