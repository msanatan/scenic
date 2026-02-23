#!/usr/bin/env bash
set -euo pipefail

# Replace all occurrences of "unibridge" (various casings) in file contents.
# Order matters: most specific patterns first, general catch-alls last.
# Run from repo root AFTER 01-rename-files.sh.

REPO_ROOT="$(git rev-parse --show-toplevel)"
cd "$REPO_ROOT"

echo "=== Phase 2: Content replacements ==="

# File extensions to process (avoids binaries, images, .meta files with GUIDs, etc.)
INCLUDE="--include=*.cs --include=*.ts --include=*.tsx --include=*.js --include=*.json --include=*.md --include=*.yml --include=*.yaml --include=*.sln --include=*.slnx --include=*.asmdef --include=*.txt"
# Directories to skip
EXCLUDE="--exclude-dir=node_modules --exclude-dir=.git --exclude-dir=Library --exclude-dir=Temp --exclude-dir=obj --exclude-dir=dist --exclude-dir=Logs"

# Portable sed in-place: macOS uses -i '' while GNU uses -i
if [[ "$(uname)" == "Darwin" ]]; then
  SED_INPLACE=(sed -i '')
else
  SED_INPLACE=(sed -i)
fi

replace() {
  local pattern="$1"
  local replacement="$2"
  local description="$3"

  # Find files containing the pattern, then replace
  local files
  files=$(grep -rl $INCLUDE $EXCLUDE "$pattern" . 2>/dev/null || true)

  if [[ -z "$files" ]]; then
    echo "  [skip] $description — no matches"
    return
  fi

  local count
  count=$(echo "$files" | wc -l | tr -d ' ')

  echo "$files" | while IFS= read -r file; do
    "${SED_INPLACE[@]}" "s|${pattern}|${replacement}|g" "$file"
  done

  echo "  [done] $description ($count files)"
}

# ─── Most specific patterns first ───

replace '@unibridge/' '@scenicai/' \
  '@unibridge/ → @scenicai/ (npm scope)'

replace 'com\.msanatan\.unibridge' 'com.msanatan.scenic' \
  'com.msanatan.unibridge → com.msanatan.scenic (UPM package ID)'

replace 'msanatan/unibridge' 'msanatan/scenic' \
  'msanatan/unibridge → msanatan/scenic (GitHub repo)'

replace 'com\.unibridge' 'com.scenic' \
  'com.unibridge → com.scenic (asmdef names)'

# ─── C# class/attribute names (before general UniBridge catch-all) ───

replace 'UniBridgeCommandAttribute' 'ScenicCommandAttribute' \
  'UniBridgeCommandAttribute → ScenicCommandAttribute'

replace 'UniBridgeSettings' 'ScenicSettings' \
  'UniBridgeSettings → ScenicSettings'

replace 'UniBridgeServer' 'ScenicServer' \
  'UniBridgeServer → ScenicServer'

# ─── Test data prefixes ───

replace 'UniBridgeLayer_' 'ScenicLayer_' \
  'UniBridgeLayer_ → ScenicLayer_ (test data)'

replace 'UnibridgeTag_' 'ScenicTag_' \
  'UnibridgeTag_ → ScenicTag_ (test data)'

replace 'unibridge-logs-' 'scenic-logs-' \
  'unibridge-logs- → scenic-logs- (test log keys)'

replace 'unibridge-test-' 'scenic-test-' \
  'unibridge-test- → scenic-test- (test temp dirs)'

replace 'unibridge smoke test' 'scenic smoke test' \
  'unibridge smoke test → scenic smoke test'

replace 'unibridge_result_' 'scenic_result_' \
  'unibridge_result_ → scenic_result_ (session keys)'

# ─── General catch-alls (least specific, last) ───

replace 'UniBridge' 'Scenic' \
  'UniBridge → Scenic (general PascalCase)'

replace 'Unibridge' 'Scenic' \
  'Unibridge → Scenic (mixed case)'

replace 'unibridge' 'scenic' \
  'unibridge → scenic (lowercase catch-all)'

echo ""
echo "=== Phase 2 complete ==="
echo "Run 'git diff' to review changes, then 'git add -A' to stage."
