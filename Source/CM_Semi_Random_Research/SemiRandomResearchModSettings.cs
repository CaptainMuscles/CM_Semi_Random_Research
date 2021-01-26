using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CM_Semi_Random_Research
{
    public enum ManualReroll
    {
        None,
        Once,
        Always
    }

    public class SemiRandomResearchModSettings : ModSettings
    {
        public bool featureEnabled = true;
        public bool rerollAllEveryTime = true;

        public ManualReroll allowManualReroll = ManualReroll.None;

        public int availableProjectCount = 3;

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref featureEnabled, "featureEnabled", true);
            Scribe_Values.Look(ref rerollAllEveryTime, "rerollAllEveryTime", true);

            Scribe_Values.Look(ref allowManualReroll, "allowManualReroll", ManualReroll.None);

            Scribe_Values.Look(ref availableProjectCount, "availableProjectCount", 3);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            string intEditBuffer = availableProjectCount.ToString();
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.ColumnWidth = (inRect.width - 34f) / 2f;

            listing_Standard.Begin(inRect);

            listing_Standard.CheckboxLabeled("CM_Semi_Random_Research_Setting_Feature_Enabled_Label".Translate(), ref featureEnabled, "CM_Semi_Random_Research_Setting_Feature_Enabled_Description".Translate());
            listing_Standard.CheckboxLabeled("CM_Semi_Random_Research_Setting_Reroll_All_Every_Time_Label".Translate(), ref rerollAllEveryTime, "CM_Semi_Random_Research_Setting_Reroll_All_Every_Time_Description".Translate());

            listing_Standard.GapLine();

            listing_Standard.Label("CM_Semi_Random_Research_Setting_Manual_Reroll_Label".Translate());
            if (listing_Standard.RadioButton_NewTemp("CM_Semi_Random_Research_Setting_No_Manual_Reroll_Label".Translate(), allowManualReroll == ManualReroll.None, 8f, "CM_Semi_Random_Research_Setting_No_Manual_Reroll_Description".Translate()))
                allowManualReroll = ManualReroll.None;
            if (listing_Standard.RadioButton_NewTemp("CM_Semi_Random_Research_Setting_Reroll_One_Time_Label".Translate(), allowManualReroll == ManualReroll.Once, 8f, "CM_Semi_Random_Research_Setting_Reroll_One_Time_Description".Translate()))
                allowManualReroll = ManualReroll.Once;
            if (listing_Standard.RadioButton_NewTemp("CM_Semi_Random_Research_Setting_Reroll_Any_Time_Label".Translate(), allowManualReroll == ManualReroll.Always, 8f, "CM_Semi_Random_Research_Setting_Reroll_Any_Time_Description".Translate()))
                allowManualReroll = ManualReroll.Always;

            listing_Standard.GapLine();

            listing_Standard.Label("CM_Semi_Random_Research_Setting_Available_Projects_Count_Label".Translate(), -1, "CM_Semi_Random_Research_Setting_Available_Projects_Count_Description".Translate());
            listing_Standard.Label(availableProjectCount.ToString());
            listing_Standard.IntAdjuster(ref availableProjectCount, 1, 1);

            listing_Standard.End();
        }

        public void UpdateSettings()
        {
        }
    }
}
