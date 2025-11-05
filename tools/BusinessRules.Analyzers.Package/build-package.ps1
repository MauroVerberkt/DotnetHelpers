#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

Write-Host "Building BusinessRulesManagement.Package..." -ForegroundColor Cyan

$projectPath = "$PSScriptRoot\BusinessRules.Analyzers.Package.csproj"
$outputDir = "$PSScriptRoot\bin\packages"

Write-Host "Cleaning previous builds..." -ForegroundColor Yellow
dotnet clean $projectPath --configuration Release

Write-Host "Building package..." -ForegroundColor Yellow
dotnet pack $projectPath --configuration Release --output $outputDir

if ($LASTEXITCODE -eq 0) {
    Write-Host "Package created successfully in: $outputDir" -ForegroundColor Green
    Get-ChildItem -Path $outputDir -Filter "*.nupkg" | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Green
    }
} else {
    Write-Host "Package build failed!" -ForegroundColor Red
    exit 1
}
