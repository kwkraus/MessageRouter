//
// https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.web/web-app-managed-identity-sql-db/main.bicep
//

param resourceToken string
param location string
param tags object

resource managedIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2022-01-31-preview' = {
  name: 'mi${resourceToken}'
  location: location
  tags: tags
}
