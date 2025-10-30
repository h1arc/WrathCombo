using Dalamud.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using WrathCombo.AutoRotation;
using WrathCombo.Services;
using WrathCombo.Window;

namespace WrathCombo.Core;

/// <summary> Plugin configuration. </summary>
[Serializable]
public partial class Configuration : IPluginConfiguration
{
    /// <summary> Gets or sets the configuration version. </summary>
    public int Version { get; set; } = 6;

    #region Settings

    /// <summary> Gets or sets a value indicating whether to output combat log to the chatbox. </summary>
    public bool EnabledOutputLog { get; set; } = false;

    /// <summary> Gets or sets a value indicating whether to hide combos which conflict with enabled presets. </summary>
    public bool HideConflictedCombos { get; set; } = false;

    /// <summary> Gets or sets a value indicating whether to hide the children of a feature if it is disabled. </summary>
    public bool HideChildren { get; set; } = false;

    /// <summary> Gets or sets the offset of the melee range check. Default is 0. </summary>
    public double MeleeOffset { get; set; } = 0;

    public bool BlockSpellOnMove = false;

    public bool ShowTargetHighlight = false;

    public Vector4 TargetHighlightColor { get; set; } = new() { W = 1, X = 0.5f, Y = 0.5f, Z = 0.5f };
    
    public bool ShowBorderAroundOptionsWithChildren = true;

    public bool OutputOpenerLogs;

    public float MovementLeeway = 0f;

    public float OpenerTimeout = 4f;

    public bool PerformanceMode = false;

    public int Throttle = 50;

    public double InterruptDelay  = 0.0f;
        
    public int MaximumWeavesPerWindow = 2;

    public bool OpenToPvE = false;

    public bool OpenToPvP = false;

    public bool OpenToCurrentJob = false;

    public bool OpenToCurrentJobOnSwitch = false;

    #region Target Settings

    public bool RetargetHealingActionsToStack = false;

    public bool AddOutOfPartyNPCsToRetargeting = false;

    public bool UseUIMouseoverOverridesInDefaultHealStack = false;

    public bool UseFieldMouseoverOverridesInDefaultHealStack = false;

    public bool UseFocusTargetOverrideInDefaultHealStack = false;

    public bool UseLowestHPOverrideInDefaultHealStack = false;

    public bool UseCustomHealStack = false;

    // Just has value so the UI element for it is more obvious from the get-go
    public string[] CustomHealStack = [
        "FocusTarget",
        "HardTarget",
        "Self",
    ];

    public string[] RaiseStack = [
        "AnyHealer",
        "AnyTank",
        "AnyRaiser",
        "AnyDeadPartyMember",
    ];

    #endregion

    public bool ActionChanging = true;

    internal void SetActionChanging(bool? newValue = null)
    {
        if (newValue is not null && newValue != ActionChanging)
        {
            ActionChanging = newValue.Value;
            Save();
        }

        // Checks if action replacing is not in line with the setting
        if (ActionChanging && !Service.ActionReplacer.getActionHook.IsEnabled)
            Service.ActionReplacer.getActionHook.Enable();
        if (!ActionChanging && Service.ActionReplacer.getActionHook.IsEnabled)
            Service.ActionReplacer.getActionHook.Disable();
    }

    public bool ShowHiddenFeatures = false;

    public bool SuppressQueuedActions = true;
        
    public bool UILeftColumnCollapsed = false;

    public bool UIShowPresetIDs = true;

    public bool UIShowSearchBar = true;

    public SearchMode SearchBehavior = SearchMode.Filter;

    public enum SearchMode
    {
        Filter,
        Highlight,
    }
    
    public bool SearchPreserveHierarchy = false; // only applicable to Filter mode

    /// <summary> Hides the message of the day. </summary>
    public bool HideMessageOfTheDay { get; set; } = false;

    /// <summary>
    ///     If the DTR Bar text should be shortened.
    /// </summary>
    public bool ShortDTRText { get; set; } = false;

    #endregion

    #region Non-Settings Configurations

    #region EnabledActions

    /// <summary> Gets or sets the collection of enabled combos. </summary>
    [JsonProperty("EnabledActionsV6")]
    public HashSet<Preset> EnabledActions { get; set; } = [];

    #endregion

    #region AutoAction Settings

    public Dictionary<Preset, bool> AutoActions { get; set; } = [];

