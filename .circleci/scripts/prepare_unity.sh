#!/usr/bin/env bash

set -e
set -x

mkdir -p /root/.cache/unity3d
mkdir -p /root/.local/share/unity3d/Unity/

set +x

echo "Extracting Unity license file..."
UNITY_LICENSE_CONTENT="${!UNITY_LICENSE_CONTENT_VAR}"
if [ -z "$UNITY_LICENSE_CONTENT" ]; then
    echo "The $UNITY_LICENSE_CONTENT_VAR environment variable is not set. Code is not trusted. Aborting."
    exit 1
fi
echo "$UNITY_LICENSE_CONTENT" | base64 --decode | tr -d '\r' > /root/.local/share/unity3d/Unity/Unity_lic.ulf

echo "Building Unity project..."
PROJECT_ROOT=$(pwd)
PACKAGE_DEST="$PROJECT_ROOT/Packages/UnityMeshSimplifier"
mkdir -p "$PACKAGE_DEST"
mv Editor "$PACKAGE_DEST/"
mv Runtime "$PACKAGE_DEST/"
mv Tests "$PACKAGE_DEST/"
mv package.json "$PACKAGE_DEST/"

mkdir "$PROJECT_ROOT/Assets"
mkdir "$PROJECT_ROOT/Library"
mkdir "$PROJECT_ROOT/ProjectSettings"
cp -Trv "$PROJECT_ROOT/.circleci/ProjectSettings/" "$PROJECT_ROOT/ProjectSettings/"
