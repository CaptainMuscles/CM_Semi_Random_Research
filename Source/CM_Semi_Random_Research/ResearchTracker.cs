using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Grammar;

namespace CM_Semi_Random_Research
{
    public class ResearchTracker : WorldComponent
    {
        private List<ResearchProjectDef> currentAvailableProjects = new List<ResearchProjectDef>();
        private ResearchProjectDef currentProject = null;

        public ResearchProjectDef CurrentProject => currentProject;

        public bool autoResearch = false;

        public ResearchTracker(World world) : base(world)
        {

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Collections.Look(ref currentAvailableProjects, "currentAvailableProjects", LookMode.Def);
            Scribe_Defs.Look(ref currentProject, "currentProject");
            Scribe_Values.Look(ref autoResearch, "autoResearch", false);
        }

        public override void WorldComponentTick()
        {
            base.WorldComponentTick();

            ResearchProjectDef activeProject = Find.ResearchManager.currentProj;

            if (currentProject == null || currentProject.IsFinished)
            {
                if (autoResearch)
                    currentProject = GetCurrentlyAvailableProjects().FirstOrDefault();
                else
                    currentProject = null;
            }

            if (activeProject != currentProject)
            {
                if (currentProject == null && currentAvailableProjects.Contains(activeProject))
                {
                    currentProject = activeProject;
                }
                else
                {
                    Find.ResearchManager.currentProj = currentProject;
                }
            }
        }

        public List<ResearchProjectDef> GetCurrentlyAvailableProjects()
        {
            currentAvailableProjects = currentAvailableProjects.Where(projectDef => !projectDef.IsFinished).ToList();

            int numberOfMissingProjects = SemiRandomResearchMod.settings.availableProjectCount - currentAvailableProjects.Count;
            if (numberOfMissingProjects > 0)
            {
                List<ResearchProjectDef> allAvailableProjects = DefDatabase<ResearchProjectDef>.AllDefsListForReading
                    .Where((ResearchProjectDef projectDef) => !currentAvailableProjects.Contains(projectDef) && projectDef.CanStartProject()).ToList();

                if (allAvailableProjects.Count > 0)
                {
                    allAvailableProjects.Shuffle();
                    currentAvailableProjects.AddRange(allAvailableProjects.Take(Math.Min(numberOfMissingProjects, allAvailableProjects.Count)).ToList());
                }
            }

            return new List<ResearchProjectDef> (currentAvailableProjects);
        }
    }
}
