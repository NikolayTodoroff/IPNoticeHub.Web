/*
  Virtual Machine Deployment (Reusable)

  Purpose:
  - Deploys a Linux VM with Trusted Launch + SystemAssigned MI
  - Creates NIC (+ optional Public IP) and attaches to existing VNet/Subnet
  - Uses SSH key auth (no passwords)
  - Reusable across environments via parameters

  Scope:
  - Resource Group
*/

targetScope = 'resourceGroup'

@description('Azure region for resources')
param location string = resourceGroup().location

@description('Base tags applied to resources')
param tags object = {}

@description('VM name')
param vmName string = 'vm-ipnoticehub-dev-weu'

@description('VM size / SKU')
param vmSize string = 'Standard_D2s_v3'

@description('Admin username for the VM')
param adminUsername string = 'vmUser'

@description('SSH public key (ed25519 or rsa). Example: ssh-ed25519 AAAA... user@host')
param sshPublicKey string

@description('Existing VNet name where VM will be placed')
param vnetName string = 'vn-ipnoticehub-dev-weu'

@description('Existing subnet name inside the VNet')
param subnetName string = 'snet-workload'

@description('NIC name (default derived)')
param nicName string = '${vmName}-nic'

@description('Whether to create a public IP and attach it to the NIC')
param enablePublicIp bool = false

@description('Public IP name (default derived)')
param publicIpName string = '${vmName}-pip'

@description('OS image publisher')
param imagePublisher string = 'Canonical'

@description('OS image offer')
param imageOffer string = 'ubuntu-24_04-lts'

@description('OS image sku')
param imageSku string = 'server'

@description('OS image version')
param imageVersion string = 'latest'

@description('OS Disk size in GB')
@minValue(30)
param osDiskSizeGb int = 30

@description('OS Disk storage SKU')
@allowed([
  'Premium_LRS'
  'StandardSSD_LRS'
  'Standard_LRS'
])
param osDiskSku string = 'StandardSSD_LRS'

@description('Enable Trusted Launch (Secure Boot + vTPM)')
param enableTrustedLaunch bool = true


// ---- Existing network references ----
resource vnet 'Microsoft.Network/virtualNetworks@2024-05-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2024-05-01' existing = {
  parent: vnet
  name: subnetName
}


// ---- Optional Public IP ----
resource pip 'Microsoft.Network/publicIPAddresses@2024-05-01' = if (enablePublicIp) {
  name: publicIpName
  location: location
  tags: union(tags, { purpose: 'vm' })
  sku: {
    name: 'Standard'
  }
  properties: {
    publicIPAllocationMethod: 'Static'
    // If you ever need a DNS label: add "dnsSettings: { domainNameLabel: ... }"
  }
}


// ---- NIC ----
resource nic 'Microsoft.Network/networkInterfaces@2024-05-01' = {
  name: nicName
  location: location
  tags: union(tags, { purpose: 'vm' })
  properties: {
    ipConfigurations: [
      {
        name: 'ipconfig1'
        properties: {
          subnet: {
            id: subnet.id
          }
          privateIPAllocationMethod: 'Dynamic'
          publicIPAddress: enablePublicIp ? {
            id: pip.id
          } : null
        }
      }
    ]
    // If you have an NSG, attach it here:
    // networkSecurityGroup: { id: nsg.id }
  }
}


// ---- VM ----
resource vm 'Microsoft.Compute/virtualMachines@2024-11-01' = {
  name: vmName
  location: location
  tags: union(tags, { purpose: 'vm' })
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    hardwareProfile: {
      vmSize: vmSize
    }

    storageProfile: {
      imageReference: {
        publisher: imagePublisher
        offer: imageOffer
        sku: imageSku
        version: imageVersion
      }
      osDisk: {
        name: '${vmName}-osdisk'
        createOption: 'FromImage'
        caching: 'ReadWrite'
        diskSizeGB: osDiskSizeGb
        managedDisk: {
          storageAccountType: osDiskSku
        }
        deleteOption: 'Delete'
      }
    }

    osProfile: {
      computerName: vmName
      adminUsername: adminUsername
      linuxConfiguration: {
        disablePasswordAuthentication: true
        ssh: {
          publicKeys: [
            {
              path: '/home/${adminUsername}/.ssh/authorized_keys'
              keyData: sshPublicKey
            }
          ]
        }
        provisionVMAgent: true
        patchSettings: {
          patchMode: 'ImageDefault'
          assessmentMode: 'AutomaticByPlatform'
        }
      }
      allowExtensionOperations: true
      requireGuestProvisionSignal: true
    }

    securityProfile: enableTrustedLaunch ? {
      securityType: 'TrustedLaunch'
      uefiSettings: {
        secureBootEnabled: true
        vTpmEnabled: true
      }
    } : null

    networkProfile: {
      networkInterfaces: [
        {
          id: nic.id
          properties: {
            // For typical lifecycle, delete NIC with VM
            deleteOption: 'Delete'
          }
        }
      ]
    }

    diagnosticsProfile: {
      bootDiagnostics: {
        enabled: true
        // Managed boot diagnostics (no storage account URI) is fine.
      }
    }
  }
}


// ---- Optional: Network Watcher Agent extension (usually not required anymore) ----
@description('Install the Network Watcher agent extension')
param installNetworkWatcherExtension bool = false

resource networkWatcherExtension 'Microsoft.Compute/virtualMachines/extensions@2024-11-01' = if (installNetworkWatcherExtension) {
  parent: vm
  name: 'AzureNetworkWatcherExtension'
  location: location
  tags: union(tags, { purpose: 'vm' })
  properties: {
    autoUpgradeMinorVersion: true
    publisher: 'Microsoft.Azure.NetworkWatcher'
    type: 'NetworkWatcherAgentLinux'
    typeHandlerVersion: '1.4'
  }
}


// ---- Outputs ----
output vmId string = vm.id
output nicId string = nic.id
output privateIp string = nic.properties.ipConfigurations[0].properties.privateIPAddress
output publicIp string = enablePublicIp ? pip.properties.ipAddress : ''
