using System.Collections.Generic;
using System.Linq;
using Verse;

namespace MilitaryStances
{
    [StaticConstructorOnStartup]
    public static class MilitaryStancesTracker
    {
        static MilitaryStancesTracker()
        {
            foreach (var thingDef in DefDatabase<ThingDef>.AllDefs.Where(x =>
                (x.race?.Humanlike == true || x.race?.IsMechanoid == true) && x.IsCorpse is false))
            {
                if (ModsConfig.IsActive("SmashPhil.VehicleFramework") &&
                    thingDef.thingClass?.FullName == "Vehicles.VehiclePawn")
                {
                    continue;
                }

                if (thingDef.comps == null)
                {
                    thingDef.comps = new List<CompProperties>();
                }

                if (!thingDef.comps.Any(c => c.compClass == typeof(CompMilitaryStances)))
                {
                    thingDef.comps.Add(new CompProperties_MilitaryStances());
                }
            }
        }
    }
}
