using HarmonyLib;
using HarmonyLib.BUTR.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Library;
using static MarryAnyone.Debug;

namespace MarryAnyone
{
    internal static class Helpers
    {
        private static readonly AccessTools.FieldRef<Hero, MBList<Hero>>? _exSpouses = AccessTools2.FieldRefAccess<Hero, MBList<Hero>>("_exSpouses");
        public enum RemoveExSpousesMode
        {
            Duplicates,
            Self,
            All
        }

        public static void ResetEndedCourtships()
        {
            foreach (Romance.RomanticState romanticState in Romance.RomanticStateList.ToList())
            {
                if (romanticState.Person1 == Hero.MainHero || romanticState.Person2 == Hero.MainHero)
                {
                    if (romanticState.Level == Romance.RomanceLevelEnum.Ended)
                    {
                        romanticState.Level = Romance.RomanceLevelEnum.Untested;
                    }
                }
            }
        }

        public static void RemoveExSpouses(Hero hero, RemoveExSpousesMode removalMode = RemoveExSpousesMode.Duplicates)
        {
            MBList<Hero> _exSpousesList = _exSpouses!(hero);

            if (removalMode == RemoveExSpousesMode.Duplicates)
            {
                // Standard remove duplicates spouse
                // Get exspouse list without duplicates
                _exSpousesList = _exSpousesList.Distinct().ToMBList<Hero>();
                // If exspouse is already a spouse, then remove from exspouses
                if (_exSpousesList.Contains(hero.Spouse))
                {
                    _exSpousesList.Remove(hero.Spouse);
                    Print($"Removed duplicate spouse {hero.Spouse.Name}");
                }
            }
            else
            {
                // Remove all exspouses
                _exSpousesList = _exSpousesList.ToMBList<Hero>();
                List<Hero> exSpouses = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();
                foreach (Hero exSpouse in exSpouses)
                {
                    if (removalMode == RemoveExSpousesMode.Self || removalMode == RemoveExSpousesMode.All)
                    {
                        // Remove exspouse from list
                        _exSpousesList.Remove(exSpouse);
                    }
                    if (removalMode == RemoveExSpousesMode.All)
                    {
                        // Look into your exspouse's exspouse to remove yourself
                        MBList<Hero> _exSpousesList2 = _exSpouses!(hero);
                        _exSpousesList2.Remove(hero);

                        MBReadOnlyList<Hero> ExSpousesReadOnlyList2 = new(_exSpousesList2);
                        _exSpouses(exSpouse) = _exSpousesList2;
                    }
                }
            }

            MBReadOnlyList<Hero> ExSpousesReadOnlyList = new(_exSpousesList);
            _exSpouses(hero) = _exSpousesList;
        }

        public static void CheatOnSpouse()
        {
            List<Hero> _exSpousesList = _exSpouses!(Hero.MainHero);
            List<Hero> cheatedHeroes = _exSpousesList.Where(exSpouse => exSpouse.IsAlive).ToList();

            foreach (Hero cheatedHero in cheatedHeroes)
            {
                RemoveExSpouses(cheatedHero, RemoveExSpousesMode.All);
                if (cheatedHero != Hero.MainHero.Spouse)
                {
                    // Almost forgot to add in an ended romantic state for cheated heroes!
                    ChangeRomanticStateAction.Apply(Hero.MainHero, cheatedHero, Romance.RomanceLevelEnum.Ended);
                    Print($"Broke off marriage with {cheatedHero.Name}");
                }
                else
                {
                    Print($"Removed duplicate spouse {cheatedHero.Name}");
                }
            }
        }
    }
}