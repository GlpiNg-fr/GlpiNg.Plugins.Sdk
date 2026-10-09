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
<PackageReference Include="GlpiNg.Plugins.Sdk" Version="0.2.0" ExcludeAssets="runtime" />
```

Target an SDK version the host already ships (GlpiNg's own or an older one): a plugin built against
a newer SDK than the host's is skipped at load time.

Entry point (a single public class implementing `IGlpiNgPlugin`):

```csharp
using GlpiNg.Modules.Abstractions.Menu;
using GlpiNg.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class MyPlugin : IGlpiNgPlugin
{
    // Range of GlpiNg versions this plugin works with. Both bounds are inclusive and optional
    // (null = no bound). Outside the range, the host does not call ConfigureServices: the plugin
    // is not loaded, and the reason shows in Configuration > Plugins.
    public string? MinimumHostVersion => "1.0.0";
    public string? MaximumHostVersion => null;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMenuProvider, MyMenu>();
        services.AddScoped<MyService>();
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

## Pages

A plugin's pages live under `/plugins/`: that is the address the host serves for a pasted link or a
reload. A plugin with a page outside `/plugins/`, or reusing a route already taken by the host or
another plugin, is skipped.

The plugin's own services are injected through the component's **constructor**. `@inject` receives
the host's services (`NavigationManager`, `IStoragePaths`...) but not the plugin's before .NET 11 —
the host warns about it at load time.

`Pages/Index.razor`:

```razor
@page "/plugins/my-plugin"
@inject NavigationManager Nav
<h1>@_message</h1>
```

`Pages/Index.razor.cs`:

```csharp
public partial class Index(MyService service)
{
    private string _message = "";
    protected override void OnInitialized() => _message = service.Hello();
}
```

## Services

`ConfigureServices` fills a container of the plugin's own. A plugin service receives through its
constructor both its own services and the host's (`ILogger<T>`, `IStoragePaths`, the `DbContext`
factory...). What reaches the host:

- `IMenuProvider`, `IReportProvider` and `ICronTask`: menu, reports and automatic actions;
- the assembly's controllers, discovered without `AddControllers`; they also receive the plugin's
  services through their constructor.

Not supported, and reported at load time: framework services registered by the plugin
(`AddHttpClient`, `AddControllers`... — it gets the host's), keyed services, open generics and
`IHostedService` (use `ICronTask`). A plugin does not replace a host service.

## Install

From **Configuration > Plugins**, upload the zip of the publish output:

```bash
dotnet publish -c Release -o publish
cd publish && zip -r ../MyPlugin.zip .
```

The plugin loads immediately, without restarting GlpiNg. Uploading a new version replaces it live:
open pages switch to the new version. The same screen disables, reloads and deletes a plugin.

You can also publish straight into the plugins folder, then click "Charger" (Load):

```bash
dotnet publish -c Release -o <storage root>/plugins/MyPlugin
```

The folder name must match the assembly name (the storage root is `data/` by default).

## Static files

A `wwwroot/` folder at the plugin's root is published next to its DLL (the package's MSBuild
targets take care of it) and served under `/_content/{AssemblyName}/` — the path ASP.NET Core gives
Razor class libraries, so a plugin that becomes one keeps its links.

```razor
<link rel="stylesheet" href="_content/MyPlugin/css/my-plugin.css" />
```

Only `wwwroot/` is exposed: the plugin's DLL and dependencies, one level up, cannot be requested.
Setting `<GlpiNgPluginStaticAssets>false</GlpiNgPluginStaticAssets>` disables the copy if the project
handles its files otherwise.

## Current limitations

- No database tables managed by the host: a plugin that persists data brings its own storage.
- An unloaded plugin disappears immediately, but its memory is only released at restart once one of
  its pages has been displayed: Blazor caches component types.

## License

[GNU Affero General Public License v3.0](https://github.com/GlpiNg-fr/GlpiNg.Plugins.Sdk/blob/main/LICENSE).
