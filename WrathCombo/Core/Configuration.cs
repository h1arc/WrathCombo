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
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;
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
        defaultValue: "#808080FF",
        type: Setting.Type.Color)]
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

    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Block Spells while Moving",
        "Will completely block actions while moving, by replacing Combo outputs with Savage Blade.\n" +
        "This would supersede combo-specific movement options, which many jobs have.",
        recommendedValue: "Off (Most Jobs will handle this better with their Features)",
        defaultValue: "Off")]
    public bool BlockSpellOnMove = false;

    /// <seealso cref="SetActionChanging" />
    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Action Replacing",
        "Controls whether Actions on your Hotbar will be Replaced with combos from the plugin.\n" +
        "If disabled, your manual presses of abilities will no longer be affected by any Wrath settings.\n\n" +
        "Auto-Rotation will work regardless of the setting.",
        recommendedValue: "On (This is essentially turning OFF most of Wrath)",
        defaultValue: "On",
        warningMark: "Wrath is largely designed with Action Replacing in mind.\n" +
                     "Disabling it may lead to unexpected behavior, such as with Retargeting.")]
    public bool ActionChanging = true;

    [SettingParent(nameof(ActionChanging))]
    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Performance Mode",
        "Controls whether Action Replacing will actually only work in the background.\n" +
        "This will prevent Combos from running on your Hotbar, but will still replace Actions before they are submitted to the server.\n",
        recommendedValue: "Off (But do try it if you have performance issues)",
        defaultValue: "Off",
        warningMark: "Wrath is largely designed with Action Replacing in mind.\n" +
                     "Disabling it -even partially- may lead to unexpected behavior, such as with Retargeting AND Openers.")]
    public bool PerformanceMode = false;

    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Queued Action Suppression",
        "While Enabled:\n" +
        "When an action is Queued that is not the same as the button on the Hotbar, Wrath will disable every other Combo, preventing them from thinking the Queued action should trigger them.\n" +
        "- This prevents combos from conflicting with each other, with overlap in actions that combos return and actions that combos replace.\n" +
        "- This does however cause the Replaced Action for each combo to 'flash' through during Suppression.\n" +
        "That 'flashed' hotbar action won't go through, it is only visual.\n\n" +
        "While Disabled:\n" +
        "Combos will not be disabled when actions are queued from a combo.\n" +
        "- This prevents your hotbars 'flashing', that is the only real benefit.\n" +
        "- This does however allow Combos to conflict with each other, if one combo returns an action that another combo has as its Replaced Action.\n" +
        "We do NOT mark these types of conflicts, and we do NOT try to avoid them as we add new features.\n\n" +
        "It is STRONGLY recommended to keep this setting On.\n" +
        "If the 'flashing' bothers you it is MUCH more advised to use Performance Mode, instead of turning this off.",
        recommendedValue: "On (NO SUPPORT if off)",
        defaultValue: "On",
        extraHelpMark: "With this enabled, whenever you queue an action that is not the same as the button you are pressing, it will disable every other button's feature from running. " +
                       "This resolves a number of issues where incorrect actions are performed due to how the game processes queued actions, however the visual experience on your hotbars is degraded. " +
                       "This is not recommended to be disabled, however if you feel uncomfortable with hotbar icons changing quickly this is one way to resolve it (or use Performance Mode) but be aware that this may introduce unintended side effects to combos if you have a lot enabled for a job.\n\n" +
                       "For a more complicated explanation, whenever an action is used, the following happens:\n" +
                       "1. If the action invokes the GCD (Weaponskills & Spells), if the GCD currently isn't active it will use it right away.\n" +
                       "2. Otherwise, if you're within the \"Queue Window\" (normally the last 0.5s of the GCD), it gets added to the queue before it is used.\n" +
                       "3. If the action is an Ability, as long as there's no animation lock currently happening it will execute right away.\n" +
                       "4. Otherwise, it is added to the queue immediately and then used when the animation lock is finished.\n\n" +
                       "For step 1, the action being passed to the game is the original, unmodified action, which is then converted at use time. " +
                       "At step 2, things get messy as the queued action still remains the unmodified action, but when the queue is executed it treats it as if the modified action *is* the unmodified action.\n\n" +
                       "E.g. Original action Cure, modified action Cure II. At step 1, the game is okay to convert Cure to Cure II because that is what we're telling it to do. However, when Cure is passed to the queue, it treats it as if the unmodified action is Cure II.\n\n" +
                       "This is similar for steps 3 & 4, except it can just happen earlier.\n\n" +
                       "How this impacts us is if using the example before, we have a feature replacing Cure with Cure II, " +
                       "and another replacing Cure II with Regen and you enable both, the following happens:\n\n" +
                       "Step 1, Cure is passed to the game, is converted to Cure II.\n" +
                       "You press Cure again at the Queue Window, Cure is passed to the queue, however the queue when it goes to execute will treat it as Cure II.\n" +
                       "Result is instead of Cure II being executed, it's Regen, because we've told it to modify Cure II to Regen.\n" +
                       "This was not part of the first Feature, but rather the result of a Feature replacing an action you did not even press, therefore an incorrect action.\n\n" +
                       "Our workaround for this is to disable all other actions being replaced if they don't match the queued action, which this setting controls.",
        warningMark: "Wrath is entirely designed with Queued Action Suppression in mind.\n" +
                     "Disabling it WILL lead to unexpected behavior, which we DO NOT support.")]
    public bool SuppressQueuedActions = true;

    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Action Updater Throttle",
        "Will restrict how often Combos will update the Action on your Hotbar.\n" +
        "At 50ms it's not really restrictive, always giving you an up to date action.\n\n" +
        "If you are looking for some (fairly minor) FPS gains then you can increase this value to make Combos run less often.\n" +
        "This makes your combos less responsive, and perhaps even clips GCDs.\n" +
        "At high values this will clip your GCDs by several seconds or break your rotation altogether.",
        recommendedValue: "20-200 (More substantial performance issues should be handled with Performance Mode instead)",
        defaultValue: "50",
        unitLabel: "milliseconds",
        type: Setting.Type.Slider_Int)]
    public int Throttle = 50;

    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Movement Check Delay",
        "This controls how long of a delay is needed before Wrath recognizes you as moving.\n" +
        "This allows you to not have to worry about small movements affecting your rotation, primarily for casters.",
        recommendedValue: "0.0-1.0 (Above that gets into the territory of breaking any Movement Options in your Job)",
        defaultValue: "0.0",
        unitLabel: "seconds",
        type: Setting.Type.Slider_Float)]
    public float MovementLeeway = 0f;

    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Opener Failure Timeout",
        "Controls how long of a gap with no action is allowed in an Opener, before it is considered failed and normal rotation is resumed.\n" +
        "Can be necessary for some casters to increase, particularly when the first action of an Opener is a hard-cast.",
        recommendedValue: "4.0-7.0 (Above that can really screw Openers)",
        defaultValue: "4.0",
        unitLabel: "seconds",
        type: Setting.Type.Slider_Float)]
    public float OpenerTimeout = 4f;

    /// The offset of the melee range check. Default: 0.
    /// <seealso cref="InMeleeRange"/>
    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Melee Distance Offset",
        "Controls what is considered to be in melee range.\n" +
        "Mainly for those who don't want to switch to ranged attacks if the boss walks slightly outside of range.\n" +
        "For example a value of -0.5 would make you have to be 0.5 yalms closer to the target,\n" +
        "or a value of 2 would allow you to be 2 yalms further away and still be considered in melee range\n" +
        "(melee actions wouldn't work, but it would give you some warning instead of just suddenly doing less optimal actions).",
        recommendedValue: "0",
        defaultValue: "0",
        unitLabel: "yalms",
        type: Setting.Type.Slider_Float)]
    public float MeleeOffset = 0;

    /// The % through a cast before interrupting. Default: 0.
    /// <seealso cref="CanInterruptEnemy"/>
    /// <seealso cref="CanStunToInterruptEnemy"/>
    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Interrupt Delay",
        "Controls the percentage of a total cast time to wait before interrupting enemy casts.\n" +
        "Applies to all interrupts (including stuns used to interrupt) in every Job's Combos.",
        recommendedValue: "below 40 (Above that and you start failing to interrupt many short casts)",
        defaultValue: "0",
        unitLabel: "% of cast",
        type: Setting.Type.Slider_Int)]
    public float InterruptDelay = 0;

    /// The maximum allowable weaves between GCDs. Default: 2.
    /// <seealso cref="CanWeave"/>
    /// <seealso cref="CanDelayedWeave"/>
    [SettingCategory(Rotation_Behavior_Options)]
    [Setting("Maximum Number of Weaves",
        "Controls how many oGCDs are allowed between GCDs.\n" +
        "The 'default' for the game is double weaving, but triple weaving is completely possible with low enough latency (of every kind);" +
        "but if you struggle with latency of any sort then single weaving may even be a good answer to try for you.\n" +
        "Triple weaving is already done in a manner where we try to avoid clipping GCDs, and as such doesn't happen particularly often even if you have good latency, and is a valid thing to do, so it is a safe option if you want.",
        recommendedValue: "2-3",
        defaultValue: "2",
        unitLabel: "oGCDs",
        type: Setting.Type.Slider_Int)]
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