using RimWorld;
using Verse;
using System.Collections.Generic;
using System.Linq;
using AlienRace;
using System.Text;
using HarmonyLib;

namespace CM_Semi_Random_Research
{
    public static class AlienRaceUtility
    {
        public static bool enabled_AlienRaces = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");

        public static bool HasRace(ResearchProjectDef rpd)
        {
            if (enabled_AlienRaces) return Alien(rpd);
            else return true;
        }

        static bool Alien(ResearchProjectDef rpd)
        {
            if (!AlienRace.RaceRestrictionSettings.researchRestrictionDict.ContainsKey(key: rpd))
            {
                return true;
            }
            HarmonyPatches.UpdateColonistRaces();
            HashSet<ThingDef> colonistRaces = AccessTools.Field(typeof(AlienRace.HarmonyPatches), "colonistRaces").GetValue(null) as HashSet<ThingDef>;
            return RaceRestrictionSettings.CanResearch(colonistRaces, rpd);
        }

    }
}