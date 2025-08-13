using Dalamud.Interface.Textures;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using ECommons.DalamudServices;
using ECommons.ExcelServices;
using Lumina.Data.Files;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.Attributes;
namespace WrathCombo.Window;

internal static class Icons
{
    public static Dictionary<uint, IDalamudTextureWrap> CachedModdedIcons = new();

    public static readonly FrozenDictionary<int, IDalamudTextureWrap?[]> OccultIcons = LoadOccultJobIcons();

    // Generates a dictionary with a pair of frames at startup
    private static FrozenDictionary<int, IDalamudTextureWrap?[]> LoadOccultJobIcons()
    {
        var dict = new Dictionary<int, IDalamudTextureWrap?[]>();

        var uld = Svc.PluginInterface.UiBuilder.LoadUld("ui/uld/MKDSupportJob.uld");

        const int maxJobId = 12; //Change later when new Occult classes have sprites
        const int animationOffset = 30;

        for (int jobId = 0; jobId <= maxJobId; jobId++)
        {
            var frames = new IDalamudTextureWrap?[2];
            frames[0] = uld.LoadTexturePart("ui/uld/MKDSupportJob_hr1.tex", jobId);
            frames[1] = uld.LoadTexturePart("ui/uld/MKDSupportJob_hr1.tex", jobId + animationOffset);

            dict[jobId] = frames;
        }

        return dict.ToFrozenDictionary();
    }

    //private static int OccultIdx = -1; // Instead of 0 to show Freelancer

    private static readonly Job MaxJob = Enum.GetValues<Job>().Max();

    public static IDalamudTextureWrap? GetJobIcon(Job job)
    {
        uint iconID = job switch
        {
            Job.ADV => 62146,
            Job.MIN or Job.BTN or Job.FSH => 82096,
            _ => (uint)ExcelJobHelper.GetIcon(job)
        };

        return GetTextureFromIconId(iconID);
    }

    public static IDalamudTextureWrap? GetRoleIcon(JobRole jobRole)
    {
        const uint BaseIconID = 62580;
        uint? iconID = jobRole switch
        {
            JobRole.Tank => BaseIconID + 1,
            JobRole.Healer => BaseIconID + 2,
            JobRole.MeleeDPS => BaseIconID + 4,
            JobRole.RangedDPS => BaseIconID + 6,
            JobRole.MagicalDPS => BaseIconID + 7,
            _ => null
        };
        return iconID is null ? null : GetTextureFromIconId(iconID.Value);
    }

    //private static IDalamudTextureWrap? GetOccultIcon()
    //{
    //    if (OccultIcons.Count < 26)
    //    {
    //        for (int i = 0; i <= 25; i++)
    //        {
    //            var uld = Svc.PluginInterface.UiBuilder.LoadUld("ui/uld/MKDSupportJob.uld");
    //            OccultIcons[i] = uld.LoadTexturePart("ui/uld/MKDSupportJob_hr1.tex", i);
    //        }
    //        for (int i = 30; i <= 55; i++)
    //        {
    //            var uld = Svc.PluginInterface.UiBuilder.LoadUld("ui/uld/MKDSupportJob.uld");
    //            OccultIcons[i] = uld.LoadTexturePart("ui/uld/MKDSupportJob_hr1.tex", i);
    //        }
    //    }

    //    if (EzThrottler.Throttle("OccultAnimateIdx", 800))
    //        OccultIdx++;

    //    if (OccultIdx == 13) // Only cycle through the current ones, set to 26 after new ones added
    //        OccultIdx = 0;

    //    return OccultIcons[OccultIdx];
    //}

    private static string ResolvePath(string path) => Svc.TextureSubstitution.GetSubstitutedPath(path);

    public static IDalamudTextureWrap? GetTextureFromIconId(uint iconId, uint stackCount = 0, bool hdIcon = true)
    {
        GameIconLookup lookup = new(iconId + stackCount, false, hdIcon);
        string path = Svc.Texture.GetIconPath(lookup);
        string resolvePath = ResolvePath(path);

        var wrap = Svc.Texture.GetFromFile(resolvePath);
        if (wrap.TryGetWrap(out var icon, out _))
            return icon;

        try
        {
            if (CachedModdedIcons.TryGetValue(iconId, out IDalamudTextureWrap? cachedIcon)) return cachedIcon;
            var tex = Svc.Data.GameData.GetFileFromDisk<TexFile>(resolvePath);
            var output = Svc.Texture.CreateFromRaw(RawImageSpecification.Rgba32(tex.Header.Width, tex.Header.Width), tex.GetRgbaImageData());
            if (output != null)
            {
                CachedModdedIcons[iconId] = output;
                return output;
            }
        }
        catch { }


        return Svc.Texture.GetFromGame(path).GetWrapOrDefault();
    }
}