#!/usr/bin/env pwsh
# Starts 5 services: Order.Api, Billing.Api + Worker, Shipping.Api + Worker
# DDD async architecture with semantic IDs
# Logs under ./logs/*.log

Push-Location (Split-Path $PSScriptRoot -Parent)

$logDir = Join-Path $PWD "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Starting DDD Async Microservices" -ForegroundColor Cyan
Write-Host "Order → Billing → Shipping (async flow)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Starting Order.Api..." -ForegroundColor Cyan
$orderArgs = "--", "--urls", "http://localhost:5294"
$jobOrder = Start-Job -ScriptBlock {
	param($projPath, $argsToPass, $logPath)
	Set-Location $projPath
	dotnet run --no-build @argsToPass 2>&1 | Tee-Object -FilePath $logPath
} -ArgumentList (Join-Path $PSScriptRoot "..\src\Order\Order.Api"), (,$orderArgs), (Join-Path $logDir 'order.log')

Start-Sleep -Seconds 2

Write-Host "Starting Billing.Api..." -ForegroundColor Cyan
$billingArgs = "--", "--urls", "http://localhost:5295"
$jobBillingApi = Start-Job -ScriptBlock {
	param($projPath, $argsToPass, $logPath)
	Set-Location $projPath
	dotnet run --no-build @argsToPass 2>&1 | Tee-Object -FilePath $logPath
} -ArgumentList (Join-Path $PSScriptRoot "..\src\Billing\Billing.Api"), (,$billingArgs), (Join-Path $logDir 'billing-api.log')

Start-Sleep -Seconds 2

Write-Host "Starting Billing.Worker (event listener)..." -ForegroundColor Cyan
$jobBillingWorker = Start-Job -ScriptBlock {
	param($projPath, $logPath)
	Set-Location $projPath
	$env:DOTNET_ENVIRONMENT = 'Development'
	dotnet run --no-build 2>&1 | Tee-Object -FilePath $logPath
} -ArgumentList (Join-Path $PSScriptRoot "..\src\Billing\Billing.Worker"), (Join-Path $logDir 'billing-worker.log')

Start-Sleep -Seconds 2

Write-Host "Starting Shipping.Api..." -ForegroundColor Cyan
$shippingArgs = "--", "--urls", "http://localhost:5296"
$jobShippingApi = Start-Job -ScriptBlock {
	param($projPath, $argsToPass, $logPath)
	Set-Location $projPath
	dotnet run --no-build @argsToPass 2>&1 | Tee-Object -FilePath $logPath
} -ArgumentList (Join-Path $PSScriptRoot "..\src\Shipping\Shipping.Api"), (,$shippingArgs), (Join-Path $logDir 'shipping-api.log')

Start-Sleep -Seconds 2

Write-Host "Starting Shipping.Worker (event listener)..." -ForegroundColor Cyan
$jobShippingWorker = Start-Job -ScriptBlock {
	param($projPath, $logPath)
	Set-Location $projPath
	$env:DOTNET_ENVIRONMENT = 'Development'
	dotnet run --no-build 2>&1 | Tee-Object -FilePath $logPath
} -ArgumentList (Join-Path $PSScriptRoot "..\src\Shipping\Shipping.Worker"), (Join-Path $logDir 'shipping-worker.log')

Write-Host ""
Write-Host "============ Services Started ============" -ForegroundColor Green
Write-Host "  Order.Api          (Job $($jobOrder.Id))          http://localhost:5294" -ForegroundColor Green
Write-Host "  Billing.Api        (Job $($jobBillingApi.Id))          http://localhost:5295" -ForegroundColor Green
Write-Host "  Billing.Worker     (Job $($jobBillingWorker.Id)) - listens to OrderPlaced" -ForegroundColor Green
Write-Host "  Shipping.Api       (Job $($jobShippingApi.Id))          http://localhost:5296" -ForegroundColor Green
Write-Host "  Shipping.Worker    (Job $($jobShippingWorker.Id)) - listens to InvoiceIssued" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host ""
Write-Host "Async Event Flow:" -ForegroundColor Magenta
Write-Host "  1. POST /orders on Order.Api → publishes OrderPlaced" -ForegroundColor Magenta
Write-Host "  2. Billing.Worker receives OrderPlaced → creates Invoice, publishes InvoiceIssued" -ForegroundColor Magenta
Write-Host "  3. Shipping.Worker receives InvoiceIssued → creates Shipment, publishes OrderShipped" -ForegroundColor Magenta
Write-Host ""

Write-Host "Logs:" -ForegroundColor Yellow
Write-Host "  Order:    $(Join-Path $logDir 'order.log')" -ForegroundColor Gray
Write-Host "  Billing:  $(Join-Path $logDir 'billing-api.log') & $(Join-Path $logDir 'billing-worker.log')" -ForegroundColor Gray
Write-Host "  Shipping: $(Join-Path $logDir 'shipping-api.log') & $(Join-Path $logDir 'shipping-worker.log')" -ForegroundColor Gray

Write-Host ""
Write-Host "To stop all: Get-Job | Stop-Job" -ForegroundColor Yellow

Pop-Location
