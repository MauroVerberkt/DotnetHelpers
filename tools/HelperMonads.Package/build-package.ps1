#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$outputDir = "$PSScriptRoot\bin\packages"

Write-Host "Building HelperMonads Package..." -ForegroundColor Cyan
Write-Host "Output directory: $outputDir" -ForegroundColor Gray
Write-Host ""

$projectPath = "$PSScriptRoot\HelperMonads.Package.csproj"

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean $projectPath --configuration Release --verbosity quiet

Write-Host "Building package..." -ForegroundColor Yellow
dotnet pack $projectPath --configuration Release --output $outputDir --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "Package created successfully in: $outputDir" -ForegroundColor Green
    Get-ChildItem -Path $outputDir -Filter "*.nupkg" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Green
    }
} else {
    Write-Host ""
    Write-Host "Package build failed!" -ForegroundColor Red
    exit 1
}
