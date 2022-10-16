using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem;
using MarryAnyone.Patches;
using HarmonyLib.BUTR.Extensions;
using System.Collections.Generic;
using Helpers;
using TaleWorlds.CampaignSystem.Party;
using static MarryAnyone.Debug;

namespace MarryAnyone.Actions
{
    internal static class MAMarriageAction
    {
        private delegate void PlayerDefaultFactionDelegate(Campaign instance, Clan @value);
        private static readonly PlayerDefaultFactionDelegate? PlayerDefaultFaction = AccessTools2.GetPropertySetterDelegate<PlayerDefaultFactionDelegate>(typeof(Campaign), "PlayerDefaultFaction");

        // Appears to ultimately avoid disbanding parties and the like...
        // Never disband party for hero, do for everyone else...

        // Marrying into a clan is very buggy
        // The game expects that the player character is always the clan leader
        // Even when this is not the case...
        // This leads to long term instability in the game
        // It might just be best to follow what Bannerlord wants and take out these settings
        // And make sure faction leaders leave their factions gracefully
        private static void ApplyInternal(Hero firstHero, Hero secondHero, bool showNotification)
        {
            MASettings settings = new();
            firstHero.Spouse = secondHero;
            secondHero.Spouse = firstHero;
            ChangeRelationAction.ApplyRelationChangeBetweenHeroes(firstHero, secondHero, Campaign.Current.Models.MarriageModel.GetEffectiveRelationIncrease(firstHero, secondHero), false);

            Clan clanAfterMarriage = Campaign.Current.Models.MarriageModel.GetClanAfterMarriage(firstHero, secondHero);
            // Cautious marriage action
            // Avoid the part with the clan checking
            if (firstHero.Clan != clanAfterMarriage)
            {
                Clan clan = firstHero.Clan;
                if (firstHero != Hero.MainHero)
                {
                    if (firstHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(firstHero);
                    }
                    if (firstHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = firstHero.PartyBelongedTo;
                        if (clan is not null)
                        {
                            if (clan.Kingdom != clanAfterMarriage.Kingdom)
                            {
                                if (firstHero.PartyBelongedTo.Army is not null)
                                {
                                    if (firstHero.PartyBelongedTo.Army.LeaderParty == firstHero.PartyBelongedTo)
                                    {
                                        firstHero.PartyBelongedTo.Army.DisperseArmy(Army.ArmyDispersionReason.Unknown);
                                    }
                                    else
                                    {
                                        firstHero.PartyBelongedTo.Army = null;
                                    }
                                }
                                IFaction kingdom = clanAfterMarriage.Kingdom;
                                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(firstHero, kingdom ?? clanAfterMarriage);
                            }
                        }
                        if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == firstHero)
                        {
                            DisbandPartyAction.StartDisband(partyBelongedTo);
                            partyBelongedTo.Party.SetCustomOwner(null);
                        }
                        firstHero.ChangeState(Hero.CharacterStates.Fugitive);
                        MobileParty partyBelongedTo2 = firstHero.PartyBelongedTo;
                        if (partyBelongedTo2 is not null)
                        {
                            partyBelongedTo2.MemberRoster.RemoveTroop(firstHero.CharacterObject, 1, default, 0);
                        }
                    }
                }
                firstHero.Clan = clanAfterMarriage;
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            else if (secondHero.Clan != clanAfterMarriage)
            {
                Clan clan = secondHero.Clan;
                if (secondHero != Hero.MainHero)
                {
                    if (secondHero.GovernorOf is not null)
                    {
                        ChangeGovernorAction.RemoveGovernorOf(secondHero);
                    }
                    if (secondHero.PartyBelongedTo is not null)
                    {
                        MobileParty partyBelongedTo = secondHero.PartyBelongedTo;
                        if (clan is not null)
                        {
                            if (clan.Kingdom != clanAfterMarriage.Kingdom)
                            {
                                if (secondHero.PartyBelongedTo.Army is not null)
                                {
                                    if (secondHero.PartyBelongedTo.Army.LeaderParty == secondHero.PartyBelongedTo)
                                    {
                                        secondHero.PartyBelongedTo.Army.DisperseArmy(Army.ArmyDispersionReason.Unknown);
                                    }
                                    else
                                    {
                                        secondHero.PartyBelongedTo.Army = null;
                                    }
                                }
                                IFaction kingdom = clanAfterMarriage.Kingdom;
                                FactionHelper.FinishAllRelatedHostileActionsOfNobleToFaction(secondHero, kingdom ?? clanAfterMarriage);
                            }
                        }
                        if (partyBelongedTo.Party.IsActive && partyBelongedTo.Party.Owner == secondHero)
                        {
                            DisbandPartyAction.StartDisband(partyBelongedTo);
                            partyBelongedTo.Party.SetCustomOwner(null);
                        }
                        secondHero.ChangeState(Hero.CharacterStates.Fugitive);
                        MobileParty partyBelongedTo2 = secondHero.PartyBelongedTo;
                        if (partyBelongedTo2 is not null)
                        {
                            partyBelongedTo2.MemberRoster.RemoveTroop(secondHero.CharacterObject, 1, default, 0);
                        }
                    }
                }
                secondHero.Clan = clanAfterMarriage;
                if (clan is not null)
                {
                    foreach (Hero hero in clan.Heroes)
                    {
                        hero.UpdateHomeSettlement();
                    }
                }
                foreach (Hero hero in clanAfterMarriage.Heroes)
                {
                    hero.UpdateHomeSettlement();
                }
            }
            EndAllCourtshipsPatch.EndAllCourtships(firstHero);
            EndAllCourtshipsPatch.EndAllCourtships(secondHero);
            ChangeRomanticStateAction.Apply(firstHero, secondHero, Romance.RomanceLevelEnum.Marriage);
            CampaignEventDispatcher.Instance.OnHeroesMarried(firstHero, secondHero, showNotification);
        }

        public static void Apply(Hero firstHero, Hero secondHero, bool showNotification = true)
        {
            ApplyInternal(firstHero, secondHero, showNotification);
        }
    }
}