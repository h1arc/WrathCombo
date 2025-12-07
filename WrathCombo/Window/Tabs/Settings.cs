#region

using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ECommons.ImGuiMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ECommons;
using ECommons.Logging;
using WrathCombo.Attributes;
using WrathCombo.Core;
using WrathCombo.Services;
using WrathCombo.Window.Functions;
using Setting = WrathCombo.Window.Functions.Setting;

#endregion

namespace WrathCombo.Window.Tabs;

internal class Settings : ConfigWindow
{
    private static          SettingCategory.Category? _currentCategory;
    private static          int                       _settingCount;
    private static          string?                   _longestLabel;
    private static readonly Dictionary<string, bool>  UnCollapsedGroup       = [];
    private static readonly Dictionary<string, float> UnCollapsedGroupHeight = [];
    private static          string[]                  _drawnCollapseGroups    = [];

    /// <summary>
    ///     A set of dictionaries to store the latest value for grouped settings,
    ///     so groups of settings can be disabled based on the value the other group
    ///     in the namespace.
    /// </summary>
    /// <value>
    ///     <c>NameSpace</c>: The namespace that several groups may share.<br />
    ///     then<br />
    ///     <c>GroupName</c>: The name of the group within that namespace.<br />
    ///     then<br />
    ///     <c>Value</c>: The latest boolean value within the group.
    /// </value>
    /// <remarks>Note: Not related to Collapsible Groups.</remarks>
    private static Dictionary<string, Dictionary<string, bool>> _groupValues = [];

    #region Loading Settings

    private static readonly List<Setting> SettingsList = typeof(Configuration)
        .GetFields()
        .Select(rawSetting =>
        {
            try
            {
                return new Setting(rawSetting.Name);
            }
            catch (Exception e)
            {
                // Skip raw settings that fail to construct.
                PluginLog.Verbose(e.Message);
                return null;
            }
        })
        .Where(setting => setting != null)
        .Select(s => s!)
        .ToList();

    #endregion
    
    internal new static void Draw()
    {
        using (ImRaii.Child("main", new Vector2(0, 0), true))
        {
            ImGui.Text("This tab allows you to customise your options for Wrath Combo.");
            
            _currentCategory = null;
            _settingCount   = 0;
            _drawnCollapseGroups = [];

            foreach (var setting in SettingsList)
            {
                // Draw collapsible group only once
                if (setting.CollapsibleGroupName is not null)
                    DrawCollapseGroup(setting.CollapsibleGroupName);
                
                // Draw normally
                else
                    DrawSetting(setting);
            }
            
            ImGui.NewLine();
            ImGuiEx.Spacing(new Vector2(5,20));
            ImGui.Separator();
            ImGui.Separator();
            ImGuiEx.Spacing(new Vector2(5,20));

            #region Troubleshooting Options

            ImGuiEx.Spacing(new Vector2(0, 20));
            ImGuiEx.TextUnderlined("Troubleshooting / Analysis Options");

            #region Combat Log

            bool showCombatLog = Service.Configuration.EnabledOutputLog;

            if (ImGui.Checkbox("Output Log to Chat", ref showCombatLog))
            {
                Service.Configuration.EnabledOutputLog = showCombatLog;
                Service.Configuration.Save();
            }

            ImGuiComponents.HelpMarker("Every time you use an action, the plugin will print it to the chat.");
            #endregion

            #region Opener Log

            if (ImGui.Checkbox($"Output opener status to chat", ref Service.Configuration.OutputOpenerLogs))
                Service.Configuration.Save();

            ImGuiComponents.HelpMarker("Every time your class's opener is ready, fails, or finishes as expected, it will print to the chat.");
            #endregion

            #region Debug File

            if (ImGui.Button("Create Debug File"))
            {
                if (Player.Available)
                    DebugFile.MakeDebugFile();
                else
                    DebugFile.MakeDebugFile(allJobs: true);
            }

            ImGuiComponents.HelpMarker("Will generate a debug file on your desktop.\nUseful to give developers to help troubleshoot issues.\nThe same as using the following command: /wrath debug");

            #endregion

            #endregion
        }
    }

