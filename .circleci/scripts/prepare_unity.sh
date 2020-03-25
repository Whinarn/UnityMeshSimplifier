#!/usr/bin/env bash

set -e
set -x

mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/

set +x

echo "Extracting Unity license file..."
UNITY_LICENSE_CONTENT="${!UNITY_LICENSE_CONTENT_VAR}"
echo "$UNITY_LICENSE_CONTENT" | base64 --decode | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf

echo "Building Unity project..."
PROJECT_ROOT=$(pwd)
PACKAGE_DEST="$PROJECT_ROOT/Packages/UnityMeshSimplifier"
mkdir -p "$PACKAGE_DEST"
mv Editor "$PACKAGE_DEST/Editor"
mv Runtime "$PACKAGE_DEST/Runtime"

mkdir "$PROJECT_ROOT/Assets"
mkdir "$PROJECT_ROOT/Library"
mkdir "$PROJECT_ROOT/ProjectSettings"
