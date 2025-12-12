#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
excel2json 转表脚本（Python 方案）。

封装 `Src/Data/json-excel/json-excel.exe`：
1) 将 `Src/Data/Tables/*.xlsx` 转为 `Src/Data/Data/*.txt`；
2) 复制部分结果到客户端 `Src/Client/Data`；
3) 全量同步到服务器 `Src/Server/GameServer/GameServer/bin/Debug/Data`（先清理旧目录）。

默认行为与 `Src/Data/Excel2Json.cmd` 保持一致。
"""

import argparse
import shutil
import subprocess
from pathlib import Path


def _auto_detect_root() -> Path:
    """Locate repo root by walking up until the json-excel tool is found."""
    current = Path(__file__).resolve()
    for parent in current.parents:
        if (parent / "Src" / "Data" / "json-excel" / "json-excel.exe").exists():
            return parent
    raise SystemExit("Unable to locate repository root containing Src/Data/json-excel/json-excel.exe")


def main():
    parser = argparse.ArgumentParser(
        description="Run the Excel2Json pipeline (wrapper around json-excel.exe)."
    )
    parser.add_argument(
        "--root",
        type=Path,
        default=None,
        help="Repository root; auto-detected if omitted.",
    )
    parser.add_argument(
        "--tables",
        type=Path,
        default=None,
        help="Directory containing .xlsx source tables. Defaults to Src/Data/Tables.",
    )
    parser.add_argument(
        "--output",
        type=Path,
        default=None,
        help="Output directory for generated .txt/.json files. Defaults to Src/Data/Data.",
    )
    parser.add_argument(
        "--skip-copy",
        action="store_true",
        help="Do not copy selected files into Src/Client/Data.",
    )
    parser.add_argument(
        "--skip-server-sync",
        action="store_true",
        help="Do not sync generated files into server Debug/Data.",
    )
    args = parser.parse_args()

    repo_root = args.root or _auto_detect_root()
    data_root = repo_root / "Src" / "Data"

    converter = data_root / "json-excel" / "json-excel.exe"
    if not converter.exists():
        raise SystemExit(f"json-excel executable not found at {converter}")

    tables_dir = (args.tables or (data_root / "Tables")).resolve()
    output_dir = (args.output or (data_root / "Data")).resolve()
    output_dir.mkdir(parents=True, exist_ok=True)

    subprocess.run(
        [str(converter), "json", str(tables_dir), str(output_dir)],
        check=True,
    )

    if not args.skip_copy:
        client_data_dir = (repo_root / "Src" / "Client" / "Data").resolve()
        client_data_dir.mkdir(parents=True, exist_ok=True)
        for name in ("CharacterDefine.txt", "MapDefine.txt", "LevelUpDefine.txt", "SpawnRuleDefine.txt"):
            src = output_dir / name
            if src.exists():
                shutil.copy2(src, client_data_dir / name)
                print(f"Copied {src} -> {client_data_dir / name}")

    if not args.skip_server_sync:
        server_data_dir = (
            repo_root
            / "Src"
            / "Server"
            / "GameServer"
            / "GameServer"
            / "bin"
            / "Debug"
            / "Data"
        ).resolve()

        if server_data_dir.exists():
            shutil.rmtree(server_data_dir)
        server_data_dir.mkdir(parents=True, exist_ok=True)

        for src in output_dir.rglob("*"):
            if src.is_dir():
                continue
            rel = src.relative_to(output_dir)
            dst = server_data_dir / rel
            dst.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(src, dst)
        print(f"Synced {output_dir} -> {server_data_dir}")


if __name__ == "__main__":
    main()
