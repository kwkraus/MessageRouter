//
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.app/container-app-create
// https://learn.microsoft.com/en-us/azure/container-apps/storage-mounts-azure-files
// https://learn.microsoft.com/en-us/azure/templates/microsoft.app/connectedenvironments/storages?pivots=deployment-language-bicep
//

param resourceToken string
param location string
param tags object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'log${resourceToken}'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' existing = {  
  name: 'st${resourceToken}'
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-06-01-preview' = {
  name: 'cae${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Consumption'
  }
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
  resource storages 'storages' = {
    name: uniqueString(storageAccount.name, 'sh${resourceToken}')
    properties: {
      azureFile: {
        accessMode: 'ReadOnly'
        accountName: storageAccount.name
        accountKey: storageAccount.listKeys().keys[0].value
        shareName: 'sh${resourceToken}' 
      }
    }
  }
}
