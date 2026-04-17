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
sleep 10
if curl -sf http://localhost/health > /dev/null; then
    echo "=== Deploy successful! API is healthy ==="
else
    echo "=== WARNING: Health check failed ==="
    docker compose -f docker-compose.prod.yml logs api --tail 50
    exit 1
fi
