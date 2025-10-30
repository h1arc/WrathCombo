#region

using System;
// ReSharper disable ClassNeverInstantiated.Global

#endregion

// ReSharper disable InconsistentNaming

namespace WrathCombo.Attributes;

[AttributeUsage(AttributeTargets.Field)]
public class SettingCategory(SettingCategory.Category category) : Attribute
{
    public enum Category
    {
        Main_UI_Options,
        Rotation_Behavior_Options,
        Targeting_Options,
        Troubleshooting_Options,
    }

    internal Category TheCategory { get; } = category;
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingGroup(
    string groupName,
    string nameSpace,
    bool shouldThisGroupGetDisabled = true) : Attribute
{
    internal string GroupName { get; } = groupName;
    internal string NameSpace { get; } = nameSpace;
    internal bool ShouldThisGroupGetDisabled { get; } = shouldThisGroupGetDisabled;
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingParent(string parentSettingFieldName) : Attribute
{
    internal string ParentSettingFieldName { get; } = parentSettingFieldName;
}

[AttributeUsage(AttributeTargets.Field)]
public class Setting(
    string name,
    string helpMark,
    string recommendedValue,
    string defaultValue,
    string? untilLabel = null,
    Setting.Type type = Setting.Type.Toggle,
    string? warningMark = null,
    string? extraText = null,
    float? sliderMax = null,
    float? sliderMin = null) : Attribute
{
    public enum Type
    {
        Toggle,
        Color,
        Slider_Int,
        Slider_Float,
    }
    
    internal string Name { get; } = name;
    internal string HelpMark { get; } = helpMark;
    internal string RecommendedValue { get; } = recommendedValue;
    internal string DefaultValue { get; } = defaultValue;
    internal string? UntilLabel { get; } = untilLabel;
    internal Type TheType { get; } = type;
    internal string? WarningMark { get; } = warningMark;
    internal string? ExtraText { get; } = extraText;
    internal float? SliderMax { get; } = sliderMax;
    internal float? SliderMin { get; } = sliderMin;
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingUI_Space : Attribute
{
}

[AttributeUsage(AttributeTargets.Field)]
public class SettingUI_Or : Attribute
{
}