#!/bin/bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
PROJECT_DIR="$(dirname "$SCRIPT_DIR")"
CONFIG="Release"
TFM="net10.0-macos"
ICON_NAME="Pomu"

cd "$PROJECT_DIR"

echo "==> Generating app icon"
ICONSET="$(python3 "$SCRIPT_DIR/generate_icon.py" "$PROJECT_DIR")"
iconutil -c icns "$ICONSET" -o "$PROJECT_DIR/$ICON_NAME.icns"
rm -rf "$ICONSET"

echo "==> Building $CONFIG"
dotnet build Pomu.csproj -c "$CONFIG" >/dev/null

APP_DIR="$(find "$PROJECT_DIR/bin/$CONFIG/$TFM" -maxdepth 2 -name "Pomu.app" -type d | head -n 1)"

if [ -z "$APP_DIR" ] || [ ! -d "$APP_DIR" ]; then
  echo "error: built app not found at $APP_DIR" >&2
  exit 1
fi

echo "==> Injecting icon into bundle"
RES_DIR="$APP_DIR/Contents/Resources"
PLIST="$APP_DIR/Contents/Info.plist"
mkdir -p "$RES_DIR"
cp "$PROJECT_DIR/$ICON_NAME.icns" "$RES_DIR/$ICON_NAME.icns"

/usr/libexec/PlistBuddy -c "Delete :CFBundleIconFile" "$PLIST" 2>/dev/null || true
/usr/libexec/PlistBuddy -c "Add :CFBundleIconFile string $ICON_NAME" "$PLIST"
/usr/libexec/PlistBuddy -c "Delete :CFBundleIconName" "$PLIST" 2>/dev/null || true
/usr/libexec/PlistBuddy -c "Add :CFBundleIconName string $ICON_NAME" "$PLIST"

echo "==> Refreshing icon cache"
touch "$APP_DIR"
codesign --force --deep --sign - "$APP_DIR" 2>/dev/null || true

DEST="$PROJECT_DIR/Pomu.app"
rm -rf "$DEST"
cp -R "$APP_DIR" "$DEST"

echo "==> Done"
echo "App bundle: $DEST"
echo "Run it with: open \"$DEST\""
