using System.Collections.Frozen;
using System.Collections.Generic;
using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Data;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.NIN.Config;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
namespace WrathCombo.Combos.PvE;

internal partial class NIN
{
    #region ID's

    public const byte ClassID = 29;
    public const byte JobID = 30;

    public const uint
        SpinningEdge = 2240,
        ShadeShift = 2241,
        GustSlash = 2242,
        Hide = 2245,
        Assassinate = 2246,
        ThrowingDaggers = 2247,
        Mug = 2248,
        DeathBlossom = 2254,
        AeolianEdge = 2255,
        TrickAttack = 2258,
        Kassatsu = 2264,
        ArmorCrush = 3563,
        DreamWithinADream = 3566,
        TenChiJin = 7403,
        Bhavacakra = 7402,
        HakkeMujinsatsu = 16488,
        Meisui = 16489,
        Bunshin = 16493,
        PhantomKamaitachi = 25774,
        ForkedRaiju = 25777,
        FleetingRaiju = 25778,
        HellfrogMedium = 7401,
        HollowNozuchi = 25776,
        TenriJendo = 36961,
        KunaisBane = 36958,
        ZeshoMeppo = 36960,
        Dokumori = 36957,

        //Mudras
        Ninjutsu = 2260,
        Rabbit = 2272,

        //-- initial state mudras (the ones with charges)
        Ten = 2259,
        Chi = 2261,
        Jin = 2263,

        //-- mudras used for combos (the ones used while you have the mudra buff)
        TenCombo = 18805,
        ChiCombo = 18806,
        JinCombo = 18807,

        //Ninjutsu
        FumaShuriken = 2265,
        Hyoton = 2268,
        Doton = 2270,
        Katon = 2266,
        Suiton = 2271,
        Raiton = 2267,
        Huton = 2269,
        GokaMekkyaku = 16491,
        HyoshoRanryu = 16492,

        //TCJ Jutsus (why they have another ID I will never know)
        TCJFumaShurikenTen = 18873,
        TCJFumaShurikenChi = 18874,
        TCJFumaShurikenJin = 18875,
        TCJKaton = 18876,
        TCJRaiton = 18877,
        TCJHyoton = 18878,
        TCJHuton = 18879,
        TCJDoton = 18880,
        TCJSuiton = 18881;

    public static class Buffs
    {
        public const ushort
            Mudra = 496,
            Kassatsu = 497,
            //Suiton = 507,
            Higi = 3850,
            TenriJendoReady = 3851,
            ShadowWalker = 3848,
            Hidden = 614,
            TenChiJin = 1186,
            AssassinateReady = 1955,
            RaijuReady = 2690,
            PhantomReady = 2723,
            Meisui = 2689,
            Doton = 501,
            Bunshin = 1954;
    }

    public static class Debuffs
    {
        public const ushort
            Dokumori = 3849,
            TrickAttack = 3254,
            KunaisBane = 3906,
            Mug = 638;
    }

    public static class Traits
    {
        public const uint
            EnhancedKasatsu = 250;
    }

    #endregion

    #region Variables

    static NINGauge gauge = GetJobGauge<NINGauge>();
    
    public static FrozenSet<uint> MudraSigns = [Ten, Chi, Jin, TenCombo, ChiCombo, JinCombo];
    
    internal static bool InMudra = false;
    internal static bool STSimpleMode => IsEnabled(Preset.NIN_ST_SimpleMode);
    internal static bool AoESimpleMode => IsEnabled(Preset.NIN_AoE_SimpleMode);
    internal static bool NinjaWeave => CanWeave(.6f, 10);
    internal static bool TNArmorCrush => !OnTargetsFlank() && TargetNeedsPositionals() && Role.CanTrueNorth();
    internal static bool TNAeolianEdge => !OnTargetsRear() && TargetNeedsPositionals() && Role.CanTrueNorth();
    internal static bool HasKassatsu => HasStatusEffect(Buffs.Kassatsu);
    internal static float KassatsuRemaining => GetStatusEffectRemainingTime(Buffs.Kassatsu);
    internal static bool NinkiWillOvercap => gauge.Ninki > 50;
    internal static bool HasDoton => HasStatusEffect(Buffs.Doton);
    internal static float DotonRemaining => GetStatusEffectRemainingTime(Buffs.Doton);
    