    private static void DrawSetting(Setting setting)
    {
        #region Variables

        _settingCount++;
        bool   changed;
        var    disabled          = false;
        var    label             = setting.Name;
        float? cursorXAfterInput = null;

        const string stackHelp =
            "The priority goes from top to bottom.\n" +
            "Scroll down to see all of your items.\n" +
            "Click the Up and Down buttons to move items in the list.\n" +
            "Click the X button to remove an item from the list.";

        #endregion

        #region Hiding Child Settings

        if (setting.Parent is not null)
        {
            var parentValue = false;
            var parentSetting = SettingsList
                .FirstOrDefault(s => s.FieldName == setting.Parent);
            if (parentSetting?.Value is true)
                parentValue = true;

            if (!parentValue)
                return;
        }

        #endregion

        #region Group Value Setup

        if (setting.GroupName is not null)
        {
            _groupValues.TryAdd(setting.GroupNameSpace!, []);
            _groupValues[setting.GroupNameSpace!].TryAdd(setting.GroupName!, false);
        }

        #endregion

        #region Unit Labels

        if (setting.UnitLabel is null)
            _longestLabel = null;
        else
        {
            label = "";

            // Save the label length count
            if (_longestLabel is null ||
                setting.UnitLabel.Length > _longestLabel.Length)
                _longestLabel = setting.UnitLabel;
        }

        #endregion

        #region Category Headings

        if (setting.Category != _currentCategory)
        {
            ImGuiEx.Spacing(new Vector2(0, 20));

            ImGuiEx.TextUnderlined(
                setting.Category.ToString().Replace("_", " "));

            _currentCategory = setting.Category;
        }

        #endregion

        #region Spacer

        if (setting.ShowSpace == true)
            ImGuiEx.Spacing(new Vector2(0, 10));

        #endregion

        #region Or

        if (setting.ShowOr == true)
        {
            ImGuiEx.Spacing(new Vector2(5, 5));
            ImGui.TextUnformatted("Or");
            ImGuiEx.Spacing(new Vector2(0, 5));
        }

        #endregion

        #region Indentation

        if (setting.Parent is not null)
            ImGui.Indent();

        #endregion

        #region Input Labels

        label = $"{label}" +
                $"##{setting.FieldName}{_settingCount}";

        #endregion

        #region Disabled Options

        // If this setting is on the side of a group that should be disabled,
        // check if the other group in the namespace is true.
        if (setting.GroupShouldBeDisabled == true &&
            _groupValues.TryGetValue(setting.GroupNameSpace!, out var nameSpace) &&
            nameSpace.FirstOrNull(x =>
                x.Key != setting.GroupName)?.Value == true)
        {
            disabled = true;
            ImGui.BeginDisabled();
        }

        #endregion

        #region Input

        switch (setting.Type)
        {
            case Attributes.Setting.Type.Toggle:
            {
                var value = (bool)setting.Value;

                // Update group value if applicable
                if (setting.GroupName is not null)
                    _groupValues[setting.GroupNameSpace!][setting.GroupName!] =
                        value;

                changed = ImGui.Checkbox(label, ref value);
                if (changed)
                    setting.Value = value;

                break;
            }
            case Attributes.Setting.Type.Color:
            {
                var value = (Vector4)setting.Value;
                changed = ImGui.ColorEdit4(label, ref value,
                    ImGuiColorEditFlags.NoInputs |
                    ImGuiColorEditFlags.AlphaPreview |
                    ImGuiColorEditFlags.AlphaBar);
                if (changed)
                    setting.Value = value;

                break;
            }
            case Attributes.Setting.Type.Number_Int:
            {
                var value = Convert.ToInt32(setting.Value);
                ImGui.PushItemWidth(75);
                changed = ImGui.InputInt(label, ref value);
                if (changed)
                    setting.Value = value;
                ImGui.SameLine();
                cursorXAfterInput = ImGui.GetCursorPosX();
                ImGui.Text(setting.UnitLabel ?? setting.Name);

                break;
            }
            case Attributes.Setting.Type.Number_Float:
            {
                var value = (float)setting.Value;
                ImGui.PushItemWidth(75);
                changed = ImGui.InputFloat(label, ref value);
                if (changed)
                    setting.Value = value;
                ImGui.SameLine();
                cursorXAfterInput = ImGui.GetCursorPosX();
                ImGui.Text(setting.UnitLabel ?? setting.Name);

                break;
            }
            case Attributes.Setting.Type.Slider_Int:
            {
                var value = Convert.ToInt32(setting.Value);
                ImGui.PushItemWidth(75);
                if (setting.SliderMin is null ||
                    setting.SliderMax is null)
                    changed = ImGui.SliderInt(label, ref value);
                else
                    changed = ImGui.SliderInt(label,
                        ref value,
                        (int)setting.SliderMin,
                        (int)setting.SliderMax);
                if (changed)
                    setting.Value = value;
                ImGui.SameLine();
                cursorXAfterInput = ImGui.GetCursorPosX();
                ImGui.Text(setting.UnitLabel ?? setting.Name);

                break;
            }
            case Attributes.Setting.Type.Slider_Float:
            {
                var value = (float)setting.Value;
                ImGui.PushItemWidth(75);
                if (setting.SliderMin is null ||
                    setting.SliderMax is null)
                    changed = ImGui.SliderFloat(label, ref value);
                else
                    changed = ImGui.SliderFloat(label,
                        ref value,
                        (float)setting.SliderMin,
                        (float)setting.SliderMax);
                if (changed)
                    setting.Value = value;
                ImGui.SameLine();
                cursorXAfterInput = ImGui.GetCursorPosX();
                ImGui.Text(setting.UnitLabel ?? setting.Name);

                break;
            }
            case Attributes.Setting.Type.Stack:
            {
                ImGui.PushItemWidth(300);
                // ReSharper disable once SuggestVarOrType_BuiltInTypes
                ref string[] t = ref Service.Configuration.CustomHealStack;
                if (setting.Name.Contains("Raise"))
                    t = ref Service.Configuration.RaiseStack;
                UserConfig.DrawCustomStackManager(
                    setting.Name,
                    ref t,
                    setting.StackStringsToExclude,
                    setting.HelpMark +
                    $"\n\nRecommended Value: {setting.RecommendedValue}\n" +
                    $"Default Value: {setting.DefaultValue}" +
                    $"\n\n{stackHelp}"
                );

                break;
            }
            default:
                PluginLog.Warning(
                    $"Unsupported setting type `{setting.Type}` " +
                    $"for setting `{setting.Name}`.");
                if (disabled)
                    ImGui.EndDisabled();
                if (setting.Parent is not null)
                    ImGui.Unindent();
                return;
        }

        #endregion

        #region Labels after Unit Labels

        if (setting.UnitLabel is not null)
        {
            ImGui.SameLine(
                cursorXAfterInput!.Value +
                ImGui.CalcTextSize(_longestLabel!).X
            );
            ImGui.Text($"   -   {setting.Name}");
        }

        #endregion

        #region Un-Disable
        
        if (disabled)
            ImGui.EndDisabled();

        #endregion

        #region Help Marks

        if (setting.Type != Attributes.Setting.Type.Stack)
            ImGuiComponents.HelpMarker(
                setting.HelpMark +
                $"\n\nRecommended Value: {setting.RecommendedValue}\n" +
                $"Default Value: {setting.DefaultValue}"
            );
        if (setting.ExtraHelpMark is not null)
            ImGuiComponents.HelpMarker(setting.ExtraHelpMark);
        if (setting.WarningMark is not null)
            WarningMarkerComponent.WarningMarker(setting.WarningMark);

        #endregion

        #region Extra Symbols

        if (setting.ShowRetarget is not null)
            Presets.DrawRetargetedSymbolForSettingsPage();

        #endregion

        #region Extra Text Label

        if (setting.ExtraText is not null)
        {
            ImGui.SameLine();
            ImGui.TextColored(ImGuiColors.DalamudGrey,
                setting.ExtraText);
        }

        #endregion

        #region Indentation

        if (setting.Parent is not null)
            ImGui.Unindent();

        #endregion
    }

