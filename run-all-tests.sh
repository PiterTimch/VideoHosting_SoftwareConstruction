#!/bin/bash

echo "======================================================="
echo "         VideoHosting QA Automation Framework"
echo "======================================================="

REPORTS_DIR="$(pwd)/test-reports"
mkdir -p "$REPORTS_DIR"
TIMESTAMP=$(date +"%Y-%m-%d %H:%M:%S")

format_report() {
    local report_name=$1
    local temp_file=$2
    local test_type=$3

    local total=$(grep -oP 'Total tests: \K\d+' "$temp_file" || echo 0)
    local passed=$(grep -oP 'Passed: \K\d+' "$temp_file" || echo 0)
    local failed=$(grep -oP 'Failed: \K\d+' "$temp_file" || echo 0)

    echo "Report: $test_type" > "$REPORTS_DIR/$report_name"
    echo "Date: $TIMESTAMP" >> "$REPORTS_DIR/$report_name"
    echo "----------------------------------------" >> "$REPORTS_DIR/$report_name"
    
    if [ "$total" -eq "0" ]; then
        if grep -qiE "failed|failing|error" "$temp_file"; then
            echo "Result: FAILED" >> "$REPORTS_DIR/$report_name"
        else
            echo "Result: PASSED" >> "$REPORTS_DIR/$report_name"
        fi
        echo "(JavaScript runner output. See raw logs for detailed checks.)" >> "$REPORTS_DIR/$report_name"
    else
        echo "Total: $total" >> "$REPORTS_DIR/$report_name"
        echo "Passed: $passed" >> "$REPORTS_DIR/$report_name"
        echo "Failed: $failed" >> "$REPORTS_DIR/$report_name"
    fi
    
    echo "----------------------------------------" >> "$REPORTS_DIR/$report_name"
    echo "Details:" >> "$REPORTS_DIR/$report_name"
    if [ "$total" -eq "0" ]; then
        cat "$temp_file" >> "$REPORTS_DIR/$report_name" 2>/dev/null || true
    else
        grep -i "fail" "$temp_file" >> "$REPORTS_DIR/$report_name" 2>/dev/null || true
    fi
}

echo "[1/6] Running Unit Tests..."
echo "Running Backend Unit Tests..."
dotnet test "VideoHostingServer/tests/UnitTests/UnitTests.csproj" --logger "console;verbosity=detailed" > "$REPORTS_DIR/1_unit_tests_report_tmp.txt" 2>&1
echo "Running Frontend Unit Tests..."
cd VideoHostingWeb
npm run test:unit >> "../test-reports/1_unit_tests_report_tmp.txt" 2>&1
cd ..
format_report "1_unit_tests_report.txt" "$REPORTS_DIR/1_unit_tests_report_tmp.txt" "Unit Tests"
rm "$REPORTS_DIR/1_unit_tests_report_tmp.txt"

echo "[2/6] Running Integration Tests..."
dotnet test "VideoHostingServer/tests/IntegrationTests/IntegrationTests.csproj" --logger "console;verbosity=detailed" > "$REPORTS_DIR/2_integration_tests_report_tmp.txt" 2>&1
format_report "2_integration_tests_report.txt" "$REPORTS_DIR/2_integration_tests_report_tmp.txt" "Integration Tests"
rm "$REPORTS_DIR/2_integration_tests_report_tmp.txt"

echo "[3/6] Running Security Tests..."
dotnet test "VideoHostingServer/tests/SecurityTests/SecurityTests.csproj" --logger "console;verbosity=detailed" > "$REPORTS_DIR/3_security_tests_report_tmp.txt" 2>&1
format_report "3_security_tests_report.txt" "$REPORTS_DIR/3_security_tests_report_tmp.txt" "Security Tests"
rm "$REPORTS_DIR/3_security_tests_report_tmp.txt"

echo "[4/6] Running UI Tests..."
cd VideoHostingWeb
npm run test:ui > "../test-reports/4_ui_tests_report_tmp.txt" 2>&1
cd ..
format_report "4_ui_tests_report.txt" "$REPORTS_DIR/4_ui_tests_report_tmp.txt" "UI Tests"
rm "$REPORTS_DIR/4_ui_tests_report_tmp.txt"

echo "[5/6] Running E2E Tests..."
cd VideoHostingWeb
npx playwright test > "../test-reports/5_e2e_tests_report_tmp.txt" 2>&1
cd ..
format_report "5_e2e_tests_report.txt" "$REPORTS_DIR/5_e2e_tests_report_tmp.txt" "E2E Tests"
rm "$REPORTS_DIR/5_e2e_tests_report_tmp.txt"

echo "[6/6] Running Performance Tests..."
echo "Loading k6..."
if command -v k6 &> /dev/null; then
    k6 run "VideoHostingWeb/tests/load-test.js" > "$REPORTS_DIR/6_performance_tests_report_tmp.txt" 2>&1
else
    echo "k6 is not installed or not in PATH. Please install k6." > "$REPORTS_DIR/6_performance_tests_report_tmp.txt"
fi
format_report "6_performance_tests_report.txt" "$REPORTS_DIR/6_performance_tests_report_tmp.txt" "Performance Tests"
rm "$REPORTS_DIR/6_performance_tests_report_tmp.txt"

echo ""
echo "All tests completed. Reports are saved in $REPORTS_DIR"
