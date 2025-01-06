#!/bin/bash

set -e

GITHUB_REPO="drzbida/mtlq"
INSTALL_DIR="${HOME}/.local/bin"

echo "Checking latest mtlq release..."
LATEST_VERSION=$(curl -s "https://api.github.com/repos/${GITHUB_REPO}/releases/latest" | grep -Po '"tag_name": *"v\K[^"]*')
echo "Found version ${LATEST_VERSION}"

DOWNLOAD_URL="https://github.com/${GITHUB_REPO}/releases/download/v${LATEST_VERSION}/mtlq_${LATEST_VERSION}_linux-x64.tar.gz"
echo "Release URL: ${DOWNLOAD_URL}"

TMP_DIR=$(mktemp -d)
cd "$TMP_DIR"

echo "Downloading mtlq v${LATEST_VERSION} for Linux x64..."
curl -L -o mtlq.tar.gz "${DOWNLOAD_URL}"
echo "Download complete, extracting..."
tar xf mtlq.tar.gz

mkdir -p "$INSTALL_DIR"
mv mtlq "$INSTALL_DIR/"
chmod +x "$INSTALL_DIR/mtlq"
echo "Installed mtlq to: $INSTALL_DIR"

rm -rf "$TMP_DIR"

echo ""
echo "Installation complete!"
echo ""
if [[ ":$PATH:" != *":$INSTALL_DIR:"* ]]; then
    echo "Note: $INSTALL_DIR is not in your PATH."
    echo "To add it, you can run:"
    echo "    export PATH=\"\$HOME/.local/bin:\$PATH\""
    echo "Add this line to your shell's rc file to make it permanent."
fi