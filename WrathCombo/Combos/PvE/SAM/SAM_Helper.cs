﻿using Dalamud.Game.ClientState.JobGauge.Enums;
using Dalamud.Game.ClientState.JobGauge.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using static FFXIVClientStructs.FFXIV.Client.Game.ActionManager;
using static WrathCombo.Combos.PvE.SAM.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
using static WrathCombo.Data.ActionWatching;
using ActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;
namespace WrathCombo.Combos.PvE;

internal partial class SAM
{
    internal static bool RefreshFugetsu =>
        GetStatusEffectRemainingTime(Buffs.Fugetsu) < GetStatusEffectRemainingTime(Buffs.Fuka);

    internal static bool RefreshFuka =>
        GetStatusEffectRemainingTime(Buffs.Fuka) < GetStatusEffectRemainingTime(Buffs.Fugetsu);

    internal static bool EnhancedSenei =>
        TraitLevelChecked(Traits.EnhancedHissatsu);

    internal static int SenCount =>
        GetSenCount();

    internal static bool M6SReady =>
        !HiddenFeaturesData.IsEnabledWith(Preset.SAM_Hid_M6SHoldSquirrelBurst, () =>
            HiddenFeaturesData.Targeting.R6SSquirrel && CombatEngageDuration().TotalSeconds < 275);

    #region Meikyo

    internal static bool UseMeikyo()
    {
        int meikyoUsed = CombatActions.Count(x => x == MeikyoShisui);
        float gcd = GetAdjustedRecastTime(ActionType.Action, Hakaze) / 100f;

        if (ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.Tendo) && !HasStatusEffect(Buffs.MeikyoShisui) &&
            (JustUsed(Gekko) || JustUsed(Kasha) || JustUsed(Yukikaze)))
        {
            if (SAM_ST_MeikyoLogic == 1 && (SAM_ST_MeikyoBossOption == 0 || InBossEncounter()) ||
                IsEnabled(Preset.SAM_ST_SimpleMode) && InBossEncounter())
            {
                if (EnhancedSenei)
                {
                    //if no opener
                    if ((IsEnabled(Preset.SAM_ST_Opener) && SAM_Balance_Content == 1 && !InBossEncounter() ||
                         IsNotEnabled(Preset.SAM_ST_Opener)) &&
                        meikyoUsed < 1 && !HasStatusEffect(Buffs.TsubameReady))
                        return true;

                    if (HasStatusEffect(Buffs.TsubameReady))
                    {
                        switch (gcd)
                        {
                            //2.14 GCD
                            case >= 2.09f when GetCooldownRemainingTime(Senei) <= 10 &&
                                               (meikyoUsed % 7 is 1 or 2 && SenCount is 3 ||
                                                meikyoUsed % 7 is 3 or 4 && SenCount is 2 ||
                                                meikyoUsed % 7 is 5 or 6 && SenCount is 1):
                            //2.08 gcd
                            case <= 2.08f when GetCooldownRemainingTime(Senei) <= 10 && SenCount is 3:
                                return true;
                        }
                    }

                    // reset meikyo
                    if (gcd >= 2.09f && meikyoUsed % 7 is 0 && JustUsed(Yukikaze))
                        return true;
                }

                //Pre Enhanced Senei
                if (!EnhancedSenei && ActionReady(MeikyoShisui) && !HasStatusEffect(Buffs.TsubameReady))
                    return true;
            }

            if (IsEnabled(Preset.SAM_ST_SimpleMode) && !InBossEncounter() ||
                SAM_ST_MeikyoLogic == 1 && SAM_ST_MeikyoBossOption == 1 && !InBossEncounter())
                return true;

            if (SAM_ST_MeikyoLogic == 0 && SenCount is 3)
                return true;
        }


