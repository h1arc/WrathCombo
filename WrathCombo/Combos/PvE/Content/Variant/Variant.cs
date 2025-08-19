using ECommons.DalamudServices;
using WrathCombo.CustomComboNS;
using static WrathCombo.Combos.PvE.RoleActions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;


namespace WrathCombo.Combos.PvE;

#region Variant Actions and Functions
// Static utility class for shared logic
internal static partial class Variant
{
    public const uint
        Ultimatum = 29730,
        Raise = 29731,
        Raise2 = 29734;

    // 1069 = The Sil'dihn Subterrane
    // 1137 = Mount Rokkon
    // 1176 = Aloalo Island
    public static uint Cure => Svc.ClientState.TerritoryType switch
    {
        1069 => 29729,
        1137 or 1176 => 33862,
        var _ => 0
    };

    public static uint SpiritDart => Svc.ClientState.TerritoryType switch
    {
        1069 => 29732,
        1137 or 1176 => 33863,
        var _ => 0
    };

    public static uint Rampart => Svc.ClientState.TerritoryType switch
    {
        1069 => 29733,
        1137 or 1176 => 33864,
        var _ => 0
    };

    public static class Buffs
    {
        internal const ushort
            EmnityUp = 3358,
            VulnDown = 3360,
            Rehabilitation = 3367,
            DamageBarrier = 3405;
    }

    public static class Debuffs
    {
        internal const ushort
            SustainedDamage = 3359;
    }

    private static bool CheckRampart(Preset preset, WeaveTypes weave = WeaveTypes.None) =>
        IsEnabled(preset) && ActionReady(Rampart) &&
        CheckWeave(weave);

    private static bool CheckSpiritDart(Preset preset) =>
        IsEnabled(preset) && ActionReady(SpiritDart) &&
        HasBattleTarget() && GetStatusEffectRemainingTime(Debuffs.SustainedDamage, CurrentTarget) <= 3;

    private static bool CheckCure(Preset preset, int healthpercent) =>
        IsEnabled(preset) && ActionReady(Cure) &&
        PlayerHealthPercentageHp() <= healthpercent;

    private static bool CheckRaise(Preset preset) =>
        IsEnabled(preset) && ActionReady(Raise)
        && HasStatusEffect(Magic.Buffs.Swiftcast);

    private static bool CheckUltimatum(Preset preset) =>
        IsEnabled(preset) && ActionReady(Ultimatum)
        && NumberOfEnemiesInRange(Ultimatum) > 0;

    internal class Variant_Magic_Raise : CustomCombo
    {
        protected internal override Preset Preset => Preset.Variant_Magic_Raise;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Magic.Swiftcast)
                return actionID;

            if (CheckRaise(Preset.Variant_Magic_Raise) && HasStatusEffect(Magic.Buffs.Swiftcast))
            {
                return Raise;
            }
            else
            {
                return actionID;
            }
        }
    }
}
#endregion