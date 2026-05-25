#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$outputDir = "$repoRoot\packages"

$packages = @(
    @{
        Name = "BusinessRulesManagement"
        Path = "$repoRoot\src\BusinessRules\BusinessRules.csproj"
        Description = "Core BusinessRules with analyzers and source generators"
    },
    @{
        Name = "BusinessRulesManagement.ResultExtensions"
        Path = "$repoRoot\src\BusinessRules.ResultExtensions\BusinessRules.ResultExtensions.csproj"
        Description = "Result monad integration for BusinessRules"
    },
    @{
        Name = "BusinessRulesManagement.Wcf"
        Path = "$repoRoot\src\BusinessRules.Wcf\BusinessRules.Wcf.csproj"
        Description = "WCF integration for BusinessRules"
    }
)

Write-Host "Building BusinessRules Packages..." -ForegroundColor Cyan
Write-Host "Output directory: $outputDir" -ForegroundColor Gray
Write-Host ""

if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

$successCount = 0
$failureCount = 0

foreach ($package in $packages) {
    Write-Host "──────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "Building: $($package.Name)" -ForegroundColor Yellow
    Write-Host "Description: $($package.Description)" -ForegroundColor Gray
    
    Write-Host "Building and packing..." -ForegroundColor DarkYellow
    dotnet pack $package.Path --configuration Release --output $outputDir
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[SUCCESS] $($package.Name) created successfully" -ForegroundColor Green
        $successCount++
    } else {
        Write-Host "[FAILED] $($package.Name) failed to build" -ForegroundColor Red
        $failureCount++
    }
}

Write-Host ""
Write-Host "──────────────────────────────────────────" -ForegroundColor DarkGray
Write-Host "Package Build Summary:" -ForegroundColor Cyan
Write-Host "  Succeeded: $successCount" -ForegroundColor Green
Write-Host "  Failed:    $failureCount" -ForegroundColor $(if ($failureCount -gt 0) { "Red" } else { "Gray" })

if ($successCount -gt 0) {
    Write-Host ""
    Write-Host "Created packages in: $outputDir" -ForegroundColor Cyan
    Get-ChildItem -Path $outputDir -Filter "BusinessRules*.nupkg" | Sort-Object LastWriteTime -Descending | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Green
    }
}

if ($failureCount -gt 0) {
    Write-Host ""
    Write-Host "Some packages failed to build!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "All BusinessRules packages built successfully!" -ForegroundColor Green
