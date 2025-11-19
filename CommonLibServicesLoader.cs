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
using WTTServerCommonLib.Models;
using Path = System.IO.Path;
namespace GuidingLight
{
    [Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 2)]
    public class CommonLibServicesLoader(
    WTTServerCommonLib.WTTServerCommonLib wttCommon
) : IOnLoad
    {
        public async Task OnLoad()
        {

            // Get your current assembly
            var assembly = Assembly.GetExecutingAssembly();

            //Time to test things!
            TraderIds.Add("Guiding_Light", "674964e1ff0cf3b00b461857");
            TraderIds.Add("Curious_Light", "674964e72eae32de73a09d41");

            // Use WTT-CommonLib services
            //await wttCommon.CustomItemServiceExtended.CreateCustomItems(assembly);
            await wttCommon.CustomLocaleService.CreateCustomLocales(assembly);
            await wttCommon.CustomQuestService.CreateCustomQuests(assembly);

            await Task.CompletedTask;
        }
    }
}