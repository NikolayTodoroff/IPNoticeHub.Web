/*
  App Service Diagnostic Settings Policy Definition Assignment

  Purpose:
  - Assigns a policy that ensures App Services have diagnostic settings configured to send metrics to a specified Log Analytics workspace.

  Scope:
  - Subscription
*/

targetScope = 'subscription'

@description('Name for the custom policy definition.')
param policyDefinitionName string = 'app-service-diag-settings'

@description('Resource ID of the Log Analytics workspace that will receive App Service logs/metrics.')
param logAnalytics string

@allowed(['DeployIfNotExists','Disabled'])
param effect string = 'DeployIfNotExists'

@description('Diagnostic settings profile name.')
param profileName string = 'AppServiceDiagnostics'

@description('Enable App Service logs.')
@allowed(['True','False'])
param logsEnabled string = 'True'

@description('Enable App Service metrics (AllMetrics).')
@allowed(['True','False'])
param metricsEnabled string = 'True'

@description('If true, require workspaceId to match the selected workspace.')
param matchWorkspace bool = true

// Policy definition to deploy App Service diagnostic settings
resource appServiceDiagPolicy 'Microsoft.Authorization/policyDefinitions@2025-01-01' = {
  name: policyDefinitionName
  properties: {
    displayName: 'Deploy Diagnostic Settings for App Service to Log Analytics workspace'
    description: 'Deploys diagnostic settings for App Service (Console/App logs + AllMetrics) to a Log Analytics workspace when missing.'
    policyType: 'Custom'
    mode: 'Indexed'
    metadata: {
      category: 'Monitoring'
      version: '1.0.0'
    }

    parameters: {
      effect: {
        type: 'String'
        metadata: {
          displayName: 'Effect'
          description: 'Enable or disable the execution of the policy'
        }
        allowedValues: [
          'DeployIfNotExists'
          'Disabled'
        ]
        defaultValue: effect
      }

      profileName: {
        type: 'String'
        metadata: {
          displayName: 'App Service Diagnostic Profile'
          description: 'The diagnostic settings profile name'
        }
        defaultValue: profileName
      }

      logAnalytics: {
        type: 'String'
        metadata: {
          displayName: 'Log Analytics workspace'
          description: 'Target Log Analytics workspace resource ID'
          strongType: 'omsWorkspace'
          assignPermissions: true
        }
        defaultValue: logAnalytics
      }

      logsEnabled: {
        type: 'String'
        metadata: {
          displayName: 'Enable logs'
          description: 'Whether to enable App Service logs'
        }
        allowedValues: [
          'True'
          'False'
        ]
        defaultValue: logsEnabled
      }

      metricsEnabled: {
        type: 'String'
        metadata: {
          displayName: 'Enable metrics'
          description: 'Whether to enable App Service metrics'
        }
        allowedValues: [
          'True'
          'False'
        ]
        defaultValue: metricsEnabled
      }

      matchWorkspace: {
        type: 'Boolean'
        metadata: {
          displayName: 'Log Analytics Workspace Id must match'
          description: 'Whether to require the workspace of the diagnostic settings matches the one deployed by this policy'
        }
        allowedValues: [
          true
          false
        ]
        defaultValue: matchWorkspace
      }
    }

    policyRule: {
      if: {
        field: 'type'
        equals: 'Microsoft.Web/sites'
      }
      then: {
        effect: '[parameters(\'effect\')]'
        details: {
          type: 'Microsoft.Insights/diagnosticSettings'
          name: '[parameters(\'profileName\')]'
          evaluationDelay: 'AfterProvisioning'

          // Check whether the required diagnostic setting already exists & matches expectations
          existenceCondition: {
            allOf: [
              // AppServiceConsoleLogs enabled must match desired state
              {
                count: {
                  field: 'Microsoft.Insights/diagnosticSettings/logs[*]'
                  where: {
                    allOf: [
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/logs[*].category'
                        equals: 'AppServiceConsoleLogs'
                      }
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/logs[*].enabled'
                        equals: '[equals(parameters(\'logsEnabled\'), \'True\')]'
                      }
                    ]
                  }
                }
                greaterOrEquals: 1
              }

              // AppServiceAppLogs enabled must match desired state
              {
                count: {
                  field: 'Microsoft.Insights/diagnosticSettings/logs[*]'
                  where: {
                    allOf: [
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/logs[*].category'
                        equals: 'AppServiceAppLogs'
                      }
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/logs[*].enabled'
                        equals: '[equals(parameters(\'logsEnabled\'), \'True\')]'
                      }
                    ]
                  }
                }
                greaterOrEquals: 1
              }

              // AllMetrics enabled must match desired state
              {
                count: {
                  field: 'Microsoft.Insights/diagnosticSettings/metrics[*]'
                  where: {
                    allOf: [
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/metrics[*].category'
                        equals: 'AllMetrics'
                      }
                      {
                        field: 'Microsoft.Insights/diagnosticSettings/metrics[*].enabled'
                        equals: '[equals(parameters(\'metricsEnabled\'), \'True\')]'
                      }
                    ]
                  }
                }
                greaterOrEquals: 1
              }

              // Workspace must match (optional strictness)
              {
                anyOf: [
                  {
                    value: '[parameters(\'matchWorkspace\')]'
                    equals: false
                  }
                  {
                    field: 'Microsoft.Insights/diagnosticSettings/workspaceId'
                    equals: '[parameters(\'logAnalytics\')]'
                  }
                ]
              }
            ]
          }

          // Monitoring Contributor (enables writing diagnostic settings)
          roleDefinitionIds: [
            '/providers/Microsoft.Authorization/roleDefinitions/749f88d5-cbae-40b8-bcfc-e573ddc772fa'
          ]

          // Deployment details for the diagnostic settings
          deployment: {
            properties: {
              mode: 'incremental'
              template: {
                '$schema': 'https://schema.management.azure.com/schemas/2019-04-01/deploymentTemplate.json#'
                contentVersion: '1.0.0.0'
                parameters: {
                  resourceName: { type: 'string' }
                  profileName: { type: 'string' }
                  logAnalytics: { type: 'string' }

                  // IMPORTANT: these remain string to match policy params, we convert to bool in the resource properties
                  logsEnabled: { type: 'string' }
                  metricsEnabled: { type: 'string' }
                }
                resources: [
                  {
                    type: 'Microsoft.Insights/diagnosticSettings'
                    apiVersion: '2021-05-01-preview'
                    name: '[parameters(\'profileName\')]'
                    scope: '[resourceId(\'Microsoft.Web/sites\', parameters(\'resourceName\'))]'
                    properties: {
                      workspaceId: '[parameters(\'logAnalytics\')]'
                      logs: [
                        {
                          category: 'AppServiceConsoleLogs'
                          enabled: '[equals(parameters(\'logsEnabled\'), \'True\')]'
                        }
                        {
                          category: 'AppServiceAppLogs'
                          enabled: '[equals(parameters(\'logsEnabled\'), \'True\')]'
                        }
                      ]
                      metrics: [
                        {
                          category: 'AllMetrics'
                          enabled: '[equals(parameters(\'metricsEnabled\'), \'True\')]'
                        }
                      ]
                    }
                  }
                ]
              }

              parameters: {
                resourceName: { value: '[field(\'name\')]' }
                profileName: { value: '[parameters(\'profileName\')]' }
                logAnalytics: { value: '[parameters(\'logAnalytics\')]' }
                logsEnabled: { value: '[parameters(\'logsEnabled\')]' }
                metricsEnabled: { value: '[parameters(\'metricsEnabled\')]' }
              }
            }
          }
        }
      }
    }
  }
}

output appServicePolicyId string = appServiceDiagPolicy.id
