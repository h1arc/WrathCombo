using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Statuses;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;
using static WrathCombo.Combos.PvE.NIN.Config;

namespace WrathCombo.Combos.PvE;

internal partial class NIN : Melee
{
    #region Simple 
    internal class NIN_ST_SimpleMode1 : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not SpinningEdge)
                return actionID;
            
            NINGauge gauge = GetJobGauge<NINGauge>();
            
            if (OriginalHook(Ninjutsu) is Rabbit or Huton or Doton or Suiton)
                return OriginalHook(Ninjutsu);
            
            if (HasStatusEffect(Buffs.TenChiJin))
                return STTenChiJin(actionID);
            
            #region Special Content
            
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            
            #endregion
            
            #region OGCDS
            
            if (InCombat() && HasBattleTarget())
            {
                if (CanKassatsu)
                    return Kassatsu;

                if (CanBunshin)
                    return Bunshin;

                if (CanTenChiJin)
                    return OriginalHook(TenChiJin);
                
                if (CanAssassinate)
                    return OriginalHook(Assassinate);

                if (CanMeisui)
                    return NinkiOvercapCheck ? OriginalHook(Bhavacakra) : OriginalHook(Meisui);

                if (CanBhavacakra)
                    return LevelChecked(Bhavacakra) ? OriginalHook(Bhavacakra) : OriginalHook(HellfrogMedium);
                
                if (CanMug)
                    return OriginalHook(Mug);

                if (CanTrick)
                    return OriginalHook(TrickAttack);
            }
            
            #endregion
            
            #region Ninjutsu
            if (CanUseHyoshoRanryu)
                return UseHyoshoRanryu(actionID);

            if (CanUseSuiton)
                return UseSuiton(actionID);
            
            if (CanUseRaiton)
                return LevelChecked(Raiton) ?
                    UseRaiton(actionID):
                    UseFumaShuriken(actionID);
            #endregion
            
            #region Selfcare
            
            if (Role.CanSecondWind(40))
                return Role.SecondWind;

            if (ActionReady(ShadeShift) && (PlayerHealthPercentageHp() < 60 || RaidWideCasting()))
                return ShadeShift;

            if (Role.CanBloodBath(40))
                return Role.Bloodbath;
            
            #endregion
           
            #region GCDS
            
            if (CanThrowingDaggers)
                return OriginalHook(ThrowingDaggers);
            
            if (CanRaiju)
                return FleetingRaiju;

            if (CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f)
            {
                switch (ComboAction)
                {
                    case SpinningEdge when GustSlash.LevelChecked():
                        return OriginalHook(GustSlash);
                    
                    case GustSlash when GetTargetHPPercent() <= 10 && gauge.Kazematoi > 0: //Kazematoi Dump Below 10%
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                    
                    case GustSlash when ArmorCrush.LevelChecked():
                        return gauge.Kazematoi switch
                        {
                            0 => TNArmorCrush ? Role.TrueNorth : ArmorCrush,
                            >= 4 => TNAeolianEdge ? Role.TrueNorth : AeolianEdge,
                            _ => OnTargetsFlank() || !TargetNeedsPositionals() ? ArmorCrush: AeolianEdge
                        };
                    case GustSlash when !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked():
                        return TNAeolianEdge ? Role.TrueNorth : AeolianEdge;
                }
            }
            return OriginalHook(SpinningEdge);
            #endregion
        }
    }
    
    internal class NIN_AoE_SimpleMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        
        {
            if (actionID is not DeathBlossom)
                return actionID;
            
            if (OriginalHook(Ninjutsu) is Rabbit or Huton or Doton or Suiton)
                return OriginalHook(Ninjutsu);
            
            if (HasStatusEffect(Buffs.TenChiJin))
                return AoETenChiJin(actionID);
            
            #region Special Content
            
            if (OccultCrescent.ShouldUsePhantomActions() && !MudraPhase)
                return OccultCrescent.BestPhantomAction();
            
            if (Variant.CanRampart(Preset.NIN_Variant_Rampart) && !MudraPhase)
                return Variant.Rampart;
            
            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure) && !MudraPhase)
                return Variant.Cure;
            
            #endregion
            
            #region OGCDS
            
            if (InCombat() && HasBattleTarget())
            {
                if (CanKassatsu)
                    return Kassatsu;

                if (CanBunshin)
                    return Bunshin;

                if (CanTenChiJin)
                    return OriginalHook(TenChiJin);
                
                if (CanAssassinate)
                    return OriginalHook(Assassinate);

                if (CanMeisui)
                    return NinkiOvercapCheck ? OriginalHook(HellfrogMedium) : OriginalHook(Meisui);

                if (CanHellfrogMedium)
                    return OriginalHook(HellfrogMedium);
                
                if (CanMug)
                    return OriginalHook(Mug);

                if (CanTrick)
                    return OriginalHook(TrickAttack);
            }
            
            #endregion
            
            #region Ninjutsu
            if (CanUseGokaMekkyaku)
                return UseGokaMekkyaku(actionID);

            if (CanUseHuton)
                return UseHuton(actionID);
            
            if (CanUseDoton && GetTargetHPPercent() >= 30 && (!HasDoton || DotonRemaining <= 2))
                return UseDoton(actionID);
            
            if (CanUseKaton)
                return LevelChecked(Katon) ?
                    UseRaiton(actionID):
                    UseFumaShuriken(actionID);
            #endregion
            
            #region Selfcare
            
            if (Role.CanSecondWind(40))
                return Role.SecondWind;

            if (ActionReady(ShadeShift) && (PlayerHealthPercentageHp() < 60 || RaidWideCasting()))
                return ShadeShift;

            if (Role.CanBloodBath(40))
                return Role.Bloodbath;
            
            #endregion
           
            #region GCDS
            
            if (CanThrowingDaggersAoE)
                return OriginalHook(ThrowingDaggers);
            
            if (CanRaiju)
                return FleetingRaiju;

            if (CanPhantomKamaitachi)
                return PhantomKamaitachi;
            
            if (ComboTimer > 1f && ComboAction is DeathBlossom && LevelChecked(HakkeMujinsatsu))
                return OriginalHook(HakkeMujinsatsu);

            return OriginalHook(DeathBlossom);
            #endregion
        }
    }
    
    #endregion
    
    #region Advanced
    internal class NIN_ST_AdvancedMode : CustomCombo
    {
        protected internal static NINOpenerMaxLevel4thGCDKunai NINOpener = new();

        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not SpinningEdge)
                return actionID;

            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();
            bool canDelayedWeave = CanDelayedWeave();
            bool inTrickBurstSaveWindow = IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack_Cooldowns) && IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) && GetCooldownRemainingTime(TrickAttack) <= Advanced_Trick_Cooldown;
            bool useBhakaBeforeTrickWindow = GetCooldownRemainingTime(TrickAttack) >= 3;
            bool setupSuitonWindow = GetCooldownRemainingTime(OriginalHook(TrickAttack)) <= Trick_CooldownRemaining && !HasStatusEffect(Buffs.ShadowWalker);
            bool setupKassatsuWindow = GetCooldownRemainingTime(TrickAttack) <= 10 && HasStatusEffect(Buffs.ShadowWalker);
            bool chargeCheck = IsNotEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_ChargeHold) || IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_ChargeHold) && (InMudra || GetRemainingCharges(Ten) == 2 || GetRemainingCharges(Ten) == 1 && GetCooldownChargeRemainingTime(Ten) < 3);
            bool poolCharges = !(bool)Advanced_ChargePool || GetRemainingCharges(Ten) == 1 && GetCooldownChargeRemainingTime(Ten) < 2 || TrickDebuff || InMudra;
            bool raitonUptime = IsEnabled(Preset.NIN_ST_AdvancedMode_Raiton_Uptime);
            bool suitonUptime = IsEnabled(Preset.NIN_ST_AdvancedMode_Suiton_Uptime);
            int bhavaPool = Ninki_BhavaPooling;
            int bunshinPool = Ninki_BunshinPoolingST;
            int burnKazematoi = BurnKazematoi;
            int secondWindThreshold = SecondWindThresholdST;
            int shadeShiftThreshold = ShadeShiftThresholdST;
            int bloodbathThreshold = BloodbathThresholdST;
            double playerHP = PlayerHealthPercentageHp();
            bool phantomUptime = IsEnabled(Preset.NIN_ST_AdvancedMode_Phantom_Uptime);
            bool trueNorthArmor = IsEnabled(Preset.NIN_ST_AdvancedMode_TrueNorth) && Role.CanTrueNorth() && !OnTargetsFlank();
            bool trueNorthEdge = IsEnabled(Preset.NIN_ST_AdvancedMode_TrueNorth) && Role.CanTrueNorth() && !OnTargetsRear();
            bool dynamic = Advanced_TrueNorth == 0;

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_BalanceOpener) && 
                Opener().FullOpener(ref actionID))
                return actionID;

            if (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus) || ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) && IsOnCooldown(TrickAttack) && MudraState.CurrentMudra == MudraCasting.MudraState.CastingSuiton && !setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) && IsOnCooldown(TrickAttack) && MudraState.CurrentMudra != MudraCasting.MudraState.CastingSuiton && setupSuitonWindow)
                MudraState.CurrentMudra = MudraCasting.MudraState.CastingSuiton;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (MudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (!Suiton.LevelChecked()) //For low level
            {
                if (Raiton.LevelChecked() && IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Raiton)) //under 45 will only use Raiton
                {
                    if (MudraState.CastRaiton(ref actionID))
                        return actionID;
                }
                else if (!Raiton.LevelChecked() && MudraState.CastFumaShuriken(ref actionID) && IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_FumaShuriken)) // 30-35 will use only fuma
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (OriginalHook(Ten) == TCJFumaShurikenTen)
                    return OriginalHook(Ten);
                if (OriginalHook(Chi) == TCJRaiton)
                    return OriginalHook(Chi);
                if (OriginalHook(Jin) == TCJSuiton)
                    return OriginalHook(Jin);
            }

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Kassatsu_HyoshoRaynryu) &&
                HasStatusEffect(Buffs.Kassatsu) &&
                TrickDebuff &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (InCombat() && !InMeleeRange())
            {
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bunshin_Phantom) &&
                    HasStatusEffect(Buffs.PhantomReady) &&
                    (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && MugDebuff) &&
                    PhantomKamaitachi.LevelChecked()
                    && phantomUptime)
                    return OriginalHook(PhantomKamaitachi);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) &&
                    setupSuitonWindow &&
                    TrickAttack.LevelChecked() &&
                    !HasStatusEffect(Buffs.ShadowWalker) &&
                    chargeCheck &&
                    suitonUptime &&
                    MudraState.CastSuiton(ref actionID))
                    return actionID;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Raiton) &&
                    !inTrickBurstSaveWindow &&
                    chargeCheck &&
                    poolCharges &&
                    raitonUptime &&
                    MudraState.CastRaiton(ref actionID))
                    return actionID;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_RangedUptime) && ThrowingDaggers.LevelChecked() && HasTarget() && !HasStatusEffect(Buffs.RaijuReady))
                    return OriginalHook(ThrowingDaggers);
            }

            if (canWeave && !InMudra)
            {
                if (Variant.CanRampart(Preset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) &&
                    IsEnabled(Preset.NIN_ST_AdvancedMode_Mug_AlignBefore) &&
                    HasStatusEffect(Buffs.ShadowWalker) &&
                    GetCooldownRemainingTime(TrickAttack) <= 3 &&
                    (IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack_Delayed) && InCombat() &&
                     CombatEngageDuration().TotalSeconds > 6 ||
                     IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack_Delayed)) &&
                    IsOffCooldown(Mug) &&
                    canDelayedWeave &&
                    Mug.LevelChecked())
                {
                    if (Dokumori.LevelChecked() && gauge.Ninki >= 60)
                        return OriginalHook(Bhavacakra);
                    return OriginalHook(Mug);
                }

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack) &&
                    HasStatusEffect(Buffs.ShadowWalker) &&
                    IsOffCooldown(TrickAttack) &&
                    canDelayedWeave &&
                    (IsEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack_Delayed) && InCombat() && CombatEngageDuration().TotalSeconds > 8 ||
                     IsNotEnabled(Preset.NIN_ST_AdvancedMode_TrickAttack_Delayed)))
                    return OriginalHook(TrickAttack);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_TenriJindo) && HasStatusEffect(Buffs.TenriJendo) && (TrickDebuff && MugDebuff || GetStatusEffectRemainingTime(Buffs.TenriJendo) <= 3))
                    return OriginalHook(TenriJendo);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bunshin) && Bunshin.LevelChecked() && IsOffCooldown(Bunshin) && gauge.Ninki >= bunshinPool)
                    return OriginalHook(Bunshin);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Kassatsu) && (TrickDebuff || setupKassatsuWindow) && IsOffCooldown(Kassatsu) && Kassatsu.LevelChecked())
                    return OriginalHook(Kassatsu);

                //healing - please move if not appropriate priority
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) &&
                    (TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 85) &&
                    (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Mug) || IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                    Bhavacakra.LevelChecked())
                    return OriginalHook(Bhavacakra);

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) &&
                    (TrickDebuff && gauge.Ninki >= 50 || useBhakaBeforeTrickWindow && gauge.Ninki >= 60) &&
                    (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Mug) || IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                    !Bhavacakra.LevelChecked() && HellfrogMedium.LevelChecked())
                    return OriginalHook(HellfrogMedium);

                if (!inTrickBurstSaveWindow)
                {
                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) && IsOffCooldown(Mug) && Mug.LevelChecked())
                    {
                        if (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Mug_AlignAfter) || IsEnabled(Preset.NIN_ST_AdvancedMode_Mug_AlignAfter) && TrickDebuff)
                            return OriginalHook(Mug);
                    }

                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_Meisui) && HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki <= 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                        return OriginalHook(Meisui);

                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) && gauge.Ninki >= bhavaPool && Bhavacakra.LevelChecked())
                        return OriginalHook(Bhavacakra);

                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bhavacakra) && gauge.Ninki >= bhavaPool && !Bhavacakra.LevelChecked() && HellfrogMedium.LevelChecked())
                        return OriginalHook(HellfrogMedium);

                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_AssassinateDWAD) && IsOffCooldown(OriginalHook(Assassinate)) && Assassinate.LevelChecked())
                        return OriginalHook(Assassinate);

                    if (IsEnabled(Preset.NIN_ST_AdvancedMode_TCJ) && IsOffCooldown(TenChiJin) && TenChiJin.LevelChecked())
                        return OriginalHook(TenChiJin);
                }

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;
            }

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Raiju) && HasStatusEffect(Buffs.RaijuReady))
            {
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Raiju_Forked) && !InMeleeRange())
                    return OriginalHook(ForkedRaiju);
                return OriginalHook(FleetingRaiju);
            }

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Kassatsu_HyoshoRaynryu) &&
                !inTrickBurstSaveWindow &&
                (IsNotEnabled(Preset.NIN_ST_AdvancedMode_Mug) || IsEnabled(Preset.NIN_ST_AdvancedMode_Mug) && IsOnCooldown(Mug)) &&
                MudraState.CastHyoshoRanryu(ref actionID))
                return actionID;

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus))
            {
                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Suiton) &&
                    setupSuitonWindow &&
                    TrickAttack.LevelChecked() &&
                    !HasStatusEffect(Buffs.ShadowWalker) &&
                    chargeCheck &&
                    MudraState.CastSuiton(ref actionID))
                    return actionID;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_Raiton) &&
                    !inTrickBurstSaveWindow &&
                    chargeCheck &&
                    poolCharges &&
                    MudraState.CastRaiton(ref actionID))
                    return actionID;

                if (IsEnabled(Preset.NIN_ST_AdvancedMode_Ninjitsus_FumaShuriken) &&
                    !Raiton.LevelChecked() &&
                    chargeCheck &&
                    MudraState.CastFumaShuriken(ref actionID))
                    return actionID;
            }

            if (IsEnabled(Preset.NIN_ST_AdvancedMode_Bunshin_Phantom) &&
                HasStatusEffect(Buffs.PhantomReady) &&
                (GetCooldownRemainingTime(TrickAttack) > GetStatusEffectRemainingTime(Buffs.PhantomReady) || TrickDebuff || HasStatusEffect(Buffs.Bunshin) && MugDebuff || GetStatusEffectRemainingTime(Buffs.PhantomReady) < 6) &&
                PhantomKamaitachi.LevelChecked())
                return OriginalHook(PhantomKamaitachi);

            if (ComboTimer > 1f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                    return OriginalHook(GustSlash);

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    if (gauge.Kazematoi == 0)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    if (GetTargetHPPercent() <= burnKazematoi && gauge.Kazematoi > 0)
                    {
                        if (trueNorthEdge)
                            return Role.TrueNorth;

                        return AeolianEdge;
                    }

                    if (dynamic)
                    {
                        if (gauge.Kazematoi >= 4)
                        {
                            if (trueNorthEdge)
                                return Role.TrueNorth;

                            return AeolianEdge;
                        }

                        if (OnTargetsFlank())
                            return ArmorCrush;
                        return AeolianEdge;
                    }
                    if (gauge.Kazematoi < 3)
                    {
                        if (trueNorthArmor)
                            return Role.TrueNorth;

                        return ArmorCrush;
                    }

                    return AeolianEdge;
                }
                if (ComboAction == GustSlash && !ArmorCrush.LevelChecked() && AeolianEdge.LevelChecked())
                {
                    if (trueNorthEdge)
                        return OriginalHook(Role.TrueNorth);
                    return OriginalHook(AeolianEdge);
                }
            }
            return OriginalHook(SpinningEdge);
        }
    }

    internal class NIN_AoE_AdvancedMode : CustomCombo
    {
        protected internal MudraCasting MudraState = new();
        protected internal override Preset Preset => Preset.NIN_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not DeathBlossom)
                return actionID;

            Status? dotonBuff = GetStatusEffect(Buffs.Doton);
            NINGauge gauge = GetJobGauge<NINGauge>();
            bool canWeave = CanWeave();
            bool chargeCheck = IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_ChargeHold) || IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_ChargeHold) && GetRemainingCharges(Ten) == 2;
            bool inMudraState = InMudra;
            int hellfrogPool = Ninki_HellfrogPooling;
            int dotonTimer = Advanced_DotonTimer;
            int dotonThreshold = Advanced_DotonHP;
            int tcjPath = Advanced_TCJEnderAoE;
            int bunshingPool = Ninki_BunshinPoolingAoE;
            int secondWindThreshold = SecondWindThresholdAoE;
            int shadeShiftThreshold = ShadeShiftThresholdAoE;
            int bloodbathThreshold = BloodbathThresholdAoE;
            double playerHP = PlayerHealthPercentageHp();

            if (IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus) || ActionWatching.TimeSinceLastAction.TotalSeconds >= 5 && !InCombat())
                MudraState.CurrentMudra = MudraCasting.MudraState.None;

            if (OriginalHook(Ninjutsu) is Rabbit)
                return OriginalHook(Ninjutsu);

            if (InMudra)
            {
                if (MudraState.ContinueCurrentMudra(ref actionID))
                    return actionID;
            }

            if (HasStatusEffect(Buffs.TenChiJin))
            {
                if (tcjPath == 0)
                {
                    if (OriginalHook(Chi) == TCJFumaShurikenChi)
                        return OriginalHook(Chi);
                    if (OriginalHook(Ten) == TCJKaton)
                        return OriginalHook(Ten);
                    if (OriginalHook(Jin) == TCJSuiton)
                        return OriginalHook(Jin);
                }
                else
                {
                    if (OriginalHook(Jin) == TCJFumaShurikenJin)
                        return OriginalHook(Jin);
                    if (OriginalHook(Ten) == TCJKaton)
                        return OriginalHook(Ten);
                    if (OriginalHook(Chi) == TCJDoton)
                        return OriginalHook(Chi);
                }
            }

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_GokaMekkyaku) && HasStatusEffect(Buffs.Kassatsu))
                MudraState.CurrentMudra = MudraCasting.MudraState.CastingGokaMekkyaku;

            if (Variant.CanCure(Preset.NIN_Variant_Cure, NIN_VariantCure))
                return Variant.Cure;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_KunaisBane))
            {
                if (!HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && GetCooldownRemainingTime(KunaisBane) < 5 && MudraState.CastHuton(ref actionID))
                    return actionID;

                if (HasStatusEffect(Buffs.ShadowWalker) && KunaisBane.LevelChecked() && IsOffCooldown(KunaisBane) && canWeave)
                    return KunaisBane;
            }

            if (canWeave && !inMudraState)
            {
                if (Variant.CanRampart(Preset.NIN_Variant_Rampart))
                    return Variant.Rampart;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_TenriJindo) && HasStatusEffect(Buffs.TenriJendo))
                    return OriginalHook(TenriJendo);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Bunshin) && Bunshin.LevelChecked() && IsOffCooldown(Bunshin) && gauge.Ninki >= bunshingPool)
                    return OriginalHook(Bunshin);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_HellfrogMedium) && gauge.Ninki >= hellfrogPool && HellfrogMedium.LevelChecked())
                {
                    if (HasStatusEffect(Buffs.Meisui) && TraitLevelChecked(440))
                        return OriginalHook(Bhavacakra);

                    return OriginalHook(HellfrogMedium);
                }

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_HellfrogMedium) && gauge.Ninki >= hellfrogPool && !HellfrogMedium.LevelChecked() && Bhavacakra.LevelChecked())
                {
                    return OriginalHook(Bhavacakra);
                }

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Kassatsu) &&
                    IsOffCooldown(Kassatsu) &&
                    Kassatsu.LevelChecked() &&
                    (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && (dotonBuff != null || GetTargetHPPercent() < dotonThreshold) ||
                     IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton)))
                    return OriginalHook(Kassatsu);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Meisui) && HasStatusEffect(Buffs.ShadowWalker) && gauge.Ninki <= 50 && IsOffCooldown(Meisui) && Meisui.LevelChecked())
                    return OriginalHook(Meisui);

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_AssassinateDWAD) && IsOffCooldown(OriginalHook(Assassinate)) && Assassinate.LevelChecked())
                    return OriginalHook(Assassinate);

                // healing - please move if not appropriate priority
                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_SecondWind) && Role.CanSecondWind(secondWindThreshold))
                    return Role.SecondWind;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_ShadeShift) && ShadeShift.LevelChecked() && playerHP <= shadeShiftThreshold && IsOffCooldown(ShadeShift))
                    return ShadeShift;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Bloodbath) && Role.CanBloodBath(bloodbathThreshold))
                    return Role.Bloodbath;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_TCJ) &&
                    IsOffCooldown(TenChiJin) &&
                    TenChiJin.LevelChecked())
                {
                    if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && tcjPath == 1 &&
                        (dotonBuff?.RemainingTime <= dotonTimer || dotonBuff is null) &&
                        GetTargetHPPercent() >= dotonThreshold &&
                        !WasLastAction(Doton) ||
                        tcjPath == 0)
                        return OriginalHook(TenChiJin);
                }
            }

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_GokaMekkyaku) &&
                MudraState.CastGokaMekkyaku(ref actionID))
                return actionID;

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus))
            {
                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) &&
                    (dotonBuff?.RemainingTime <= dotonTimer || dotonBuff is null) &&
                    GetTargetHPPercent() >= dotonThreshold &&
                    chargeCheck &&
                    !(WasLastAction(Doton) || WasLastAction(TCJDoton) || dotonBuff is not null) &&
                    MudraState.CastDoton(ref actionID))
                    return actionID;

                if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Katon) &&
                    chargeCheck &&
                    (IsEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton) && (dotonBuff != null || GetTargetHPPercent() < dotonThreshold) ||
                     IsNotEnabled(Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton)) &&
                    MudraState.CastKaton(ref actionID))
                    return actionID;
            }

            if (IsEnabled(Preset.NIN_AoE_AdvancedMode_Bunshin_Phantom) && HasStatusEffect(Buffs.PhantomReady) && PhantomKamaitachi.LevelChecked())
                return OriginalHook(PhantomKamaitachi);

            if (ComboTimer > 1f)
            {
                if (ComboAction is DeathBlossom && HakkeMujinsatsu.LevelChecked())
                    return OriginalHook(HakkeMujinsatsu);
            }

            return OriginalHook(DeathBlossom);
        }
    }
    #endregion
    
    #region Standalone
    
    internal class NIN_ST_AeolianEdgeCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_ST_AeolianEdgeCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not AeolianEdge)
                return actionID;

            if (ComboTimer > 0)
            {
                if (ComboAction is SpinningEdge && LevelChecked(GustSlash))
                    return GustSlash;

                if (ComboAction is GustSlash && LevelChecked(AeolianEdge))
                    return AeolianEdge;
            }

            return SpinningEdge;
        }
    }

    internal class NIN_ArmorCrushCombo : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_ArmorCrushCombo;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not ArmorCrush)
                return actionID;
            if (ComboTimer > 0f)
            {
                if (ComboAction == SpinningEdge && GustSlash.LevelChecked())
                {
                    return GustSlash;
                }

                if (ComboAction == GustSlash && ArmorCrush.LevelChecked())
                {
                    return ArmorCrush;
                }
            }
            return SpinningEdge;
        }
    }

    internal class NIN_HideMug : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_HideMug;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Hide)
                return actionID;

            if (HasCondition(ConditionFlag.InCombat))
            {
                return OriginalHook(Mug);
            }

            if (HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }

            return actionID;
        }
    }

    internal class NIN_KassatsuChiJin : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_KassatsuChiJin;

        protected override uint Invoke(uint actionID)
        {
            if (actionID == Chi && TraitLevelChecked(250) && HasStatusEffect(Buffs.Kassatsu))
            {
                return Jin;
            }
            return actionID;
        }
    }

    internal class NIN_KassatsuTrick : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_KassatsuTrick;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Kassatsu)
                return actionID;
            if (HasStatusEffect(Buffs.ShadowWalker) || HasStatusEffect(Buffs.Hidden))
            {
                return OriginalHook(TrickAttack);
            }
            return OriginalHook(Kassatsu);
        }
    }

    internal class NIN_TCJMeisui : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_TCJMeisui;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not TenChiJin)
                return actionID;

            if (HasStatusEffect(Buffs.ShadowWalker))
                return Meisui;

            if (HasStatusEffect(Buffs.TenChiJin) && IsEnabled(Preset.NIN_TCJ))
            {
                float tcjTimer = GetStatusEffectRemainingTime(Buffs.TenChiJin, anyOwner: true);

                if (tcjTimer > 5)
                    return OriginalHook(Ten);

                if (tcjTimer > 4)
                    return OriginalHook(Chi);

                if (tcjTimer > 3)
                    return OriginalHook(Jin);
            }
            return actionID;
        }
    }

    internal class NIN_Simple_Mudras : CustomCombo
    {
        protected internal override Preset Preset => Preset.NIN_Simple_Mudras;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Ten or Chi or Jin) || !HasStatusEffect(Buffs.Mudra))
                return actionID;

            int mudrapath = NIN_SimpleMudra_Choice;

            if (mudrapath == 1)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(JinCombo);
                    }

                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is HyoshoRanryu)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return JinCombo;

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);

                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (OriginalHook(Ninjutsu) is Hyoton)
                    {
                        return OriginalHook(TenCombo);
                    }

                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(JinCombo);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is GokaMekkyaku or Katon)
                    {
                        return OriginalHook(ChiCombo);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(TenCombo);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            if (mudrapath == 2)
            {
                if (Ten.LevelChecked() && actionID == Ten)
                {
                    if (Chi.LevelChecked() && OriginalHook(Ninjutsu) is Hyoton or HyoshoRanryu)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (Jin.LevelChecked())
                            return OriginalHook(JinCombo);

                        if (Chi.LevelChecked())
                            return OriginalHook(ChiCombo);
                    }
                }

                if (Chi.LevelChecked() && actionID == Chi)
                {
                    if (Jin.LevelChecked() && OriginalHook(Ninjutsu) is Katon or GokaMekkyaku)
                    {
                        return OriginalHook(Jin);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        return OriginalHook(Ten);
                    }
                }

                if (Jin.LevelChecked() && actionID == Jin)
                {
                    if (OriginalHook(Ninjutsu) is Raiton)
                    {
                        return OriginalHook(Ten);
                    }

                    if (OriginalHook(Ninjutsu) == GokaMekkyaku)
                    {
                        return OriginalHook(Chi);
                    }

                    if (OriginalHook(Ninjutsu) == FumaShuriken)
                    {
                        if (HasStatusEffect(Buffs.Kassatsu) && Traits.EnhancedKasatsu.TraitLevelChecked())
                            return OriginalHook(Ten);
                        return OriginalHook(Chi);
                    }
                }

                return OriginalHook(Ninjutsu);
            }

            return actionID;
        }
    }
    
    #endregion
}
