#!/bin/bash
# ============================================
# StockNova - EC2 Setup Script
# Run as: sudo bash setup-ec2.sh
# ============================================

set -e

echo "=== Updating system ==="
apt-get update && apt-get upgrade -y

echo "=== Installing Docker ==="
apt-get install -y ca-certificates curl gnupg
install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /etc/apt/keyrings/docker.gpg
chmod a+r /etc/apt/keyrings/docker.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu $(. /etc/os-release && echo "$VERSION_CODENAME") stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

echo "=== Adding ubuntu user to docker group ==="
usermod -aG docker ubuntu

echo "=== Creating app directory ==="
mkdir -p /opt/stocknova
chown ubuntu:ubuntu /opt/stocknova

echo "=== Setup complete ==="
echo "Logout and login again for docker group to take effect"
echo "Then run: cd /opt/stocknova && docker compose -f docker-compose.prod.yml up -d"
