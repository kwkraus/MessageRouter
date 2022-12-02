//
// https://github.com/Azure-Samples/pubsub-dapr-csharp-servicebus/blob/main/infra/resources.bicep
//

param name string
param location string
param principalId string
param messageRouterImageName string
param resourceToken string
param tags object

module logAnalyticsResources './loganalytics.bicep' = {
  name: 'logAnalyticsResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
}

module appInsightsResources './appinsights.bicep' = {
  name: 'appInsightsResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
  dependsOn: [
    logAnalyticsResources
  ]
}

module storageResources './storage.bicep' = {
  name: 'storageResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
}

module containerAppEnvironmentResources './containerappenvironment.bicep' = {
  name: 'containerAppEnvironmentResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
  dependsOn: [
    logAnalyticsResources
    storageResources
  ]  
}

module managedIdentityResources './managedidentity.bicep' = {
  name: 'managedIdentityResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }  
}

module containerRegistryResources './containerregistry.bicep' = {
  name: 'containerRegistryResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
  dependsOn: [
    managedIdentityResources
  ]
}

module serviceBusResources './servicebus.bicep' = {
  name: 'serviceBusResources'
  params: {
    resourceToken: resourceToken
    location: location
    tags: tags
  }
  dependsOn: [
    managedIdentityResources
  ]
}

module messageRouterAppResources './messagerouterapp.bicep' = {
  name: 'messageRouterAppResources'
  params: {
    name: name
    location: location
    imageName: messageRouterImageName != '' ? messageRouterImageName : 'mcr.microsoft.com/dotnet/samples:aspnetapp'
  }
  dependsOn: [
    managedIdentityResources
    containerAppEnvironmentResources
    containerRegistryResources
    serviceBusResources
    appInsightsResources
  ]  
}  

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistryResources.outputs.AZURE_CONTAINER_REGISTRY_ENDPOINT
