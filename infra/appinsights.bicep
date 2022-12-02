//
// https://github.com/Azure-Samples/pubsub-dapr-csharp-servicebus/blob/main/infra/appinsights.bicep
//

param resourceToken string
param location string
param tags object

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'log${resourceToken}'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'ai${resourceToken}'
  location: location
  tags: tags
  kind: 'other'
  properties: {
    Application_Type: 'other'
    WorkspaceResourceId: logAnalyticsWorkspace.id
  }
}
