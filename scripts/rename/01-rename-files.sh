#!/usr/bin/env bash
set -euo pipefail

# Rename files and directories that contain "unibridge" in their name.
# Uses git mv so history is preserved. Run from repo root.

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

echo "=== Phase 1: File & directory renames ==="

# --- Unity source files ---
git mv unity/Editor/UnibridgeServer.cs unity/Editor/ScenicServer.cs
git mv unity/Editor/UnibridgeServer.cs.meta unity/Editor/ScenicServer.cs.meta
echo "  Renamed UnibridgeServer.cs → ScenicServer.cs"

# --- Assembly definitions ---
git mv unity/Editor/com.unibridge.plugin.editor.asmdef unity/Editor/com.scenic.plugin.editor.asmdef
git mv unity/Editor/com.unibridge.plugin.editor.asmdef.meta unity/Editor/com.scenic.plugin.editor.asmdef.meta
echo "  Renamed com.unibridge.plugin.editor.asmdef → com.scenic.plugin.editor.asmdef"

git mv unity/Tests/Editor/com.unibridge.plugin.tests.editor.asmdef unity/Tests/Editor/com.scenic.plugin.tests.editor.asmdef
git mv unity/Tests/Editor/com.unibridge.plugin.tests.editor.asmdef.meta unity/Tests/Editor/com.scenic.plugin.tests.editor.asmdef.meta
echo "  Renamed com.unibridge.plugin.tests.editor.asmdef → com.scenic.plugin.tests.editor.asmdef"

# --- Solution file ---
git mv unibridge.sln scenic.sln
echo "  Renamed unibridge.sln → scenic.sln"

# --- Test project directory ---
git mv TestProjects/Unibridge3Dv6.3 TestProjects/Scenic3Dv6.3
echo "  Renamed TestProjects/Unibridge3Dv6.3 → TestProjects/Scenic3Dv6.3"

echo ""
echo "=== Phase 1 complete ==="
echo "Run 'git status' to review staged renames."
