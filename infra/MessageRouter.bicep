param token string = guid(resourceGroup().id)
param location string = resourceGroup().location

var suffix = toLower(take(token, 4))

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'log${suffix}'
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

//
// Create a storage account with file share
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.storage/storage-file-share
//

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {  
  name: 'store${suffix}'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
  }
}

resource fileShare 'Microsoft.Storage/storageAccounts/fileServices/shares@2022-05-01' = {
  name: '${storageAccount.name}/default/share${suffix}'
}

//
// Create a Service Bus Namespace and Ingress Queue for Message Router
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.servicebus/servicebus-create-queue
//

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'sb${suffix}'
  location: location
  sku: {
    name: 'Basic'
  }
}

resource serviceBusQueue 'Microsoft.ServiceBus/namespaces/queues@2022-01-01-preview' = {
  parent: serviceBusNamespace
  name: 'ingress'
}

//
// Create a Container App within a Container App Environment
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.app/container-app-create
//

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-06-01-preview' = {
  name: 'env${suffix}'
  location: location
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
}

resource containerApp 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: 'aca${suffix}'
  location: location
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {

    }
  }
}

resource applicationInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai${suffix}'
  location: location
  kind: 'other'
  properties: {
    Application_Type: 'other'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}
