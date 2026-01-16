# Starts Billing and Shipping Workers
param()

$repoRoot = Split-Path $PSScriptRoot -Parent
Push-Location $repoRoot

# Ensure workers run in Development environment to load appsettings.Development.json
$env:DOTNET_ENVIRONMENT = "Development"

$logDir = Join-Path $PWD "logs"
if (-not (Test-Path $logDir)) { New-Item -ItemType Directory -Path $logDir | Out-Null }

$processes = @()

function Start-Worker {
  param(
    [string]$projectPath,
    [string]$name
  )
  $outFile = Join-Path $logDir "$name-worker.out.log"
  $errFile = Join-Path $logDir "$name-worker.err.log"
  $runArgs = "run -- --environment Development"
  Write-Host "Starting $name worker" -ForegroundColor Cyan
  $projDir = Join-Path $repoRoot (Split-Path $projectPath -Parent)
  $p = Start-Process -FilePath "dotnet" -ArgumentList $runArgs -WorkingDirectory $projDir -RedirectStandardOutput $outFile -RedirectStandardError $errFile -PassThru
  $processes += [pscustomobject]@{ Name=$name; Id=$p.Id; Out=$outFile; Err=$errFile }
}

Start-Worker "src/Billing/Billing.Worker/Billing.Worker.csproj" "billing"
Start-Worker "src/Shipping/Shipping.Worker/Shipping.Worker.csproj" "shipping"

$pidFile = Join-Path $logDir "worker-pids.json"
$processes | ConvertTo-Json | Set-Content -Path $pidFile -Encoding UTF8

Write-Host "\nWorkers started:" -ForegroundColor Green
$processes | Format-Table Name, Id

Write-Host "\nLogs:" -ForegroundColor Yellow
$processes | ForEach-Object { Write-Host "  $($_.Name): $($_.Out) / $($_.Err)" }

Pop-Location
