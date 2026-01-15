# Starts all three APIs using Start-Process with explicit HTTP URLs and logs
param()

$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot

$logDir = Join-Path $PWD "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

$processes = @()

function Start-App {
  param(
    [string]$projectPath,
    [string]$url,
    [string]$name
  )
  $outFile = Join-Path $logDir "$name.out.log"
  $errFile = Join-Path $logDir "$name.err.log"
  $args = "run --project `"$projectPath`" -- --urls $url"
  Write-Host "Starting $name at $url" -ForegroundColor Cyan
  $p = Start-Process -FilePath "dotnet" -ArgumentList $args -WorkingDirectory $repoRoot -RedirectStandardOutput $outFile -RedirectStandardError $errFile -PassThru
  $processes += [pscustomobject]@{ Name=$name; Url=$url; Id=$p.Id; Out=$outFile; Err=$errFile }
}

Start-App "src/Order/Order.Api/Order.Api.csproj"     "http://localhost:5294" "order"
Start-App "src/Billing/Billing.Api/Billing.Api.csproj" "http://localhost:5295" "billing"
Start-App "src/Shipping/Shipping.Api/Shipping.Api.csproj" "http://localhost:5296" "shipping"

$pidFile = Join-Path $logDir "pids.json"
$processes | ConvertTo-Json | Set-Content -Path $pidFile -Encoding UTF8

Write-Host "\nProcesses started:" -ForegroundColor Green
$processes | Format-Table Name, Url, Id

Write-Host "\nSwagger UIs:" -ForegroundColor Yellow
Write-Host "  Order:    http://localhost:5294/swagger"
Write-Host "  Billing:  http://localhost:5295/swagger"
Write-Host "  Shipping: http://localhost:5296/swagger"

Write-Host "\nLogs:" -ForegroundColor Yellow
$processes | ForEach-Object { Write-Host "  $($_.Name): $($_.Out) / $($_.Err)" }

Pop-Location
