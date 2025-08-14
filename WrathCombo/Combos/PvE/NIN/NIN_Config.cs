using WrathCombo.CustomComboNS.Functions;
using WrathCombo.Extensions;
using static WrathCombo.Window.Functions.UserConfig;
namespace WrathCombo.Combos.PvE;

internal partial class NIN
{
    internal static class Config
    {
        internal static UserInt
            NIN_ST_AdvancedMode_BurnKazematoi = new("NIN_ST_AdvancedMode_BurnKazematoi", 10),
            NIN_ST_AdvancedMode_SuitonSetup = new("NIN_ST_AdvancedMode_SuitonSetup", 18),
            NIN_ST_AdvancedMode_SecondWindThreshold = new("NIN_ST_AdvancedMode_SecondWindThreshold", 40),
            NIN_ST_AdvancedMode_ShadeShiftThreshold = new("NIN_ST_AdvancedMode_ShadeShiftThreshold", 20),
            NIN_ST_AdvancedMode_BloodbathThreshold = new("NIN_ST_AdvancedMode_BloodbathThreshold", 40),
            
            
            Ninki_HellfrogPooling = new("Ninki_HellfrogPooling", 50),
            Ninki_BunshinPoolingAoE = new("Ninki_BunshinPoolingAoE", 50),
            Advanced_DotonTimer = new("Advanced_DotonTimer", 4),
            Advanced_DotonHP = new("Advanced_DotonHP", 20),
            Advanced_TCJEnderAoE = new("Advanced_TCJEnderAoe", 0),
            SecondWindThresholdAoE = new("SecondWindThresholdAoE"),
            ShadeShiftThresholdAoE = new("ShadeShiftThresholdAoE"),
            BloodbathThresholdAoE = new("BloodbathThresholdAoE"),
            NIN_VariantCure = new("NIN_VariantCure"),
            NIN_Adv_Opener_Selection = new("NIN_Adv_Opener_Selection", 0),
            NIN_Balance_Content = new("NIN_Balance_Content", 1),
            NIN_SimpleMudra_Choice = new("NIN_SimpleMudra_Choice", 1);

        internal static UserBool
            NIN_ST_AdvancedMode_Bhavacakra_Pooling = new("Ninki_BhavaPooling"),
            NIN_ST_AdvancedMode_TrueNorth = new("NIN_ST_AdvancedMode_TrueNorth"),
            NIN_ST_AdvancedMode_ShadeShiftRaidwide = new("NIN_ST_AdvancedMode_ShadeShiftRaidwide"),
            NIN_ST_AdvancedMode_ForkedRaiju = new("NIN_ST_AdvancedMode_ForkedRaiju");

        internal static UserBoolArray
            NIN_ST_AdvancedMode_Ninjitsus_Options = new("NIN_ST_AdvancedMode_Ninjitsus_Options"),
            NIN_ST_AdvancedMode_Raiton_Options = new("NIN_ST_AdvancedMode_Raiton_Options"),
            NIN_ST_AdvancedMode_TenChiJin_Options = new("NIN_ST_AdvancedMode_TenChiJin_Options");