    internal static int BhavaPool()
    {
        if (HasStatusEffect(Buffs.Bunshin))
            return ComboAction == GustSlash ? 75: 95;
        return ComboAction == GustSlash ? 90 : 100;
    }
    
    
    #region Mudra Logic
    internal static bool MudraPhase => OriginalHook(Ten) != Ten || OriginalHook(Chi) != Chi || OriginalHook(Jin) != Jin;
    internal static bool MudraReady => ActionReady(Ten);
    internal static uint MudraCharges => GetRemainingCharges(Ten);
    internal static bool MudraAlmostReady => MudraCharges == 1 && GetCooldownChargeRemainingTime(Ten) < 3;
    internal static bool MudraPool => MudraAlmostReady ||
                                      MudraCharges >= 1 && TrickDebuff ||
                                      MudraCharges == 2;
    #endregion
    
    #region Buff Window Logic
    internal static bool BuffWindow => TrickDebuff || MugDebuff && TrickCD >= 30;
    internal static float TrickCD => GetCooldownRemainingTime(OriginalHook(TrickAttack));
    internal static float MugCD => GetCooldownRemainingTime(OriginalHook(Mug));
    
    internal static bool CanTrick => ActionReady(OriginalHook(TrickAttack)) && NinjaWeave &&
                                       (!MudraPhase || HasKassatsu) &&
                                       HasStatusEffect(Buffs.ShadowWalker) && 
                                       (MugDebuff || MugCD >= 50);
    
    internal static bool CanMug => ActionReady(OriginalHook(Mug)) && CanDelayedWeave(1.25f, .6f, 10) && 
                                   (HasStatusEffect(Buffs.ShadowWalker) || !LevelChecked(Huton)) &&
                                   (!MudraPhase || HasKassatsu) &&
                                   (LevelChecked(Dokumori) && GetTargetDistance() <= 8 || InMeleeRange());
    
    internal static bool TrickDebuff => HasStatusEffect(Debuffs.TrickAttack, CurrentTarget) || 
                                        HasStatusEffect(Debuffs.KunaisBane, CurrentTarget) || 
                                        JustUsed(OriginalHook(TrickAttack));
    internal static bool MugDebuff => HasStatusEffect(Debuffs.Mug, CurrentTarget) || 
                                      HasStatusEffect(Debuffs.Dokumori, CurrentTarget) ||
                                      JustUsed(OriginalHook(Mug));
    internal static int s => ComboAction == GustSlash ? 90: 100;

    #endregion
    
    #region Action Logic

    internal static bool CanBhavacakra => NinjaWeave && !MudraPhase;
    
    internal static bool BhavacakraPooling => gauge.Ninki >= BhavaPool() || 
                                              TrickDebuff && gauge.Ninki >= 50 ||
                                              MugCD < 5 && gauge.Ninki >= 50;
    
    internal static bool CanHellfrogMedium => ActionReady(OriginalHook(HellfrogMedium)) && NinjaWeave &&
                                          !MudraPhase &&
                                          (gauge.Ninki >= BhavaPool() || 
                                           TrickDebuff && gauge.Ninki >= 50 ||
                                           MugCD < 5 && gauge.Ninki >= 50);

    internal static bool CanMeisui => ActionReady(Meisui) && NinjaWeave && BuffWindow && HasStatusEffect(Buffs.ShadowWalker) &&
                                      !MudraPhase;

    internal static bool CanAssassinate => ActionReady(OriginalHook(Assassinate)) && NinjaWeave && BuffWindow && 
                                           !MudraPhase;

    internal static bool CanTenChiJin => IsOffCooldown(TenChiJin) && LevelChecked(TenChiJin) && NinjaWeave && BuffWindow 
                                         && !MudraPhase && !MudraAlmostReady;

    internal static bool CanTenriJindo => NinjaWeave && HasStatusEffect(Buffs.TenriJendoReady);

    internal static bool CanBunshin => ActionReady(Bunshin) && NinjaWeave && 
                                       (!MudraPhase || HasKassatsu);

    internal static bool CanKassatsu => ActionReady(Kassatsu) && NinjaWeave && HasStatusEffect(Buffs.ShadowWalker) &&
                                        !MudraPhase && 
                                        (TrickCD < 10 || BuffWindow);
     