    public AutoRotationConfig RotationConfig { get; set; } = new();

    public Dictionary<uint, uint> IgnoredNPCs { get; set; } = new();

    #endregion

    #region Job-specific

    /// <summary> Gets active Blue Mage (BLU) spells. </summary>
    public List<uint> ActiveBLUSpells { get; set; } = [];

    /// <summary> Gets or sets an array of 4 ability IDs to interact with the <see cref="Preset.DNC_CustomDanceSteps"/> combo. </summary>
    public uint[] DancerDanceCompatActionIDs { get; set; } = [ 0, 0, 0, 0, ];

    #endregion

    #region Popups

    /// <summary>
    ///     Whether the Major Changes window was hidden for a
    ///     specific version.
    /// </summary>
    /// <seealso cref="MajorChangesWindow"/>
    public Version HideMajorChangesForVersion { get; set; } =
        System.Version.Parse("0.0.0");

    #endregion

    #region UserConfig Values

    #region Custom Float Values

    [JsonProperty("CustomFloatValuesV6")]
    internal static Dictionary<string, float> CustomFloatValues { get; set; } = [];

    /// <summary> Gets a custom float value. </summary>
    public static float GetCustomFloatValue(string config, float defaultMinValue = 0)
    {
        if (!CustomFloatValues.TryGetValue(config, out float configValue))
        {
            SetCustomFloatValue(config, defaultMinValue);
            return defaultMinValue;
        }

        return configValue;
    }

    /// <summary> Sets a custom float value. </summary>
    public static void SetCustomFloatValue(string config, float value) => CustomFloatValues[config] = value;

    #endregion

    #region Custom Int Values

    [JsonProperty("CustomIntValuesV6")]
    internal static Dictionary<string, int> CustomIntValues { get; set; } = [];

    /// <summary> Gets a custom integer value. </summary>
    public static int GetCustomIntValue(string config, int defaultMinVal = 0)
    {
        if (!CustomIntValues.TryGetValue(config, out int configValue))
        {
            SetCustomIntValue(config, defaultMinVal);
            return defaultMinVal;
        }

        return configValue;
    }

    /// <summary> Sets a custom integer value. </summary>
    public static void SetCustomIntValue(string config, int value) => CustomIntValues[config] = value;

    #endregion

    #region Custom Int Array Values
    [JsonProperty("CustomIntArrayValuesV6")]
    internal static Dictionary<string, int[]> CustomIntArrayValues { get; set; } = [];

    /// <summary> Gets a custom integer array value. </summary>
    public static int[] GetCustomIntArrayValue(string config)
    {
        if (!CustomIntArrayValues.TryGetValue(config, out int[]? configValue))
        {
            SetCustomIntArrayValue(config, []);
            return [];
        }

        return configValue;
    }

    /// <summary> Sets a custom integer array value. </summary>
    public static void SetCustomIntArrayValue(string config, int[] value) => CustomIntArrayValues[config] = value;

    #endregion

    #region Custom Bool Values

    [JsonProperty("CustomBoolValuesV6")]
    internal static Dictionary<string, bool> CustomBoolValues { get; set; } = [];

    /// <summary> Gets a custom boolean value. </summary>
    public static bool GetCustomBoolValue(string config)
    {
        if (!CustomBoolValues.TryGetValue(config, out bool configValue))
        {
            SetCustomBoolValue(config, false);
            return false;
        }

        return configValue;
    }

    /// <summary> Sets a custom boolean value. </summary>
    public static void SetCustomBoolValue(string config, bool value) => CustomBoolValues[config] = value;

    #endregion

    #region Custom Bool Array Values

    [JsonProperty("CustomBoolArrayValuesV6")]
    internal static Dictionary<string, bool[]> CustomBoolArrayValues { get; set; } = [];

    /// <summary> Gets a custom boolean array value. </summary>
    public static bool[] GetCustomBoolArrayValue(string config)
    {
        if (!CustomBoolArrayValues.TryGetValue(config, out bool[]? configValue))
        {
            SetCustomBoolArrayValue(config, Array.Empty<bool>());
            return Array.Empty<bool>();
        }

        return configValue;
    }

    /// <summary> Sets a custom boolean array value. </summary>
    public static void SetCustomBoolArrayValue(string config, bool[] value) => CustomBoolArrayValues[config] = value;

    #endregion

    #endregion

    #endregion
}