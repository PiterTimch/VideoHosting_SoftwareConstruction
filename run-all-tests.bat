@echo off
setlocal enabledelayedexpansion

echo =======================================================
echo          VideoHosting QA Automation Framework
echo =======================================================

set REPORTS_DIR=%~dp0test-reports
if not exist "%REPORTS_DIR%" mkdir "%REPORTS_DIR%"

set TIMESTAMP=%date% %time%
set START_TIME=%time%

echo [1/6] Running Unit Tests...
echo Running Backend Unit Tests...
dotnet test "%~dp0VideoHostingServer\tests\UnitTests\UnitTests.csproj" --logger "console;verbosity=detailed" > "%REPORTS_DIR%\1_unit_tests_report_tmp.txt" 2>&1
echo Running Frontend Unit Tests...
cd "%~dp0VideoHostingWeb"
call npm.cmd run test:unit >> "%~dp0test-reports\1_unit_tests_report_tmp.txt" 2>&1
cd "%~dp0"
call :FormatReport "1_unit_tests_report.txt" "%REPORTS_DIR%\1_unit_tests_report_tmp.txt" "Unit Tests"
del "%REPORTS_DIR%\1_unit_tests_report_tmp.txt"

echo Starting Backend Server in background...
start "BackendAPI" /MIN cmd /c "cd %~dp0VideoHostingServer\VideoHostingServer && dotnet run"
timeout /t 5 /nobreak >nul

echo [2/6] Running Integration Tests...
dotnet test "%~dp0VideoHostingServer\tests\IntegrationTests\IntegrationTests.csproj" --logger "console;verbosity=detailed" > "%REPORTS_DIR%\2_integration_tests_report_tmp.txt" 2>&1
call :FormatReport "2_integration_tests_report.txt" "%REPORTS_DIR%\2_integration_tests_report_tmp.txt" "Integration Tests"
del "%REPORTS_DIR%\2_integration_tests_report_tmp.txt"

echo [3/6] Running Security Tests...
dotnet test "%~dp0VideoHostingServer\tests\SecurityTests\SecurityTests.csproj" --logger "console;verbosity=detailed" > "%REPORTS_DIR%\3_security_tests_report_tmp.txt" 2>&1
call :FormatReport "3_security_tests_report.txt" "%REPORTS_DIR%\3_security_tests_report_tmp.txt" "Security Tests"
del "%REPORTS_DIR%\3_security_tests_report_tmp.txt"

echo [4/6] Running UI Tests...
cd "%~dp0VideoHostingWeb"
call npm.cmd run test:ui > "%~dp0test-reports\4_ui_tests_report_tmp.txt" 2>&1
cd "%~dp0"
call :FormatReport "4_ui_tests_report.txt" "%REPORTS_DIR%\4_ui_tests_report_tmp.txt" "UI Tests"
del "%REPORTS_DIR%\4_ui_tests_report_tmp.txt"

echo [5/6] Running E2E Tests...
cd "%~dp0VideoHostingWeb"
call npx.cmd playwright test > "%~dp0test-reports\5_e2e_tests_report_tmp.txt" 2>&1
cd "%~dp0"
call :FormatReport "5_e2e_tests_report.txt" "%REPORTS_DIR%\5_e2e_tests_report_tmp.txt" "E2E Tests"
del "%REPORTS_DIR%\5_e2e_tests_report_tmp.txt"

echo [6/6] Running Performance Tests...
echo Loading k6...
where k6 >nul 2>nul
if %ERRORLEVEL% EQU 0 (
    k6 run "%~dp0VideoHostingWeb\tests\load-test.js" > "%REPORTS_DIR%\6_performance_tests_report_tmp.txt" 2>&1
) else (
    echo k6 is not installed or not in PATH. Please install k6. > "%REPORTS_DIR%\6_performance_tests_report_tmp.txt"
    echo FAILED (k6 missing) >> "%REPORTS_DIR%\6_performance_tests_report_tmp.txt"
)
call :FormatReport "6_performance_tests_report.txt" "%REPORTS_DIR%\6_performance_tests_report_tmp.txt" "Performance Tests"
del "%REPORTS_DIR%\6_performance_tests_report_tmp.txt"

echo.
echo Stopping Backend Server...
taskkill /FI "WINDOWTITLE eq BackendAPI" /T /F >nul 2>&1

echo All tests completed. Reports are saved in %REPORTS_DIR%

echo Opening E2E Test Report in browser...
start cmd /c "cd %~dp0VideoHostingWeb && npx.cmd playwright show-report"
goto :EOF

:FormatReport
set REPORT_NAME=%~1
set TEMP_FILE=%~2
set TEST_TYPE=%~3

set TOTAL=0
set PASSED=0
set FAILED=0

REM Extract stats from dotnet test output and k6
for /f "tokens=*" %%a in ('findstr /C:"Total tests:" "%TEMP_FILE%" 2^>nul') do (
    for /f "tokens=3,5,7 delims=: " %%b in ("%%a") do (
        set TOTAL=%%b
        set PASSED=%%c
        set FAILED=%%d
    )
)

echo Report: %TEST_TYPE% > "%REPORTS_DIR%\%REPORT_NAME%"
echo Date: %TIMESTAMP% >> "%REPORTS_DIR%\%REPORT_NAME%"
echo ---------------------------------------- >> "%REPORTS_DIR%\%REPORT_NAME%"

if "!TOTAL!"=="0" (
    findstr /I /C:"failed" /C:"failing" /C:"error" "%TEMP_FILE%" >nul
    if !ERRORLEVEL! EQU 0 (
        echo Result: FAILED >> "%REPORTS_DIR%\%REPORT_NAME%"
    ) else (
        echo Result: PASSED >> "%REPORTS_DIR%\%REPORT_NAME%"
    )
    echo (JavaScript runner output. See raw logs for detailed checks.^) >> "%REPORTS_DIR%\%REPORT_NAME%"
) else (
    echo Total: !TOTAL! >> "%REPORTS_DIR%\%REPORT_NAME%"
    echo Passed: !PASSED! >> "%REPORTS_DIR%\%REPORT_NAME%"
    echo Failed: !FAILED! >> "%REPORTS_DIR%\%REPORT_NAME%"
)

echo ---------------------------------------- >> "%REPORTS_DIR%\%REPORT_NAME%"
echo Details: >> "%REPORTS_DIR%\%REPORT_NAME%"
if "!TOTAL!"=="0" (
    type "%TEMP_FILE%" >> "%REPORTS_DIR%\%REPORT_NAME%" 2>nul
) else (
    findstr /I /C:"failed " /C:"fail" "%TEMP_FILE%" >> "%REPORTS_DIR%\%REPORT_NAME%" 2>nul
)
goto :EOF
