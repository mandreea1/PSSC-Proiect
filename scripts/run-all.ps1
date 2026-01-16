<#
 Starts all three APIs in background PowerShell jobs with explicit URLs.
 Logs output under ./logs/*.log
#>
Push-Location (Split-Path $PSScriptRoot -Parent)

$logDir = Join-Path $PWD "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

Write-Host "Starting Order.Api..." -ForegroundColor Cyan
$orderArgs = "--", "--urls", "http://localhost:5294"
$jobOrder = Start-Job -ScriptBlock {
	param($argsToPass,$logPath)
	dotnet run --no-build --project "src/Order/Order.Api/Order.Api.csproj" @argsToPass *> $logPath
} -ArgumentList (,$orderArgs),(Join-Path $logDir 'order.log')

Write-Host "Starting Billing.Api..." -ForegroundColor Cyan
$billingArgs = "--", "--urls", "http://localhost:5295"
$jobBilling = Start-Job -ScriptBlock {
	param($argsToPass,$logPath)
	dotnet run --no-build --project "src/Billing/Billing.Api/Billing.Api.csproj" @argsToPass *> $logPath
} -ArgumentList (,$billingArgs),(Join-Path $logDir 'billing.log')

Write-Host "Starting Shipping.Api..." -ForegroundColor Cyan
$shippingArgs = "--", "--urls", "http://localhost:5296"
$jobShipping = Start-Job -ScriptBlock {
	param($argsToPass,$logPath)
	dotnet run --no-build --project "src/Shipping/Shipping.Api/Shipping.Api.csproj" @argsToPass *> $logPath
} -ArgumentList (,$shippingArgs),(Join-Path $logDir 'shipping.log')

Write-Host "`nJobs started:" -ForegroundColor Green
Write-Host "  Order   Job Id: $($jobOrder.Id)"
Write-Host "  Billing Job Id: $($jobBilling.Id)"
Write-Host "  Shipping Job Id: $($jobShipping.Id)"

Write-Host "`nSwagger UIs (after apps start):" -ForegroundColor Yellow
Write-Host "  Order:    http://localhost:5294/swagger"
Write-Host "  Billing:  http://localhost:5295/swagger"
Write-Host "  Shipping: http://localhost:5296/swagger"

Write-Host "`nLogs:" -ForegroundColor Yellow
Write-Host (Join-Path $logDir 'order.log')
Write-Host (Join-Path $logDir 'billing.log')
Write-Host (Join-Path $logDir 'shipping.log')

Pop-Location
