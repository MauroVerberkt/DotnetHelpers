#!/usr/bin/env pwsh

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)
$outputDir = "$PSScriptRoot\bin\packages"

$packages = @(
    @{
        Name = "BusinessRulesManagement"
        Path = "$PSScriptRoot\BusinessRules.Analyzers.Package.csproj"
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

$successCount = 0
$failureCount = 0

foreach ($package in $packages) {
    Write-Host "──────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host "Building: $($package.Name)" -ForegroundColor Yellow
    Write-Host "Description: $($package.Description)" -ForegroundColor Gray
    
    Write-Host "Cleaning..." -ForegroundColor DarkYellow
    dotnet clean $package.Path --configuration Release --verbosity quiet
    
    Write-Host "Packing..." -ForegroundColor DarkYellow
    dotnet pack $package.Path --configuration Release --output $outputDir --verbosity quiet
    
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
    Get-ChildItem -Path $outputDir -Filter "*.nupkg" | Sort-Object LastWriteTime -Descending | ForEach-Object {
        Write-Host "  - $($_.Name)" -ForegroundColor Green
    }
}

if ($failureCount -gt 0) {
    Write-Host ""
    Write-Host "Some packages failed to build!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "All packages built successfully!" -ForegroundColor Green
