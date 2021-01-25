using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using RimWorld;
using Verse;
using Verse.Sound;

namespace CM_Semi_Random_Research
{
    public class MainTabWindow_NextResearch : MainTabWindow
    {
        protected ResearchProjectDef selectedProject;

        protected override float Margin => 6f;

        private float betweenColumnSpace => 24f;

        private Vector2 rightScrollPosition = Vector2.zero;

        private float rightScrollViewHeight;

        private static readonly Color FulfilledPrerequisiteColor = Color.green;

        private static readonly Texture2D ResearchBarFillTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.2f, 0.8f, 0.85f));

        private static readonly Texture2D ResearchBarBGTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.1f, 0.1f, 0.1f));

        private Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>> cachedUnlockedDefsGroupedByPrerequisites;

        private static List<Building> tmpAllBuildings = new List<Building>();

        private bool ColonistsHaveResearchBench
        {
            get
            {
                bool result = false;
                List<Map> maps = Find.Maps;
                for (int i = 0; i < maps.Count; i++)
                {
                    if (maps[i].listerBuildings.ColonistsHaveResearchBench())
                    {
                        result = true;
                        break;
                    }
                }
                return result;
            }
        }

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public List<ResearchProjectDef> currentAvailableProjects = new List<ResearchProjectDef>();

        public MainTabWindow_NextResearch()
        {

        }

        public override void PreOpen()
        {
            base.PreOpen();

            ResearchTracker researchTracker = Current.Game.World.GetComponent<ResearchTracker>();

            if (researchTracker != null)
            {
                currentAvailableProjects = researchTracker.GetCurrentlyAvailableProjects();
                selectedProject = researchTracker.CurrentProject;
            }

            cachedUnlockedDefsGroupedByPrerequisites = null;
        }

        public override void DoWindowContents(Rect rect)
        {
            base.DoWindowContents(rect);

            float columnWidth = ((rect.width - ((Margin * 2) + betweenColumnSpace)) * 0.5f);
            float columnHeight = rect.height - (Margin * 2);

            Rect leftOutRect = new Rect(Margin,
                                        Margin,
                                        columnWidth,
                                        columnHeight);

            Rect rightOutRect = new Rect(leftOutRect.xMax + betweenColumnSpace,
                                         Margin,
                                         columnWidth,
                                         columnHeight);

            DrawLeftColumn(leftOutRect);
            DrawRightColumn(rightOutRect);
        }

        private void DrawLeftColumn(Rect leftRect)
        {
            Listing_Standard leftListing = new Listing_Standard();
            leftListing.Begin(leftRect);

            Text.Font = GameFont.Medium;
            leftListing.Label("CM_Semi_Random_Research_Available_Projects".Translate());
            leftListing.GapLine();

            foreach (ResearchProjectDef projectDef in currentAvailableProjects)
            {
                Rect buttonRect = leftListing.GetRect(50.0f);
                DrawResearchButton(buttonRect, projectDef);
                leftListing.Gap();
            }

            leftListing.End();
        }

        private void DrawResearchButton(Rect drawRect, ResearchProjectDef projectDef)
        {
            TextAnchor startingTextAnchor = Text.Anchor;

            Text.Font = GameFont.Small;

            Color backgroundColor = default(Color);
            Color textColor = Widgets.NormalOptionColor;
            Color borderColor = default(Color);

            if (projectDef == Find.ResearchManager.currentProj)
            {
                backgroundColor = TexUI.ActiveResearchColor;
            }
            else
            {
                backgroundColor = TexUI.AvailResearchColor;
            }

            if (selectedProject == projectDef)
            {
                backgroundColor += TexUI.HighlightBgResearchColor;
                borderColor = TexUI.HighlightBorderResearchColor;
            }
            else
            {
                borderColor = TexUI.DefaultBorderResearchColor;
            }

            Rect buttonRect = drawRect;
            Rect textRect = drawRect;
            textRect.width = textRect.width - (Margin * 2);
            textRect.center = buttonRect.center;
            //buttonRect.width = 140.0f;
            //buttonRect.center = drawRect.center;

            //Rect labelOffsetRect = buttonRect;
            //Widgets.LabelCacheHeight(ref labelOffsetRect, " ");
            if (Widgets.CustomButtonText(ref buttonRect, "", backgroundColor, textColor, borderColor))
            {
                SoundDefOf.Click.PlayOneShotOnCamera();
                selectedProject = projectDef;
            }

            //labelOffsetRect.y = buttonRect.y + buttonRect.height - labelOffsetRect.height;
            //Rect costRect = labelOffsetRect;
            //costRect.x += 10f;
            //costRect.width = costRect.width / 2f - 10f;
            //Rect techPrintRect = costRect;
            //techPrintRect.x += costRect.width;

            TextAnchor anchor = Text.Anchor;
            Color rememberGuiColor = GUI.color;
            GUI.color = textColor;
            Text.Anchor = TextAnchor.MiddleLeft;
            Widgets.Label(textRect, projectDef.LabelCap);

            GUI.color = textColor;
            Text.Anchor = TextAnchor.MiddleRight;
            Widgets.Label(textRect, projectDef.CostApparent.ToString());

            //if (projectDef.TechprintCount > 0)
            //{
            //    GUI.color = FulfilledPrerequisiteColor;
            //    Text.Anchor = TextAnchor.MiddleRight;
            //    Widgets.Label(techPrintRect, $"{projectDef.TechprintsApplied.ToString()} / {projectDef.TechprintCount.ToString()}");
            //}
            GUI.color = rememberGuiColor;

            Text.Anchor = startingTextAnchor;
        }

        private void DrawRightColumn(Rect rightRect)
        {
            Rect position = rightRect;
            GUI.BeginGroup(position);
            if (selectedProject != null)
            {
                float projectNameHeight = 50.0f;
                float gapHeight = 10.0f;
                float startResearchButtonHeight = 68.0f;
                float progressBarHeight = 35.0f;
                float detailsAreaHeight = position.height - (gapHeight + startResearchButtonHeight + gapHeight + progressBarHeight);

                float debugFinishResearchNowButtonHeight = 30.0f;

                float currentY = 0f;

                Rect outRect = new Rect(0f, 0f, position.width, detailsAreaHeight);
                Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, rightScrollViewHeight);

                Log.Message(outRect.yMax.ToString());

                Widgets.BeginScrollView(outRect, ref rightScrollPosition, viewRect);

                // Selected project name
                Text.Font = GameFont.Medium;
                GenUI.SetLabelAlign(TextAnchor.MiddleLeft);
                Rect projectNameRect = new Rect(0f, currentY, viewRect.width, projectNameHeight);
                Widgets.LabelCacheHeight(ref projectNameRect, selectedProject.LabelCap);
                GenUI.ResetLabelAlign();

                // Selected project description
                Text.Font = GameFont.Small;
                currentY += projectNameRect.height;
                Rect projectDescriptionRect = new Rect(0f, currentY, viewRect.width, 0f);
                Widgets.LabelCacheHeight(ref projectDescriptionRect, selectedProject.description);
                currentY += projectDescriptionRect.height;

                // Tech level research cost multiplier description
                if ((int)selectedProject.techLevel > (int)Faction.OfPlayer.def.techLevel)
                {
                    float costMultiplier = selectedProject.CostFactor(Faction.OfPlayer.def.techLevel);
                    Rect techLevelMultilplierDescriptionRect = new Rect(0f, currentY, viewRect.width, 0f);
                    string text = "TechLevelTooLow".Translate(Faction.OfPlayer.def.techLevel.ToStringHuman(), selectedProject.techLevel.ToStringHuman(), (1f / costMultiplier).ToStringPercent());
                    if (costMultiplier != 1f)
                    {
                        text += " " + "ResearchCostComparison".Translate(selectedProject.baseCost.ToString("F0"), selectedProject.CostApparent.ToString("F0"));
                    }
                    Widgets.LabelCacheHeight(ref techLevelMultilplierDescriptionRect, text);
                    currentY += techLevelMultilplierDescriptionRect.height;
                }

                // Prerequisites
                currentY += DrawResearchPrereqs(rect: new Rect(0f, currentY, viewRect.width, detailsAreaHeight), project: selectedProject);
                currentY += DrawResearchBenchRequirements(rect: new Rect(0f, currentY, viewRect.width, detailsAreaHeight), project: selectedProject);

                // Unlockables
                Rect projectUnlockablesRect = new Rect(0f, currentY, viewRect.width, detailsAreaHeight);
                currentY += DrawUnlockableHyperlinks(projectUnlockablesRect, selectedProject);
                currentY = (rightScrollViewHeight = currentY + 3f);

                Widgets.EndScrollView();

                // Start research button
                
                Rect startResearchButtonRect = new Rect(0f, detailsAreaHeight + gapHeight, position.width, startResearchButtonHeight);
                if (selectedProject.CanStartProject() && selectedProject != Find.ResearchManager.currentProj)
                {
                    if (Widgets.ButtonText(startResearchButtonRect, "Research".Translate()))
                    {
                        SoundDefOf.ResearchStart.PlayOneShotOnCamera();
                        Find.ResearchManager.currentProj = selectedProject;
                        TutorSystem.Notify_Event("StartResearchProject");
                        if (!ColonistsHaveResearchBench)
                        {
                            Messages.Message("MessageResearchMenuWithoutBench".Translate(), MessageTypeDefOf.CautionInput);
                        }
                    }
                }
                else
                {
                    string projectStatus = "";
                    if (selectedProject.IsFinished)
                    {
                        projectStatus = "Finished".Translate();
                        Text.Anchor = TextAnchor.MiddleCenter;
                    }
                    else if (selectedProject == Find.ResearchManager.currentProj)
                    {
                        projectStatus = "InProgress".Translate();
                        Text.Anchor = TextAnchor.MiddleCenter;
                    }
                    else
                    {
                        projectStatus = "Locked".Translate() + ":";
                        if (!selectedProject.PrerequisitesCompleted)
                        {
                            projectStatus += "\n  " + "PrerequisitesNotCompleted".Translate();
                        }
                        if (!selectedProject.TechprintRequirementMet)
                        {
                            projectStatus += "\n  " + "InsufficientTechprintsApplied".Translate(selectedProject.TechprintsApplied, selectedProject.TechprintCount);
                        }
                    }
                    Widgets.DrawHighlight(startResearchButtonRect);
                    Widgets.Label(startResearchButtonRect.ContractedBy(5f), projectStatus);
                    Text.Anchor = TextAnchor.UpperLeft;
                }

                // Progress bar
                Rect progressBarRect = new Rect(0f, startResearchButtonRect.yMax + gapHeight, position.width, progressBarHeight);
                Widgets.FillableBar(progressBarRect, selectedProject.ProgressPercent, ResearchBarFillTex, ResearchBarBGTex, doBorder: true);
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(progressBarRect, selectedProject.ProgressApparent.ToString("F0") + " / " + selectedProject.CostApparent.ToString("F0"));
                Text.Anchor = TextAnchor.UpperLeft;
                if (Prefs.DevMode && !selectedProject.IsFinished && Widgets.ButtonText(new Rect(startResearchButtonRect.x, startResearchButtonRect.y - debugFinishResearchNowButtonHeight, 120f, debugFinishResearchNowButtonHeight), "Debug: Finish now"))
                {
                    Find.ResearchManager.currentProj = selectedProject;
                    Find.ResearchManager.FinishProject(selectedProject);
                }
            }

            GUI.EndGroup();
        }

        private float DrawResearchPrereqs(ResearchProjectDef project, Rect rect)
        {
            if (project.prerequisites.NullOrEmpty())
            {
                return 0f;
            }
            float xMin = rect.xMin;
            float yMin = rect.yMin;
            Widgets.LabelCacheHeight(ref rect, "ResearchPrerequisites".Translate() + ":");
            rect.yMin += rect.height;
            rect.xMin += 6f;
            for (int i = 0; i < project.prerequisites.Count; i++)
            {
                GUI.color = FulfilledPrerequisiteColor;
                Widgets.LabelCacheHeight(ref rect, project.prerequisites[i].LabelCap);
                rect.yMin += rect.height;
            }
            if (project.hiddenPrerequisites != null)
            {
                for (int j = 0; j < project.hiddenPrerequisites.Count; j++)
                {
                    GUI.color = FulfilledPrerequisiteColor;
                    Widgets.LabelCacheHeight(ref rect, project.hiddenPrerequisites[j].LabelCap);
                    rect.yMin += rect.height;
                }
            }
            GUI.color = Color.white;
            rect.xMin = xMin;
            return rect.yMin - yMin;
        }

        private float DrawResearchBenchRequirements(ResearchProjectDef project, Rect rect)
        {
            float xMin = rect.xMin;
            float yMin = rect.yMin;
            if (project.requiredResearchBuilding != null)
            {
                List<Map> maps = Find.Maps;
                Widgets.LabelCacheHeight(ref rect, "RequiredResearchBench".Translate() + ":");
                rect.xMin += 6f;
                rect.yMin += rect.height;
                GUI.color = FulfilledPrerequisiteColor;
                rect.height = Text.CalcHeight(project.requiredResearchBuilding.LabelCap, rect.width - 24f - 6f);
                Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(project.requiredResearchBuilding));
                rect.yMin += rect.height + 4f;
                GUI.color = Color.white;
                rect.xMin = xMin;
            }
            if (!project.requiredResearchFacilities.NullOrEmpty())
            {
                Widgets.LabelCacheHeight(ref rect, "RequiredResearchBenchFacilities".Translate() + ":");
                rect.yMin += rect.height;
                Building_ResearchBench building_ResearchBench = FindBenchFulfillingMostRequirements(project.requiredResearchBuilding, project.requiredResearchFacilities);
                CompAffectedByFacilities bestMatchingBench = null;
                if (building_ResearchBench != null)
                {
                    bestMatchingBench = building_ResearchBench.TryGetComp<CompAffectedByFacilities>();
                }
                rect.xMin += 6f;
                for (int j = 0; j < project.requiredResearchFacilities.Count; j++)
                {
                    DrawResearchBenchFacilityRequirement(project.requiredResearchFacilities[j], bestMatchingBench, project, ref rect);
                    rect.yMin += rect.height;
                }
                rect.yMin += 4f;
            }
            GUI.color = Color.white;
            rect.xMin = xMin;
            return rect.yMin - yMin;
        }

        private Building_ResearchBench FindBenchFulfillingMostRequirements(ThingDef requiredResearchBench, List<ThingDef> requiredFacilities)
        {
            tmpAllBuildings.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                tmpAllBuildings.AddRange(maps[i].listerBuildings.allBuildingsColonist);
            }
            float num = 0f;
            Building_ResearchBench building_ResearchBench = null;
            for (int j = 0; j < tmpAllBuildings.Count; j++)
            {
                Building_ResearchBench building_ResearchBench2 = tmpAllBuildings[j] as Building_ResearchBench;
                if (building_ResearchBench2 != null && (requiredResearchBench == null || building_ResearchBench2.def == requiredResearchBench))
                {
                    float researchBenchRequirementsScore = GetResearchBenchRequirementsScore(building_ResearchBench2, requiredFacilities);
                    if (building_ResearchBench == null || researchBenchRequirementsScore > num)
                    {
                        num = researchBenchRequirementsScore;
                        building_ResearchBench = building_ResearchBench2;
                    }
                }
            }
            tmpAllBuildings.Clear();
            return building_ResearchBench;
        }

        private void DrawResearchBenchFacilityRequirement(ThingDef requiredFacility, CompAffectedByFacilities bestMatchingBench, ResearchProjectDef project, ref Rect rect)
        {
            Thing thing = null;
            Thing thing2 = null;
            if (bestMatchingBench != null)
            {
                thing = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility);
                thing2 = bestMatchingBench.LinkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacility && bestMatchingBench.IsFacilityActive(x));
            }
            GUI.color = FulfilledPrerequisiteColor;
            string text = requiredFacility.LabelCap;
            if (thing != null && thing2 == null)
            {
                text += " (" + "InactiveFacility".Translate() + ")";
            }
            rect.height = Text.CalcHeight(text, rect.width - 24f - 6f);
            Widgets.HyperlinkWithIcon(rect, new Dialog_InfoCard.Hyperlink(requiredFacility), text);
        }

        private float GetResearchBenchRequirementsScore(Building_ResearchBench bench, List<ThingDef> requiredFacilities)
        {
            float num = 0f;
            for (int i = 0; i < requiredFacilities.Count; i++)
            {
                CompAffectedByFacilities benchComp = bench.GetComp<CompAffectedByFacilities>();
                if (benchComp != null)
                {
                    List<Thing> linkedFacilitiesListForReading = benchComp.LinkedFacilitiesListForReading;
                    if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i] && benchComp.IsFacilityActive(x)) != null)
                    {
                        num += 1f;
                    }
                    else if (linkedFacilitiesListForReading.Find((Thing x) => x.def == requiredFacilities[i]) != null)
                    {
                        num += 0.6f;
                    }
                }
            }
            return num;
        }

        private float DrawUnlockableHyperlinks(Rect rect, ResearchProjectDef project)
        {
            List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> list = UnlockedDefsGroupedByPrerequisites(project);
            if (list.NullOrEmpty())
            {
                return 0f;
            }
            float yMin = rect.yMin;
            float x = rect.x;
            foreach (Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>> item in list)
            {
                ResearchPrerequisitesUtility.UnlockedHeader first = item.First;
                rect.x = x;
                if (!first.unlockedBy.Any())
                {
                    Widgets.LabelCacheHeight(ref rect, "Unlocks".Translate() + ":");
                }
                else
                {
                    Widgets.LabelCacheHeight(ref rect, string.Concat("UnlockedWith".Translate(), " ", HeaderLabel(first), ":"));
                }
                rect.x += 6f;
                rect.yMin += rect.height;
                foreach (Def item2 in item.Second)
                {
                    Widgets.HyperlinkWithIcon(hyperlink: new Dialog_InfoCard.Hyperlink(item2), rect: new Rect(rect.x, rect.yMin, rect.width, 24f));
                    rect.yMin += 24f;
                }
            }
            return rect.yMin - yMin;
        }


        private string HeaderLabel(ResearchPrerequisitesUtility.UnlockedHeader headerProject)
        {
            StringBuilder stringBuilder = new StringBuilder();
            string value = "";
            for (int i = 0; i < headerProject.unlockedBy.Count; i++)
            {
                ResearchProjectDef researchProjectDef = headerProject.unlockedBy[i];
                string text = researchProjectDef.LabelCap;
                stringBuilder.Append(text).Append(value);
                value = ", ";
            }
            return stringBuilder.ToString();
        }

        private List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>> UnlockedDefsGroupedByPrerequisites(ResearchProjectDef project)
        {
            if (cachedUnlockedDefsGroupedByPrerequisites == null)
            {
                cachedUnlockedDefsGroupedByPrerequisites = new Dictionary<ResearchProjectDef, List<Pair<ResearchPrerequisitesUtility.UnlockedHeader, List<Def>>>>();
            }
            if (!cachedUnlockedDefsGroupedByPrerequisites.TryGetValue(project, out var value))
            {
                value = ResearchPrerequisitesUtility.UnlockedDefsGroupedByPrerequisites(project);
                cachedUnlockedDefsGroupedByPrerequisites.Add(project, value);
            }
            return value;
        }
    }
}