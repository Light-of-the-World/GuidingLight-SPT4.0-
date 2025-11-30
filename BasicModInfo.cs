using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Reflection;
using WTTServerCommonLib;
using Path = System.IO.Path;

namespace GuidingLight;

// This record holds the various properties for your mod
public record ModMetadata : AbstractModMetadata
{
    public override string ModGuid { get; init; } = "com.lightoftheworld.guidinglight";
    public override string Name { get; init; } = "GuidingLight";
    public override string Author { get; init; } = "LightoftheWorld";
    public override List<string>? Contributors { get; init; }
    public override SemanticVersioning.Version Version { get; init; } = new("1.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; } = ["ReadJsonConfigExample"];
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; }
    public override string? Url { get; init; } = "https://github.com/sp-tarkov/server-mod-examples";
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";
}

/// <summary>
/// Feel free to use this as a base for your mod
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class BasicModInfo(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    TimeUtil timeUtil,
    GLAddCustomTraderHelper addCustomTraderHelper, // This is a custom class we add for this mod, we made it injectable so it can be accessed like other classes here
    WTTServerCommonLib.WTTServerCommonLib wttCommon
)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();


    public Task OnLoad()
    {
        // Get your current assembly (WTT)
        var assembly = Assembly.GetExecutingAssembly();

        // A path to the mods files we use below
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        // A relative path to the trader icon to show
        var GLImagePath = Path.Combine(pathToMod, "data/GL/Staragainstblack.png");

        // The base json containing trader settings we will add to the server
        var GLBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/GL/GL.json");//
        //bruh

        // Create a helper class and use it to register our traders image/icon + set its stock refresh time
        imageRouter.AddRoute(GLBase.Avatar.Replace(".png", ""), GLImagePath);
        addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, GLBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));

        // Add our trader to the config file, this lets it be seen by the flea market
        _ragfairConfig.Traders.TryAdd(GLBase.Id, true);
        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(GLBase);

        // Add localisation text for our trader to the database so it shows to people playing in different languages
        addCustomTraderHelper.AddTraderToLocales(GLBase, "Guiding Light", "One of the celestial beings watching over the Tarkov conflict. All you really know about it is that it seems to have a higher-pitched voice compared to the other one.");

        // Get the assort data from JSON
        var GLassort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "data/GL/GLassort.json");

        // Save the data we loaded above into the trader we've made
        addCustomTraderHelper.OverwriteTraderAssort(GLBase.Id, GLassort);

        //Copy what we did above for the second trader, FL.
        var CLImagePath = Path.Combine(pathToMod, "data/CL/CLIcon.jpg");
        var CLBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/CL/CL.json");
        imageRouter.AddRoute(CLBase.Avatar.Replace(".jpg", ""), CLImagePath);
        addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, CLBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));
        _ragfairConfig.Traders.TryAdd(CLBase.Id, true);
        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(CLBase);
        addCustomTraderHelper.AddTraderToLocales(CLBase, "Curious Light", "One of the celestial beings watching over the Tarkov conflict. All you really know about it is that it seems to have a lower-pitched voice compared to the other one.");
        var CLassort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "data/CL/CLassort.json");
        addCustomTraderHelper.OverwriteTraderAssort(CLBase.Id, CLassort);

        // Send back a success to the server to say our trader is good to go
        return Task.CompletedTask;
    }
}