#!/usr/bin/env bash

set -x

echo "Testing for $TEST_PLATFORM"

PROJECT_ROOT="$(pwd)"
TEST_RESULT_DIR="$PROJECT_ROOT/test-results/nunit"
TEST_RESULT_FILE_NAME="$TEST_PLATFORM-results.xml"
TEST_RESULT_FILE_PATH="$TEST_RESULT_DIR/$TEST_RESULT_FILE_NAME"
${UNITY_EXECUTABLE:-xvfb-run --auto-servernum --server-args='-screen 0 640x480x24' /opt/Unity/Editor/Unity} \
  -projectPath "$PROJECT_ROOT" \
  -runTests \
  -testPlatform "$TEST_PLATFORM" \
  -testResults "$TEST_RESULT_FILE_PATH" \
  -logFile - \
  -batchmode

UNITY_EXIT_CODE=$?

if [ $UNITY_EXIT_CODE -eq 0 ]; then
  echo "Run succeeded, no failures occurred";
elif [ $UNITY_EXIT_CODE -eq 2 ]; then
  echo "Run succeeded, some tests failed";
elif [ $UNITY_EXIT_CODE -eq 3 ]; then
  echo "Run failure (other failure)";
else
  echo "Unexpected exit code $UNITY_EXIT_CODE";
fi

grep test-run < "$TEST_RESULT_FILE_PATH" | grep Passed
exit $UNITY_TEST_EXIT_CODE
