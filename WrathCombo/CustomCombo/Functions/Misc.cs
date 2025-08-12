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
        private static TextInfo? _cachedTextInfo;

        public static string JobToShorthand(Job job)
        {
            if (job != 0)
            {
                if (job is Job.MIN) return "DOL";
                return job.GetData().Abbreviation.ToString();
            }
            else return string.Empty;
        }

        public static string JobToName(Job job)
        {
            // Special Cases
            switch (job)
            {
                case Job.ADV:     return "Roles and Content";
                //case 99:    return "Global";
                //case 100:   return OccultCrescent.ContentName;
            }

            // Override DoH/DoL
            job = job switch
            {
                Job.BTN   => Job.MIN, // Miner
                Job.FSH   => Job.MIN,
                _           => job
            };

            // Combat Jobs: Use Job Name
            // DoL Jobs: Use Category Name
            string jobName = (job is Job.MIN)
                ? job.GetData().ClassJobCategory.Value.Name.ToString()
                : job.GetData().Name.ToString();

            return GetTextInfo().ToTitleCase(jobName);
        }

        public static TextInfo GetTextInfo()
        {
            // Use cached TextInfo if available
            // Otherwise create new and cache for future use
            if (_cachedTextInfo is null)
            {
                // Job names are lowercase by default
                // This capitalizes based on regional rules
                var cultureId = Svc.ClientState.ClientLanguage switch
                {
                    Dalamud.Game.ClientLanguage.French      => "fr-FR",
                    Dalamud.Game.ClientLanguage.Japanese    => "ja-JP",
                    Dalamud.Game.ClientLanguage.German      => "de-DE",
                    _                                       => "en-US",
                };

                _cachedTextInfo = new CultureInfo(cultureId, useUserOverride: false).TextInfo;
            }

            return _cachedTextInfo;
        }

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