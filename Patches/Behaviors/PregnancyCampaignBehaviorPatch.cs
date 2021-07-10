using HarmonyLib;
using MarryAnyone.Settings;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.Core;

namespace MarryAnyone.Patches.Behaviors
{
    // Add in a setting for enabling polyamory so it does not have to be a harem
    [HarmonyPatch(typeof(PregnancyCampaignBehavior), "DailyTickHero")]
    internal class PregnancyCampaignBehaviorPatch
    {
        private static void Prefix(Hero hero)
        {
            ISettingsProvider settings = new MASettings();
            if (hero.IsFemale && hero.IsAlive && hero.Age > Campaign.Current.Models.AgeModel.HeroComesOfAge)
            {
                // If you are the MainHero go through advanced process
                if (hero == Hero.MainHero || hero == Hero.MainHero.Spouse || Hero.MainHero.ExSpouses.Contains(hero))
                {
                    if (hero.Spouse is null && (hero.ExSpouses.IsEmpty() || hero.ExSpouses is null))
                    {
                        MAHelper.Print("   No Spouse");
                        return;
                    }
                    _spouses = new List<Hero>();
                    RemovePolyamory(hero);
                    MAHelper.Print("Hero: " + hero);
                    if (hero.Spouse is not null && hero == Hero.MainHero)
                    {
                        _spouses.Add(hero.Spouse);
                        MAHelper.Print("Spouse to Collection: " + _spouses.Last());
                    }
                    if (settings.Polyamory && hero != Hero.MainHero)
                    {
                        MAHelper.Print("Polyamory:");
                        _spouses.Add(Hero.MainHero);
                        MAHelper.Print("   Main Hero to Collection: " + _spouses.Last());
                        if (Hero.MainHero.Spouse is not null && Hero.MainHero.Spouse != hero)
                        {
                            _spouses.Add(Hero.MainHero.Spouse);
                            MAHelper.Print("   Main Hero's Spouse to Collection: " + _spouses.Last());
                        }
                        foreach (Hero exSpouse in Hero.MainHero.ExSpouses.Distinct().ToList())
                        {
                            if (exSpouse != hero && exSpouse.IsAlive)
                            {
                                _spouses.Add(exSpouse);
                                MAHelper.Print("   Main Hero's ExSpouse to Collection: " + _spouses.Last());
                            }
                        }
                    }
                    else
                    {
                        // Taken out of polyamory mode
                        if (hero.Spouse != Hero.MainHero && hero != Hero.MainHero)
                        {
                            _spouses.Add(Hero.MainHero);
                            MAHelper.Print("Spouse is Main Hero: " + _spouses.Last());
                        }
                        if (hero == Hero.MainHero)
                        {
                            foreach (Hero exSpouse in hero.ExSpouses.Distinct().ToList())
                            {
                                if (exSpouse.IsAlive)
                                {
                                    _spouses.Add(exSpouse);
                                    MAHelper.Print("ExSpouse to Collection: " + _spouses.Last());
                                }
                            }
                        }
                    }
                    if (_spouses.Count() > 1)
                    {
                        // The shuffle!
                        List<int> attractionGoal = new();
                        int attraction = 0;
                        foreach (Hero spouse in _spouses)
                        {
                            attraction += Romance.GetAttractionValueAsPercent(hero, spouse);
                            attractionGoal.Add(attraction);
                            MAHelper.Print("Spouse: " + spouse);
                            MAHelper.Print("Attraction: " + attraction);
                        }
                        int attractionRandom = MBRandom.RandomInt(attraction);
                        MAHelper.Print("Random: " + attractionRandom);
                        int i = 0;
                        while (i < _spouses.Count)
                        {
                            if (attractionRandom < attractionGoal[i])
                            {
                                MAHelper.Print("Index: " + i);
                                break;
                            }
                            i++;
                        }
                        hero.Spouse = _spouses[i];
                        _spouses[i].Spouse = hero;
                    }
                    else
                    {
                        var spouse = _spouses.FirstOrDefault();
                        if (spouse is not null)
                        {
                            hero.Spouse = spouse;
                            spouse.Spouse = hero;
                        }
                    }
                    if (hero.Spouse is null)
                    {
                        MAHelper.Print("   No Spouse");
                    }
                    else
                    {
                        MAHelper.Print("   Spouse Assigned: " + hero.Spouse);
                    }
                }
            }
            // Outside of female pregnancy behavior
            if (hero.Spouse is not null)
            {
                if (hero.IsFemale == hero.Spouse.IsFemale)
                {
                    // Decided to do this at the end so that you are not always going out with the opposite gender
                    MAHelper.Print("   Spouse Unassigned: " + hero.Spouse);
                    hero.Spouse.Spouse = null;
                    hero.Spouse = null;
                }
            }
        }

        private static void Postfix(Hero hero)
        {
            // Make things looks better in the encyclopedia
            if (hero == Hero.MainHero)
            {
                hero.Spouse = null;
            }
            else if (Hero.MainHero.ExSpouses.Contains(hero) || hero.Spouse == Hero.MainHero)
            {
                RemovePolyamory(hero);
            }
            // Remove any extra duplicate exspouses
            foreach (Hero exSpouse in hero.ExSpouses.ToList())
            {
                MAHelper.RemoveExSpouses(hero);
                MAHelper.RemoveExSpouses(exSpouse);
            }
        }

        private static void RemovePolyamory(Hero hero)
        {
            if (hero != Hero.MainHero)
            {
                ISettingsProvider settings = new MASettings();
                if (!settings.Polyamory)
                {
                    // Remove any former spouses that are not the Main Hero
                    MAHelper.RemoveExSpouses(hero, true);
                }
                hero.Spouse = Hero.MainHero;
            }
        }

        private static List<Hero>? _spouses;
    }
}