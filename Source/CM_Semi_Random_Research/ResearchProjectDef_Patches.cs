using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    public static class ResearchProjectDef_Patches
    {
        [HarmonyPatch(typeof(ResearchProjectDef))]
        [HarmonyPatch("CanStartNow", MethodType.Getter)]
        public static class ResearchProjectDef_CanStartNow
        {
            [HarmonyPostfix]
            public static void Postfix(ResearchProjectDef __instance, ref bool __result)
            {
                __result = false;
            }
        }
    }
}
