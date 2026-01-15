from __future__ import annotations

import re
from dataclasses import dataclass
from pathlib import Path


@dataclass(frozen=True)
class SplitSpec:
    input_path: str
    output_dir: str
    # Ordered outputs; each item is (filename, heading_regex_or_None)
    # - heading_regex_or_None means "preamble before first split heading"
    outputs: list[tuple[str, str | None]]
    split_heading_regex: str


def _repo_root() -> Path:
    return Path(__file__).resolve().parents[1].parent


def _read_md(path: Path) -> str:
    return path.read_text(encoding="utf-8-sig", errors="strict")


def _write_md(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.rstrip() + "\r\n", encoding="utf-8-sig", newline="\r\n")


def _split_by_top_headings(text: str, heading_re: re.Pattern[str]) -> list[tuple[str, str]]:
    matches = list(heading_re.finditer(text))
    if not matches:
        return [("preamble", text)]

    parts: list[tuple[str, str]] = []
    preamble = text[: matches[0].start()]
    parts.append(("preamble", preamble))

    for idx, m in enumerate(matches):
        start = m.start()
        end = matches[idx + 1].start() if idx + 1 < len(matches) else len(text)
        title = m.group(0).strip()
        parts.append((title, text[start:end]))
    return parts


def _pick_section(sections: list[tuple[str, str]], heading_regex: str) -> str:
    rx = re.compile(heading_regex, re.MULTILINE)
    for title, content in sections:
        if rx.search(title):
            return content
    raise RuntimeError(f"section not found: {heading_regex}")


def run() -> int:
    root = _repo_root()
    specs: list[SplitSpec] = [
        SplitSpec(
            input_path=".codex/ONBOARDING.md",
            output_dir=".codex/docs/onboarding",
            split_heading_regex=r"^##\s+\d+\)\s+.*$",
            outputs=[
                ("00_preamble.md", None),
                ("01_repo-structure.md", r"^##\s+1\)\s+"),
                ("02_env-setup.md", r"^##\s+2\)\s+"),
                ("03_database-setup.md", r"^##\s+3\)\s+"),
                ("04_first-run.md", r"^##\s+4\)\s+"),
                ("05_daily-workflows.md", r"^##\s+5\)\s+"),
                ("06_core-code-tour.md", r"^##\s+6\)\s+"),
                ("07_debugging-and-logs.md", r"^##\s+7\)\s+"),
                ("08_troubleshooting.md", r"^##\s+8\)\s+"),
                ("09_exercises.md", r"^##\s+9\)\s+"),
                ("10_further-reading.md", r"^##\s+10\)\s+"),
            ],
        ),
        SplitSpec(
            input_path=".codex/PROJECT_GUIDELINES.md",
            output_dir=".codex/docs/project-guidelines",
            split_heading_regex=r"^##\s+\d+\)\s+.*$",
            outputs=[
                ("00_preamble.md", None),
                ("01_overview.md", r"^##\s+1\)\s+"),
                ("02_dependencies.md", r"^##\s+2\)\s+"),
                ("03_structure.md", r"^##\s+3\)\s+"),
                ("04_common-actions.md", r"^##\s+4\)\s+"),
                ("05_build-run.md", r"^##\s+5\)\s+"),
                ("06_style.md", r"^##\s+6\)\s+"),
                ("07_tests.md", r"^##\s+7\)\s+"),
                ("08_pr-process.md", r"^##\s+8\)\s+"),
                ("09_security-config.md", r"^##\s+9\)\s+"),
                ("10_troubleshooting.md", r"^##\s+10\)\s+"),
                ("11_quick-reference.md", r"^##\s+11\)\s+"),
                ("12_unity-mcp.md", r"^##\s+12\)\s+"),
            ],
        ),
    ]

    for spec in specs:
        input_path = root / spec.input_path
        text = _read_md(input_path)

        if spec.split_heading_regex == r"^$^":
            out_path = root / spec.output_dir / spec.outputs[0][0]
            _write_md(out_path, text)
            continue

        heading_re = re.compile(spec.split_heading_regex, re.MULTILINE)
        sections = _split_by_top_headings(text, heading_re)

        preamble = sections[0][1]
        for filename, heading_rx in spec.outputs:
            if heading_rx is None:
                content = preamble
            else:
                content = _pick_section(sections[1:], heading_rx)
            _write_md(root / spec.output_dir / filename, content)

    print("[OK] split into .codex/docs/**")
    print("Next: python .codex/tools/build_codex_docs.py")
    return 0


if __name__ == "__main__":
    raise SystemExit(run())
