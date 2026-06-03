$rgName = "rg-BlazorFullStack-CentralUs"
$apiAppName = "app-dn-api-hsh8"
$blazorAppName = "app-dn-blazor-hsh8"

$apiOut = Join-Path $PWD "pub-api"
$blazorOut = Join-Path $PWD "pub-blazor"

# Ensure we remove old zips first
Remove-Item -Force .\api.zip -ErrorAction SilentlyContinue
Remove-Item -Force .\blazor.zip -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $apiOut -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $blazorOut -ErrorAction SilentlyContinue

Add-Type -AssemblyName System.IO.Compression.FileSystem

Write-Host "Publishing DailyNotes.Api..." -ForegroundColor Cyan
dotnet publish .\DailyNotes.Api\DailyNotes.Api.csproj -c Release -o $apiOut
Write-Host "Zipping API..." -ForegroundColor Cyan
[System.IO.Compression.ZipFile]::CreateFromDirectory($apiOut, "$PWD\api.zip")

Write-Host "Deploying DailyNotes.Api to Azure..." -ForegroundColor Cyan
az webapp deploy --resource-group $rgName --name $apiAppName --src-path .\api.zip --type zip

Write-Host "Publishing DailyNotes.Blazor..." -ForegroundColor Cyan
dotnet publish .\DailyNotes.Blazor\DailyNotes.Blazor.csproj -c Release -o $blazorOut
Write-Host "Zipping Blazor..." -ForegroundColor Cyan
[System.IO.Compression.ZipFile]::CreateFromDirectory($blazorOut, "$PWD\blazor.zip")

Write-Host "Deploying DailyNotes.Blazor to Azure..." -ForegroundColor Cyan
az webapp deploy --resource-group $rgName --name $blazorAppName --src-path .\blazor.zip --type zip

Write-Host "Cleaning up temporary files..."
Remove-Item -Recurse -Force $apiOut -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $blazorOut -ErrorAction SilentlyContinue
Remove-Item -Force .\api.zip -ErrorAction SilentlyContinue
Remove-Item -Force .\blazor.zip -ErrorAction SilentlyContinue

Write-Host "Deployment Complete!" -ForegroundColor Green
