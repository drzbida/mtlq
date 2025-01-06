#!/bin/bash

set -e

INSTALL_DIR="${HOME}/.local/bin"
BINARY_PATH="$INSTALL_DIR/mtlq"

if [[ -f "$BINARY_PATH" ]]; then
    echo "Removing mtlq from $INSTALL_DIR"
    rm "$BINARY_PATH"
    echo "Removed mtlq binary"
    echo ""
    echo "Uninstallation complete!"
else
    echo "mtlq is not installed in $INSTALL_DIR"
    exit 1
fi