    private static void DrawCollapseGroup(string groupName)
    {
        if (_drawnCollapseGroups.Contains(groupName))
            return;

        #region Setup Collapse

        var collapsedHeight = ImGui.CalcTextSize("I").Y + 5f.Scale();
        
        UnCollapsedGroup.TryAdd(groupName, false);
        UnCollapsedGroupHeight.TryAdd(groupName, collapsedHeight);
        
        var dynamicHeight = UnCollapsedGroup[groupName]
            ? UnCollapsedGroupHeight[groupName]
            : ImGui.CalcTextSize("I").Y + 5f.Scale();
        
        ImGui.BeginChild($"##{groupName}",
            new Vector2(ImGui.CalcTextSize(groupName).X * 2.2f, dynamicHeight),
            false,
            ImGuiWindowFlags.NoScrollbar);
        UnCollapsedGroup[groupName] = ImGui.CollapsingHeader(groupName,
            ImGuiTreeNodeFlags.SpanAvailWidth);
        var collapsibleHeight = ImGui.GetItemRectSize().Y;

        #endregion

        if (UnCollapsedGroup[groupName])
        {
            ImGui.BeginGroup();
            
            var settings = SettingsList
                .Where(s => s.CollapsibleGroupName == groupName).ToList();
            
            foreach (var setting in settings)
                DrawSetting(setting);
            
            ImGui.EndGroup();
            UnCollapsedGroupHeight[groupName] =
                ImGui.GetItemRectSize().Y + collapsibleHeight + 5f.Scale();
        }

        ImGui.EndChild();

        if (UnCollapsedGroup[groupName])
            ImGuiEx.Spacing(new Vector2(0, 10));

        _drawnCollapseGroups = _drawnCollapseGroups.Append(groupName).ToArray();
    }
}