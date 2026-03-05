param(
    [string]$SdkId = "Microsoft.DotNet.SDK.8"
)

$existing = Get-Command dotnet -ErrorAction SilentlyContinue
if ($existing) {
    Write-Host "dotnet already available at $($existing.Source)"
    dotnet --info
    exit 0
}

Write-Host "Installing .NET SDK via winget: $SdkId"
winget install --id $SdkId --source winget --accept-package-agreements --accept-source-agreements

Write-Host ""
Write-Host "Open a new terminal and run:"
Write-Host "  dotnet --info"
Write-Host "  dotnet restore MacroPro.sln"
Write-Host "  dotnet build MacroPro.sln -c Release"
