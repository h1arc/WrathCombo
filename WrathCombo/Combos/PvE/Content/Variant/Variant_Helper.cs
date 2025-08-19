using ECommons.GameHelpers;
using WrathCombo.Attributes;
using static WrathCombo.Attributes.RoleAttribute;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE
{
    internal static partial class Variant
    {
        public static bool CanCure()
        {
            return GetRoleFromJob(Player.Job) switch
            {
                JobRole.Tank => IsEnabled(Preset.Variant_Tank) && CheckCure(Preset.Variant_Tank_Cure, Config.Variant_Tank_Cure),
                JobRole.RangedDPS => IsEnabled(Preset.Variant_PhysRanged) && CheckCure(Preset.Variant_PhysRanged_Cure, Config.Variant_PhysRanged_Cure),
                JobRole.MeleeDPS => IsEnabled(Preset.Variant_Melee_Cure) && CheckCure(Preset.Variant_Melee_Cure, Config.Variant_Melee_Cure),
                JobRole.MagicalDPS => IsEnabled(Preset.Variant_Magic) && CheckCure(Preset.Variant_Magic_Cure, Config.Variant_Magic_Cure),
                _ => false
            };
        }

        public static bool CanUltimatum()
        {
            return GetRoleFromJob(Player.Job) switch
            {
                JobRole.Tank => IsEnabled(Preset.Variant_Tank) && CheckUltimatum(Preset.Variant_Tank_Ultimatum),
                JobRole.Healer => IsEnabled(Preset.Variant_Healer_Ultimatum) && CheckUltimatum(Preset.Variant_Healer_Ultimatum),
                JobRole.RangedDPS => IsEnabled(Preset.Variant_PhysRanged) && CheckUltimatum(Preset.Variant_PhysRanged_Ultimatum),
                JobRole.MeleeDPS => IsEnabled(Preset.Variant_Melee) && CheckUltimatum(Preset.Variant_Melee_Ultimatum),
                JobRole.MagicalDPS => IsEnabled(Preset.Variant_Magic) && CheckUltimatum(Preset.Variant_Magic_Ultimatum),
                _ => false
            };
        }

        public static bool CanRaise()
        {
            return GetRoleFromJob(Player.Job) switch
            {
                JobRole.Tank => IsEnabled(Preset.Variant_Tank) && CheckRaise(Preset.Variant_Tank_Raise),
                JobRole.RangedDPS => IsEnabled(Preset.Variant_PhysRanged) && CheckRaise(Preset.Variant_PhysRanged_Raise),
                JobRole.MeleeDPS => IsEnabled(Preset.Variant_Melee) && CheckRaise(Preset.Variant_Melee_Raise),
                JobRole.MagicalDPS => IsEnabled(Preset.Variant_Magic) && CheckRaise(Preset.Variant_Magic_Raise),
                _ => false
            };
        }

        public static bool CanSpiritDart()
        {
            return GetRoleFromJob(Player.Job) switch
            {
                JobRole.Tank => IsEnabled(Preset.Variant_Tank) && CheckSpiritDart(Preset.Variant_Tank_SpiritDart),
                JobRole.Healer => IsEnabled(Preset.Variant_Healer) && CheckSpiritDart(Preset.Variant_Healer_SpiritDart),
                _ => false
            };
        }

        public static bool CanRampart(WeaveTypes weave = WeaveTypes.None)
        {
            return GetRoleFromJob(Player.Job) switch
            {
                JobRole.Healer => IsEnabled(Preset.Variant_Healer) && CheckRampart(Preset.Variant_Healer_Rampart, weave),
                JobRole.RangedDPS => IsEnabled(Preset.Variant_PhysRanged) && CheckRampart(Preset.Variant_PhysRanged_Rampart, weave),
                JobRole.MeleeDPS => IsEnabled(Preset.Variant_Melee) && CheckRampart(Preset.Variant_Melee_Rampart, weave),
                JobRole.MagicalDPS => IsEnabled(Preset.Variant_Magic) && CheckRampart(Preset.Variant_Magic_Rampart, weave),
                _ => false
            };
        }


    }
}