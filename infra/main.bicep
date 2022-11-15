targetScope = 'subscription'

@minLength(1)
@maxLength(50)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param name string

@minLength(1)
@description('Primary location for all resources')
param location string

@minLength(1)
@description('URL of the GitHub repository to be used for deployment')
param repositoryUrl string

@minLength(1)
@description('Branch to be used for deployment')
param branch string

@minLength(1)
@description('GitHub access token to allow SWA to connect to GitHub repository')
param repositoryToken string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2020-06-01' = {
    name: 'rg-fullstack-${name}'
    location: location
    tags: tags
}

var resourceToken = toLower(uniqueString(subscription().id, name))
var tags = {
    'azd-env-name': name
}

module resources './resources.bicep' = {
    name: 'resources-${resourceToken}'
    scope: resourceGroup
    params: {
        location: location
        resourceToken: resourceToken
        repositoryUrl: repositoryUrl
        branch: branch
        repositoryToken: repositoryToken    
        tags: tags    
    }
}

output APP_WEB_BASE_URL string = resources.outputs.WEB_URI
output AZURE_LOCATION string = location
