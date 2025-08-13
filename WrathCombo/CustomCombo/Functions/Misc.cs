using ECommons.DalamudServices;
using ECommons.ExcelServices;
using Lumina.Excel.Sheets;
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using WrathCombo.Core;
namespace WrathCombo.CustomComboNS.Functions;

internal abstract partial class CustomComboFunctions
{
    /// <summary> Checks if the given preset is enabled. </summary>
    public static bool IsEnabled(Preset preset)
    {
        if ((int)preset < 100)
            return true;

        try
        {
            (string controllers, bool enabled, bool autoMode)? checkControlled = P.UIHelper.PresetControlled(preset);
            bool controlled = checkControlled is not null;
            bool? controlledState = checkControlled?.enabled;

            return controlled
                ? (bool)controlledState!
                : PresetStorage.IsEnabled(preset);
        }
        // IPC is not loaded yet
        catch
        {
            return PresetStorage.IsEnabled(preset);
        }
    }

    /// <summary> Checks if the given preset is not enabled. </summary>
    public static bool IsNotEnabled(Preset preset) => !IsEnabled(preset);

    public class JobIDs
    {
        public static readonly FrozenSet<uint> Tank =
            Svc.Data.GetExcelSheet<ClassJob>()!
            .Where(cj => cj.Role == 1)
            .Select(cj => cj.RowId)
            .ToFrozenSet();

        public static readonly FrozenSet<uint> Healer =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 4)
                .Select(cj => cj.RowId)
                .ToFrozenSet();

        public static readonly FrozenSet<uint> Melee =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 2)
                .Select(cj => cj.RowId)
                .ToFrozenSet();

        public static readonly FrozenSet<uint> Ranged =
            Svc.Data.GetExcelSheet<ClassJob>()!
                .Where(cj => cj.Role == 3)
                .Select(cj => cj.RowId)
                .ToFrozenSet();
    }
}