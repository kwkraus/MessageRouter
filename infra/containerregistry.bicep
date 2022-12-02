//
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.containerregistry/container-registry
//

param resourceToken string
param location string
param tags object

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' existing = {
  name: 'mi${resourceToken}'
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' = {
  name: 'acr${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: true
  }
}

// 
// AcrPull: 7f951dda-4ed3-4680-a7ca-43fe172d538d
// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
//
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(managedIdentity.id, resourceGroup().id, '7f951dda-4ed3-4680-a7ca-43fe172d538d')
  properties: {
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')
    principalId: managedIdentity.properties.principalId
  }
}

output AZURE_CONTAINER_REGISTRY_ENDPOINT string = containerRegistry.properties.loginServer
