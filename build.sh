#!/usr/bin/env bash
#
# Build the Tower of Hanoi game targeting .NET Framework 4.8 using Mono's C#
# compiler (mcs). This is the reliable build path on macOS/Linux where the full
# .NET Framework targeting pack for MSBuild is not available.
#
# Usage:
#   ./build.sh            # build Release into src/TowerOfHanoi/bin/Release
#   ./build.sh Debug      # build Debug
#
set -euo pipefail

CONFIG="${1:-Release}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SRC_DIR="$ROOT_DIR/src/TowerOfHanoi"
OUT_DIR="$SRC_DIR/bin/$CONFIG"
OUT_EXE="$OUT_DIR/TowerOfHanoi.exe"

if ! command -v mcs >/dev/null 2>&1; then
  echo "error: 'mcs' (Mono C# compiler) not found. Install Mono first (e.g. 'brew install mono')." >&2
  exit 1
fi

mkdir -p "$OUT_DIR"

# Collect all source files.
SOURCES=$(find "$SRC_DIR" -name '*.cs' -not -path '*/obj/*' -not -path '*/bin/*')

MCS_FLAGS=(-sdk:4.8 -target:exe -out:"$OUT_EXE" -langversion:7.2)
if [ "$CONFIG" = "Debug" ]; then
  MCS_FLAGS+=(-debug -define:DEBUG -define:TRACE)
else
  MCS_FLAGS+=(-optimize+ -define:TRACE)
fi

echo "Building TowerOfHanoi ($CONFIG) with mcs..."
# shellcheck disable=SC2086
mcs "${MCS_FLAGS[@]}" $SOURCES
echo "Build succeeded: $OUT_EXE"
echo "Run with: mono \"$OUT_EXE\""