    internal static bool CanUseRaiton =>  LevelChecked(FumaShuriken) && 
                                          (MudraPool || 
                                           MudraReady && !LevelChecked(Suiton) || //Dont Pool because of Suiton
                                           MudraReady && (!NIN_ST_AdvancedMode_Raiton_Options[0] && !STSimpleMode || //Dont Pool because of Raiton Option
                                           MudraReady && NIN_ST_AdvancedMode_Raiton_Options[1] &&!InMeleeRange() && 
                                           GetCooldownChargeRemainingTime(Ten) <= GetCooldownRemainingTime(OriginalHook(TrickAttack) - 10) || //Uptime option
                                           MudraPhase && !HasKassatsu || // Finish if you dont have Kassatsu for Hyosho Ranryu
                                           MudraPhase && !LevelChecked(HyoshoRanryu))); // Use if you have Kassatsu before you get Hosho Ranryun // Use if you have Kassatsu before you get Hosho Ranryu
    
    internal static bool CanUseSuiton => !HasStatusEffect(Buffs.ShadowWalker) && LevelChecked(Suiton) && 
                                         (MudraPhase || MudraReady);

    internal static bool CanUseHyoshoRanryu => HasKassatsu && LevelChecked(HyoshoRanryu) &&
                                               (MudraPhase || MudraReady) &&
                                               (TrickDebuff || KassatsuRemaining < 3);
    
    
    internal static bool CanUseDoton =>  LevelChecked(Doton) && 
                                         (MudraPool || //Start based on pooling
                                         MudraReady && !LevelChecked(Huton) || //Start without pooling because of no Suiton
                                         MudraPhase && !HasKassatsu || // Finish if you dont have Kassatsu for Hyosho Ranryu
                                         MudraPhase && !LevelChecked(HyoshoRanryu)); // Use if you have Kassatsu before you get Hosho Ranryu
    
    internal static bool CanUseKaton =>  LevelChecked(Katon) &&
                                         (MudraPool || //Start based on pooling
                                         MudraReady && !LevelChecked(Huton) || //Start without pooling because of no Suiton
                                         MudraPhase && !HasKassatsu || // Finish if you dont have Kassatsu for Hyosho Ranryu
                                         MudraPhase && !LevelChecked(HyoshoRanryu)); // Use if you have Kassatsu before you get Hosho Ranryu
    
    internal static bool CanUseHuton => GetCooldownRemainingTime(OriginalHook(TrickAttack)) <= 18 && !HasStatusEffect(Buffs.ShadowWalker) && LevelChecked(Huton) && 
                                         (MudraPhase || MudraReady);

    internal static bool CanUseGokaMekkyaku => HasKassatsu && LevelChecked(GokaMekkyaku) &&
                                               (MudraPhase || MudraReady) &&
                                               (TrickDebuff || KassatsuRemaining < 3);

    internal static bool CanPhantomKamaitachi => HasStatusEffect(Buffs.PhantomReady) &&
                                              (TrickDebuff && ComboAction != GustSlash ||
                                               !TrickDebuff);

    internal static bool CanThrowingDaggers => ActionReady(ThrowingDaggers) && HasTarget() && !InMeleeRange() &&
                                               !HasStatusEffect(Buffs.RaijuReady);
    internal static bool CanThrowingDaggersAoE => ActionReady(ThrowingDaggers) && HasTarget() && GetTargetDistance() >= 4.5 &&
                                               !HasStatusEffect(Buffs.RaijuReady);

    internal static bool CanRaiju => HasStatusEffect(Buffs.RaijuReady);
    
    #endregion
    
    #endregion

