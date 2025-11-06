#!/usr/bin/env pwsh

# Start ClickHouse 23.6-alpine Docker container
# Author: Generated script
# Description: Starts ClickHouse with username 'clickhouse' and password 'clickhouse'

param(
    [string]$ContainerName = "clickhouse-server",
    [int]$Port = 8123,
    [int]$NativePort = 9000
)

Write-Host "Starting ClickHouse 23.6-alpine Docker container..." -ForegroundColor Green

# Stop and remove existing container if it exists
$existingContainer = docker ps -a -q -f name=$ContainerName
if ($existingContainer) {
    Write-Host "Stopping existing container '$ContainerName'..." -ForegroundColor Yellow
    docker stop $ContainerName | Out-Null
    docker rm $ContainerName | Out-Null
}

# Start new ClickHouse container
Write-Host "Starting new ClickHouse container..." -ForegroundColor Green

docker run -d `
    --name $ContainerName `
    -p "${Port}:8123" `
    -p "${NativePort}:9000" `
    -e CLICKHOUSE_USER=clickhouse `
    -e CLICKHOUSE_PASSWORD=clickhouse `
    -e CLICKHOUSE_DEFAULT_ACCESS_MANAGEMENT=1 `
    clickhouse/clickhouse-server:23.6-alpine

if ($LASTEXITCODE -eq 0) {
    Write-Host "ClickHouse container started successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Connection Details:" -ForegroundColor Cyan
    Write-Host "  HTTP Port:   $Port" -ForegroundColor White
    Write-Host "  Native Port: $NativePort" -ForegroundColor White
    Write-Host "  Username:    clickhouse" -ForegroundColor White
    Write-Host "  Password:    clickhouse" -ForegroundColor White
    Write-Host ""
    Write-Host "HTTP URL:      http://localhost:$Port" -ForegroundColor Yellow
    Write-Host "Native URL:    clickhouse://clickhouse:clickhouse@localhost:$NativePort" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "To connect via clickhouse-client:" -ForegroundColor Cyan
    Write-Host "  docker exec -it $ContainerName clickhouse-client --user=clickhouse --password=clickhouse" -ForegroundColor White
    Write-Host ""
    Write-Host "To view logs:" -ForegroundColor Cyan
    Write-Host "  docker logs -f $ContainerName" -ForegroundColor White
} else {
    Write-Host "Failed to start ClickHouse container!" -ForegroundColor Red
    exit 1
}
