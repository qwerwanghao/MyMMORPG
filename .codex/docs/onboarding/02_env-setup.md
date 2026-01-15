## 2) 环境准备（Windows）

### 2.1 必需软件
1. **Unity Hub + Unity Editor 6000.0.53f1+**
   - 通过 Unity Hub 安装对应版本。
   - 首次打开 `Src/Client` 可能需要较长导入时间。
2. **Visual Studio 2019+**
   - 勾选工作负载：“.NET 桌面开发”。
   - 确保安装 **.NET Framework 4.6.2 Targeting Pack**。
3. **SQL Server**
   - 推荐：SQL Server Express / Developer + SSMS。
   - 也可用 LocalDB（见 3.2）。
4. **Python 3.10+**
   - 用于默认转表 `Src/Data/excel2json.py`。
5. （可选）`protoc` 已内置，无需额外安装。

### 2.2 仓库准备
```bash
cd F:\Git
git clone <https://github.com/qwerwanghao/MyMMORPG> MyMMORPG
cd MyMMORPG
git status --short
```
若 `git status` 有大量脏文件，先问负责人是否需要清理或切到干净分支。

---






