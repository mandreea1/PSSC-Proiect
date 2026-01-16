# Stops processes started by run-all-proc.ps1
param()

$repoRoot = Split-Path $PSScriptRoot -Parent
$pidFile = Join-Path $repoRoot "logs\pids.json"

if (-not (Test-Path $pidFile)) { Write-Host "No pid file found." -ForegroundColor Yellow; return }

$pList = Get-Content $pidFile | ConvertFrom-Json
foreach ($p in $pList) {
  try {
    $proc = Get-Process -Id $p.Id -ErrorAction Stop
    Write-Host "Stopping $($p.Name) (PID $($p.Id))" -ForegroundColor Cyan
    $proc | Stop-Process -Force
  } catch {
    Write-Host "Process $($p.Id) already stopped." -ForegroundColor Yellow
  }
}
