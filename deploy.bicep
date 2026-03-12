@description('The location of the resources.')
param location string = resourceGroup().location

@description('The name of the App Service Plan.')
param appServicePlanName string = 'app-dailynotes-plan'

@description('The name of the API Web App.')
param apiWebAppName string = 'app-dailynotes-api-hsh'

@description('The name of the Blazor Web App.')
param blazorWebAppName string = 'app-dailynotes-blazor-hsh'

@description('The name of the SQL Server.')
param sqlServerName string = 'sql-dailynotes-srv-hsh'

@description('The name of the SQL Database.')
param sqlDatabaseName string = 'db-dailynotes'

@description('The administrator login for the SQL Server.')
param sqlAdminLogin string = 'dailynotesadmin'

@description('The administrator password for the SQL Server.')
@secure()
param sqlAdminPassword string

@description('The Client Secret for the Azure AD App Registration.')
@secure()
param azureAdClientSecret string = ''

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
}

resource sqlServer 'Microsoft.Sql/servers@2022-11-01-preview' = {
  name: sqlServerName
  location: location
  properties: {
    administratorLogin: sqlAdminLogin
    administratorLoginPassword: sqlAdminPassword
    version: '12.0'
  }
}

resource sqlDatabase 'Microsoft.Sql/servers/databases@2022-11-01-preview' = {
  parent: sqlServer
  name: sqlDatabaseName
  location: location
  sku: {
    name: 'Basic'
    tier: 'Basic'
    capacity: 5
  }
}

resource sqlServerFirewallRule 'Microsoft.Sql/servers/firewallRules@2022-11-01-preview' = {
  parent: sqlServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource apiWebApp 'Microsoft.Web/sites@2022-09-01' = {
  name: apiWebAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v10.0' // Windows uses netFrameworkVersion for .NET Core too
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
      ]
    }
  }
}

resource blazorWebApp 'Microsoft.Web/sites@2022-09-01' = {
  name: blazorWebAppName
  location: location
  properties: {
    serverFarmId: appServicePlan.id
    siteConfig: {
      netFrameworkVersion: 'v10.0'
      appSettings: [
        {
          name: 'DailyNotesApi__BaseAddress'
          value: 'https://${apiWebApp.properties.defaultHostName}/'
        }
        {
          name: 'AzureAd__ClientSecret'
          value: azureAdClientSecret
        }
      ]
    }
  }
}

output apiWebAppUrl string = 'https://${apiWebApp.properties.defaultHostName}'
output blazorWebAppUrl string = 'https://${blazorWebApp.properties.defaultHostName}'