        return false;
    }

    #endregion

    #region Iaijutsu

    internal static bool UseIaijutsu(bool useHiganbana, bool useTenkaGoken, bool useMidare)
    {
        int higanbanaHPThreshold = SAM_ST_HiganbanaHPThreshold;
        int higanbanaRefresh = SAM_ST_HiganbanaRefresh;

        if (LevelChecked(Iaijutsu) && InActionRange(OriginalHook(Iaijutsu)))
        {
            //Higanbana
            if (IsEnabled(Preset.SAM_ST_AdvancedMode) &&
                useHiganbana &&
                SenCount is 1 && GetTargetHPPercent() > higanbanaHPThreshold &&
                (SAM_ST_HiganbanaBossOption == 0 || TargetIsBoss()) &&
                CanApplyStatus(CurrentTarget, Debuffs.Higanbana) &&
                (JustUsed(MeikyoShisui, 15f) && GetStatusEffectRemainingTime(Debuffs.Higanbana, CurrentTarget) <= higanbanaRefresh ||
                 !HasStatusEffect(Debuffs.Higanbana, CurrentTarget)))
                return true;

            //Tenka Goken
            if (useTenkaGoken && SenCount is 2 &&
                !LevelChecked(MidareSetsugekka))
                return true;

            //Midare Setsugekka
            if (useMidare && SenCount is 3 &&
                LevelChecked(MidareSetsugekka) && !HasStatusEffect(Buffs.TsubameReady))
                return true;

            //Higanbana Simple Mode
            if (IsEnabled(Preset.SAM_ST_SimpleMode) &&
                useHiganbana &&
                SenCount is 1 && GetTargetHPPercent() > 1 && TargetIsBoss() &&
                CanApplyStatus(CurrentTarget, Debuffs.Higanbana) &&
                (JustUsed(MeikyoShisui, 15f) && GetStatusEffectRemainingTime(Debuffs.Higanbana, CurrentTarget) <= 15 ||
                 !HasStatusEffect(Debuffs.Higanbana, CurrentTarget)))
                return true;
        }
        return false;
    }

    #endregion

    #region Rescourses

    internal static class SAMKenki
    {
        internal const int MaxKenki = 100;

        internal static int Zanshin => GetResourceCost(SAM.Zanshin);

        internal static int Senei => GetResourceCost(SAM.Senei);

        internal static int Guren => GetResourceCost(SAM.Guren);

        internal static int Shinten => GetResourceCost(SAM.Shinten);
    }

    #endregion

    #region Burst Management

    internal static bool UseIkishoten() =>
        ActionReady(Ikishoten) &&
        !HasStatusEffect(Buffs.ZanshinReady) && Kenki <= 50 &&
        (NumberOfGcdsUsed is 2 ||
         JustUsed(TendoKaeshiSetsugekka, 15f) ||
         !LevelChecked(TendoKaeshiSetsugekka));

    internal static bool UseSenei() =>
        ActionReady(Senei) && NumberOfGcdsUsed >= 4 &&
        InActionRange(Senei) &&
        (!LevelChecked(KaeshiSetsugekka) ||
         LevelChecked(KaeshiSetsugekka) &&
         (JustUsed(KaeshiSetsugekka, 5f) ||
          JustUsed(TendoSetsugekka, 5f)));

    internal static bool UseTsubame() =>
        LevelChecked(TsubameGaeshi) &&
        (HasStatusEffect(Buffs.TendoKaeshiSetsugekkaReady) ||
         HasStatusEffect(Buffs.TsubameReady) && (SenCount is 3 || GetCooldownRemainingTime(Senei) > 33));

    internal static bool UseShoha() =>
        ActionReady(Shoha) && MeditationStacks is 3 &&
        InActionRange(Shoha) &&
        (EnhancedSenei && JustUsed(Senei, 20f) ||
         !EnhancedSenei && JustUsed(KaeshiSetsugekka, 10f));

    //TODO Buffcheck
    internal static bool UseZanshin() =>
        ActionReady(Zanshin) && Kenki >= SAMKenki.Zanshin &&
        InActionRange(Zanshin) && HasStatusEffect(Buffs.ZanshinReady) &&
        (JustUsed(Senei, 20f) || GetStatusEffectRemainingTime(Buffs.ZanshinReady) <= 8);

    internal static bool UseShinten()
    {
        int shintenTreshhold = SAM_ST_ExecuteThreshold;

        if (ActionReady(Shinten) && Kenki >= SAMKenki.Shinten && InActionRange(Shinten))
        {
            if (GetTargetHPPercent() < shintenTreshhold)
                return true;

            if (Kenki >= 95)
                return true;

            if (EnhancedSenei &&
                !HasStatusEffect(Buffs.ZanshinReady))
            {
                if (JustUsed(Senei, 15f) &&
                    !JustUsed(Ikishoten))
                    return true;

                if (GetCooldownRemainingTime(Senei) >= 25 &&
                    Kenki >= SAM_ST_KenkiOvercapAmount)
                    return true;
            }

            if (!EnhancedSenei && Kenki >= SAM_ST_KenkiOvercapAmount)
                return true;
        }
        return false;
    }

    internal static bool UseOgi()
    {
        if (NamikiriReady)
            return true;

        if (ActionReady(OgiNamikiri) && InActionRange(OriginalHook(OgiNamikiri)) &&
            HasStatusEffect(Buffs.OgiNamikiriReady) && NumberOfGcdsUsed >= 5)
        {
            if (GetStatusEffectRemainingTime(Buffs.OgiNamikiriReady) <= 8)
                return true;

            if (IsEnabled(Preset.SAM_ST_AdvancedMode) &&
                IsNotEnabled(Preset.SAM_ST_CDs_UseHiganbana) && JustUsed(Ikishoten, 15f))
                return true;

            if (JustUsed(Higanbana, 15f))
                return true;

            if (IsEnabled(Preset.SAM_ST_AdvancedMode) &&
                SAM_ST_HiganbanaBossOption == 1 && !TargetIsBoss())
                return true;
        }
        return false;
    }

    #endregion

    #region Openers

    internal static WrathOpener Opener()
    {
        if (Lvl70.LevelChecked)
            return Lvl70;

        if (Lvl80.LevelChecked)
            return Lvl80;

        if (Lvl90.LevelChecked)
            return Lvl90;

        if (Lvl100.LevelChecked)
            return Lvl100;

        return WrathOpener.Dummy;
    }

    internal static SAMLvl70Opener Lvl70 = new();
    internal static SAMLvl80Opener Lvl80 = new();
    internal static SAMLvl90Opener Lvl90 = new();
    internal static SAMLvl100Opener Lvl100 = new();

    internal class SAMLvl70Opener : WrathOpener
    {
        public override int MinOpenerLevel => 70;

        public override int MaxOpenerLevel => 70;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth, //2
            Gekko,
            Kasha,
            Ikishoten,
            Yukikaze,
            Shinten,
            MidareSetsugekka,
            Shinten,
            Hakaze,
            Guren,
            Yukikaze,
            Shinten,
            Higanbana
        ];

        internal override UserData ContentCheckConfig => SAM_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => SAM_Opener_PrePullDelay)
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([2], 11, () => !TargetNeedsPositionals())
        ];

        public override bool HasCooldowns() =>
            IsOffCooldown(MeikyoShisui) &&
            GetRemainingCharges(Role.TrueNorth) >= 1 &&
            IsOffCooldown(Guren) &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    internal class SAMLvl80Opener : WrathOpener
    {
        public override int MinOpenerLevel => 80;

        public override int MaxOpenerLevel => 80;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth, //2
            Gekko,
            Ikishoten,
            Kasha,
            Yukikaze,
            MidareSetsugekka,
            Senei,
            KaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Higanbana,
            Gekko,
            Kasha,
            Hakaze,
            Yukikaze,
            MidareSetsugekka,
            Shoha,
            KaeshiSetsugekka
        ];

        internal override UserData ContentCheckConfig => SAM_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => SAM_Opener_PrePullDelay)
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([2], 11, () => !TargetNeedsPositionals())
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            GetRemainingCharges(Role.TrueNorth) >= 1 &&
            IsOffCooldown(Senei) &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    internal class SAMLvl90Opener : WrathOpener
    {
        public override int MinOpenerLevel => 90;

        public override int MaxOpenerLevel => 90;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth, //2
            Gekko,
            Ikishoten,
            Kasha,
            Yukikaze,
            MidareSetsugekka,
            Senei,
            KaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Higanbana,
            OgiNamikiri,
            Shoha,
            KaeshiNamikiri,
            Kasha,
            Gekko,
            Hakaze,
            Yukikaze,
            MidareSetsugekka,
            KaeshiSetsugekka
        ];

        internal override UserData ContentCheckConfig => SAM_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => SAM_Opener_PrePullDelay)
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([2], 11, () => !TargetNeedsPositionals())
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            GetRemainingCharges(Role.TrueNorth) >= 1 &&
            IsOffCooldown(Senei) &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    internal class SAMLvl100Opener : WrathOpener
    {
        public override int MinOpenerLevel => 100;

        public override int MaxOpenerLevel => 109;

        public override List<uint> OpenerActions { get; set; } =
        [
            MeikyoShisui,
            Role.TrueNorth, //2
            Gekko,
            Kasha,
            Ikishoten,
            Yukikaze,
            TendoSetsugekka,
            Senei,
            TendoKaeshiSetsugekka,
            MeikyoShisui,
            Gekko,
            Zanshin,
            Higanbana,
            OgiNamikiri,
            Shoha,
            KaeshiNamikiri,
            Kasha,
            Shinten,
            Gekko,
            Gyoten, //20
            Gyofu,
            Yukikaze,
            Shinten,
            TendoSetsugekka,
            TendoKaeshiSetsugekka
        ];

        internal override UserData ContentCheckConfig => SAM_Balance_Content;

        public override List<(int[] Steps, Func<int> HoldDelay)> PrepullDelays { get; set; } =
        [
            ([2], () => SAM_Opener_PrePullDelay)
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([2], 11, () => !TargetNeedsPositionals()),
            ([20], Shinten, () => Kenki >= SAMKenki.Shinten)
        ];

        public override bool HasCooldowns() =>
            GetRemainingCharges(MeikyoShisui) is 2 &&
            GetRemainingCharges(Role.TrueNorth) >= 1 &&
            IsOffCooldown(Senei) &&
            IsOffCooldown(Ikishoten) &&
            SenCount is 0;
    }

    #endregion

    #region Gauge

    internal static SAMGauge Gauge = GetJobGauge<SAMGauge>();

    internal static bool HasGetsu => Gauge.HasGetsu;

    internal static bool HasSetsu => Gauge.HasSetsu;

    internal static bool HasKa => Gauge.HasKa;

    internal static byte Kenki => Gauge.Kenki;

    internal static byte MeditationStacks => Gauge.MeditationStacks;

    internal static Kaeshi Kaeshi => Gauge.Kaeshi;

    internal static bool NamikiriReady => Kaeshi is Kaeshi.Namikiri;

    private static int GetSenCount()
    {
        int senCount = 0;

        if (HasGetsu)
            senCount++;

        if (HasSetsu)
            senCount++;

        if (HasKa)
            senCount++;

        return senCount;
    }

    #endregion

    #region ID's

    public const uint
        Hakaze = 7477,
        Yukikaze = 7480,
        Gekko = 7481,
        Enpi = 7486,
        Jinpu = 7478,
        Kasha = 7482,
        Shifu = 7479,
        Mangetsu = 7484,
        Fuga = 7483,
        Oka = 7485,
        Higanbana = 7489,
        TenkaGoken = 7488,
        MidareSetsugekka = 7487,
        Shinten = 7490,
        Kyuten = 7491,
        Hagakure = 7495,
        Guren = 7496,
        Meditate = 7497,
        Senei = 16481,
        MeikyoShisui = 7499,
        Seigan = 7501,
        ThirdEye = 7498,
        Iaijutsu = 7867,
        TsubameGaeshi = 16483,
        KaeshiHiganbana = 16484,
        Shoha = 16487,
        Ikishoten = 16482,
        Fuko = 25780,
        OgiNamikiri = 25781,
        KaeshiNamikiri = 25782,
        Yaten = 7493,
        Gyoten = 7492,
        KaeshiSetsugekka = 16486,
        TendoGoken = 36965,
        TendoKaeshiSetsugekka = 36968,
        Zanshin = 36964,
        TendoSetsugekka = 36966,
        Tengentsu = 7498,
        Gyofu = 36963;

    public static class Buffs
    {
        public const ushort
            MeikyoShisui = 1233,
            EnhancedEnpi = 1236,
            EyesOpen = 1252,
            Meditate = 1231,
            OgiNamikiriReady = 2959,
            Fuka = 1299,
            Fugetsu = 1298,
            TsubameReady = 4216,
            TendoKaeshiSetsugekkaReady = 4218,
            KaeshiGokenReady = 3852,
            TendoKaeshiGokenReady = 4217,
            ZanshinReady = 3855,
            Tengentsu = 3853,
            Tendo = 3856;
    }

    public static class Debuffs
    {
        public const ushort
            Higanbana = 1228;
    }

    public static class Traits
    {
        public const ushort
            EnhancedHissatsu = 591,
            EnhancedMeikyoShishui = 443,
            EnhancedMeikyoShishui2 = 593;
    }

    #endregion
}
