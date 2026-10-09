#!/bin/bash
# ZAP Baseline Scan for VideoHosting API
# Requires Docker to run the zap2docker-stable image

TARGET_URL="http://localhost:5000/swagger/v1/swagger.json" # Adjust to your local API URL or deployed environment

echo "Starting OWASP ZAP Baseline Scan against ${TARGET_URL}..."

docker run -t owasp/zap2docker-stable zap-api-scan.py -t $TARGET_URL -f openapi -r zap_report.html

echo "ZAP scan completed. Check zap_report.html for details."