        internal static void Draw(Preset preset)
        {
            switch (preset)
            {
                case Preset.NIN_ST_AdvancedMode:
                    DrawAdditionalBoolChoice(NIN_ST_AdvancedMode_TrueNorth, "Dynamic True North", 
                        "Dynamic choice of combo finisher based on position and available charges.\nGo to Flank to build charges, Rear to spend them. \nPrevents overcap or waste and will use true north as needed.");
                    DrawSliderInt(0, 10, NIN_ST_AdvancedMode_BurnKazematoi, $"Focus on using {AeolianEdge.ActionName()} to use Kazematoi when target is below Health %");
                    break;
                
                case Preset.NIN_ST_AdvancedMode_BalanceOpener:
                    DrawRadioButton(NIN_Adv_Opener_Selection, $"Standard Opener - 4th GCD {KunaisBane.ActionName()}", "", 0);
                    DrawRadioButton(NIN_Adv_Opener_Selection, $"Standard Opener - 3rd GCD {Dokumori.ActionName()}", "", 1);
                    DrawRadioButton(NIN_Adv_Opener_Selection, $"Standard Opener - 3rd GCD {KunaisBane.ActionName()}", "", 2);

                    DrawBossOnlyChoice(NIN_Balance_Content);
                    break;

                case Preset.NIN_ST_AdvancedMode_Ninjitsus:
                    DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_Ninjitsus_Options, "Raiton/Fuma Shuriken",
                        "Adds Raiton to the rotation, will use Fuma Shuriken if below level", 3, 0);
                    DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_Ninjitsus_Options, "Suiton",
                        "Adds Suiton to the rotation when trick attack cooldown will be ready.", 3, 1);
                    DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_Ninjitsus_Options, "Hyosho Ranryu",
                        "Adds Hyosho Ranryu to the rotation when you has Kassatsu", 3, 2);
                    if (NIN_ST_AdvancedMode_Ninjitsus_Options[0])
                    {
                        DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_Raiton_Options, "Raiton Pooling",
                            "Will Pool the charges, saving them for Trick Window", 2, 0);
                        DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_Raiton_Options, "Raiton Uptime",
                            "Will Use Raiton when out of Melee range of the target, " +
                            "\nThis can negatively affect your burst windows", 2, 1);
                    }
                    if (NIN_ST_AdvancedMode_Ninjitsus_Options[1])
                    {
                        DrawSliderInt(0, 21, NIN_ST_AdvancedMode_SuitonSetup,
                            "Set the amount of time remaining on Trick Attack cooldown before trying to set up with Suiton.");
                    }
                    break;
                
                case Preset.NIN_ST_AdvancedMode_TenChiJin:
                    DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_TenChiJin_Options, "Auto TCJ Option",
                        "Will automatically Fuma Shuriken then Raiton then Suiton", 2, 0);
                    DrawHorizontalMultiChoice(NIN_ST_AdvancedMode_TenChiJin_Options, "TenriJindo Option",
                        "Will weave followup OGCD, TenriJindo, when available", 2, 1);
                    break;
                
                case Preset.NIN_ST_AdvancedMode_Bhavacakra:
                    DrawAdditionalBoolChoice(NIN_ST_AdvancedMode_Bhavacakra_Pooling, "Bhavacakra Pooling", "Will pool Ninki for the buff windows, while preventing overcap ");
                    break;
                
                case Preset.NIN_ST_AdvancedMode_Raiju:
                    DrawAdditionalBoolChoice(NIN_ST_AdvancedMode_ForkedRaiju, "Forked Raiju", "Allows the Use of forked Raiju instead of Fleeting if out of melee range. (Gap Closer)");
                    break;
                
                case Preset.NIN_ST_AdvancedMode_SecondWind:
                    DrawSliderInt(0, 100, NIN_ST_AdvancedMode_SecondWindThreshold,
                        "Set a HP% threshold for when Second Wind will be used.");
                    break;

                case Preset.NIN_ST_AdvancedMode_ShadeShift:
                    DrawSliderInt(0, 100, NIN_ST_AdvancedMode_ShadeShiftThreshold,
                        "Set a HP% threshold for when Shade Shift will be used.");
                    DrawAdditionalBoolChoice(NIN_ST_AdvancedMode_ShadeShiftRaidwide, "Raidwide Option", "Use Shade Shift when Raidwide casting is detected regardless of health");
                    break;

                case Preset.NIN_ST_AdvancedMode_Bloodbath:
                    DrawSliderInt(0, 100, NIN_ST_AdvancedMode_BloodbathThreshold,
                        "Set a HP% threshold for when Bloodbath will be used.");
                    break;
                
                
                
                
                
                case Preset.NIN_Simple_Mudras:
                    DrawRadioButton(NIN_SimpleMudra_Choice, "Mudra Path Set 1",
                        $"1. {Ten.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Raiton.ActionName()}/{HyoshoRanryu.ActionName()}, {Suiton.ActionName()} ({Doton.ActionName()} under {Kassatsu.ActionName()}).\n{Chi.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Hyoton.ActionName()}, {Huton.ActionName()}.\n{Jin.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Katon.ActionName()}/{GokaMekkyaku.ActionName()}, {Doton.ActionName()}",
                        1);

                    DrawRadioButton(NIN_SimpleMudra_Choice, "Mudra Path Set 2",
                        $"2. {Ten.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Hyoton.ActionName()}/{HyoshoRanryu.ActionName()}, {Doton.ActionName()}.\n{Chi.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Katon.ActionName()}, {Suiton.ActionName()}.\n{Jin.ActionName()} Mudras -> {FumaShuriken.ActionName()}, {Raiton.ActionName()}/{GokaMekkyaku.ActionName()}, {Huton.ActionName()} ({Doton.ActionName()} under {Kassatsu.ActionName()}).",
                        2);

                    break;
                


                

               
                

                

                case Preset.NIN_AoE_AdvancedMode_SecondWind:
                    DrawSliderInt(0, 100, SecondWindThresholdAoE,
                        "Set a HP% threshold for when Second Wind will be used.");

                    break;

                case Preset.NIN_AoE_AdvancedMode_ShadeShift:
                    DrawSliderInt(0, 100, ShadeShiftThresholdAoE,
                        "Set a HP% threshold for when Shade Shift will be used.");

                    break;

                case Preset.NIN_AoE_AdvancedMode_Bloodbath:
                    DrawSliderInt(0, 100, BloodbathThresholdAoE,
                        "Set a HP% threshold for when Bloodbath will be used.");

                    break;

                case Preset.NIN_AoE_AdvancedMode_HellfrogMedium:
                    DrawSliderInt(50, 100, Ninki_HellfrogPooling,
                        "Set the amount of Ninki required to have before spending on Hellfrog Medium.");

                    break;

                case Preset.NIN_AoE_AdvancedMode_Ninjitsus_Doton:
                    DrawSliderInt(0, 18, Advanced_DotonTimer,
                        "Sets the amount of time remaining on Doton before casting again.");

                    DrawSliderInt(0, 100, Advanced_DotonHP,
                        "Sets the max remaining HP percentage of the current target to cast Doton.");

                    break;

                case Preset.NIN_AoE_AdvancedMode_TCJ:
                    DrawRadioButton(Advanced_TCJEnderAoE, "Ten Chi Jin Ender 1",
                        "Ends Ten Chi Jin with Suiton.", 0);

                    DrawRadioButton(Advanced_TCJEnderAoE, "Ten Chi Jin Ender 2",
                        "Ends Ten Chi Jin with Doton.\nIf you have Doton enabled, Ten Chi Jin will be delayed according to the settings in that feature.",
                        1);

                    break;
                
                case Preset.NIN_Variant_Cure:
                    DrawSliderInt(1, 100, NIN_VariantCure, "HP% to be at or under", 200);

                    break;
                                   
            }
        }
    }
}
