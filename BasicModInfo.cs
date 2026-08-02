using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services.Mod;
using SPTarkov.Server.Core.Utils;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
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
    public override SemanticVersioning.Version Version { get; init; } = new("2.0.0");
    public override SemanticVersioning.Range SptVersion { get; init; } = new("~4.0.0");
    public override List<string>? Incompatibilities { get; init; }
    public override Dictionary<string, SemanticVersioning.Range>? ModDependencies { get; init; } = new()
    {
        {"com.wtt.commonlib", new SemanticVersioning.Range(">=2.0.0") },
        {"com.lightoftheworld.questsextended", new SemanticVersioning.Range(">=4.0.2") }
    };
    public override string? Url { get; init; } = "https://github.com/Light-of-the-World/GuidingLight-SPT4.0-";
    public override bool? IsBundleMod { get; init; } = false;
    public override string? License { get; init; } = "MIT";

    //I generate ids here. 6a6ae1a7015427276b4de501
}

/// <summary>
/// Feel free to use this as a base for your mod
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class BasicModInfo(
    ModHelper modHelper,
    ImageRouter imageRouter,
    ConfigServer configServer,
    InventoryConfig inventoryConfig,
    TimeUtil timeUtil,
    GLAddCustomTraderHelper addCustomTraderHelper, // This is a custom class we add for this mod, we made it injectable so it can be accessed like other classes here
    WTTServerCommonLib.WTTServerCommonLib wttCommon,
    CustomItemService customItemService
)
    : IOnLoad
{
    private readonly TraderConfig _traderConfig = configServer.GetConfig<TraderConfig>();
    private readonly RagfairConfig _ragfairConfig = configServer.GetConfig<RagfairConfig>();
    private ItemConfig itemConfig = configServer.GetConfig<ItemConfig>();


    public Task OnLoad()
    {
        // Get your current assembly (WTT)
        var assembly = Assembly.GetExecutingAssembly();

        // A path to the mods files we use below
        var pathToMod = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

        // A relative path to the trader icon to show
        var GLImagePath = Path.Combine(pathToMod, "data/GL/Staragainstblack.png");

        // The base json containing trader settings we will add to the server
        var GLBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/GL/GL.json");
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
        addCustomTraderHelper.OverwriteTraderAssort(GLBase.Id, GLassort); //6957497de9ab90680adc76e9, 695749a1e9ab90680adc76ea

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

        //And once more for the Cultist

        var CultImagePath = Path.Combine(pathToMod, "data/Cultist/CultistIcon.jpg");
        var CultBase = modHelper.GetJsonDataFromFile<TraderBase>(pathToMod, "data/Cultist/Cultist.json");
        imageRouter.AddRoute(CultBase.Avatar.Replace(".jpg", ""), CultImagePath);
        addCustomTraderHelper.SetTraderUpdateTime(_traderConfig, CultBase, timeUtil.GetHoursAsSeconds(1), timeUtil.GetHoursAsSeconds(2));
        _ragfairConfig.Traders.TryAdd(CultBase.Id, true);
        addCustomTraderHelper.AddTraderWithEmptyAssortToDb(CultBase);
        addCustomTraderHelper.AddTraderToLocales(CultBase, "Sektant", "One of the cultist followers who acts as their mouthpiece to (and for) you. You've killed him before, yet here he is.");
        var Cultassort = modHelper.GetJsonDataFromFile<TraderAssort>(pathToMod, "data/Cultist/Cultistassort.json");
        addCustomTraderHelper.OverwriteTraderAssort(CultBase.Id, Cultassort);

        //Below here is item creation!
        wttCommon.CustomBuffService.CreateCustomBuffs(assembly);
        List<NewItemFromCloneDetails> items = new List<NewItemFromCloneDetails>();
        var DDS = new NewItemFromCloneDetails
        {
            ItemTplToClone = ItemTpl.STIM_ADRENALINE_INJECTOR,
            // ParentId refers to the Node item the gun will be under, you can check it in https://db.sp-tarkov.com/search
            ParentId = "5448f3a64bdc2d60728b456a",
            // The new id of our cloned item - MUST be a valid mongo id, search online for mongo id generators
            NewId = "66dd0c09edd01e906e7f628f",
            // Flea price of item
            FleaPriceRoubles = 50000,
            // Price of item in handbook
            HandbookPriceRoubles = 42500,
            // Handbook Parent Id refers to the category the gun will be under
            HandbookParentId = "5b5f6fa186f77409407a7eb7",
            //you see those side box tab thing that only select gun under specific icon? Handbook parent can be found in Spt_Data\Server\database\templates.
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = "Mark of the Beast",
                        ShortName = "DD",
                        Description = "An incredibly powerful stimulant, infusing the user with demonic power. How this was even turned into an injectible is unknown. It doesn't seem to have been a perfect process either, as the survivability of the patient is not guarunteed. However, that chance is small. The real cost is ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~",
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                CanSellOnRagfair = false,
                SpawnChance = 0,
                StimulatorBuffs = "Buffs_damned_drug_buffs",
                BackgroundColor = "red"
            },
        };
        items.Add(DDS);
        var Barricade = new NewItemFromCloneDetails
        {
            ItemTplToClone = ItemTpl.BARTER_PHASED_ARRAY_ELEMENT,
            // ParentId refers to the Node item the gun will be under, you can check it in https://db.sp-tarkov.com/search
            ParentId = "5795f317245977243854e041",
            // The new id of our cloned item - MUST be a valid mongo id, search online for mongo id generators
            NewId = "67ff16474665a93b4a3b4150",
            // Flea price of item
            FleaPriceRoubles = 50000,
            // Price of item in handbook
            HandbookPriceRoubles = 42500,
            // Handbook Parent Id refers to the category the gun will be under
            HandbookParentId = "5b5f6fa186f77409407a7eb7",
            //you see those side box tab thing that only select gun under specific icon? Handbook parent can be found in Spt_Data\Server\database\templates.
            Locales = new Dictionary<string, LocaleDetails>
            {
                {
                    "en", new LocaleDetails
                    {
                        Name = "Metal Barricade",
                        ShortName = "Barricade",
                        Description = "A metal barricade acquired from Ragman, for the purpose of increasing the defense of the Mall. You're sure that, some day, this might actually look like a barricade, and not a Phased Array Element. Until this, you decide, this will do.",
                    }
                }
            },
            OverrideProperties = new TemplateItemProperties
            {
                CanSellOnRagfair = false,
                SpawnChance = 0,
                BackgroundColor = "blue"
            },
        };
        items.Add(Barricade);
        foreach (var item in items)
        {
            customItemService.CreateItemFromClone(item);
            itemConfig.Blacklist.Add(item.NewId);
        }
        //customItemService.CreateItemFromClone(DDS);
        //customItemService.CreateItemFromClone(Barricade);
        // Send back a success to the server to say our trader is good to go
        return Task.CompletedTask;
    }
}