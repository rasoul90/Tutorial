#!/usr/bin/env bash
set -euo pipefail

if ! command -v git >/dev/null 2>&1; then
  echo "This script requires git to be installed." >&2
  exit 1
fi

ROOT_DIR="$(git rev-parse --show-toplevel)"
DIST_DIR="$ROOT_DIR/dist"
mkdir -p "$DIST_DIR"

VERSION_SUFFIX="${1:-$(date +%Y%m%d-%H%M%S)}"
ARCHIVE_NAME="hrgov-project-${VERSION_SUFFIX}.zip"
OUTPUT_PATH="$DIST_DIR/$ARCHIVE_NAME"

git -C "$ROOT_DIR" archive --format=zip HEAD -o "$OUTPUT_PATH"

echo "\nتم إنشاء الأرشيف بنجاح: $OUTPUT_PATH"
echo "يمكنك مشاركة الملف الناتج أو تنزيله عبر أي قناة مناسبة."
