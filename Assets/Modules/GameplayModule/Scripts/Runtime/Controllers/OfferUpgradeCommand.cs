using System.Collections.Generic;
using FlowIoC.BaseModule.Controller;
using FlowIoC.BaseModule.Injectable.Attributes;
using Modules.GameplayModule.Data.ValueObjects;
using Modules.GameplayModule.Models;
using Modules.GameplayModule.Shared.Data.ValueObjects;
using Modules.GameplayModule.Shared.Enums;
using Modules.GameplayModule.Signals;
using UnityEngine;

namespace Modules.GameplayModule.Controllers
{
    /// <summary>
    /// Opens the next level-up choice when one is earned and none is open. A hero the level has
    /// unlocked is always one of the cards until it has joined; the rest are upgrades that can still
    /// stack and reach somebody in the party, drawn by weight without repeats. The wait for an
    /// answer runs on its own signal, so the battle never waits on the player.
    /// </summary>
    internal class OfferUpgradeCommand : Command
    {
        [Inject] private IHeroModel _heroModel { get; set; }
        [Inject] private IUpgradeOfferModel _offerModel { get; set; }
        [InjectSignal] private GameplaySignals _signals { get; set; }
        [InjectSignal] private GameplayInternalSignals _internalSignals { get; set; }

        private readonly List<UpgradeVO> _candidates = new();
        private readonly HashSet<PartyRole> _recruited = new();

        public override void Execute()
        {
            if (_offerModel.HasOpenOffer || _heroModel.PendingUpgrades <= 0)
                return;

            // The earned choice is spent when a card is applied, not here, so a game closed with an
            // offer up offers it again on the next run.
            List<UpgradeOptionRVO> options = BuildOptions(_heroModel.Config.OfferOptionCount);
            if (options.Count == 0)
                return;

            UpgradeOfferRVO offer = _offerModel.Open(options);

            _signals.Outgoing.UpgradeOffered.Dispatch(_offerModel.GetOpenOfferState(_heroModel.Config.AutoPickSeconds));
            _internalSignals.WatchUpgradeOffer.Dispatch(offer.OfferId);
        }

        private List<UpgradeOptionRVO> BuildOptions(int count)
        {
            var options = new List<UpgradeOptionRVO>();

            foreach (PartyMemberCVO recruit in _heroModel.RecruitableMembers())
            {
                options.Add(new UpgradeOptionRVO
                {
                    Recruit = recruit,
                    Card = new UpgradeOptionVO
                    {
                        Kind = UpgradeKind.Recruit,
                        Role = recruit.Role,
                        Title = $"Recruit {recruit.Name}",
                        Description = recruit.Description,
                    },
                });
                break;
            }

            _recruited.Clear();
            foreach (PartyMemberVO member in _heroModel.Members)
                _recruited.Add(member.Config.Role);

            _candidates.Clear();
            foreach (UpgradeVO upgrade in _heroModel.Upgrades)
            {
                if (_heroModel.CanStack(upgrade) && (upgrade.Config.AllRoles || _recruited.Contains(upgrade.Config.Role)))
                    _candidates.Add(upgrade);
            }

            while (options.Count < count && _candidates.Count > 0)
            {
                UpgradeVO picked = PickWeighted(_candidates);
                _candidates.Remove(picked);

                UpgradeCVO config = picked.Config;
                options.Add(new UpgradeOptionRVO
                {
                    Upgrade = config,
                    Card = new UpgradeOptionVO
                    {
                        Kind = config.Kind,
                        Role = config.Role,
                        AllRoles = config.AllRoles,
                        Title = picked.Save.Count > 0 ? $"{config.Title} {Roman(picked.Save.Count + 1)}" : config.Title,
                        Description = config.Description,
                    },
                });
            }

            return options;
        }

        private static UpgradeVO PickWeighted(List<UpgradeVO> candidates)
        {
            float total = 0f;
            foreach (UpgradeVO candidate in candidates)
                total += Mathf.Max(0.01f, candidate.Config.Weight);

            float roll = Random.value * total;
            foreach (UpgradeVO candidate in candidates)
            {
                roll -= Mathf.Max(0.01f, candidate.Config.Weight);
                if (roll <= 0f)
                    return candidate;
            }

            return candidates[candidates.Count - 1];
        }

        private static string Roman(int number) => number switch
        {
            2 => "II", 3 => "III", 4 => "IV", 5 => "V", 6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
            _ => number.ToString(),
        };
    }
}
