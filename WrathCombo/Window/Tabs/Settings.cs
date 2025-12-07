#region

using Dalamud.Interface.Colors;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.ExcelServices;
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
using WrathCombo.Extensions;
using WrathCombo.Services;
using WrathCombo.Window.Functions;
using EZ = ECommons.Throttlers.EzThrottler;
using Setting = WrathCombo.Window.Functions.Setting;
using TS = System.TimeSpan;

#endregion

namespace WrathCombo.Window.Tabs;

internal class Settings : ConfigWindow
{
    private static SettingCategory.Category? _currentCategory;
    private static int                       _settingCount;
    private static string?                   _longestLabel;
    private static Dictionary<string, bool>  _unCollapsedGroup       = [];
    private static Dictionary<string, float> _unCollapsedGroupHeight = [];
    private static string[]                  _drawnCollapseGroups    = [];

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

            #region Targeting Options

            ImGuiEx.Spacing(new Vector2(0, 20));
            ImGuiEx.TextUnderlined("Targeting Options");

            var useCusHealStack = Service.Configuration.UseCustomHealStack;

            #region Retarget ST Healing Actions

            bool retargetHealingActions =
                Service.Configuration.RetargetHealingActionsToStack;
            if (ImGui.Checkbox("Retarget (Single Target) Healing Actions", ref retargetHealingActions))
            {
                Service.Configuration.RetargetHealingActionsToStack =
                    retargetHealingActions;
                Service.Configuration.Save();
            }

            ImGuiComponents.HelpMarker(
                "This will retarget all single target healing actions to the Heal Stack as shown below,\nsimilarly to how Redirect or Reaction would.\nThis ensures that the target used to check HP% threshold logic for healing actions is the same target that will receive that heal.\n\nIt is recommended to enable this if you customize the Heal Stack at all.\nDefault: Off");
            Presets.DrawRetargetedSymbolForSettingsPage();

            bool addNpcs = 
                Service.Configuration.AddOutOfPartyNPCsToRetargeting;

            if (ImGui.Checkbox("Add Out of Party NPCs to Retargeting", ref addNpcs))
            {
                Service.Configuration.AddOutOfPartyNPCsToRetargeting = addNpcs;
                Service.Configuration.Save();
            }

            ImGuiComponents.HelpMarker(
                "This will add any NPCs that are not in your party to the retargeting logic for healing actions.\n\n" +
                "This is useful for healers who want to be able to target NPCs that are not in their party, such as quest NPCs.\n\n" +
                "These NPCs will not work with any role based custom stacks (even if an NPC looks like a tank, they're not classed as one)\n\n" +
                "Default: Off");

            #endregion

            ImGuiEx.Spacing(new Vector2(0, 10));

            #region Current Heal Stack

            ImGui.TextUnformatted("Current Heal Stack:");

            ImGuiComponents.HelpMarker(
                "This is the order in which Wrath will try to select a healing target.\n\n" +
                "If the 'Retarget Healing Actions' option is disabled, that is just the target that will be used for checking the HP threshold to trigger different healing actions to show up in their rotations.\n" +
                "If the 'Retarget Healing Actions' option is enabled, that target is also the one that healing actions will be targeted onto (even when the action does not first check the HP of that target, like the combo's Replaced Action, for example).");

            ImGuiEx.Spacing(new Vector2(10, 0));
            ImGuiEx.TextWrapped(ImGuiColors.DalamudGrey,
                useCusHealStack.DisplayStack());

            ImGuiEx.Spacing(new Vector2(0, 10));

            #endregion

            #region Heal Stack Customization Options

            var labelText = "Heal Stack Customization Options";
            // Nest the Collapse into a Child of varying size, to be able to limit its width
            var dynamicHeight = _unCollapsed
                ? _healStackCustomizationHeight
                : ImGui.CalcTextSize("I").Y + 5f.Scale();
            ImGui.BeginChild("##HealStackCustomization",
                new Vector2(ImGui.CalcTextSize(labelText).X * 2.2f, dynamicHeight),
                false,
                ImGuiWindowFlags.NoScrollbar);

