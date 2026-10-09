using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GlpiNg.Plugins;

/// <summary>
/// Point d'entrée d'un plugin : l'hôte l'instancie (constructeur sans paramètre) au démarrage, ou
/// à chaud depuis Configuration &gt; Plugins, et appelle <see cref="ConfigureServices"/>.
///
/// Les services déclarés vont dans un conteneur propre au plugin, qui retombe sur ceux de l'hôte :
/// c'est ce qui permet de le charger, le mettre à jour et le décharger sans redémarrer. Ses
/// <c>IMenuProvider</c>, <c>IReportProvider</c> et <c>ICronTask</c> rejoignent ceux de l'hôte ; ses
/// composants et ses contrôleurs reçoivent ses services par leur constructeur.
///
/// Les pages <c>@page</c> de l'assembly, sous <c>/plugins/</c>, sont routées sans autre
/// déclaration ; ses contrôleurs sont découverts sans <c>AddControllers</c>.
/// </summary>
public interface IGlpiNgPlugin
{
    void ConfigureServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>
    /// Version de GlpiNg à partir de laquelle ce plugin fonctionne, incluse (« 1.2.0 »).
    /// <c>null</c> : aucune borne basse.
    ///
    /// L'hôte compare sa propre version à cet intervalle <b>avant</b> d'appeler
    /// <see cref="ConfigureServices"/>. Hors intervalle, le plugin n'est pas chargé du tout : il
    /// vaut mieux une fonctionnalité absente et signalée qu'un plugin qui s'exécute contre des
    /// contrats qu'il ne connaît pas.
    ///
    /// Seule la partie numérique compte : le suffixe de pré-version et l'empreinte de commit
    /// (« 1.2.0-RC1+abc1234 ») sont ignorés de part et d'autre.
    /// </summary>
    string? MinimumHostVersion => null;

    /// <summary>
    /// Dernière version de GlpiNg sur laquelle ce plugin a été éprouvé, incluse.
    /// <c>null</c> : aucune borne haute — le plugin accepte les versions futures.
    /// </summary>
    string? MaximumHostVersion => null;
}
