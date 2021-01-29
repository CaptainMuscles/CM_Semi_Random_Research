using System.Collections.Generic;

using UnityEngine;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace CM_Semi_Random_Research
{
    [StaticConstructorOnStartup]
    public static class MainTabWindow_Research_Patches
    {
        [HarmonyPatch(typeof(MainTabWindow_Research))]
        [HarmonyPatch("DrawLeftRect", MethodType.Normal)]
        public static class MainTabWindow_Research_DrawLeftRect
        {
            private static readonly Texture2D NextResearchButtonIcon = ContentFinder<Texture2D>.Get("UI/Buttons/MainButtons/CM_Semi_Random_Research_Random");

            [HarmonyPostfix]
            public static void Postfix(ResearchProjectDef __instance, Rect leftOutRect)
            {
                float buttonSize = 32.0f;
                Rect buttonRect = new Rect(leftOutRect.xMax - buttonSize, leftOutRect.yMin, buttonSize, buttonSize);

                // I'm just going to check both buttons in case either snatches up the event
                bool pressedButton1 = Widgets.ButtonTextSubtle(buttonRect, "");
                bool pressedButton2 = Widgets.ButtonImage(buttonRect, NextResearchButtonIcon);

                if (pressedButton1 || pressedButton2)
                {
                    SoundDefOf.ResearchStart.PlayOneShotOnCamera();

                    MainTabWindow currentWindow = Find.WindowStack.WindowOfType<MainTabWindow>();
                    MainTabWindow newWindow = SemiRandomResearchDefOf.CM_Semi_Random_Research_MainButton_Next_Research.TabWindow;

                    //Log.Message(string.Format("Has currentWindow {0}, has newWindow {1}", (currentWindow != null).ToString(), (newWindow != null).ToString()));
                    
                    if (currentWindow != null && newWindow != null)
                    {
                        Find.WindowStack.TryRemove(currentWindow, false);
                        Find.WindowStack.Add(newWindow);
                        SoundDefOf.TabOpen.PlayOneShotOnCamera();
                    }
                }
            }
        }
    }
}