            // Collapsing Header for the Heal Stack Customization Options
            _unCollapsed = ImGui.CollapsingHeader(labelText,
                ImGuiTreeNodeFlags.SpanAvailWidth);
            var collapsibleHeight = ImGui.GetItemRectSize().Y;
            if (_unCollapsed)
            {
                ImGui.BeginGroup();

                #region Default Heal Stack Include: UI MouseOver

                if (useCusHealStack) ImGui.BeginDisabled();

                bool useUIMouseoverOverridesInDefaultHealStack =
                    Service.Configuration.UseUIMouseoverOverridesInDefaultHealStack;
                if (ImGui.Checkbox("Add UI MouseOver to the Default Healing Stack", ref useUIMouseoverOverridesInDefaultHealStack))
                {
                    Service.Configuration.UseUIMouseoverOverridesInDefaultHealStack =
                        useUIMouseoverOverridesInDefaultHealStack;
                    Service.Configuration.Save();
                }

                if (useCusHealStack) ImGui.EndDisabled();

                ImGuiComponents.HelpMarker("This will add any UI MouseOver targets to the top of the Default Heal Stack, overriding the rest of the stack if you are mousing over any party member UI.\n\nIt is recommended to enable this if you are a keyboard+mouse user and enable Retarget Healing Actions (or have UI MouseOver targets in your Redirect/Reaction configuration).\nDefault: Off");

                #endregion

                #region Default Heal Stack Include: Field MouseOver

                if (useCusHealStack) ImGui.BeginDisabled();

                bool useFieldMouseoverOverridesInDefaultHealStack =
                    Service.Configuration.UseFieldMouseoverOverridesInDefaultHealStack;
                if (ImGui.Checkbox("Add Field MouseOver to the Default Healing Stack", ref useFieldMouseoverOverridesInDefaultHealStack))
                {
                    Service.Configuration.UseFieldMouseoverOverridesInDefaultHealStack =
                        useFieldMouseoverOverridesInDefaultHealStack;
                    Service.Configuration.Save();
                }

                if (useCusHealStack) ImGui.EndDisabled();

                ImGuiComponents.HelpMarker("This will add any MouseOver targets to the top of the Default Heal Stack, overriding the rest of the stack if you are mousing over any nameplate UI or character model.\n\nIt is recommended to enable this only if you regularly intentionally use field mouseover targeting already.\nDefault: Off");

                #endregion

                #region Default Heal Stack Include: Focus Target

                if (useCusHealStack) ImGui.BeginDisabled();

                bool useFocusTargetOverrideInDefaultHealStack =
                    Service.Configuration.UseFocusTargetOverrideInDefaultHealStack;
                if (ImGui.Checkbox("Add Focus Target to the Default Healing Stack", ref useFocusTargetOverrideInDefaultHealStack))
                {
                    Service.Configuration.UseFocusTargetOverrideInDefaultHealStack =
                        useFocusTargetOverrideInDefaultHealStack;
                    Service.Configuration.Save();
                }

                if (useCusHealStack) ImGui.EndDisabled();

                ImGuiComponents.HelpMarker("This will add your focus target under your hard and soft targets in the Default Heal Stack, overriding the rest of the stack if you have a living focus target.\n\nDefault: Off");

                #endregion

                #region Default Heal Stack Include: Lowest HP Ally

                if (useCusHealStack) ImGui.BeginDisabled();

                bool useLowestHPOverrideInDefaultHealStack =
                    Service.Configuration.UseLowestHPOverrideInDefaultHealStack;
                if (ImGui.Checkbox("Add Lowest HP% Ally to the Default Healing Stack", ref useLowestHPOverrideInDefaultHealStack))
                {
                    Service.Configuration.UseLowestHPOverrideInDefaultHealStack =
                        useLowestHPOverrideInDefaultHealStack;
                    Service.Configuration.Save();
                }

                if (useCusHealStack) ImGui.EndDisabled();

                ImGuiComponents.HelpMarker("This will add a nearby party member with the lowest HP% to bottom of the Default Heal Stack, overriding only yourself.\n\nTHIS SHOULD BE USED WITH THE 'RETARGET HEALING ACTIONS' SETTING!\n\nDefault: Off");

                if (useCusHealStack) ImGui.BeginDisabled();
                if (useLowestHPOverrideInDefaultHealStack)
                {
                    ImGuiEx.Spacing(new Vector2(30, 0));
                    ImGuiEx.Text(ImGuiColors.DalamudYellow, "This should be used with the 'Retarget Healing Actions' setting above!");
                }
                if (useCusHealStack) ImGui.EndDisabled();

                #endregion

                ImGuiEx.Spacing(new Vector2(5, 5));
                ImGui.TextUnformatted("Or");
                ImGuiEx.Spacing(new Vector2(0, 5));

                #region Use Custom Heal Stack

                bool useCustomHealStack = Service.Configuration.UseCustomHealStack;
                if (ImGui.Checkbox("Use a Custom Heal Stack Instead", ref useCustomHealStack))
                {
                    Service.Configuration.UseCustomHealStack = useCustomHealStack;
                    Service.Configuration.Save();
                }

                ImGuiComponents.HelpMarker("Select this if you would rather make your own stack of target priorities for Heal Targets instead of using our default stack.\n\nIt is recommended to use this to align with your Redirect/Reaction configuration if you're not using the Retarget Healing Actions setup; otherwise it is preference.\nDefault: Off");

                #endregion

                #region Custom Heal Stack Manager

                if (Service.Configuration.UseCustomHealStack)
                {
                    ImGui.Indent();
                    UserConfig.DrawCustomStackManager(
                        "CustomHealStack",
                        ref Service.Configuration.CustomHealStack,
                        ["Enemy", "Attack", "Dead", "Living"],
                        "The priority goes from top to bottom.\n" +
                        "Scroll down to see all of your items.\n" +
                        "Click the Up and Down buttons to move items in the list.\n" +
                        "Click the X button to remove an item from the list.\n\n" +
                        "If there are fewer than 4 items, and all return nothing when checked, will fall back to Self.\n\n" +
                        "These targets will only be considered valid if they are friendly and within 25y.\n" +
                        "These targets will be checked for being Dead or having a Cleansable Debuff\n" +
                        "when this Stack is applied to Raises or Esuna, respectively.\n" +
                        "(For Raises: the Stack will fall back to your Hard Target or any Dead Party Member)\n\n" +
                        "Default: Focus Target > Hard Target > Self"
                    );
                    ImGui.Unindent();
                }

                #endregion

                ImGui.EndGroup();

                // Get the max height of the section above
                _healStackCustomizationHeight =
                    ImGui.GetItemRectSize().Y + collapsibleHeight + 5f.Scale();
            }

