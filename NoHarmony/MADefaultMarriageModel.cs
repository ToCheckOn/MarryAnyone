﻿using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;

namespace MarryAnyone
{
    internal class MADefaultMarriageModel : DefaultMarriageModel
    {
        public override bool IsSuitableForMarriage(Hero maidenOrSuitor)
        {
            MAConfig config = MASubModule.Config;
            bool isPolygamous = config.IsPolygamous && (maidenOrSuitor == Hero.MainHero || maidenOrSuitor == Hero.OneToOneConversationHero);

            if (maidenOrSuitor.IsTemplate || maidenOrSuitor.IsDead || Hero.MainHero.ExSpouses.Contains(maidenOrSuitor))
            {
                return false;
            }
            if (maidenOrSuitor.Spouse == null || isPolygamous)
            {
                if (maidenOrSuitor.IsFemale)
                {
                    return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeFemale;
                }
                return maidenOrSuitor.CharacterObject.Age >= (float)MinimumMarriageAgeMale;
            }
            return false;
        }

        public override bool IsCoupleSuitableForMarriage(Hero firstHero, Hero secondHero)
        {
            MAConfig config = MASubModule.Config;
            bool isMainHero = firstHero == Hero.MainHero || secondHero == Hero.MainHero;
            bool isHomosexual = config.SexualOrientation == SexualOrientation.Homosexual && isMainHero;
            bool isBisexual = config.SexualOrientation == SexualOrientation.Bisexual && isMainHero;
            bool isIncestual = config.IsIncestual && isMainHero;
            bool discoverAncestors = !DiscoverAncestors(firstHero, 3).Intersect(DiscoverAncestors(secondHero, 3)).Any();

            if (isIncestual)
            {
                discoverAncestors = true;
            }
            if (isHomosexual)
            {
                return firstHero.IsFemale == secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            if (isBisexual)
            {
                return discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
            }
            return firstHero.IsFemale != secondHero.IsFemale && discoverAncestors && IsSuitableForMarriage(firstHero) && IsSuitableForMarriage(secondHero);
        }

        public static IEnumerable<Hero> DiscoverAncestors(Hero hero, int n)
        {
            if (hero != null)
            {
                yield return hero;
                if (n > 0)
                {
                    foreach (Hero hero2 in DiscoverAncestors(hero.Mother, n - 1))
                    {
                        yield return hero2;
                    }
                    foreach (Hero hero3 in DiscoverAncestors(hero.Father, n - 1))
                    {
                        yield return hero3;
                    }
                }
            }
            yield break;
        }

        public override Clan GetClanAfterMarriage(Hero firstHero, Hero secondHero)
        {
            MASubModule.MADebug("Marriage between " + firstHero.Name + " and " + secondHero.Name);
            if (firstHero.IsFactionLeader && firstHero.MapFaction.IsKingdomFaction)
            {
                MASubModule.MADebug("Kingdom leader is the dominant clan in marriage");
                return firstHero.Clan;
            }
            if (secondHero.IsFactionLeader && secondHero.MapFaction.IsKingdomFaction)
            {
                MASubModule.MADebug("Kingdom leader is the dominant clan in marriage");
                return secondHero.Clan;
            }
            if (firstHero.IsHumanPlayerCharacter)
            {
                MASubModule.MADebug("Human player is the dominant clan in marriage");
                return firstHero.Clan;
            }
            if (secondHero.IsHumanPlayerCharacter)
            {
                MASubModule.MADebug("Human player is the dominant clan in marriage");
                return secondHero.Clan;
            }
            if (firstHero.IsFactionLeader)
            {
                MASubModule.MADebug("Faction leader is the dominant clan in marriage");
                return firstHero.Clan;
            }
            if (secondHero.IsFactionLeader)
            {
                MASubModule.MADebug("Faction leader is the dominant clan in marriage");
                return secondHero.Clan;
            }
            if (firstHero.Clan.Leader == firstHero)
            {
                MASubModule.MADebug("Clan leader is the dominant clan in marriage");
                return firstHero.Clan;
            }
            if (secondHero.Clan.Leader == secondHero)
            {
                MASubModule.MADebug("Clan leader is the dominant clan in marriage");
                return secondHero.Clan;
            }
            if (!firstHero.IsFemale)
            {
                return firstHero.Clan;
            }
            return secondHero.Clan;
        }
    }
}