param location string
param resourceToken string
param repositoryUrl string 
param repositoryToken string 
param branch string
param tags object

resource staticWebApp 'Microsoft.Web/staticSites@2021-01-15' = {
  name: 'swa-${resourceToken}'
  location: location
  sku: {
    name: 'Free'
  }
  tags: tags
  properties: {
    repositoryUrl: repositoryUrl
    branch: branch
    repositoryToken: repositoryToken 
    buildProperties: {
      githubActionSecretNameOverride: 'AZURE_SQL_DB_FULLSTACK'
      skipGithubActionWorkflowGeneration: true
    }
  }
}

output WEB_URI string = 'https://${staticWebApp.properties.defaultHostname}'