            ImGui.EndChild();

            if (_unCollapsed)
                ImGuiEx.Spacing(new Vector2(0, 10));

            #endregion

            ImGuiEx.Spacing(new Vector2(0, 10));

            #region Raise Stack Manager

            ImGui.TextUnformatted("Current Raise Stack:");

            ImGuiComponents.HelpMarker(
                "This is the order in which Wrath will try to select a " +
                "target to Raise,\nif Retargeting of any Raise Feature is enabled.\n\n" +
                "You can find Raise Features under PvE>General,\n" +
                "or under each caster that has a Raise.");

            ImGui.Indent();
            UserConfig.DrawCustomStackManager(
                "CustomRaiseStack",
                ref Service.Configuration.RaiseStack,
                ["Enemy", "Attack", "MissingHP", "Lowest", "Chocobo", "Living"],
                "The priority goes from top to bottom.\n" +
                "Scroll down to see all of your items.\n" +
                "Click the Up and Down buttons to move items in the list.\n" +
                "Click the X button to remove an item from the list.\n\n" +
                "If there are fewer than 5 items, and all return nothing when checked, will fall back to:\n" +
                "your Hard Target if they're dead, or <Any Dead Party Member>.\n\n"+
                "These targets will only be considered valid if they are friendly, dead, and within 30y.\n" +
                "Default: Any Healer > Any Tank > Any Raiser > Any Dead Party Member",
                true
            );
            ImGui.TextDisabled("(all targets are checked for rezz-ability)");
            ImGui.Unindent();

