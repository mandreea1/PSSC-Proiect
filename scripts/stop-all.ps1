# Stops all running API jobs started by run-all.ps1
$jobs = Get-Job | Where-Object { $_.Command -like '*dotnet run --project*' }
if ($jobs) {
	$jobs | Stop-Job -PassThru | Remove-Job
	Write-Host "Stopped $($jobs.Count) background job(s)." -ForegroundColor Green
} else {
	Write-Host "No background jobs found." -ForegroundColor Yellow
}
