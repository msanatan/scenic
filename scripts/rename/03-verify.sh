#!/usr/bin/env bash
set -euo pipefail

# Verify that no "unibridge" references remain in tracked files.
# Run from repo root AFTER 01 and 02 scripts.

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

echo "=== Phase 3: Verification ==="
echo ""

FAILED=0

# --- Check file/directory names ---
echo "--- Checking file/directory names ---"
NAMED=$(git ls-files | grep -i unibridge || true)
if [[ -n "$NAMED" ]]; then
  echo "  FOUND files with 'unibridge' in name:"
  echo "$NAMED" | sed 's/^/    /'
  FAILED=1
else
  echo "  OK — no files with 'unibridge' in name"
fi

echo ""

# --- Check file contents (tracked files only) ---
echo "--- Checking file contents ---"

# Use git grep to search only tracked files, skipping binaries and this script
CONTENT=$(git grep -il 'unibridge' -- \
  ':!scripts/rename/' \
  ':!*.png' ':!*.jpg' ':!*.asset' ':!*.unity' ':!*.wlt' \
  ':!*.prefab' ':!*.lighting' ':!*.inputactions' \
  ':!packages-lock.json' \
  2>/dev/null || true)

if [[ -n "$CONTENT" ]]; then
  echo "  FOUND 'unibridge' in file contents:"
  echo "$CONTENT" | sed 's/^/    /'
  echo ""
  echo "  Details:"
  git grep -in 'unibridge' -- \
    ':!scripts/rename/' \
    ':!*.png' ':!*.jpg' ':!*.asset' ':!*.unity' ':!*.wlt' \
    ':!*.prefab' ':!*.lighting' ':!*.inputactions' \
    ':!packages-lock.json' \
    2>/dev/null | sed 's/^/    /'
  FAILED=1
else
  echo "  OK — no 'unibridge' in tracked file contents"
fi

echo ""

# --- TypeScript build check ---
echo "--- TypeScript build check ---"
if command -v npm &> /dev/null && [[ -f "package.json" ]]; then
  if npm run build --if-present 2>&1; then
    echo "  OK — TypeScript build succeeded"
  else
    echo "  FAIL — TypeScript build failed"
    FAILED=1
  fi
else
  echo "  SKIP — npm not available or no package.json"
fi

echo ""

# --- Summary ---
if [[ $FAILED -eq 0 ]]; then
  echo "=== All checks passed ==="
else
  echo "=== Some checks failed — review output above ==="
  exit 1
fi
