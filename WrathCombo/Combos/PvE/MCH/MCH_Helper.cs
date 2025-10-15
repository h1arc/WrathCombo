﻿using Dalamud.Game.ClientState.JobGauge.Types;
using FFXIVClientStructs.FFXIV.Client.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.Combos.PvE.MCH.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using static WrathCombo.Data.ActionWatching;
namespace WrathCombo.Combos.PvE;

internal partial class MCH
{
    internal static int BSUsed =>
        CombatActions.Count(x => x == BarrelStabilizer);

    internal static bool UseGaussRound =>
        GetRemainingCharges(OriginalHook(GaussRound)) >= GetRemainingCharges(OriginalHook(Ricochet));

    internal static bool UseRicochet =>
        GetRemainingCharges(OriginalHook(Ricochet)) > GetRemainingCharges(OriginalHook(GaussRound));

    internal static bool HasNotWeaved =>
        GetAttackType(LastAction) != ActionAttackType.Ability;

    #region Queen

    internal static bool UseQueen()
    {
        if (!HasStatusEffect(Buffs.Wildfire) &&
            !JustUsed(OriginalHook(Heatblast)) && ActionReady(RookAutoturret) &&
            !RobotActive && Battery >= 50)
        {
            if ((MCH_ST_QueenBossOption == 0 || InBossEncounter() ||
                 IsEnabled(Preset.MCH_ST_SimpleMode) && InBossEncounter()) &&
                (GetCooldownRemainingTime(Wildfire) > GCD || !LevelChecked(Wildfire)))
            {
                if (LevelChecked(BarrelStabilizer))
                {
                    //1min
                    if (BSUsed == 1 && Battery >= 90)
                        return true;

                    //even mins
                    if (BSUsed >= 2 && Battery == 100)
                        return true;

                    //odd mins 1st queen
                    if (BSUsed >= 2 && Battery is 50 && LastSummonBattery is 100)
                        return true;

                    //odd mins 2nd queen
                    if ((BSUsed % 3 is 2 && Battery >= 60 ||
                         BSUsed % 3 is 0 && Battery >= 70 ||
                         BSUsed % 3 is 1 && Battery >= 80) && LastSummonBattery is 50)
                        return true;
                }

                if (!LevelChecked(BarrelStabilizer))
                    return true;
            }

            if (IsEnabled(Preset.MCH_ST_SimpleMode) && !InBossEncounter() && Battery is 100 ||
                MCH_ST_QueenBossOption == 1 && !InBossEncounter() && Battery >= MCH_ST_TurretUsage)
                return true;
        }

        return false;
    }

    #endregion

    #region HP Treshold

    internal static int HPThresholdHyperchargeST =>
        MCH_ST_HyperchargeBossOption == 1 ||
        !TargetIsBoss() ? MCH_ST_HyperchargeHPOption : 0;

    internal static int HPThresholdReassembleST =>
        MCH_ST_ReassembleBossOption == 1 ||
        !TargetIsBoss() ? MCH_ST_ReassembleHPOption : 0;

    internal static int HPThresholdChainsawST =>
        MCH_ST_ChainsawBossOption == 1 ||
        !TargetIsBoss() ? MCH_ST_ChainsawHPOption : 0;

    #endregion

    #region Reassembled

    internal static bool ReassembledExcavatorST =>
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[0] && (HasStatusEffect(Buffs.Reassembled) || !HasStatusEffect(Buffs.Reassembled)) ||
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && !MCH_ST_Reassembled[0] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_ST_ReassemblePool ||
        !IsEnabled(Preset.MCH_ST_Adv_Reassemble);

    internal static bool ReassembledChainsawST =>
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[1] && (HasStatusEffect(Buffs.Reassembled) || !HasStatusEffect(Buffs.Reassembled)) ||
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && !MCH_ST_Reassembled[1] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_ST_ReassemblePool ||
        !IsEnabled(Preset.MCH_ST_Adv_Reassemble);

    internal static bool ReassembledAnchorST =>
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[2] && (HasStatusEffect(Buffs.Reassembled) || !HasStatusEffect(Buffs.Reassembled)) ||
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && !MCH_ST_Reassembled[2] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_ST_ReassemblePool ||
        !IsEnabled(Preset.MCH_ST_Adv_Reassemble);

