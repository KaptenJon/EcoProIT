# .NET 10 Upgrade Plan

## Execution Steps

Execute steps below sequentially one by one in the order they are listed.

1. Validate that a .NET 10 SDK required for this upgrade is installed on the machine and if not, help to get it installed.
2. Ensure that the SDK version specified in global.json files is compatible with the .NET 10 upgrade.
3. Upgrade HelpClasses\HelpClasses.csproj
4. Upgrade DESEngine\DESEngine.csproj
5. Upgrade IntelliBox\IntelliBox.csproj
6. Upgrade EcoProIT.DataLayer\EcoProIT.DataLayer.csproj
7. Upgrade EcoProIT.UserControles\EcoProIT.UserControles.csproj
8. Upgrade Chart\Chart.csproj
9. Upgrade EcoProIT.UI\EcoProIT.UI.csproj

## Settings

This section contains settings and data used by execution steps.

### Aggregate NuGet packages modifications across all projects

NuGet packages used across all selected projects or their dependencies that need version update in projects that reference them.

| Package Name                        | Current Version | New Version | Description                                                            |
|:------------------------------------|:---------------:|:-----------:|:-----------------------------------------------------------------------|
| Extended.Wpf.Toolkit                |   3.5.0         |  5.0.0      | Required for .NET 10                                                   |
| Microsoft.SqlServer.Compact         |   4.0.8876.1    |             | No supported version found - needs replacement or removal             |
| MvvmLightLibs                       |   5.4.1.1       |             | Deprecated and incompatible - replace with CommunityToolkit.Mvvm      |

### Project upgrade details

This section contains details about each project upgrade and modifications that need to be done in the project.

#### HelpClasses\HelpClasses.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

#### DESEngine\DESEngine.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0`

#### IntelliBox\IntelliBox.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

NuGet packages changes:
  - MvvmLightLibs should be removed (deprecated and no supported version found)
  - Consider replacing with CommunityToolkit.Mvvm

#### EcoProIT.DataLayer\EcoProIT.DataLayer.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

NuGet packages changes:
  - Microsoft.SqlServer.Compact (4.0.8876.1) - no supported version found, needs replacement or removal

#### EcoProIT.UserControles\EcoProIT.UserControles.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

NuGet packages changes:
  - Extended.Wpf.Toolkit should be updated from `3.5.0` to `5.0.0` (*required for .NET 10*)
  - MvvmLightLibs should be removed (deprecated and no supported version found)
  - Consider replacing with CommunityToolkit.Mvvm

#### Chart\Chart.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

NuGet packages changes:
  - MvvmLightLibs should be removed (deprecated and no supported version found)
  - Consider replacing with CommunityToolkit.Mvvm

#### EcoProIT.UI\EcoProIT.UI.csproj modifications

Project properties changes:
  - Project file needs to be converted to SDK-style
  - Target framework should be changed from `net472` to `net10.0-windows`

NuGet packages changes:
  - Extended.Wpf.Toolkit should be updated from `3.5.0` to `5.0.0` (*required for .NET 10*)
  - Microsoft.SqlServer.Compact (4.0.8876.1) - no supported version found, needs replacement or removal
  - MvvmLightLibs should be removed (deprecated and no supported version found)
  - Consider replacing with CommunityToolkit.Mvvm
