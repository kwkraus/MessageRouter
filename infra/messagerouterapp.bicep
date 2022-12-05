//
// https://github.com/Azure-Samples/pubsub-dapr-csharp-servicebus/blob/main/infra/orders.bicep
// https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.app/container-app-acr/main.bicep
//

param name string
param location string
param imageName string

var resourceToken = toLower(uniqueString(subscription().id, name, location))
var tags = {
  'azd-env-name': name
}

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' existing = {
  name: 'mi${resourceToken}'
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' existing = {  
  name: 'st${resourceToken}'
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2022-06-01-preview' existing = {
  name: 'cae${resourceToken}'
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-02-01-preview' existing = {
  name: 'acr${resourceToken}'
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2022-01-01-preview' existing = {
  name: 'sb${resourceToken}'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'ai${resourceToken}'
}

resource messageRouterApp 'Microsoft.App/containerApps@2022-06-01-preview' = {
  name: '${name}messagerouterapp'
  location: location
  tags: tags
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${managedIdentity.id}': {}
    }
  }
  properties: {
    managedEnvironmentId: containerAppEnvironment.id
    configuration: {
      ingress: {
        external: true
        targetPort: 5075
        transport: 'auto'
      }
      registries: [
        {
          identity: managedIdentity.id
          server: containerRegistry.properties.loginServer
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'messageroutercontainer'
          image: imageName
          env: [
            {
              name: 'AZURE_CLIENT_ID'
              value: managedIdentity.properties.clientId
            }
            {
              name: 'AzureDefaults__Retry__MaxRetries'
              value: '3'
            }
            {
              name: 'AzureDefaults__Retry__Mode'
              value: 'Exponential'
            }
            {
              name: 'ServiceBus__FullyQualifiedNamespace'
              value: '${serviceBusNamespace.name}.servicebus.windows.net'
            }
            {
              name: 'MessageRouter__IngressQueue'
              value: 'ingress'
            }
            {
              name: 'MessageRouter__WorkflowFile'
              value: '/app/config/workflow.json'
            }
            {
              name: 'MessageRouter__SchemaDirectory'
              value: '/app/config/schemas'
            }
            {
              name: 'ApplicationInsights__ConnectionString'
              value: appInsights.properties.ConnectionString
            }
          ]
          volumeMounts: [
            {
              volumeName: 'volume01'
              mountPath: '/app/config'
            }
          ]
          resources: {
            cpu: json('.25')
            memory: '.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 1
      }
      volumes: [
        {
          name: 'volume01'
          storageName: 'st${uniqueString(storageAccount.name, 'sh${resourceToken}')}'
          storageType: 'AzureFile'
        }
      ]
    }
  }
}

output MESSAGEROUTERAPP_URI string = 'https://${messageRouterApp.properties.configuration.ingress.fqdn}'
