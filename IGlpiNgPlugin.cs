using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlpiNg.Plugins;

/// <summary>
/// Point d'entrée d'un plugin : l'hôte l'instancie au démarrage (constructeur sans paramètre) et
/// appelle <see cref="ConfigureServices"/> après avoir enregistré ses propres modules — un plugin
/// se branche donc exactement comme un module intégré (<c>AddXxxModule</c>) : il contribue ses
/// services et ses <c>IMenuProvider</c>, <c>IReportProvider</c>, <c>ICronTask</c>... au conteneur.
///
/// Les pages <c>@page</c> de l'assembly du plugin sont routées sans autre déclaration. Ses
/// contrôleurs, comme ceux des modules, s'enregistrent par
/// <c>services.AddControllers().AddApplicationPart(typeof(MonPlugin).Assembly)</c>.
/// </summary>
public interface IGlpiNgPlugin
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}