    internal static bool ReassembledDrillST =>
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[3] && (HasStatusEffect(Buffs.Reassembled) || !HasStatusEffect(Buffs.Reassembled)) ||
        IsEnabled(Preset.MCH_ST_Adv_Reassemble) && !MCH_ST_Reassembled[3] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_ST_ReassemblePool ||
        !IsEnabled(Preset.MCH_ST_Adv_Reassemble);

    internal static bool ReassembledExcavatorAoE =
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && MCH_AoE_Reassembled[3] && HasStatusEffect(Buffs.Reassembled) ||
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && !MCH_AoE_Reassembled[3] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_AoE_ReassemblePool ||
        !IsEnabled(Preset.MCH_AoE_Adv_Reassemble);

    internal static bool ReassembledChainsawAoE =
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && MCH_AoE_Reassembled[2] && HasStatusEffect(Buffs.Reassembled) ||
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && !MCH_AoE_Reassembled[2] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_AoE_ReassemblePool ||
        !IsEnabled(Preset.MCH_AoE_Adv_Reassemble);

    internal static bool ReassembledAirAnchorAoE =
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && MCH_AoE_Reassembled[1] && HasStatusEffect(Buffs.Reassembled) ||
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && !MCH_AoE_Reassembled[1] && !HasStatusEffect(Buffs.Reassembled) ||
        !HasStatusEffect(Buffs.Reassembled) && GetRemainingCharges(Reassemble) <= MCH_AoE_ReassemblePool ||
        !IsEnabled(Preset.MCH_AoE_Adv_Reassemble);

    internal static bool ReassembledScattergunAoE =
        IsEnabled(Preset.MCH_AoE_Adv_Reassemble) && MCH_AoE_Reassembled[0] && HasStatusEffect(Buffs.Reassembled);

    internal static bool Reassembled()
    {
        if (!JustUsed(OriginalHook(Heatblast)) && !HasStatusEffect(Buffs.Reassembled) &&
            ActionReady(Reassemble) && !JustUsed(OriginalHook(Heatblast)))
        {
            if ((IsEnabled(Preset.MCH_ST_SimpleMode) && !InBossEncounter() ||
                 IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[0] &&
                 IsNotEnabled(Preset.MCH_ST_Adv_TurretQueen)) &&
                LevelChecked(Excavator) && HasStatusEffect(Buffs.ExcavatorReady))
                return true;

            if ((IsEnabled(Preset.MCH_ST_SimpleMode) && InBossEncounter() ||
                 IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[0] &&
                 IsEnabled(Preset.MCH_ST_Adv_TurretQueen)) &&
                LevelChecked(Excavator) && HasStatusEffect(Buffs.ExcavatorReady) &&
                (BSUsed is 1 ||
                 BSUsed % 3 is 2 && Battery <= 40 ||
                 BSUsed % 3 is 0 && Battery <= 50 ||
                 BSUsed % 3 is 1 && Battery <= 60 ||
                 GetStatusEffectRemainingTime(Buffs.ExcavatorReady) <= 6))
                return true;

            if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
                 IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[1]) &&
                !LevelChecked(Excavator) && !MaxBattery && LevelChecked(Chainsaw) &&
                GetCooldownRemainingTime(Chainsaw) <= GCD)
                return true;

