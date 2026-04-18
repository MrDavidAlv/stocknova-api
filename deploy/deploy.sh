#!/bin/bash
# ============================================
# StockNova - Deploy Script (called by CI/CD)
# ============================================

set -e

APP_DIR=/opt/stocknova

echo "=== Deploying StockNova API ==="
cd $APP_DIR

echo "=== Pulling latest images ==="
docker compose -f docker-compose.prod.yml pull

echo "=== Restarting services ==="
docker compose -f docker-compose.prod.yml up -d

echo "=== Cleaning old images ==="
docker image prune -f

echo "=== Waiting for health check ==="
for i in 1 2 3 4 5 6; do
    sleep 10
    if curl -sf http://localhost/health > /dev/null; then
        echo "=== Deploy successful! API is healthy ==="
        exit 0
    fi
    echo "Attempt $i: API not ready yet, retrying..."
done

echo "=== WARNING: Health check failed after 60s ==="
docker compose -f docker-compose.prod.yml logs api --tail 50
exit 1
