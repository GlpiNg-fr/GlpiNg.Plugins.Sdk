# GlpiNg.Plugins.Sdk

Contracts for writing a GlpiNg plugin.

## Create a plugin

```bash
dotnet new razorclasslib -n MyPlugin
cd MyPlugin
dotnet add package GlpiNg.Plugins.Sdk
```

In `MyPlugin.csproj`, prevent the assemblies provided by the host from being copied (otherwise
two copies of the contracts would coexist):

```xml
<PackageReference Include="GlpiNg.Plugins.Sdk" Version="0.1.0" ExcludeAssets="runtime" />
```

Entry point (a single public class implementing `IGlpiNgPlugin`):

```csharp
using GlpiNg.Modules.Abstractions.Menu;
using GlpiNg.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class MyPlugin : IGlpiNgPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMenuProvider, MyMenu>();
    }
}

public sealed class MyMenu : IMenuProvider
{
    // "outils" / "Outils" stay in French: the key merges this entry into the host's "Tools"
    // group, and GlpiNg translates labels from their French text.
    public IReadOnlyList<MenuGroup> GetMenuGroups() =>
    [
        new("outils", "ti-briefcase", "Outils", [new("My plugin", "/plugins/my-plugin", "ti-puzzle")]),
    ];
}
```

A page (`Pages/Index.razor`):

```razor
@page "/plugins/my-plugin"
<h1>Hello from MyPlugin</h1>
```

## Install

```bash
dotnet publish -c Release -o <storage root>/plugins/MyPlugin
```

The host loads `plugins/<Name>/<Name>.dll` at startup (the storage root is `data/` by default):
the folder name must match the assembly name. Restart GlpiNg after installing.

## Current limitations

- No database tables managed by the host: a plugin that persists data brings its own storage.
- A plugin's static files (`wwwroot`) are not served.
- Private dependencies of different plugins share a single load context: two plugins requiring
  two versions of the same library cannot coexist.
