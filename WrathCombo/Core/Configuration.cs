#region

using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Configuration;
using Newtonsoft.Json;
using WrathCombo.AutoRotation;
using WrathCombo.Window;
using WrathCombo.Attributes;
using static WrathCombo.Attributes.SettingCategory.Category;
using Space = WrathCombo.Attributes.SettingUI_Space;
using Or = WrathCombo.Attributes.SettingUI_Or;

#endregion

// ReSharper disable RedundantDefaultMemberInitializer

namespace WrathCombo.Core;

/// <summary> Plugin configuration. </summary>
[Serializable]
public partial class Configuration : IPluginConfiguration
{
    /// <summary> Gets or sets the configuration version. </summary>
    public int Version { get; set; } = 6;

    #region Settings

    #region UI Settings

    /// <summary>
    ///     Gets or sets a value indicating whether to hide the children of a feature
    ///     if it is disabled.
    /// </summary>
    [SettingCategory(Main_UI_Options)]
    [Setting("Hide Sub-Combo Options",
        "Will hide the Features and Options under disabled Combos.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool HideChildren = false;

    /// <summary>
    ///     Gets or sets a value indicating whether to hide combos which conflict with
    ///     enabled presets.
    /// </summary>
    [SettingCategory(Main_UI_Options)]
    [Setting("Hide Conflicted Combos",
        "Will hide Combos that conflict with Combos that you have enabled.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool HideConflictedCombos = false;

    /// <summary>
    ///     If the DTR Bar text should be shortened.
    /// </summary>
    [SettingCategory(Main_UI_Options)]
    [Setting("Shorten Server Info Bar Text",
        "Will hide the number of active Auto-Mode Combos.\n" +
        "By default the Server Info Bar shows:\n" +
        "- Whether Auto-Rotation is on or off\n" +
        "- (if on) The number of active Auto-Mode Combos\n" +
        "- (if applicable) If another plugin is controlling the state of Auto-Rotation.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool ShortDTRText = false;

    /// <summary> Hides the message of the day. </summary>
    [SettingCategory(Main_UI_Options)]
    [Setting("Hide Message of the Day",
        "Will prevent the Message of the Day from being shown in your chat upon login.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool HideMessageOfTheDay = false;

    [SettingCategory(Main_UI_Options)]
    [Setting("Show Target Highlighter",
        "Draws a box around party members in the vanilla Party List, when targeted by certain Features.",
        recommendedValue: "Preference",
        defaultValue: "Off",
        extraText: "(Only used by AST and DNC currently)")]
    public bool ShowTargetHighlight = false;

    [SettingParent(nameof(ShowTargetHighlight))]
    [SettingCategory(Main_UI_Options)]
    [Setting("Target Highlighter Color",
        "Controls the color of the box drawn around party members.",
        recommendedValue: "Preference",
        defaultValue: "#808080FF")]
    public Vector4 TargetHighlightColor =
        new() { W = 1, X = 0.5f, Y = 0.5f, Z = 0.5f };

    [SettingCategory(Main_UI_Options)]
    [Setting("Show Borders around Combos and Features with Options",
        "Will draw a border around Combos and Features that have Features and Options of their own.",
        recommendedValue: "Preference",
        defaultValue: "On")]
    public bool ShowBorderAroundOptionsWithChildren = true;

    [SettingCategory(Main_UI_Options)]
    [Setting("Show Preset IDs next to Combo Names",
        "Displays the Preset ID number next to the name of each Combo and Feature.\n" +
        "These are the IDs used for commands like `/wrath toggle <ID>`.\n" +
        "Pre-7.3 the behavior was to show a number here, but it was much shorter, and did not work in commands.",
        recommendedValue: "On",
        defaultValue: "On")]
    public bool UIShowPresetIDs = true;

    [SettingCategory(Main_UI_Options)]
    [Setting("Show Search Bars",
        "Controls whether Search Bars should be shown in Settings, and PvE and PvP Jobs.",
        recommendedValue: "On",
        defaultValue: "On")]
    public bool UIShowSearchBar = true;

    #region Future Search Settings

    public SearchMode SearchBehavior = SearchMode.Filter;

    public enum SearchMode
    {
        Filter,
        Highlight,
    }

    public bool SearchPreserveHierarchy = false; // only applicable to Filter mode

    #endregion

    [Space]
    [SettingCategory(Main_UI_Options)]
    [Setting("Open Wrath to the PvE Features Tab",
        "When you open Wrath with `/wrath`, it will open to the PvE Features tab, instead of the last tab you were on." +
        "\nSame as always using the `/wrath pve` command to open Wrath.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool OpenToPvE = false;

    [SettingCategory(Main_UI_Options)]
    [Setting("Open Wrath to the PvP Features Tab in PvP areas",
        "Same as above, when you open Wrath with `/wrath`, it will open to the PvP Features tab, instead of the last tab you were on, when in a PvP area." +
        "\nSimilar to using the `/wrath pvp` command to open Wrath.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool OpenToPvP = false;

    [SettingCategory(Main_UI_Options)]
    [Setting("Open PvE Features Tab to Current Job on Opening",
        "When the PvE Features tab is opened it will automatically open to your current Job.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool OpenToCurrentJob = false;

    [SettingCategory(Main_UI_Options)]
    [Setting("Open PvE Features Tab to Current Job on Opening",
        "Will automatically switch the PvE Features tab to the job you are currently playing, when you switch jobs.",
        recommendedValue: "Preference",
        defaultValue: "Off")]
    public bool OpenToCurrentJobOnSwitch = false;

    #endregion

    #region Rotation Behavior Settings

    public bool BlockSpellOnMove = false;

    /// <seealso cref="SetActionChanging" />
    public bool ActionChanging = true;

    [SettingParent(nameof(ActionChanging))]
    public bool PerformanceMode = false;

    public bool SuppressQueuedActions = true;

    public int Throttle = 50;

    public float MovementLeeway = 0f;

    public float OpenerTimeout = 4f;

    /// <summary> Gets or sets the offset of the melee range check. Default is 0. </summary>
    public float MeleeOffset = 0;

    public float InterruptDelay = 0;

    public int MaximumWeavesPerWindow = 2;

    #endregion

    #region Target Settings

    public bool RetargetHealingActionsToStack = false;

    public bool AddOutOfPartyNPCsToRetargeting = false;

    #region Default+ Heal Stack

    public bool UseUIMouseoverOverridesInDefaultHealStack = false;

    public bool UseFieldMouseoverOverridesInDefaultHealStack = false;

    public bool UseFocusTargetOverrideInDefaultHealStack = false;

    public bool UseLowestHPOverrideInDefaultHealStack = false;

    #endregion

    #region Custom Heal Stack

    public bool UseCustomHealStack = false;

    public string[] CustomHealStack =
    [
        "FocusTarget",
        "HardTarget",
        "Self",
    ];

    #endregion

    #region Rez Stack

    public string[] RaiseStack =
    [
        "AnyHealer",
        "AnyTank",
        "AnyRaiser",
        "AnyDeadPartyMember",
    ];

    #endregion

    #endregion

    #region Troubleshooting

    /// <summary>
    ///     Gets or sets a value indicating whether to output combat log to the
    ///     chatbox.
    /// </summary>
    public bool EnabledOutputLog { get; set; } = false;

    public bool OutputOpenerLogs;

    #endregion

    #endregion

    #region Non-Settings Configurations

    public bool UILeftColumnCollapsed = false;

    public bool ShowHiddenFeatures = false;

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

    /// <summary>
    ///     Gets or sets an array of 4 ability IDs to interact with the
    ///     <see cref="Preset.DNC_CustomDanceSteps" /> combo.
    /// </summary>
    public uint[] DancerDanceCompatActionIDs { get; set; } = [0, 0, 0, 0,];

    #endregion

    #region Popups

    /// <summary>
    ///     Whether the Major Changes window was hidden for a
    ///     specific version.
    /// </summary>
    /// <seealso cref="MajorChangesWindow" />
    public Version HideMajorChangesForVersion =
        System.Version.Parse("0.0.0");

    #endregion

    #region UserConfig Values

    [JsonProperty("CustomFloatValuesV6")]
    internal static Dictionary<string, float>
        CustomFloatValues { get; set; } = [];

    [JsonProperty("CustomIntValuesV6")]
    internal static Dictionary<string, int>
        CustomIntValues { get; set; } = [];

    [JsonProperty("CustomIntArrayValuesV6")]
    internal static Dictionary<string, int[]>
        CustomIntArrayValues { get; set; } = [];

    [JsonProperty("CustomBoolValuesV6")]
    internal static Dictionary<string, bool>
        CustomBoolValues { get; set; } = [];

    [JsonProperty("CustomBoolArrayValuesV6")]
    internal static Dictionary<string, bool[]>
        CustomBoolArrayValues { get; set; } = [];

    #endregion

    #endregion
}