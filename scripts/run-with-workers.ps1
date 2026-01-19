#!/usr/bin/env pwsh
# Starts 3 services in separate windows: Order.Api, Billing.Api, Shipping.Api
# DDD async architecture with semantic IDs

Push-Location (Split-Path $PSScriptRoot -Parent)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting DDD Async Microservices" -ForegroundColor Cyan
Write-Host "Order → Billing → Shipping (async flow)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$orderPath = Join-Path $PSScriptRoot "..\src\Order\Order.Api"
$billingPath = Join-Path $PSScriptRoot "..\src\Billing\Billing.Api"
$shippingPath = Join-Path $PSScriptRoot "..\src\Shipping\Shipping.Api"

Write-Host "Starting Order.Api on http://localhost:5294..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$orderPath'; dotnet run --urls http://localhost:5294"

Start-Sleep -Seconds 3

Write-Host "Starting Billing.Api on http://localhost:5295..." -ForegroundColor Magenta
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$billingPath'; dotnet run --urls http://localhost:5295"

Start-Sleep -Seconds 3

Write-Host "Starting Shipping.Api on http://localhost:5296..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$shippingPath'; dotnet run --urls http://localhost:5296"

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ All services started in separate windows!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Order.Api:    http://localhost:5294/swagger" -ForegroundColor Green
Write-Host "Billing.Api:  http://localhost:5295/swagger" -ForegroundColor Magenta
Write-Host "Shipping.Api: http://localhost:5296/swagger" -ForegroundColor Cyan
Write-Host ""
Write-Host "To stop all: Close the windows or use Get-Process | Where Name -match 'dotnet' | Stop-Process" -ForegroundColor Yellow

Pop-Location
