﻿using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using static WrathCombo.Combos.PvE.MNK.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class MNK
{
    private static float GCD =>
        GetCooldown(OriginalHook(Bootshine)).CooldownTotal;

    private static int HPThresholdBuffs =>
        MNK_ST_BuffsBossOption == 1 ||
        !InBossEncounter() ? MNK_ST_BuffsHPThreshold : 0;

    private static bool M6SReady =>
        !HiddenFeaturesData.IsEnabledWith(Preset.MNK_Hid_M6SHoldSquirrelBurst, () =>
            HiddenFeaturesData.Targeting.R6SSquirrel && CombatEngageDuration().TotalSeconds < 300);

    #region 1-2-3

    private static uint DetermineCoreAbility(uint actionId, bool useTrueNorthIfEnabled = true)
    {
        if (!LevelChecked(TrueStrike))
            return Bootshine;

        if (HasStatusEffect(Buffs.OpoOpoForm) || HasStatusEffect(Buffs.FormlessFist))
            return OpoOpo is 0 && LevelChecked(DragonKick)
                ? DragonKick
                : OriginalHook(Bootshine);

        if (HasStatusEffect(Buffs.RaptorForm))
            return Raptor is 0 && LevelChecked(TwinSnakes)
                ? TwinSnakes
                : OriginalHook(TrueStrike);

        if (HasStatusEffect(Buffs.CoeurlForm))
        {
            if (Coeurl is 0 && LevelChecked(Demolish))
                return !OnTargetsRear() &&
                       Role.CanTrueNorth() &&
                       useTrueNorthIfEnabled
                    ? TrueNorth
                    : Demolish;

            if (LevelChecked(SnapPunch))
                return !OnTargetsFlank() &&
                       Role.CanTrueNorth() &&
                       useTrueNorthIfEnabled
                    ? TrueNorth
                    : OriginalHook(SnapPunch);
        }

        return actionId;
    }

    #endregion

    #region PB

    private static bool CanPerfectBalance(bool onAoE = false)
    {
        switch (onAoE)
        {
            case false when
                ActionReady(PerfectBalance) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.FormlessFist) && IsOriginal(MasterfulBlitz) &&
                HasBattleTarget():
            {
                // Odd window
                if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(DragonKick, GCD)) &&
                    !JustUsed(PerfectBalance, 20) && HasStatusEffect(Buffs.RiddleOfFire) && !HasStatusEffect(Buffs.Brotherhood))
                    return true;

                // Even window first use
                if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(DragonKick, GCD)) &&
                    GetCooldownRemainingTime(Brotherhood) <= GCD * 3 && GetCooldownRemainingTime(RiddleOfFire) <= GCD * 3)
                    return true;

                // Even window second use
                if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(DragonKick, GCD)) &&
                    HasStatusEffect(Buffs.Brotherhood) && HasStatusEffect(Buffs.RiddleOfFire) && !HasStatusEffect(Buffs.FiresRumination))
                    return true;

                // Low level
                if ((JustUsed(OriginalHook(Bootshine), GCD) || JustUsed(DragonKick, GCD)) &&
                    (HasStatusEffect(Buffs.RiddleOfFire) && !LevelChecked(Brotherhood) ||
                     !LevelChecked(RiddleOfFire)))
                    return true;
                break;
            }

            case true when
                ActionReady(PerfectBalance) && !HasStatusEffect(Buffs.PerfectBalance) &&
                !HasStatusEffect(Buffs.FormlessFist) && HasBattleTarget() &&
                GetTargetHPPercent() >= MNK_AoE_PerfectBalanceHPThreshold:
            {
                //Initial/Failsafe
                if (GetRemainingCharges(PerfectBalance) == GetMaxCharges(PerfectBalance))
                    return true;

                // Odd window
                if (HasStatusEffect(Buffs.RiddleOfFire) && !HasStatusEffect(Buffs.Brotherhood))
                    return true;

                // Even window
                if ((GetCooldownRemainingTime(Brotherhood) <= GCD * 2 || HasStatusEffect(Buffs.Brotherhood)) &&
                    (GetCooldownRemainingTime(RiddleOfFire) <= GCD * 2 || HasStatusEffect(Buffs.RiddleOfFire)))
                    return true;

                // Low level
                if (HasStatusEffect(Buffs.RiddleOfFire) && !LevelChecked(Brotherhood) ||
                    !LevelChecked(RiddleOfFire))
                    return true;
                break;
            }
        }

        return false;
    }

    #endregion

    #region PB Combo

    private static bool DoPerfectBalanceCombo(ref uint actionID, bool onAoE = false)
    {
        switch (onAoE)
        {
            case false when HasStatusEffect(Buffs.PerfectBalance):
            {

            #region Open Lunar

                if (!LunarNadi || BothNadisOpen || !SolarNadi && !LunarNadi)
                {
                    switch (OpoOpo)
                    {
                        case 0:
                            actionID = DragonKick;
                            return true;

                        case > 0:
                            actionID = OriginalHook(Bootshine);
                            return true;
                    }
                }

        #endregion

            #region Open Solar

                if (!SolarNadi && !BothNadisOpen)
                {
                    if (CoeurlChakra is 0)
                    {
                        switch (Coeurl)
                        {
                            case 0:
                                actionID = Demolish;
                                return true;

                            case > 0:
                                actionID = OriginalHook(SnapPunch);
                                return true;
                        }
                    }

                    if (RaptorChakra is 0)
                    {
                        switch (Raptor)
                        {
                            case 0:
                                actionID = TwinSnakes;
                                return true;

                            case > 0:
                                actionID = OriginalHook(TrueStrike);
                                return true;
                        }
                    }

                    if (OpoOpoChakra is 0)
                    {
                        switch (OpoOpo)
                        {
                            case 0:
                                actionID = DragonKick;
                                return true;

                            case > 0:
                                actionID = OriginalHook(Bootshine);
                                return true;
                        }
                    }
                }

        #endregion

                break;
            }
            case true when HasStatusEffect(Buffs.PerfectBalance):
            {

            #region Open Lunar

                if (!LunarNadi || BothNadisOpen || !SolarNadi && !LunarNadi)
                {
                    if (LevelChecked(ShadowOfTheDestroyer))
                    {
                        actionID = ShadowOfTheDestroyer;
                        return true;
                    }

                    if (!LevelChecked(ShadowOfTheDestroyer))
                    {
                        actionID = Rockbreaker;
                        return true;
                    }
                }

        #endregion

            #region Open Solar

                switch (SolarNadi)
                {
                    case false when !BothNadisOpen:
                        switch (GetStatusEffectStacks(Buffs.PerfectBalance))
                        {
                            case 3:
                                actionID = OriginalHook(ArmOfTheDestroyer);
                                return true;

                            case 2:
                                actionID = FourPointFury;
                                return true;

                            case 1:
                                actionID = Rockbreaker;
                                return true;
                        }
                        break;
                }

        #endregion

                break;
            }
        }

        return false;
    }

    #endregion

    #region Masterful Blitz

    private static bool CanMasterfulBlitz(bool onAoE)
    {
        switch (onAoE)
        {
            case false when
                LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                InMasterfulRange() && !IsOriginal(MasterfulBlitz):
            {
                //Only use when buff is active
                if (LevelChecked(RiddleOfFire) && HasStatusEffect(Buffs.RiddleOfFire))
                    return true;

                //Use whenever since no buff
                if (!LevelChecked(RiddleOfFire))
                    return true;
                break;
            }

            case true when
                LevelChecked(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.PerfectBalance) &&
                InMasterfulRange() && !IsOriginal(MasterfulBlitz):
                return true;
        }

        return false;
    }

    internal static bool InMasterfulRange()
    {
        if (NumberOfEnemiesInRange(ElixirField) >= 1 &&
            (OriginalHook(MasterfulBlitz) == ElixirField ||
             OriginalHook(MasterfulBlitz) == FlintStrike ||
             OriginalHook(MasterfulBlitz) == ElixirBurst ||
             OriginalHook(MasterfulBlitz) == RisingPhoenix))
            return true;

        if (NumberOfEnemiesInRange(TornadoKick, CurrentTarget) >= 1 &&
            (OriginalHook(MasterfulBlitz) == TornadoKick ||
             OriginalHook(MasterfulBlitz) == CelestialRevolution ||
             OriginalHook(MasterfulBlitz) == PhantomRush))
            return true;

        return false;
    }

    #endregion

    #region Chakra

    private static bool CanFormshift() =>
        LevelChecked(FormShift) && !InCombat() &&
        !HasStatusEffect(Buffs.FormlessFist) &&
        !HasStatusEffect(Buffs.PerfectBalance) &&
        !HasStatusEffect(Buffs.OpoOpoForm) &&
        !HasStatusEffect(Buffs.RaptorForm) &&
        !HasStatusEffect(Buffs.CoeurlForm);

    private static bool CanMeditate(bool onAoE = false)
    {
        switch (onAoE)
        {
            case false when
                LevelChecked(SteeledMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination):
            case true when
                LevelChecked(InspiritedMeditation) &&
                (!InCombat() || !InMeleeRange()) &&
                Chakra < 5 &&
                IsOriginal(MasterfulBlitz) &&
                !HasStatusEffect(Buffs.RiddleOfFire) &&
                !HasStatusEffect(Buffs.WindsRumination) &&
                !HasStatusEffect(Buffs.FiresRumination):
                return true;

            default:
                return false;
        }

    }

    private static bool CanUseChakra(bool onAoE = false)
    {
        switch (onAoE)
        {
            case false when
                Chakra >= 5 && LevelChecked(SteeledMeditation) &&
                !JustUsed(Brotherhood) && !JustUsed(RiddleOfFire) &&
                InActionRange(OriginalHook(SteeledMeditation)):

            case true when
                Chakra >= 5 &&
                LevelChecked(InspiritedMeditation) &&
                HasBattleTarget() && !JustUsed(Brotherhood) &&
                !JustUsed(RiddleOfFire) &&
                InActionRange(OriginalHook(InspiritedMeditation)):
                return true;

            default:
                return false;
        }

    }

    #endregion

    #region Buffs

    //RoF
    private static bool CanRoF() =>
        ActionReady(RiddleOfFire) &&
        !HasStatusEffect(Buffs.FiresRumination) &&
        (HasWeavedAction(Brotherhood) ||
         GetCooldownRemainingTime(Brotherhood) is > 50 and < 65 ||
         !LevelChecked(Brotherhood));

    private static bool CanFiresReply() =>
        HasStatusEffect(Buffs.FiresRumination) &&
        !HasStatusEffect(Buffs.FormlessFist) &&
        !HasStatusEffect(Buffs.PerfectBalance) &&
        IsOriginal(MasterfulBlitz) &&
        (JustUsed(OriginalHook(Bootshine)) ||
         JustUsed(DragonKick) ||
         GetStatusEffectRemainingTime(Buffs.FiresRumination) < GCD * 2 ||
         !InMeleeRange());

    //Brotherhood
    private static bool CanBrotherhood() =>
        ActionReady(Brotherhood) &&
        ActionReady(RiddleOfFire) &&
        CanWeave(GCD / 2);

    //RoW
    private static bool CanRoW() =>
        ActionReady(RiddleOfWind) &&
        !HasStatusEffect(Buffs.WindsRumination);

    private static bool CanWindsReply() =>
        HasStatusEffect(Buffs.WindsRumination) &&
        (GetCooldownRemainingTime(RiddleOfFire) > 5 ||
         HasStatusEffect(Buffs.RiddleOfFire) ||
         GetStatusEffectRemainingTime(Buffs.WindsRumination) < GCD * 2 ||
         !InMeleeRange());

    private static bool CanMantra() =>
        ActionReady(Mantra) &&
        !HasStatusEffect(Buffs.Mantra) &&
        RaidWideCasting(3f);

    private static bool CanRoE() =>
        ActionReady(RiddleOfEarth) &&
        RaidWideCasting(2f) &&
        !HasStatusEffect(Buffs.RiddleOfEarth) &&
        !HasStatusEffect(Buffs.EarthsRumination);

    private static bool CanEarthsReply() =>
        HasStatusEffect(Buffs.EarthsRumination) &&
        NumberOfAlliesInRange(EarthsReply) >= GetPartyMembers().Count * .75 &&
        GetPartyAvgHPPercent() <= MNK_ST_EarthsReplyHPThreshold;

    #endregion

    #region Openers

    internal static WrathOpener Opener()
    {
        if (LLOpener.LevelChecked &&
            MNK_SelectedOpener == 0)
            return LLOpener;

        if (SLOpener.LevelChecked &&
            MNK_SelectedOpener == 1)
            return SLOpener;

        return WrathOpener.Dummy;
    }

    internal static MNKLLOpener LLOpener = new();
    internal static MNKSLOpener SLOpener = new();

    internal class MNKLLOpener : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            ForbiddenMeditation,
            FormShift,
            DragonKick,
            PerfectBalance,
            LeapingOpo,
            DragonKick,
            Brotherhood,
            RiddleOfFire,
            LeapingOpo,
            TheForbiddenChakra,
            RiddleOfWind,
            ElixirBurst,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];

        public override List<(int[] Steps, Func<bool> Condition)> SkipSteps { get; set; } =
        [
            ([1], () => Chakra >= 5),
            ([2], () => HasStatusEffect(Buffs.FormlessFist))
        ];

        internal override UserData ContentCheckConfig => MNK_Balance_Content;

        public override bool HasCooldowns() =>
            GetRemainingCharges(PerfectBalance) is 2 &&
            IsOffCooldown(Brotherhood) &&
            IsOffCooldown(RiddleOfFire) &&
            IsOffCooldown(RiddleOfWind) &&
            Nadi is Nadi.None &&
            Raptor is 0 &&
            Coeurl is 0;
    }

    internal class MNKSLOpener : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            ForbiddenMeditation,
            FormShift,
            DragonKick,
            PerfectBalance,
            TwinSnakes,
            Demolish,
            Brotherhood,
            RiddleOfFire,
            LeapingOpo,
            TheForbiddenChakra,
            RiddleOfWind,
            RisingPhoenix,
            DragonKick,
            WindsReply,
            FiresReply,
            LeapingOpo,
            PerfectBalance,
            DragonKick,
            LeapingOpo,
            DragonKick,
            ElixirBurst,
            LeapingOpo
        ];

        public override List<(int[] Steps, Func<bool> Condition)> SkipSteps { get; set; } =
        [
            ([1], () => Chakra >= 5),
            ([2], () => HasStatusEffect(Buffs.FormlessFist))
        ];

        internal override UserData ContentCheckConfig => MNK_Balance_Content;

        public override bool HasCooldowns() =>
            GetRemainingCharges(PerfectBalance) is 2 &&
            IsOffCooldown(Brotherhood) &&
            IsOffCooldown(RiddleOfFire) &&
            IsOffCooldown(RiddleOfWind) &&
            Nadi is Nadi.None &&
            Raptor is 0 &&
            Coeurl is 0;
    }

    #endregion

    #region Gauge

    private static MNKGauge Gauge = GetJobGauge<MNKGauge>();

    private static byte Chakra => Gauge.Chakra;

    private static int OpoOpoChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.OpoOpo);

    private static int OpoOpo => Gauge.OpoOpoFury;

    private static int RaptorChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.Raptor);

    private static int Raptor => Gauge.RaptorFury;

    private static int CoeurlChakra => Gauge.BeastChakra.Count(x => x == BeastChakra.Coeurl);

    private static int Coeurl => Gauge.CoeurlFury;

    private static Nadi Nadi => Gauge.Nadi;

    private static bool BothNadisOpen => Nadi.ToString() == "Lunar, Solar";

    private static bool SolarNadi => Nadi is Nadi.Solar;

    private static bool LunarNadi => Nadi is Nadi.Lunar;

    #endregion

    #region ID's

    public const uint
        Bootshine = 53,
        TrueStrike = 54,
        SnapPunch = 56,
        TwinSnakes = 61,
        ArmOfTheDestroyer = 62,
        Demolish = 66,
        DragonKick = 74,
        Rockbreaker = 70,
        Thunderclap = 25762,
        HowlingFist = 25763,
        FourPointFury = 16473,
        FormShift = 4262,
        SixSidedStar = 16476,
        ShadowOfTheDestroyer = 25767,
        LeapingOpo = 36945,
        RisingRaptor = 36946,
        PouncingCoeurl = 36947,
        TrueNorth = 7546,

        //Blitzes
        PerfectBalance = 69,
        MasterfulBlitz = 25764,
        ElixirField = 3545,
        ElixirBurst = 36948,
        FlintStrike = 25882,
        RisingPhoenix = 25768,
        CelestialRevolution = 25765,
        TornadoKick = 3543,
        PhantomRush = 25769,

        //Riddles + Buffs
        RiddleOfEarth = 7394,
        EarthsReply = 36944,
        RiddleOfFire = 7395,
        FiresReply = 36950,
        RiddleOfWind = 25766,
        WindsReply = 36949,
        Brotherhood = 7396,
        Mantra = 65,

        //Meditations
        InspiritedMeditation = 36941,
        SteeledMeditation = 36940,
        EnlightenedMeditation = 36943,
        ForbiddenMeditation = 36942,
        TheForbiddenChakra = 3547,
        Enlightenment = 16474,
        SteelPeak = 25761;

    internal static class Buffs
    {
        public const ushort
            TwinSnakes = 101,
            Mantra = 102,
            OpoOpoForm = 107,
            RaptorForm = 108,
            CoeurlForm = 109,
            PerfectBalance = 110,
            RiddleOfEarth = 1179,
            RiddleOfFire = 1181,
            Brotherhood = 1185,
            TrueNorth = 1250,
            FormlessFist = 2513,
            RiddleOfWind = 2687,
            EarthsRumination = 3841,
            WindsRumination = 3842,
            FiresRumination = 3843;
    }

    #endregion

}
