from __future__ import annotations

import json
import os
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class OutputSpec:
    path: str
    sources: list[str]
    add_generated_header: bool


def _repo_root() -> Path:
    return Path(__file__).resolve().parents[1].parent


def _read_text(path: Path) -> str:
    # utf-8-sig: tolerate/strip BOM (Windows editors sometimes add it)
    return path.read_text(encoding="utf-8-sig", errors="strict")


def _write_markdown(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    # Always write with BOM to avoid garbled Chinese on some Windows setups.
    path.write_text(content, encoding="utf-8-sig", newline="\r\n")


def _load_manifest(manifest_path: Path) -> list[OutputSpec]:
    raw = json.loads(manifest_path.read_text(encoding="utf-8", errors="strict"))
    outputs: list[OutputSpec] = []
    for item in raw.get("outputs", []):
        outputs.append(
            OutputSpec(
                path=item["path"],
                sources=list(item["sources"]),
                add_generated_header=bool(item.get("add_generated_header", True)),
            )
        )
    return outputs


def _generated_header(output_path: str) -> str:
    return (
        "<!--\n"
        "This file is generated.\n"
        "Source of truth: inputs listed in .codex/tools/manifest.json\n"
        "Regenerate: python .codex/tools/build_codex_docs.py\n"
        f"Output: {output_path}\n"
        "-->\n\n"
    )


def build() -> int:
    root = _repo_root()
    manifest_path = root / ".codex" / "tools" / "manifest.json"
    outputs = _load_manifest(manifest_path)

    missing: list[str] = []
    for spec in outputs:
        out_path = root / spec.path
        parts: list[str] = []
        for src in spec.sources:
            src_path = root / src
            if not src_path.exists():
                missing.append(src)
                continue
            text = _read_text(src_path)
            parts.append(text.strip("\ufeff"))
        if missing:
            continue

        content = "\n\n".join([p.rstrip() for p in parts if p.strip() != ""]).rstrip() + "\n"
        if spec.add_generated_header:
            content = _generated_header(spec.path) + content
        _write_markdown(out_path, content)

    if missing:
        for m in missing:
            print(f"[ERROR] missing source: {m}")
        return 2

    print("[OK] generated:")
    for spec in outputs:
        print(f"  - {spec.path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(build())
