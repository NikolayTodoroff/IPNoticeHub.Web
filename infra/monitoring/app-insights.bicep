/* SCOPE: Resource Group
   PROJECT: IPNoticeHub
   DESCRIPTION: Application Performance Management (APM) telemetry for the C# app.
*/

@description('The name of the Application Insights resource')
param name string = 'appi-ipnoticehub-lab-weu'

@description('The type of application being monitored')
param type string = 'web'

@description('The Azure region for the resource')
param regionId string = resourceGroup().location

@description('The resource tags')
param tagsArray object

@description('Describes what tool created this component')
param requestSource string = 'rest'

@description('The resource ID of the Log Analytics workspace to link with')
param workspaceResourceId string

// --- APPLICATION INSIGHTS DEFINITION ---
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: name
  location: regionId
  tags: tagsArray
  kind: 'web'
  properties: {
    ApplicationId: name
    Application_Type: type
    
    // 'Redfield' is the internal engine type used for workspace-based components
    Flow_Type: 'Redfield' 
    
    // Identifies the deployment source (e.g., 'rest', 'Visual Studio', 'IbizaAIExtension')
    Request_Source: requestSource
    
    // Links App Insights to Log Analytics [Mandatory for modern deployments]
    WorkspaceResourceId: workspaceResourceId
    
    // Security: Restrict access to ingestion/query if needed (Defaulting to Enabled)
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
}

output instrumentationKey string = appInsights.properties.InstrumentationKey
output connectionString string = appInsights.properties.ConnectionString