    #region Ninjutsu Methods
    internal static uint STTenChiJin(uint actionId)
    {
        if (OriginalHook(Ten) == TCJFumaShurikenTen)
            return OriginalHook(Ten);
        if (OriginalHook(Chi) == TCJRaiton)
            return OriginalHook(Chi);
        return OriginalHook(Jin) == TCJSuiton ? OriginalHook(Jin) : actionId;
    }
    internal static uint AoETenChiJin(uint actionId)
    {
        if (OriginalHook(Jin) == TCJFumaShurikenJin)
            return OriginalHook(Jin);
        if (OriginalHook(Ten) == TCJKaton)
            return OriginalHook(Ten);
        return OriginalHook(Chi) == TCJDoton ? OriginalHook(Chi) : actionId;
    }
    // Single Target
    internal static uint UseFumaShuriken(uint actionId)
    {
        return OriginalHook(Ninjutsu) == FumaShuriken ? OriginalHook(Ninjutsu) : Ten;
    }
    internal static uint UseRaiton(uint actionId) // Ten Chi
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu) 
            return HasKassatsu ? TenCombo : Ten;
        return OriginalHook(Ninjutsu) == FumaShuriken &&
               ActionWatching.LastAction is TenCombo or Ten or JinCombo or Jin 
            ? ChiCombo 
            : OriginalHook(Ninjutsu);
    }
    internal static uint UseHyoshoRanryu(uint actionId) // Ten Jin
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
            return TenCombo;
        return OriginalHook(Ninjutsu) == FumaShuriken &&
               ActionWatching.LastAction is TenCombo 
            ? JinCombo 
            : OriginalHook(Ninjutsu);
    }
    internal static uint UseSuiton(uint actionId) // Ten Chi Jin
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
            return Ten;
        if (ActionWatching.LastAction is Ten)
            return ChiCombo;
        return ActionWatching.LastAction is ChiCombo ? JinCombo : OriginalHook(Ninjutsu);
    }
    //Multi Target
    internal static uint UseGokaMekkyaku(uint actionId) // Jin Ten
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
            return HasKassatsu ? JinCombo : Jin;
        return OriginalHook(Ninjutsu) == FumaShuriken && 
               ActionWatching.LastAction is Jin or JinCombo 
            ? TenCombo 
            : OriginalHook(Ninjutsu);
    }
    internal static uint UseKaton(uint actionId) // Jin Ten
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
        {
            if (LevelChecked(Jin))
                return HasKassatsu ? JinCombo : Jin;
            return HasKassatsu ? ChiCombo : Chi;
        }
        return OriginalHook(Ninjutsu) == FumaShuriken && 
               ActionWatching.LastAction is Jin or JinCombo or ChiCombo or Chi
            ? TenCombo 
            : OriginalHook(Ninjutsu);
    }
    internal static uint UseDoton(uint actionId)  //Jin Ten Chi
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
            return HasKassatsu ? JinCombo : Jin;
        if (ActionWatching.LastAction is Jin or JinCombo)
            return TenCombo;
        return ActionWatching.LastAction is TenCombo ? ChiCombo : OriginalHook(Ninjutsu);
    }
    internal static uint UseHuton(uint actionId) // Jin Chi Ten
    {
        if (OriginalHook(Ninjutsu) == Ninjutsu)
            return Jin;
        if (ActionWatching.LastAction is Jin)
            return ChiCombo;
        return ActionWatching.LastAction is ChiCombo ? TenCombo : Huton;
    }
    #endregion

    #region old shit
    public static uint CurrentNinjutsu => OriginalHook(Ninjutsu);
    
    #region Mudra
    internal class MudraCasting
    {
        public enum MudraState
        {
            None,
            CastingFumaShuriken,
            CastingKaton,
            CastingRaiton,
            CastingHyoton,
            CastingHuton,
            CastingDoton,
            CastingSuiton,
            CastingGokaMekkyaku,
            CastingHyoshoRanryu
        }

        public MudraState CurrentMudra = MudraState.None;
        

        ///<summary> Checks if the player is in a state to be able to cast a ninjitsu.</summary>
        private static bool CanCast()
        {
            if (InMudra)
                return true;

            float gcd = GetCooldown(GustSlash).CooldownTotal;

            if (gcd == 0.5)
                return true;

            if (GetRemainingCharges(Ten) == 0 &&
                !HasStatusEffect(Buffs.Mudra) &&
                !HasStatusEffect(Buffs.Kassatsu))
                return false;

            return true;
        }

        /// <summary> Simple method of casting Fuma Shuriken.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastFumaShuriken(ref uint actionID)
        {
            if (FumaShuriken.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingFumaShuriken)
            {
                if (!CanCast() || ActionWatching.LastAction == FumaShuriken)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Ten or TenCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Ten);
                CurrentMudra = MudraState.CastingFumaShuriken;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Raiton.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastRaiton(ref uint actionID)
        {
            if (Raiton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingRaiton)
            {
                if (!CanCast() || ActionWatching.LastAction == Raiton)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Ten or TenCombo)
                {
                    actionID = OriginalHook(Chi);

                    return true;
                }

                if (ActionWatching.LastAction == ChiCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Ten);
                CurrentMudra = MudraState.CastingRaiton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Katon.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastKaton(ref uint actionID)
        {
            if (Katon.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingKaton)
            {
                if (!CanCast() || ActionWatching.LastAction == Katon)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Chi or ChiCombo)
                {
                    actionID = OriginalHook(Ten);

                    return true;
                }

                if (ActionWatching.LastAction == TenCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Chi);
                CurrentMudra = MudraState.CastingKaton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Hyoton.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastHyoton(ref uint actionID)
        {
            if (Hyoton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingHyoton)
            {
                if (!CanCast() || HasStatusEffect(Buffs.Kassatsu) || ActionWatching.LastAction == Hyoton)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction == TenCombo)
                {
                    actionID = OriginalHook(Jin);

                    return true;
                }

                if (ActionWatching.LastAction == JinCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Ten);
                CurrentMudra = MudraState.CastingHyoton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Huton.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastHuton(ref uint actionID)
        {
            if (Huton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingHuton)
            {
                if (!CanCast() || ActionWatching.LastAction == Huton)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Chi or ChiCombo)
                {
                    actionID = OriginalHook(Jin);

                    return true;
                }

                if (ActionWatching.LastAction == JinCombo)
                {
                    actionID = OriginalHook(Ten);

                    return true;
                }

                if (ActionWatching.LastAction == TenCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Chi);
                CurrentMudra = MudraState.CastingHuton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Doton.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastDoton(ref uint actionID)
        {
            if (Doton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingDoton)
            {
                if (!CanCast() || ActionWatching.LastAction == Doton)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Ten or TenCombo)
                {
                    actionID = OriginalHook(Jin);

                    return true;
                }

                if (ActionWatching.LastAction == JinCombo)
                {
                    actionID = OriginalHook(Chi);

                    return true;
                }

                if (ActionWatching.LastAction == ChiCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Ten);
                CurrentMudra = MudraState.CastingDoton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Suiton.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastSuiton(ref uint actionID)
        {
            if (Suiton.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingSuiton)
            {
                if (!CanCast() || ActionWatching.LastAction == Suiton)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction is Ten or TenCombo)
                {
                    actionID = OriginalHook(Chi);

                    return true;
                }

                if (ActionWatching.LastAction == ChiCombo)
                {
                    actionID = OriginalHook(Jin);

                    return true;
                }

                if (ActionWatching.LastAction == JinCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Ten);
                CurrentMudra = MudraState.CastingSuiton;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Goka Mekkyaku.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastGokaMekkyaku(ref uint actionID)
        {
            if (GokaMekkyaku.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingGokaMekkyaku)
            {
                if (!CanCast() || !HasStatusEffect(Buffs.Kassatsu) || ActionWatching.LastAction == GokaMekkyaku)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction == ChiCombo)
                {
                    actionID = OriginalHook(Ten);

                    return true;
                }

                if (ActionWatching.LastAction == TenCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Chi);
                CurrentMudra = MudraState.CastingGokaMekkyaku;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        /// <summary> Simple method of casting Hyosho Ranryu.</summary>
        /// <param name="actionID">The actionID from the combo.</param>
        /// <returns>True if in a state to cast or continue the ninjitsu, modifies actionID to the step of the ninjitsu.</returns>
        public bool CastHyoshoRanryu(ref uint actionID)
        {
            if (HyoshoRanryu.LevelChecked() && CurrentMudra is MudraState.None or MudraState.CastingHyoshoRanryu)
            {
                if (!CanCast() || !HasStatusEffect(Buffs.Kassatsu) || ActionWatching.LastAction == HyoshoRanryu)
                {
                    CurrentMudra = MudraState.None;

                    return false;
                }

                if (ActionWatching.LastAction == ChiCombo)
                {
                    actionID = OriginalHook(Jin);

                    return true;
                }

                if (ActionWatching.LastAction == JinCombo)
                {
                    actionID = OriginalHook(Ninjutsu);

                    return true;
                }

                actionID = OriginalHook(Chi);
                CurrentMudra = MudraState.CastingHyoshoRanryu;

                return true;
            }

            CurrentMudra = MudraState.None;

            return false;
        }

        public bool ContinueCurrentMudra(ref uint actionID)
        {
            if (ActionWatching.TimeSinceLastAction.TotalSeconds >= 1 && CurrentNinjutsu == Ninjutsu)
            {
                InMudra = false;
                ActionWatching.LastAction = 0;
                CurrentMudra = MudraState.None;
                return false;
            }

            if (ActionWatching.LastAction == FumaShuriken ||
                 ActionWatching.LastAction == Katon ||
                 ActionWatching.LastAction == Raiton ||
                 ActionWatching.LastAction == Hyoton ||
                 ActionWatching.LastAction == Huton ||
                 ActionWatching.LastAction == Doton ||
                 ActionWatching.LastAction == Suiton ||
                 ActionWatching.LastAction == GokaMekkyaku ||
                 ActionWatching.LastAction == HyoshoRanryu)
            {
                CurrentMudra = MudraState.None;
                InMudra = false;
            }

            return CurrentMudra switch
            {
                MudraState.None => false,
                MudraState.CastingFumaShuriken => CastFumaShuriken(ref actionID),
                MudraState.CastingKaton => CastKaton(ref actionID),
                MudraState.CastingRaiton => CastRaiton(ref actionID),
                MudraState.CastingHyoton => CastHyoton(ref actionID),
                MudraState.CastingHuton => CastHuton(ref actionID),
                MudraState.CastingDoton => CastDoton(ref actionID),
                MudraState.CastingSuiton => CastSuiton(ref actionID),
                MudraState.CastingGokaMekkyaku => CastGokaMekkyaku(ref actionID),
                MudraState.CastingHyoshoRanryu => CastHyoshoRanryu(ref actionID),
                _ => false
            };
        }
    }
    #endregion
    
    #endregion
    
    #region Opener
    internal static NINOpenerMaxLevel4thGCDKunai Opener1 = new();
    internal static NINOpenerMaxLevel3rdGCDDokumori Opener2 = new();
    internal static NINOpenerMaxLevel3rdGCDKunai Opener3 = new();
    
    internal static WrathOpener Opener()
    {
        if (IsEnabled(Preset.NIN_ST_AdvancedMode))
        {
            if (NIN_Adv_Opener_Selection == 0 && Opener1.LevelChecked)
                return Opener1;

            if (NIN_Adv_Opener_Selection == 1 && Opener2.LevelChecked)
                return Opener2;

            if (NIN_Adv_Opener_Selection == 2 && Opener3.LevelChecked)
                return Opener3;
        }

        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }
     internal class NINOpenerMaxLevel4thGCDKunai : WrathOpener
    {
        //4th GCD Kunai
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            ArmorCrush,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            Bhavacakra,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            12
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }

    internal class NINOpenerMaxLevel3rdGCDDokumori : WrathOpener
    {
        //3rd GCD Dokumori
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            ArmorCrush,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
            Bhavacakra,
            SpinningEdge
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            12
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }

    internal class NINOpenerMaxLevel3rdGCDKunai : WrathOpener
    {
        //3rd GCD Kunai
        public override List<uint> OpenerActions { get; set; } =
        [
            Ten,
            ChiCombo,
            JinCombo,
            Suiton,
            Kassatsu,
            SpinningEdge,
            GustSlash,
            Dokumori,
            Bunshin,
            PhantomKamaitachi,
            KunaisBane,
            ChiCombo,
            JinCombo,
            HyoshoRanryu,
            DreamWithinADream,
            Ten,
            ChiCombo,
            Raiton,
            TenChiJin,
            TCJFumaShurikenTen,
            TCJRaiton,
            TCJSuiton,
            Meisui,
            FleetingRaiju,
            ZeshoMeppo,
            TenriJendo,
            FleetingRaiju,
            ArmorCrush,
            Bhavacakra,
            Ten,
            ChiCombo,
            Raiton,
            FleetingRaiju,
        ];

        public override List<int> DelayedWeaveSteps { get; set; } =
        [
            11
        ];

        internal override UserData? ContentCheckConfig => NIN_Balance_Content;
        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        public override bool HasCooldowns()
        {
            if (GetRemainingCharges(Ten) < 1) return false;
            if (IsOnCooldown(Mug)) return false;
            if (IsOnCooldown(TenChiJin)) return false;
            if (IsOnCooldown(PhantomKamaitachi)) return false;
            if (IsOnCooldown(Bunshin)) return false;
            if (IsOnCooldown(DreamWithinADream)) return false;
            if (IsOnCooldown(Kassatsu)) return false;
            if (IsOnCooldown(TrickAttack)) return false;

            return true;
        }
    }
    #endregion
}