            #endregion

            #endregion

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
        _settingCount++;
        var     changed           = false;
        var     disabled          = false;
        object? newValue          = null;
        var     label             = setting.Name;
        float?  cursorXAfterInput = null;

        #region Hiding Child Settings

        if (setting.Parent is not null)
        {
            var parentValue = false;
            var parentSetting = SettingsList
                .First(s => s.FieldName == setting.Parent);
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
                    newValue = setting.Value = value;

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
                    newValue = setting.Value = value;

                break;
            }
            case Attributes.Setting.Type.Number_Int:
            {
                var value = Convert.ToInt32(setting.Value);
                ImGui.PushItemWidth(75);
                changed = ImGui.InputInt(label, ref value);
                if (changed)
                    newValue = setting.Value = value;
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
                    newValue = setting.Value = value;
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
                    newValue = setting.Value = value;
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
                    newValue = setting.Value = value;
                ImGui.SameLine();
                cursorXAfterInput = ImGui.GetCursorPosX();
                ImGui.Text(setting.UnitLabel ?? setting.Name);

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

        #region Saving

        if (changed)
        {
            Service.Configuration.TriggerUserConfigChanged(
                Configuration.ConfigChangeType.Setting,
                Configuration.ConfigChangeSource.UI,
                setting.Name, newValue!);

            Service.Configuration.Save();
        }

        #endregion
    }

    private static void DrawCollapseGroup(string groupName)
    {
        if (_drawnCollapseGroups.Contains(groupName))
            return;

        #region Setup Collapse

        var collapsedHeight = ImGui.CalcTextSize("I").Y + 5f.Scale();
        
        _unCollapsedGroup.TryAdd(groupName, false);
        _unCollapsedGroupHeight.TryAdd(groupName, collapsedHeight);
        
        var dynamicHeight = _unCollapsedGroup[groupName]
            ? _unCollapsedGroupHeight[groupName]
            : ImGui.CalcTextSize("I").Y + 5f.Scale();
        
        ImGui.BeginChild($"##{groupName}",
            new Vector2(ImGui.CalcTextSize(groupName).X * 2.2f, dynamicHeight),
            false,
            ImGuiWindowFlags.NoScrollbar);
        _unCollapsedGroup[groupName] = ImGui.CollapsingHeader(groupName,
            ImGuiTreeNodeFlags.SpanAvailWidth);
        var collapsibleHeight = ImGui.GetItemRectSize().Y;

        #endregion

        if (_unCollapsedGroup[groupName])
        {
            ImGui.BeginGroup();
            
            var settings = SettingsList
                .Where(s => s.CollapsibleGroupName == groupName).ToList();
            
            foreach (var setting in settings)
                DrawSetting(setting);
            
            ImGui.EndGroup();
            _unCollapsedGroupHeight[groupName] =
                ImGui.GetItemRectSize().Y + collapsibleHeight + 5f.Scale();
        }

        ImGui.EndChild();

        if (_unCollapsed)
            ImGuiEx.Spacing(new Vector2(0, 10));

        _drawnCollapseGroups = _drawnCollapseGroups.Append(groupName).ToArray();
    }

    #region Custom Heal Stack Manager Methods

    private static bool _unCollapsed;
    private static float _healStackCustomizationHeight = 0;

    #endregion
}