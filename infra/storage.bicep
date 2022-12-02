//
// https://github.com/Azure/azure-quickstart-templates/tree/master/quickstarts/microsoft.storage/storage-file-share
// https://learn.microsoft.com/en-us/azure/container-apps/storage-mounts-azure-files?tabs=bash
//

param resourceToken string
param location string
param tags object
param scriptName string = 'psf${utcNow()}'

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-05-01' = {  
  name: 'st${resourceToken}'
  location: location
  tags: tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    accessTier: 'Hot'
  }
  resource fileServices 'fileServices' = {
    name: 'default'
    resource share 'shares' = {
      name: 'sh${resourceToken}'
      properties: {
        shareQuota: 1024
        enabledProtocols: 'SMB'
      }
    }
  }
}

resource prepareShareFolders 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: scriptName
  location: location
  tags: tags
  kind: 'AzureCLI'
  properties: {
    azCliVersion: '2.5.0'
    cleanupPreference: 'OnSuccess'
    retentionInterval: 'P1D'
    environmentVariables: [
      {
        name: 'SHARE_NAME'
        value: 'sh${resourceToken}'
      }
      {
        name: 'AZURE_STORAGE_ACCOUNT'
        value: storageAccount.name
      }
      {
        name: 'AZURE_STORAGE_KEY'
        value: storageAccount.listKeys().keys[0].value
      }
    ]
    scriptContent: loadTextContent('storage.sh')
  }
}
