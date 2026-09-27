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
<PackageReference Include="GlpiNg.Plugins.Sdk" Version="0.1.0" ExcludeAssets="runtime" />
```

Point d'entrée (une seule classe publique implémentant `IGlpiNgPlugin`) :

```csharp
using GlpiNg.Modules.Abstractions.Menu;
using GlpiNg.Plugins;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public sealed class MonPlugin : IGlpiNgPlugin
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMenuProvider, MonMenu>();
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

Une page (`Pages/Index.razor`) :

```razor
@page "/plugins/mon-plugin"
<h1>Bonjour depuis MonPlugin</h1>
```

## Installer

```bash
dotnet publish -c Release -o <racine du stockage>/plugins/MonPlugin
```

L'hôte charge `plugins/<Nom>/<Nom>.dll` au démarrage (la racine du stockage est `data/` par
défaut) : le nom du dossier doit être celui de l'assembly. Redémarrez GlpiNg après installation.

## Limites actuelles

- Pas de tables en base gérées par l'hôte : un plugin qui persiste des données apporte son propre stockage.
- Les fichiers statiques (`wwwroot`) d'un plugin ne sont pas servis.
- Les dépendances privées de plugins différents partagent un même contexte de chargement : deux
  plugins exigeant deux versions d'une même bibliothèque ne peuvent pas cohabiter.

## Licence

[GNU Affero General Public License v3.0](https://github.com/GlpiNg-fr/GlpiNg.Plugins.Sdk/blob/main/LICENSE).
