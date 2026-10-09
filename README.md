# GlpiNg.Plugins.Sdk

Contrats pour écrire un plugin GlpiNg.

## Créer un plugin

```bash
dotnet new razorclasslib -n MonPlugin
cd MonPlugin
dotnet add package GlpiNg.Plugins.Sdk
```

Dans `MonPlugin.csproj`, empêchez la copie des assemblies fournies par l'hôte (sinon deux
copies des contrats coexisteraient) :

```xml
<PackageReference Include="GlpiNg.Plugins.Sdk" Version="0.2.0" ExcludeAssets="runtime" />
```

Visez une version du SDK que l'hôte possède déjà (celle de GlpiNg ou une plus ancienne) : un plugin
compilé contre un SDK plus récent que celui de l'hôte est écarté au chargement.

Point d'entrée (une seule classe publique implémentant `IGlpiNgPlugin`) :

```csharp
using GlpiNg.Modules.Abstractions.Menu;
using GlpiNg.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class MonPlugin : IGlpiNgPlugin
{
    // Intervalle de versions de GlpiNg sur lequel ce plugin fonctionne. Les deux bornes sont
    // incluses et facultatives (null = pas de borne). Hors intervalle, l'hôte n'appelle pas
    // ConfigureServices : le plugin n'est pas chargé, et la raison s'affiche dans
    // Configuration > Plugins.
    public string? MinimumHostVersion => "1.0.0";
    public string? MaximumHostVersion => null;

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMenuProvider, MonMenu>();
        services.AddScoped<MonService>();
    }
}

public sealed class MonMenu : IMenuProvider
{
    public IReadOnlyList<MenuGroup> GetMenuGroups() =>
    [
        new("outils", "ti-briefcase", "Outils", [new("Mon plugin", "/plugins/mon-plugin", "ti-puzzle")]),
    ];
}
```

## Pages

Les pages d'un plugin vivent sous `/plugins/` : c'est cette adresse que l'hôte sert pour un lien
collé ou un rechargement. Un plugin dont une page sort de `/plugins/`, ou reprend une route déjà
prise par l'hôte ou par un autre plugin, est écarté.

Les services du plugin s'injectent par le **constructeur** du composant. `@inject` reçoit les
services de l'hôte (`NavigationManager`, `IStoragePaths`...) mais pas ceux du plugin avant .NET 11 —
l'hôte le signale au chargement.

`Pages/Index.razor` :

```razor
@page "/plugins/mon-plugin"
@inject NavigationManager Nav
<h1>@_message</h1>
```

`Pages/Index.razor.cs` :

```csharp
public partial class Index(MonService service)
{
    private string _message = "";
    protected override void OnInitialized() => _message = service.Bonjour();
}
```

## Services

`ConfigureServices` remplit un conteneur propre au plugin. Un service du plugin reçoit par son
constructeur ses propres services et ceux de l'hôte (`ILogger<T>`, `IStoragePaths`, la fabrique de
`DbContext`...). Ce qui rejoint l'hôte :

- `IMenuProvider`, `IReportProvider` et `ICronTask` : menu, rapports et actions automatiques ;
- les contrôleurs de l'assembly, découverts sans `AddControllers` ; ils reçoivent eux aussi les
  services du plugin par leur constructeur.

Ne sont pas pris en charge, et signalés au chargement : les services du framework enregistrés par le
plugin (`AddHttpClient`, `AddControllers`... — il reçoit ceux de l'hôte), les services keyed, les
génériques ouverts et `IHostedService` (utilisez `ICronTask`). Un plugin ne remplace pas un service
de l'hôte.

## Installer

Depuis **Configuration > Plugins**, déposez l'archive zip de la sortie de publication :

```bash
dotnet publish -c Release -o publish
cd publish && zip -r ../MonPlugin.zip .
```

Le plugin est chargé aussitôt, sans redémarrer GlpiNg. Déposer une nouvelle version la remplace à
chaud : les pages ouvertes passent sur la nouvelle version. Le même écran désactive, recharge et
supprime un plugin.

On peut aussi publier directement dans le dossier des plugins, puis cliquer sur « Charger » :

```bash
dotnet publish -c Release -o <racine du stockage>/plugins/MonPlugin
```

Le nom du dossier doit être celui de l'assembly (la racine du stockage est `data/` par défaut).

## Fichiers statiques

Un dossier `wwwroot/` à la racine du plugin est publié à côté de sa DLL (les cibles MSBuild du
paquet s'en chargent) et servi sous `/_content/{NomDeLAssembly}/` — le même chemin qu'ASP.NET Core
donne aux bibliothèques de classes Razor, pour qu'un plugin qui en deviendrait une garde ses liens.

```razor
<link rel="stylesheet" href="_content/MonPlugin/css/mon-plugin.css" />
```

Seul `wwwroot/` est exposé : la DLL du plugin et ses dépendances, au niveau au-dessus, ne sont pas
accessibles par une requête. Poser `<GlpiNgPluginStaticAssets>false</GlpiNgPluginStaticAssets>`
désactive la copie si le projet gère ses fichiers autrement.

## Limites actuelles

- Pas de tables en base gérées par l'hôte : un plugin qui persiste des données apporte son propre stockage.
- Un plugin déchargé disparaît aussitôt, mais sa mémoire n'est rendue qu'au redémarrage dès qu'une de
  ses pages a été affichée : Blazor garde en cache les types de composants.

## Licence

[GNU Affero General Public License v3.0](https://github.com/GlpiNg-fr/GlpiNg.Plugins.Sdk/blob/main/LICENSE).
