# 1. Register the essential Resource Providers
Write-Host "Registering Resource Providers..." -ForegroundColor Cyan
$providers = @("Microsoft.AlertsManagement", "Microsoft.Insights", "Microsoft.Web", "Microsoft.Storage")

foreach ($provider in $providers) {
    az provider register --namespace $provider
}

# 2. Define your new project home
$rgName = "rg-BlazorFullStack-CentralUs"
$location = "centralus" # Changed to centralus to help avoid quota limits

# 3. Create the new Resource Group
Write-Host "Creating fresh Resource Group: $rgName" -ForegroundColor Green
az group create --name $rgName --location $location --output none

# 4. Set up Azure AD Application
Write-Host "Setting up Azure AD App Registration..." -ForegroundColor Cyan
$appName = "DailyNotesBlazorDemoApp"
$appRes = az ad app create --display-name $appName | ConvertFrom-Json
$clientId = $appRes.appId
$tenantId = az account show --query tenantId -o tsv

Write-Host "Generating Client Secret for the App..." -ForegroundColor Cyan
$secretRes = az ad app credential reset --id $clientId --append --display-name "BlazorDemoSecret" | ConvertFrom-Json
$clientSecret = $secretRes.password

# 5. Deploy the Bicep template
Write-Host "Deploying Bicep template..." -ForegroundColor Cyan
$sqlAdminPassword = Read-Host -Prompt "Enter a secure password for the SQL Administrator"

az deployment group create `
    --resource-group $rgName `
    --template-file ".\deploy.bicep" `
    --parameters sqlAdminPassword=$sqlAdminPassword azureAdClientSecret=$clientSecret azureAdClientId=$clientId azureAdTenantId=$tenantId

Write-Host "Ready for deployment! (Deployment triggered)" -ForegroundColor Yellow