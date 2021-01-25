using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Semi_Random_Research
{
    public class SemiRandomResearchModSettings : ModSettings
    {
        public int availableProjectCount = 3;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref availableProjectCount, "availableProjectCount", 3);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            string intEditBuffer = availableProjectCount.ToString();
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;

            listing_Standard.Begin(inRect);

            listing_Standard.Label("CM_Semi_Random_Research_Setting_Available_Projects_Count_Label".Translate(), -1, "CM_Semi_Random_Research_Setting_Available_Projects_Count_Description".Translate());
            //listing_Standard.IntEntry(ref availableProjectCount, intEditBuffer);
            listing_Standard.Label(availableProjectCount.ToString());
            listing_Standard.IntAdjuster(ref availableProjectCount, 1, 1);

            listing_Standard.End();
        }

        public void UpdateSettings()
        {
        }
    }
}
