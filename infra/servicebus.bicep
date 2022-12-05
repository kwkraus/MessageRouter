//
// Create a Service Bus Namespace and Queue
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.servicebus/servicebus-create-queue
//

param resourceToken string
param location string
param tags object

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' existing = {
  name: 'mi${resourceToken}'
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' = {
  name: 'sb${resourceToken}'
  location: location
  tags: tags
  sku: {
    name: 'Basic'
  }
  resource ingressQueue 'queues' = {
    name: 'ingress'
  }
  resource egressQueue 'queues' = {
    name: 'unknown'
  }
}

// 
// Azure Service Bus Data Receiver: 4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0
// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
//
resource receiverRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(managedIdentity.id, resourceGroup().id, '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0')
  properties: {
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4f6d3b9b-027b-4f4c-9142-0e5a2a2247e0')
    principalId: managedIdentity.properties.principalId
  }
}

// 
// Azure Service Bus Data Sender: 69a216fc-b8fb-44d8-bc22-1f3c2cd27a39
// https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles
//
resource senderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: serviceBusNamespace
  name: guid(managedIdentity.id, resourceGroup().id, '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39')
  properties: {
    principalType: 'ServicePrincipal'
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '69a216fc-b8fb-44d8-bc22-1f3c2cd27a39')
    principalId: managedIdentity.properties.principalId
  }
}