            if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
                 IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[2]) &&
                !MaxBattery && LevelChecked(AirAnchor) &&
                GetCooldownRemainingTime(AirAnchor) <= GCD)
                return true;

            if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
                 IsEnabled(Preset.MCH_ST_Adv_Reassemble) && MCH_ST_Reassembled[3]) &&
                LevelChecked(Drill) &&
                (!LevelChecked(AirAnchor) && MCH_ST_Reassembled[2] || !MCH_ST_Reassembled[2]) &&
                ActionReady(Drill))
                return true;
        }

        return false;
    }

    #endregion

    #region Cooldowns

    internal static bool DrillCD =>
        !LevelChecked(Drill) ||
        !TraitLevelChecked(Traits.EnhancedMultiWeapon) && GetCooldownRemainingTime(Drill) >= 9 ||
        TraitLevelChecked(Traits.EnhancedMultiWeapon) && GetRemainingCharges(Drill) < GetMaxCharges(Drill) && GetCooldownRemainingTime(Drill) >= 9;

    internal static bool AnchorCD =>
        !LevelChecked(AirAnchor) ||
        LevelChecked(AirAnchor) && GetCooldownRemainingTime(AirAnchor) >= 9;

    internal static bool SawCD =>
        !LevelChecked(Chainsaw) ||
        LevelChecked(Chainsaw) && GetCooldownRemainingTime(Chainsaw) >= 9;

    internal static bool Tools(ref uint actionID)
    {
        if ((IsEnabled(Preset.MCH_ST_SimpleMode) && !InBossEncounter() ||
             IsEnabled(Preset.MCH_ST_Adv_Excavator) && ReassembledExcavatorST &&
             (IsNotEnabled(Preset.MCH_ST_Adv_TurretQueen) || MCH_ST_QueenBossOption == 1 && !InBossEncounter())) &&
            LevelChecked(Excavator) && HasStatusEffect(Buffs.ExcavatorReady))
        {
            actionID = Excavator;
            return true;
        }

        if ((IsEnabled(Preset.MCH_ST_SimpleMode) && InBossEncounter() ||
             IsEnabled(Preset.MCH_ST_Adv_Excavator) && ReassembledExcavatorST &&
             IsEnabled(Preset.MCH_ST_Adv_TurretQueen) && (MCH_ST_QueenBossOption == 0 || InBossEncounter())) &&
            LevelChecked(Excavator) && HasStatusEffect(Buffs.ExcavatorReady) &&
            (BSUsed is 1 ||
             BSUsed % 3 is 2 && Battery <= 40 ||
             BSUsed % 3 is 0 && Battery <= 50 ||
             BSUsed % 3 is 1 && Battery <= 60 ||
             GetStatusEffectRemainingTime(Buffs.ExcavatorReady) <= 6))
        {
            actionID = Excavator;
            return true;
        }

        if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
             IsEnabled(Preset.MCH_ST_Adv_Chainsaw) && ReassembledChainsawST) &&
            !MaxBattery && !HasStatusEffect(Buffs.ExcavatorReady) && LevelChecked(Chainsaw) &&
            GetCooldownRemainingTime(Chainsaw) <= GCD / 2 &&
            GetTargetHPPercent() > HPThresholdChainsawST)
        {
            actionID = Chainsaw;
            return true;
        }

        if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
             IsEnabled(Preset.MCH_ST_Adv_AirAnchor) && ReassembledAnchorST) &&
            !MaxBattery && LevelChecked(AirAnchor) &&
            GetCooldownRemainingTime(AirAnchor) <= GCD / 2)
        {
            actionID = AirAnchor;
            return true;
        }

        if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
             IsEnabled(Preset.MCH_ST_Adv_Drill) && ReassembledDrillST) &&
            !JustUsed(Drill) &&
            ActionReady(Drill) && GetCooldownRemainingTime(Wildfire) is >= 20 or <= 10)
        {
            actionID = Drill;
            return true;
        }

        if ((IsEnabled(Preset.MCH_ST_SimpleMode) ||
             IsEnabled(Preset.MCH_ST_Adv_AirAnchor)) &&
            LevelChecked(HotShot) && !LevelChecked(AirAnchor) && !MaxBattery &&
            GetCooldownRemainingTime(HotShot) <= GCD / 2)
        {
            actionID = HotShot;
            return true;
        }

        return false;
    }

    #endregion

    #region Combos

    internal static float GCD => GetCooldown(OriginalHook(SplitShot)).CooldownTotal;

    internal static unsafe bool IsComboExpiring(float times)
    {
        float gcd = GCD * times;

        return ActionManager.Instance()->Combo.Timer != 0 && ActionManager.Instance()->Combo.Timer < gcd;
    }

  #endregion

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Lvl90EarlyTools.LevelChecked)
            return Lvl90EarlyTools;

        if (StandardOpener.LevelChecked)
            return StandardOpener;

        return WrathOpener.Dummy;
    }

    internal static MCHStandardOpener StandardOpener = new();
    internal static MCHLvl90EarlyToolsOpener Lvl90EarlyTools = new();

    internal class MCHStandardOpener : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            Reassemble,
            AirAnchor,
            CheckMate,
            DoubleCheck,
            Drill,
            BarrelStabilizer,
            Chainsaw,
            Excavator,
            AutomatonQueen,
            Reassemble,
            Drill,
            CheckMate,
            Wildfire,
            FullMetalField,
            Hypercharge,
            DoubleCheck,
            BlazingShot,
            CheckMate,
            BlazingShot,
            DoubleCheck,
            BlazingShot,
            CheckMate,
            BlazingShot,
            DoubleCheck,
            BlazingShot,
            CheckMate,
            Drill,
            DoubleCheck,
            CheckMate,
            HeatedSplitShot,
            DoubleCheck,
            HeatedSlugShot,
            HeatedCleanShot
        ];

        internal override UserData ContentCheckConfig => MCH_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => 4)
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(Reassemble) is 2 &&
            GetRemainingCharges(OriginalHook(GaussRound)) is 3 &&
            GetRemainingCharges(OriginalHook(Ricochet)) is 3 &&
            IsOffCooldown(Chainsaw) &&
            IsOffCooldown(Wildfire) &&
            IsOffCooldown(BarrelStabilizer) &&
            IsOffCooldown(Excavator) &&
            IsOffCooldown(FullMetalField);
    }

    internal class MCHLvl90EarlyToolsOpener : WrathOpener
    {
        public override int MinOpenerLevel => 90;

        public override int MaxOpenerLevel => 90;

        public override List<uint> OpenerActions { get; set; } =
        [
            Reassemble,
            AirAnchor,
            GaussRound,
            Ricochet,
            Drill,
            BarrelStabilizer,
            Chainsaw,
            GaussRound,
            Ricochet,
            HeatedSplitShot,
            GaussRound,
            Ricochet,
            HeatedSlugShot,
            Wildfire,
            HeatedCleanShot,
            AutomatonQueen,
            Hypercharge,
            BlazingShot,
            Ricochet,
            BlazingShot,
            GaussRound,
            BlazingShot,
            Ricochet,
            BlazingShot,
            GaussRound,
            BlazingShot,
            Reassemble,
            Drill
        ];

        internal override UserData ContentCheckConfig => MCH_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => 4)
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            14
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(Reassemble) is 2 &&
            GetRemainingCharges(OriginalHook(GaussRound)) is 3 &&
            GetRemainingCharges(OriginalHook(Ricochet)) is 3 &&
            IsOffCooldown(Chainsaw) &&
            IsOffCooldown(Wildfire) &&
            IsOffCooldown(BarrelStabilizer);
    }

    #endregion

    #region Gauge

    internal static MCHGauge Gauge = GetJobGauge<MCHGauge>();

    internal static bool IsOverheated => Gauge.IsOverheated;

    internal static bool RobotActive => Gauge.IsRobotActive;

    internal static byte LastSummonBattery => Gauge.LastSummonBatteryPower;

    internal static byte Heat => Gauge.Heat;

    internal static byte Battery => Gauge.Battery;

    internal static bool MaxBattery => Battery >= 100;

    #endregion

    #region ID's

    public const uint
        CleanShot = 2873,
        HeatedCleanShot = 7413,
        SplitShot = 2866,
        HeatedSplitShot = 7411,
        SlugShot = 2868,
        HeatedSlugShot = 7412,
        GaussRound = 2874,
        Ricochet = 2890,
        Reassemble = 2876,
        Drill = 16498,
        HotShot = 2872,
        AirAnchor = 16500,
        Hypercharge = 17209,
        Heatblast = 7410,
        SpreadShot = 2870,
        Scattergun = 25786,
        AutoCrossbow = 16497,
        RookAutoturret = 2864,
        RookOverdrive = 7415,
        AutomatonQueen = 16501,
        QueenOverdrive = 16502,
        Tactician = 16889,
        Chainsaw = 25788,
        BioBlaster = 16499,
        BarrelStabilizer = 7414,
        Wildfire = 2878,
        Dismantle = 2887,
        Flamethrower = 7418,
        BlazingShot = 36978,
        DoubleCheck = 36979,
        CheckMate = 36980,
        Excavator = 36981,
        FullMetalField = 36982;

    public static class Buffs
    {
        public const ushort
            Reassembled = 851,
            Tactician = 1951,
            Wildfire = 1946,
            Overheated = 2688,
            Flamethrower = 1205,
            Hypercharged = 3864,
            ExcavatorReady = 3865,
            FullMetalMachinist = 3866;
    }

    public static class Debuffs
    {
        public const ushort
            Dismantled = 860,
            Wildfire = 861,
            Bioblaster = 1866;
    }

    public static class Traits
    {
        public const ushort
            EnhancedMultiWeapon = 605;
    }

    #endregion
}
