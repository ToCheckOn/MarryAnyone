using HarmonyLib;
using MarryAnyone.Actions;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using static MarryAnyone.Helpers;

namespace MarryAnyone.Patches
{
    [HarmonyPatch(typeof(MarriageAction), "Apply")]
    internal sealed class MarriageActionPatch
    {
        private static void Postfix(Hero firstHero, Hero secondHero)
        {
            if (firstHero == Hero.MainHero || secondHero == Hero.MainHero)
            {
                MASettings settings = new();
                // Do NOT break off marriages if polygamy is on...
                if (settings.Cheating && !settings.Polygamy)
                {
                    CheatOnSpouse();
                }
                RemoveExSpouses(firstHero);
                RemoveExSpouses(secondHero);
            }
        }
    }
}