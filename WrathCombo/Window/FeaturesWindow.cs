using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using WrathCombo.Core;
using WrathCombo.Services;
using WrathCombo.Window.Functions;

namespace WrathCombo.Window;

internal class FeaturesWindow : ConfigWindow
{
    internal static void DrawSearchBar()
    {
        if (!Service.Configuration.UIShowSearchBar)
            return;
        
        var width = ImGui.GetContentRegionAvail().X;
        var letterWidth = ImGui.CalcTextSize("W").X.Scale();

        using var id = ImRaii.Child("SearchBar",
            new Vector2(width, 22f.Scale()));
        if (!id)
            return;
        
        var searchLabelText = "Search:";
        var searchHintText = "Option name, ID, Internal Name, Description, etc";
        var searchDescriptionText = "Descriptions";

        var searchWidth = letterWidth * 25 + 4f.Scale();
        // line width for the search bar
        var lineWidth = searchWidth
                        // label
                        + ImGui.CalcTextSize(searchLabelText).X
                        + ImGui.GetStyle().ItemSpacing.X
                        // checkbox
                        + ImGui.GetStyle().ItemSpacing.X * 2
                        + ImGui.GetFrameHeight()
                        + ImGui.GetStyle().FramePadding.X * 2
                        + ImGui.CalcTextSize(searchDescriptionText).X;
        ImGui.SetCursorPosX((width - lineWidth) / 2f);
                        
        ImGui.Text(searchLabelText);
        ImGui.SameLine();
        ImGui.SetNextItemWidth(letterWidth*30+8f.Scale());
        ImGui.InputTextWithHint(
            "", searchHintText,
            ref Search, 30,
            ImGuiInputTextFlags.AutoSelectAll);
        ImGui.SameLine();
        ImGui.Checkbox(searchDescriptionText, ref SearchDescription);
    }

    internal static bool PresetMatchesSearch(Preset preset)
    {
        const StringComparison lower = StringComparison.OrdinalIgnoreCase;
        
        if (!IsSearching)
            return false;

        if (PresetStorage.ShouldBeHidden(preset))
            return false;

        if (!Presets.Attributes.TryGetValue(preset, out var attributes))
            attributes = new Presets.PresetAttributes(preset);

        // ID matching
        if (UsableSearch.Replace(" ", "").All(char.IsDigit) &&
            int.TryParse(UsableSearch.Replace("_", ""), out var searchNum) &&
            (int)preset == searchNum)
            return true;
        
        // Internal name matching
        if (preset.ToString().Contains(UsableSearch, lower))
            return true;
        
        // Internal name matching (without underscores)
        if (preset.ToString().Replace("_", "")
            .Contains(UsableSearch.Replace("_", ""), lower))
            return true;
        
        // Title matching
        if (attributes.CustomComboInfo.Name.Contains(UsableSearch, lower))
            return true;
        
        // Title matching (without spaces)
        if (attributes.CustomComboInfo.Name.Replace(" ", "")
            .Contains(UsableSearch.Replace(" ", ""), lower))
            return true;
        
        // Title matching (without punctuation or spaces)
        if (new string(attributes.CustomComboInfo.Name.Replace(" ", "")
                .Where(c => !char.IsPunctuation(c))
                .ToArray())
            .Contains(new string(UsableSearch.Replace(" ", "")
                .Where(c => !char.IsPunctuation(c))
                .ToArray()), lower))
            return true;

        if (SearchDescription)
        {
            // Description matching
            if (attributes.CustomComboInfo.Description.Contains(UsableSearch, lower))
                return true;
            
            // Description matching (without spaces)
            if (attributes.CustomComboInfo.Description.Replace(" ", "")
                .Contains(UsableSearch.Replace(" ", ""), lower))
                return true;
            
            // Description matching (without punctuation or spaces)
            if (new string(attributes.CustomComboInfo.Description.Replace(" ", "")
                    .Where(c => !char.IsPunctuation(c))
                    .ToArray())
                .Contains(new string(UsableSearch.Replace(" ", "")
                    .Where(c => !char.IsPunctuation(c))
                    .ToArray()), lower))
                return true;
        }

        return false;
    }
}