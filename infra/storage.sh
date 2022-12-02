#!/bin/bash
echo '{"WorkflowName":"Workflow01","Rules":[{"RuleName":"Rule01","SuccessEvent":"Rule01","ErrorMessage":"Not Rule01","Expression":"false"}]}' > workflow.json
az storage directory create --account-key $AZURE_STORAGE_KEY --account-name $AZURE_STORAGE_ACCOUNT --share-name $SHARE_NAME --name schemas --output none
az storage file upload --account-key $AZURE_STORAGE_KEY --account-name $AZURE_STORAGE_ACCOUNT --share-name $SHARE_NAME --source workflow.json