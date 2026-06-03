@description('The location of the resources.')
param location string = resourceGroup().location

@description('The name of the App Service Plan.')
param appServicePlanName string = 'app-dn-plan-hsh8'

@description('The name of the API Web App.')
param apiWebAppName string = 'app-dn-api-hsh8'

@description('The name of the Blazor Web App.')
param blazorWebAppName string = 'app-dn-blazor-hsh8'

@description('The name of the SQL Server.')
param sqlServerName string = 'sql-dn-srv-hsh8'

@description('The name of the Key Vault.')
param keyVaultName string = 'kv-dn-${uniqueString(resourceGroup().id)}'

@description('The name of the SQL Database.')
param sqlDatabaseName string = 'db-dn-hsh8'

@description('The administrator login for the SQL Server.')
param sqlAdminLogin string = 'dailynotesadmin'

@description('The administrator password for the SQL Server.')
@secure()
param sqlAdminPassword string

@description('The Client Secret for the Azure AD App Registration.')
@secure()
param azureAdClientSecret string = ''

@description('The Client ID for the Azure AD App Registration.')
param azureAdClientId string = '8ac752a3-8ea2-45e2-8579-1621b5dc0dde'

@description('The Tenant ID for the Azure AD.')
param azureAdTenantId string = '3800a8aa-7593-4bfa-87fe-bd84a01620b4'

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
      netFrameworkVersion: 'v10.0' 
      appSettings: [
        {
          name: 'ConnectionStrings__DefaultConnection'
          value: 'Server=tcp:${sqlServer.properties.fullyQualifiedDomainName},1433;Initial Catalog=${sqlDatabaseName};Persist Security Info=False;User ID=${sqlAdminLogin};Password=${sqlAdminPassword};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;'
        }
        {
          name: 'AzureAd__TenantId'
          value: azureAdTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: azureAdClientId
        }
      ]
    }
  }
}

resource apiWebAppBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: apiWebApp
  name: 'scm'
  properties: {
    allow: true
  }
}

resource apiWebAppFtpBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: apiWebApp
  name: 'ftp'
  properties: {
    allow: true
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2022-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: subscription().tenantId
    enableRbacAuthorization: false
    accessPolicies: []
  }
}

resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2022-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: blazorWebApp.identity.principalId
        permissions: {
          secrets: [
            'get'
          ]
        }
      }
    ]
  }
}

resource azureAdSecret 'Microsoft.KeyVault/vaults/secrets@2022-07-01' = {
  parent: keyVault
  name: 'AzureAd--ClientSecret'
  properties: {
    value: azureAdClientSecret
  }
}

resource blazorWebApp 'Microsoft.Web/sites@2022-09-01' = {
  name: blazorWebAppName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
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
          value: '@Microsoft.KeyVault(SecretUri=${azureAdSecret.properties.secretUri})'
        }
        {
          name: 'AzureAd__TenantId'
          value: azureAdTenantId
        }
        {
          name: 'AzureAd__ClientId'
          value: azureAdClientId
        }
      ]
    }
  }
}

resource blazorWebAppBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: blazorWebApp
  name: 'scm'
  properties: {
    allow: true
  }
}

resource blazorWebAppFtpBasicAuth 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2022-09-01' = {
  parent: blazorWebApp
  name: 'ftp'
  properties: {
    allow: true
  }
}

output apiWebAppUrl string = 'https://${apiWebApp.properties.defaultHostName}'
output blazorWebAppUrl string = 'https://${blazorWebApp.properties.defaultHostName}'
